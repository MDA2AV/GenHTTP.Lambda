using System.Collections.Concurrent;

using GenHTTP.Api.Content;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// Compiles lambdas with Roslyn and keeps one built handler per lambda around.
/// </summary>
/// <remarks>
/// Everything runs in this process for now. Once lambdas are moved into their
/// own containers, this is the service that would start and track them.
/// </remarks>
public sealed class DeploymentService : IDeploymentService, IDisposable
{
    private readonly ConcurrentDictionary<long, CompiledLambda> _deployed = [];

    /// <summary>
    /// What is online but would not build, and why - so the requests to it
    /// are told at once instead of each compiling it again.
    /// </summary>
    private readonly ConcurrentDictionary<long, Breakage> _broken = [];

    // compilation is memory hungry, so only one snippet is built at a time
    private readonly SemaphoreSlim _compiling = new(1, 1);

    #region Get-/Setters

    private IStorageService Storage { get; }

    private ServerRegistry Servers { get; }

    private ILogger Logger { get; }

    #endregion

    #region Initialization

    public DeploymentService(IStorageService storage, ServerRegistry servers, ILogger<DeploymentService> logger)
    {
        Storage = storage;
        Servers = servers;
        Logger = logger;

        ModuleCatalog.LoadModules();
    }

    #endregion

    #region Functionality

    public async ValueTask<CompilationOutcome> ValidateAsync(string code, long? lambdaId = null, WorkspaceLimits? limits = null, CancellationToken cancellation = default)
    {
        var id = lambdaId ?? 0;

        var request = new CompilationRequest(LambdaSource.Parse(code), Storage.GetWorkspace(id), Storage.GetAssetDirectory(id),
                                            Storage.GetAssemblyDirectory(id), $"check_{id}", false, limits ?? WorkspaceLimits.Standard);

        await _compiling.WaitAsync(cancellation);

        try
        {
            var (outcome, _) = await LambdaCompiler.CompileAsync(request);

            return outcome;
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "Failed to check the code of lambda {LambdaId}", lambdaId);

            return CompilationOutcome.Failed(e.Message);
        }
        finally
        {
            _compiling.Release();
        }
    }

    public ValueTask<CompilationOutcome> ActivateAsync(long lambdaId, int version, int revision, WorkspaceLimits limits, CancellationToken cancellation = default)
        => ActivateAsync(lambdaId, version, revision, limits, false, cancellation);

    /// <param name="online">
    /// Whether to build what was deployed - for serving it - rather than the
    /// version as it was last saved, which is what a deployment puts online
    /// </param>
    private async ValueTask<CompilationOutcome> ActivateAsync(long lambdaId, int version, int revision, WorkspaceLimits limits, bool online,
                                                              CancellationToken cancellation)
    {
        if (IsDeployed(lambdaId, version, revision, limits))
        {
            return CompilationOutcome.Succeeded();
        }

        /*
         * Everything from reading the version on happens inside the lock, not
         * only the compilation. The requests that reach a lambda nobody has
         * compiled since the server started - or since its tier moved - all
         * arrive here together, and each of them used to read, unpack and
         * write out the whole version before queueing for the compiler; with
         * a hundred megabytes of assets that is a hundred megabytes, several
         * times over, per request. Now the first one does it and the others
         * find it done.
         */
        await _compiling.WaitAsync(cancellation);

        try
        {
            if (IsDeployed(lambdaId, version, revision, limits))
            {
                return CompilationOutcome.Succeeded();
            }

            var code = online ? await Storage.ReadOnlineAsync(lambdaId, version, cancellation)
                              : await Storage.ReadAsync(lambdaId, version, cancellation);

            if (code == null)
            {
                return CompilationOutcome.Failed($"Version {version} of this lambda does not exist anymore.");
            }

            var files = LambdaSource.Parse(code);

            // what this version ships is written out before it is compiled, so the
            // handler it returns is serving the assets of the version going online
            // rather than whatever the last one left behind
            Materialize(lambdaId, files);

            var request = new CompilationRequest(files, Storage.GetWorkspace(lambdaId), Storage.GetAssetDirectory(lambdaId),
                                                Storage.GetAssemblyDirectory(lambdaId), $"{lambdaId}_{version}", true, limits);

            var outcome = await CompileAsync(lambdaId, version, revision, request);

            if (outcome.Success)
            {
                _broken.TryRemove(lambdaId, out _);
            }
            else
            {
                await RestoreAsync(lambdaId, cancellation);
            }

            return outcome;
        }
        finally
        {
            _compiling.Release();
        }
    }

    private async ValueTask<CompilationOutcome> CompileAsync(long lambdaId, int version, int revision, CompilationRequest request)
    {
        try
        {
            var (outcome, handler) = await LambdaCompiler.CompileAsync(request);

            if (!outcome.Success || handler == null)
            {
                return outcome;
            }

            await handler.PrepareAsync(Servers.Require());

            var broken = HandlerInspector.Inspect(handler);

            if (broken.Count > 0)
            {
                return CompilationOutcome.Failed(broken);
            }

            _deployed[lambdaId] = new CompiledLambda(version, revision, request.Limits, handler);

            Logger.LogInformation("Deployed lambda {LambdaId} in version {Version}, revision {Revision}", lambdaId, version, revision);

            return outcome;
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "Failed to deploy lambda {LambdaId} in version {Version}", lambdaId, version);

            return CompilationOutcome.Failed(e.Message);
        }
    }

    /// <summary>
    /// Writes the assets of what is being served back, after a version that
    /// was meant to replace it did not build.
    /// </summary>
    /// <remarks>
    /// The assets of a version are written out before it is compiled, because
    /// its code may read them while it builds its handler - so a deployment
    /// that fails has already replaced them, and the handler that is still
    /// online would serve the assets of code that never went online. That was
    /// rare while every change was a new version; now the version online is
    /// saved over and deployed again as a matter of course.
    /// </remarks>
    private async ValueTask RestoreAsync(long lambdaId, CancellationToken cancellation)
    {
        if (!_deployed.TryGetValue(lambdaId, out var running))
        {
            return;
        }

        try
        {
            var code = await Storage.ReadOnlineAsync(lambdaId, running.Version, cancellation);

            if (code != null)
            {
                Materialize(lambdaId, LambdaSource.Parse(code));
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "The assets of lambda {LambdaId} could not be written back", lambdaId);
        }
    }

    public async ValueTask<IHandler> ResolveAsync(long lambdaId, int version, int revision, WorkspaceLimits limits, CancellationToken cancellation = default)
    {
        if (_deployed.TryGetValue(lambdaId, out var existing) && existing.Version == version && existing.Revision == revision && existing.Limits == limits)
        {
            return existing.Handler;
        }

        // the same thing failed a moment ago and nothing about it has changed:
        // compiling it again for every visitor would change nothing but the load
        if (_broken.TryGetValue(lambdaId, out var breakage) && breakage.Matches(version, revision, limits))
        {
            throw new InvalidOperationException(breakage.Message);
        }

        var outcome = await ActivateAsync(lambdaId, version, revision, limits, true, cancellation);

        if (!outcome.Success)
        {
            var message = outcome.Diagnostics.Count > 0 ? outcome.Diagnostics[0].Message : "unknown error";

            var reason = $"The code of this lambda does not compile: {message}";

            _broken[lambdaId] = new Breakage(version, revision, limits, reason);

            throw new InvalidOperationException(reason);
        }

        return _deployed[lambdaId].Handler;
    }

    /// <summary>
    /// Whether the version is online already, in this revision, compiled with these limits.
    /// </summary>
    private bool IsDeployed(long lambdaId, int version, int revision, WorkspaceLimits limits)
        => _deployed.TryGetValue(lambdaId, out var existing) && existing.Version == version && existing.Revision == revision && existing.Limits == limits;

    /// <summary>
    /// Writes what a version ships into the directory the lambda reads it from.
    /// </summary>
    /// <remarks>
    /// Emptied first, so an asset dropped from a version stops being served
    /// rather than lingering because nothing overwrote it. Names were checked
    /// before they got here, and the path is resolved against the root again
    /// anyway - a file that would land outside is skipped rather than trusted.
    /// </remarks>
    private void Materialize(long lambdaId, IReadOnlyList<LambdaFile> files)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Storage.GetAssetDirectory(lambdaId)))
                 + Path.DirectorySeparatorChar;

        try
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }

            Directory.CreateDirectory(root);

            foreach (var file in files)
            {
                if (file.IsCode)
                {
                    continue;
                }

                var path = Path.GetFullPath(Path.Combine(root, file.Name));

                if (!path.StartsWith(root, StringComparison.Ordinal))
                {
                    Logger.LogWarning("The asset '{Name}' of lambda {LambdaId} would land outside its directory and was skipped", file.Name, lambdaId);

                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                File.WriteAllBytes(path, file.Bytes);
            }
        }
        catch (Exception e)
        {
            // a lambda that ships nothing is still a lambda; failing the whole
            // deployment because a file could not be written would be worse
            Logger.LogWarning(e, "The assets of lambda {LambdaId} could not be written", lambdaId);
        }
    }

    public void Evict(long lambdaId)
    {
        _broken.TryRemove(lambdaId, out _);

        if (_deployed.TryRemove(lambdaId, out _))
        {
            Logger.LogInformation("Removed the running deployment of lambda {LambdaId}", lambdaId);
        }
    }

    public void Dispose()
    {
        _deployed.Clear();
        _broken.Clear();

        _compiling.Dispose();
    }

    #endregion

    /// <summary>
    /// What is online, as it was when it failed to build.
    /// </summary>
    /// <remarks>
    /// Believed for a minute and then tried again, since a snippet can fail
    /// on something outside itself - a service it calls while it starts up
    /// being down - that mends without anybody deploying anything.
    /// </remarks>
    private sealed record Breakage(int Version, int Revision, WorkspaceLimits Limits, string Message)
    {
        private readonly DateTime _noticed = DateTime.UtcNow;

        public bool Matches(int version, int revision, WorkspaceLimits limits)
            => Version == version && Revision == revision && Limits == limits && DateTime.UtcNow - _noticed < TimeSpan.FromMinutes(1);
    }

}
