using GenHTTP.Lambda.Configuration;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Stores the code of a lambda as <c>{data}/code/{id}/v{version}.cs</c> and gives
/// every lambda a private directory below <c>{data}/workspaces/{id}</c>.
/// </summary>
public sealed class FileSystemStorageService : IStorageService
{

    #region Get-/Setters

    private LambdaOptions Options { get; }

    private ILogger Logger { get; }

    #endregion

    #region Initialization

    public FileSystemStorageService(LambdaOptions options, ILogger<FileSystemStorageService> logger)
    {
        Options = options;
        Logger = logger;

        Directory.CreateDirectory(options.CodeDirectory);
        Directory.CreateDirectory(options.WorkspaceDirectory);

        // nothing is loaded yet, so this is the one moment where the artifacts
        // of previous runs can safely be thrown away
        Remove(options.AssemblyDirectory);

        Directory.CreateDirectory(options.AssemblyDirectory);
    }

    #endregion

    #region Functionality

    public async ValueTask WriteAsync(long lambdaId, int version, string code, CancellationToken cancellation = default)
    {
        var directory = GetCodeDirectory(lambdaId);

        Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(GetFile(lambdaId, version), code, cancellation);
    }

    public async ValueTask<string?> ReadAsync(long lambdaId, int version, CancellationToken cancellation = default)
    {
        var file = GetFile(lambdaId, version);

        if (!File.Exists(file))
        {
            return null;
        }

        return await File.ReadAllTextAsync(file, cancellation);
    }

    public ValueTask DeleteVersionAsync(long lambdaId, int version, CancellationToken cancellation = default)
    {
        var file = GetFile(lambdaId, version);

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(long lambdaId, CancellationToken cancellation = default)
    {
        Remove(GetCodeDirectory(lambdaId));
        Remove(GetWorkspaceDirectory(lambdaId));
        Remove(GetAssetDirectory(lambdaId));

        // the generated assembly stays: it cannot be unloaded and GenHTTP builds
        // its invocation code from the files behind the loaded assemblies, so
        // removing one here would break the compilation of every handler that
        // follows. The next start of the server wipes the directory instead.

        Logger.LogInformation("Removed stored content of lambda {LambdaId}", lambdaId);

        return ValueTask.CompletedTask;
    }

    public string GetWorkspace(long lambdaId)
    {
        var directory = GetWorkspaceDirectory(lambdaId);

        Directory.CreateDirectory(directory);

        return directory;
    }

    public string GetAssemblyDirectory(long lambdaId) => Path.Combine(Options.AssemblyDirectory, lambdaId.ToString());

    public string GetAssetDirectory(long lambdaId) => Path.Combine(Options.AssetDirectory, lambdaId.ToString());

    private string GetCodeDirectory(long lambdaId) => Path.Combine(Options.CodeDirectory, lambdaId.ToString());

    private string GetWorkspaceDirectory(long lambdaId) => Path.Combine(Options.WorkspaceDirectory, lambdaId.ToString());

    private string GetFile(long lambdaId, int version) => Path.Combine(GetCodeDirectory(lambdaId), $"v{version}.cs");

    private static void Remove(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
        catch (IOException)
        {
            // content that is still in use cannot be removed on every
            // platform - the next start cleans it up
        }
    }

    #endregion

}
