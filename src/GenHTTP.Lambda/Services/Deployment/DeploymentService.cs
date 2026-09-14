using System.Collections.Concurrent;

using GenHTTP.Api.Content;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Storage;

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

    public async ValueTask<CompilationOutcome> ValidateAsync(string code, long? lambdaId = null, CancellationToken cancellation = default)
    {
        var id = lambdaId ?? 0;

        var request = new CompilationRequest(LambdaSource.Parse(code), Storage.GetWorkspace(id), Storage.GetAssetDirectory(id),
                                            Storage.GetAssemblyDirectory(id), $"check_{id}", false);

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

    public async ValueTask<CompilationOutcome> ActivateAsync(long lambdaId, int version, CancellationToken cancellation = default)
    {
        if (_deployed.TryGetValue(lambdaId, out var existing) && existing.Version == version)
        {
            return CompilationOutcome.Succeeded();
        }

        var code = await Storage.ReadAsync(lambdaId, version, cancellation);

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
                                            Storage.GetAssemblyDirectory(lambdaId), $"{lambdaId}_{version}", true);

        await _compiling.WaitAsync(cancellation);

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

            _deployed[lambdaId] = new CompiledLambda(version, handler);

            Logger.LogInformation("Deployed lambda {LambdaId} in version {Version}", lambdaId, version);

            return outcome;
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "Failed to deploy lambda {LambdaId} in version {Version}", lambdaId, version);

            return CompilationOutcome.Failed(e.Message);
        }
        finally
        {
            _compiling.Release();
        }
    }

    public async ValueTask<IHandler> ResolveAsync(long lambdaId, int version, CancellationToken cancellation = default)
    {
        if (_deployed.TryGetValue(lambdaId, out var existing) && existing.Version == version)
        {
            return existing.Handler;
        }

        var outcome = await ActivateAsync(lambdaId, version, cancellation);

        if (!outcome.Success)
        {
            var message = outcome.Diagnostics.Count > 0 ? outcome.Diagnostics[0].Message : "unknown error";

            throw new InvalidOperationException($"The code of this lambda does not compile: {message}");
        }

        return _deployed[lambdaId].Handler;
    }

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
        if (_deployed.TryRemove(lambdaId, out _))
        {
            Logger.LogInformation("Removed the running deployment of lambda {LambdaId}", lambdaId);
        }
    }

    public void Dispose()
    {
        _deployed.Clear();

        _compiling.Dispose();
    }

    #endregion

}
