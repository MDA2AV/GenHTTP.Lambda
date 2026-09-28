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
/// Compiles lambdas with Roslyn and keeps the built handlers around: one for
/// what a lambda has online, and one for the preview of each of its features.
/// </summary>
/// <remarks>
/// Everything runs in this process for now. Once lambdas are moved into their
/// own containers, this is the service that would start and track them.
///
/// A preview is built exactly like the lambda, from the feature's files, into
/// a place of its own: its own assets, and the feature's copy of the
/// workspace compiled in as the workspace - so trying a feature can do
/// anything to the data without the lambda noticing.
/// </remarks>
public sealed class DeploymentService : IDeploymentService, IDisposable
{
    private readonly ConcurrentDictionary<Slot, CompiledLambda> _deployed = [];

    /// <summary>
    /// What is online but would not build, and why - so the requests to it
    /// are told at once instead of each compiling it again.
    /// </summary>
    private readonly ConcurrentDictionary<Slot, Breakage> _broken = [];

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

    public ValueTask<CompilationOutcome> ActivateAsync(long lambdaId, int version, WorkspaceLimits limits, CancellationToken cancellation = default)
        => ActivateAsync(new Slot(lambdaId, null), version, limits, _ => Storage.ReadAsync(lambdaId, version, cancellation), cancellation);

    public ValueTask<IHandler> ResolveAsync(long lambdaId, int version, WorkspaceLimits limits, CancellationToken cancellation = default)
        => ResolveAsync(new Slot(lambdaId, null), version, limits, _ => Storage.ReadAsync(lambdaId, version, cancellation), cancellation);

    public ValueTask<CompilationOutcome> PreviewAsync(long lambdaId, long featureId, int preview, string code, WorkspaceLimits limits,
                                                      CancellationToken cancellation = default)
        => ActivateAsync(new Slot(lambdaId, featureId), preview, limits, _ => ValueTask.FromResult<string?>(code), cancellation);

    public ValueTask<IHandler> ResolvePreviewAsync(long lambdaId, long featureId, int preview, WorkspaceLimits limits,
                                                   CancellationToken cancellation = default)
        => ResolveAsync(new Slot(lambdaId, featureId), preview, limits, _ => Storage.ReadPreviewAsync(lambdaId, featureId, cancellation), cancellation);

    /// <summary>
    /// Builds what a slot is to serve and makes it what it serves.
    /// </summary>
    /// <param name="stamp">Which build this is: the version, or the deployment of a preview</param>
    /// <param name="read">Where the code comes from</param>
    private async ValueTask<CompilationOutcome> ActivateAsync(Slot slot, int stamp, WorkspaceLimits limits, Func<CancellationToken, ValueTask<string?>> read,
                                                              CancellationToken cancellation)
    {
        if (IsDeployed(slot, stamp, limits))
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
            if (IsDeployed(slot, stamp, limits))
            {
                return CompilationOutcome.Succeeded();
            }

            var code = await read(cancellation);

            if (code == null)
            {
                return CompilationOutcome.Failed(slot.FeatureId == null
                    ? $"Version {stamp} of this lambda does not exist anymore."
                    : "The preview of this feature has nothing to serve anymore.");
            }

            var files = LambdaSource.Parse(code);

            // what this version ships is written out before it is compiled, so the
            // handler it returns is serving the assets of the version going online
            // rather than whatever the last one left behind
            Materialize(slot, files);

            var name = slot.FeatureId is { } feature ? $"{slot.LambdaId}_f{feature}_{stamp}" : $"{slot.LambdaId}_{stamp}";

            var request = new CompilationRequest(files, Storage.GetWorkspace(slot.LambdaId, slot.FeatureId),
                                                 Storage.GetAssetDirectory(slot.LambdaId, slot.FeatureId),
                                                 Storage.GetAssemblyDirectory(slot.LambdaId), name, true, limits);

            var outcome = await CompileAsync(slot, stamp, request);

            if (outcome.Success)
            {
                _broken.TryRemove(slot, out _);
            }
            else
            {
                await RestoreAsync(slot, cancellation);
            }

            return outcome;
        }
        finally
        {
            _compiling.Release();
        }
    }

    private async ValueTask<CompilationOutcome> CompileAsync(Slot slot, int stamp, CompilationRequest request)
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

            _deployed[slot] = new CompiledLambda(stamp, request.Limits, handler);

            Logger.LogInformation("Deployed {Slot} as build {Stamp}", slot, stamp);

            return outcome;
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "Failed to deploy {Slot} as build {Stamp}", slot, stamp);

            return CompilationOutcome.Failed(e.Message);
        }
    }

    /// <summary>
    /// Writes the assets of what is being served back, after what was meant
    /// to replace it did not build.
    /// </summary>
    /// <remarks>
    /// The assets are written out before the code is compiled, because the
    /// code may read them while it builds its handler - so a deployment that
    /// fails has already replaced them, and the handler that is still online
    /// would serve the assets of code that never went online. A preview is
    /// deployed again and again while a feature is worked on, which made this
    /// a matter of course rather than a rarity.
    /// </remarks>
    private async ValueTask RestoreAsync(Slot slot, CancellationToken cancellation)
    {
        if (!_deployed.TryGetValue(slot, out var running))
        {
            return;
        }

        try
        {
            var code = slot.FeatureId is { } feature
                ? await Storage.ReadPreviewAsync(slot.LambdaId, feature, cancellation)
                : await Storage.ReadAsync(slot.LambdaId, running.Stamp, cancellation);

            if (code != null)
            {
                Materialize(slot, LambdaSource.Parse(code));
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "The assets of {Slot} could not be written back", slot);
        }
    }

    private async ValueTask<IHandler> ResolveAsync(Slot slot, int stamp, WorkspaceLimits limits, Func<CancellationToken, ValueTask<string?>> read,
                                                   CancellationToken cancellation)
    {
        if (_deployed.TryGetValue(slot, out var existing) && existing.Stamp == stamp && existing.Limits == limits)
        {
            return existing.Handler;
        }

        // the same thing failed a moment ago and nothing about it has changed:
        // compiling it again for every visitor would change nothing but the load
        if (_broken.TryGetValue(slot, out var breakage) && breakage.Matches(stamp, limits))
        {
            throw new InvalidOperationException(breakage.Message);
        }

        var outcome = await ActivateAsync(slot, stamp, limits, read, cancellation);

        if (!outcome.Success)
        {
            var message = outcome.Diagnostics.Count > 0 ? outcome.Diagnostics[0].Message : "unknown error";

            var reason = $"The code of this lambda does not compile: {message}";

            _broken[slot] = new Breakage(stamp, limits, reason);

            throw new InvalidOperationException(reason);
        }

        return _deployed[slot].Handler;
    }

    /// <summary>
    /// Whether the slot serves this build already, compiled with these limits.
    /// </summary>
    private bool IsDeployed(Slot slot, int stamp, WorkspaceLimits limits)
        => _deployed.TryGetValue(slot, out var existing) && existing.Stamp == stamp && existing.Limits == limits;

    /// <summary>
    /// Writes what is being deployed into the directory it reads its assets from.
    /// </summary>
    /// <remarks>
    /// Emptied first, so an asset dropped from a version stops being served
    /// rather than lingering because nothing overwrote it. Names were checked
    /// before they got here, and the path is resolved against the root again
    /// anyway - a file that would land outside is skipped rather than trusted.
    /// </remarks>
    private void Materialize(Slot slot, IReadOnlyList<LambdaFile> files)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Storage.GetAssetDirectory(slot.LambdaId, slot.FeatureId)))
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
                    Logger.LogWarning("The asset '{Name}' of {Slot} would land outside its directory and was skipped", file.Name, slot);

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
            Logger.LogWarning(e, "The assets of {Slot} could not be written", slot);
        }
    }

    public void Evict(long lambdaId) => Drop(new Slot(lambdaId, null));

    public void EvictAll(long lambdaId)
    {
        foreach (var slot in _deployed.Keys.Concat(_broken.Keys).Where(s => s.LambdaId == lambdaId).Distinct().ToList())
        {
            Drop(slot);
        }
    }

    private void Drop(Slot slot)
    {
        _broken.TryRemove(slot, out _);

        if (_deployed.TryRemove(slot, out _))
        {
            Logger.LogInformation("Removed the running deployment of {Slot}", slot);
        }
    }

    public void EvictPreview(long lambdaId, long featureId) => Drop(new Slot(lambdaId, featureId));

    public void Dispose()
    {
        _deployed.Clear();
        _broken.Clear();

        _compiling.Dispose();
    }

    #endregion

    /// <summary>
    /// Where a built handler is kept: the lambda itself, or the preview of one
    /// of its features.
    /// </summary>
    private readonly record struct Slot(long LambdaId, long? FeatureId)
    {
        public override string ToString() => FeatureId is { } feature ? $"lambda {LambdaId}, feature {feature}" : $"lambda {LambdaId}";
    }

    /// <summary>
    /// What is online, as it was when it failed to build.
    /// </summary>
    /// <remarks>
    /// Believed for a minute and then tried again, since a snippet can fail
    /// on something outside itself - a service it calls while it starts up
    /// being down - that mends without anybody deploying anything.
    /// </remarks>
    private sealed record Breakage(int Stamp, WorkspaceLimits Limits, string Message)
    {
        private readonly DateTime _noticed = DateTime.UtcNow;

        public bool Matches(int stamp, WorkspaceLimits limits)
            => Stamp == stamp && Limits == limits && DateTime.UtcNow - _noticed < TimeSpan.FromMinutes(1);
    }

}
