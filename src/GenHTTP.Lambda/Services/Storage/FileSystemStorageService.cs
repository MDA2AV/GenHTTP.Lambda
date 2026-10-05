using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Stores the code of a lambda as <c>{data}/code/{id}/v{version}.cs</c> and gives
/// every lambda a private directory below <c>{data}/workspaces/{id}</c>.
/// </summary>
/// <remarks>
/// A feature keeps everything of its own below <c>{data}/features/{id}/{feature}</c>:
/// <c>files.json</c> as it is being worked on, <c>preview.json</c> as its
/// preview was deployed, its copy of the workspace in <c>workspace</c> and
/// what its preview serves in <c>assets</c> - so deleting a feature is
/// deleting a folder, and deleting a lambda takes its features along.
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

    #region Versions

    public void Write(long lambdaId, int version, string code)
    {
        var directory = GetCodeDirectory(lambdaId);

        Directory.CreateDirectory(directory);

        // in one write, rather than an asynchronous step per buffer: a version
        // is held to the allowance of its tier, and saving one is rare
        File.WriteAllText(GetFile(lambdaId, version), code);
    }

    public string? Read(long lambdaId, int version)
    {
        var file = GetFile(lambdaId, version);

        if (!File.Exists(file))
        {
            return null;
        }

        return File.ReadAllText(file);
    }

    public void DeleteVersion(long lambdaId, int version)
    {
        var file = GetFile(lambdaId, version);

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        // and the project its published source was packed into, which no
        // visitor can ask for any more
        var packed = Path.Combine(Options.SourceDirectory, lambdaId.ToString());

        if (Directory.Exists(packed))
        {
            foreach (var project in Directory.EnumerateFiles(packed, $"v{version}-*"))
            {
                try
                {
                    File.Delete(project);
                }
                catch (IOException)
                {
                    // being read right now: the cache lets go of it in time
                }
            }
        }
    }

    public void Delete(long lambdaId)
    {
        Remove(GetCodeDirectory(lambdaId));
        Remove(GetWorkspaceDirectory(lambdaId));
        Remove(Path.Combine(Options.DatabaseDirectory, lambdaId.ToString()));
        Remove(GetAssetDirectory(lambdaId));
        Remove(Path.Combine(Options.FeatureDirectory, lambdaId.ToString()));
        // what its published source was packed into, if it was published
        Remove(Path.Combine(Options.SourceDirectory, lambdaId.ToString()));
        // and the commits it was read as with git, if anybody did
        Remove(Path.Combine(Options.GitDirectory, lambdaId.ToString()));

        // the generated assembly stays: it cannot be unloaded and GenHTTP builds
        // its invocation code from the files behind the loaded assemblies, so
        // removing one here would break the compilation of every handler that
        // follows. The next start of the server wipes the directory instead.

        Logger.LogInformation("Removed storage of lambda #{LambdaId}", lambdaId);
    }

    public string GetWorkspace(long lambdaId, long? featureId = null)
    {
        var directory = featureId is { } feature
            ? Path.Combine(GetFeatureDirectory(lambdaId, feature), "workspace")
            : GetWorkspaceDirectory(lambdaId);

        Directory.CreateDirectory(directory);

        return directory;
    }

    public string GetDatabase(long lambdaId, long? featureId = null)
    {
        var directory = featureId is { } feature
            ? GetFeatureDirectory(lambdaId, feature)
            : Path.Combine(Options.DatabaseDirectory, lambdaId.ToString());

        Directory.CreateDirectory(directory);

        return Path.Combine(directory, "database.db");
    }

    public string GetAssemblyDirectory(long lambdaId) => Path.Combine(Options.AssemblyDirectory, lambdaId.ToString());

    public string GetAssetDirectory(long lambdaId, long? featureId = null)
        => featureId is { } feature
         ? Path.Combine(GetFeatureDirectory(lambdaId, feature), "assets")
         : Path.Combine(Options.AssetDirectory, lambdaId.ToString());

    #endregion

    #region Features

    public void WriteFeature(long lambdaId, long featureId, string code)
        => WriteWhole(Path.Combine(GetFeatureDirectory(lambdaId, featureId), "files.json"), code);

    public string? ReadFeature(long lambdaId, long featureId)
        => ReadIfThere(Path.Combine(GetFeatureDirectory(lambdaId, featureId), "files.json"));

    public void WritePreview(long lambdaId, long featureId, string code)
        => WriteWhole(Path.Combine(GetFeatureDirectory(lambdaId, featureId), "preview.json"), code);

    public string? ReadPreview(long lambdaId, long featureId)
        => ReadIfThere(Path.Combine(GetFeatureDirectory(lambdaId, featureId), "preview.json"));

    /// <remarks>
    /// Copied beside the copy it replaces and swapped in once complete, so a
    /// copy that fails halfway leaves the one there was. On a thread of its
    /// own, since a workspace can be large and this is asked for by a request.
    /// </remarks>
    public async ValueTask CopyWorkspaceAsync(long lambdaId, long featureId, CancellationToken cancellation = default)
    {
        var target = Path.Combine(GetFeatureDirectory(lambdaId, featureId), "workspace");

        var staging = $"{target}.copying";

        var source = GetWorkspaceDirectory(lambdaId);

        // a whole workspace, gigabytes in the premium tier: copied away from the
        // reactor (see Offload)
        await Offload.Run(() =>
        {
            Remove(staging);

            Directory.CreateDirectory(staging);

            try
            {
                if (Directory.Exists(source))
                {
                    Copy(new DirectoryInfo(source), staging, cancellation);
                }
            }
            catch
            {
                Remove(staging);
                throw;
            }

            Remove(target);

            Directory.Move(staging, target);
        }, cancellation);
    }

    public void DeleteFeature(long lambdaId, long featureId)
    {
        Remove(GetFeatureDirectory(lambdaId, featureId));
    }

    public IEnumerable<(long LambdaId, long FeatureId)> ListFeatures()
    {
        if (!Directory.Exists(Options.FeatureDirectory))
        {
            yield break;
        }

        foreach (var lambda in Directory.EnumerateDirectories(Options.FeatureDirectory))
        {
            if (!long.TryParse(Path.GetFileName(lambda), out var lambdaId))
            {
                continue;
            }

            foreach (var feature in Directory.EnumerateDirectories(lambda))
            {
                if (long.TryParse(Path.GetFileName(feature), out var featureId))
                {
                    yield return (lambdaId, featureId);
                }
            }
        }
    }

    /// <summary>
    /// Copies a folder with everything in it, empty folders included - an
    /// empty folder is a real thing in a workspace, which code may rely on.
    /// </summary>
    private static void Copy(DirectoryInfo source, string target, CancellationToken cancellation)
    {
        foreach (var file in source.EnumerateFiles())
        {
            cancellation.ThrowIfCancellationRequested();

            // an upload still arriving is not part of what is there yet
            if (file.Name.EndsWith(".uploading", StringComparison.Ordinal))
            {
                continue;
            }

            file.CopyTo(Path.Combine(target, file.Name), true);
        }

        foreach (var folder in source.EnumerateDirectories())
        {
            var inner = Path.Combine(target, folder.Name);

            Directory.CreateDirectory(inner);

            Copy(folder, inner, cancellation);
        }
    }

    /// <summary>
    /// Writes a file through a temporary one beside it, so it is replaced
    /// whole or not at all - and a reader that opened it before keeps reading
    /// what it opened.
    /// </summary>
    private static void WriteWhole(string file, string code)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);

        var staging = $"{file}.saving";

        File.WriteAllText(staging, code);

        File.Move(staging, file, true);
    }

    private static string? ReadIfThere(string file)
    {
        try
        {
            return File.Exists(file) ? File.ReadAllText(file) : null;
        }
        catch (FileNotFoundException)
        {
            // removed between looking and reading
            return null;
        }
    }

    #endregion

    #region Helpers

    private string GetCodeDirectory(long lambdaId) => Path.Combine(Options.CodeDirectory, lambdaId.ToString());

    private string GetWorkspaceDirectory(long lambdaId) => Path.Combine(Options.WorkspaceDirectory, lambdaId.ToString());

    private string GetFeatureDirectory(long lambdaId, long featureId)
        => Path.Combine(Options.FeatureDirectory, lambdaId.ToString(), featureId.ToString());

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
