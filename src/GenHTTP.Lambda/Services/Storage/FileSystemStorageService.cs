using GenHTTP.Lambda.Configuration;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Stores the code of a lambda as <c>{data}/code/{id}/v{version}.cs</c> and gives
/// every lambda a private directory below <c>{data}/workspaces/{id}</c>.
/// </summary>
/// <remarks>
/// <c>{data}/code/{id}/online.cs</c> is there only while the version that is
/// online has been saved over since it was deployed, and holds what it was.
/// </remarks>
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
        => await WriteWholeAsync(GetFile(lambdaId, version), code, cancellation);

    public ValueTask<string?> ReadAsync(long lambdaId, int version, CancellationToken cancellation = default)
        => ReadIfThereAsync(GetFile(lambdaId, version), cancellation);

    public async ValueTask PreserveOnlineAsync(long lambdaId, int version, CancellationToken cancellation = default)
    {
        var code = await ReadAsync(lambdaId, version, cancellation);

        if (code != null)
        {
            await WriteWholeAsync(GetOnlineFile(lambdaId), code, cancellation);
        }
    }

    public async ValueTask<string?> ReadOnlineAsync(long lambdaId, int version, CancellationToken cancellation = default)
        => await ReadIfThereAsync(GetOnlineFile(lambdaId), cancellation) ?? await ReadAsync(lambdaId, version, cancellation);

    public ValueTask DropOnlineAsync(long lambdaId, CancellationToken cancellation = default)
    {
        var file = GetOnlineFile(lambdaId);

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Writes a file through a temporary one beside it, so it is replaced
    /// whole or not at all - and a reader that opened it before keeps reading
    /// what it opened.
    /// </summary>
    private static async ValueTask WriteWholeAsync(string file, string code, CancellationToken cancellation)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);

        var staging = $"{file}.saving";

        await File.WriteAllTextAsync(staging, code, cancellation);

        File.Move(staging, file, true);
    }

    private static async ValueTask<string?> ReadIfThereAsync(string file, CancellationToken cancellation)
    {
        if (!File.Exists(file))
        {
            return null;
        }

        try
        {
            return await File.ReadAllTextAsync(file, cancellation);
        }
        catch (FileNotFoundException)
        {
            // dropped between looking and reading
            return null;
        }
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

    private string GetOnlineFile(long lambdaId) => Path.Combine(GetCodeDirectory(lambdaId), "online.cs");

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
