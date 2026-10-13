using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Deployment.Model;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Stores the code of a lambda as <c>{data}/code/{id}/v{version}.cs</c> and gives
/// every lambda a private directory below <c>{data}/workspaces/{id}</c>.
/// </summary>
/// <remarks>
/// The resources of all versions and features of a lambda are kept below
/// <c>{data}/code/{id}/resources</c>, each once and named by what it holds
/// (<see cref="ResourceStore"/>), and the versions name them. What nothing
/// names any more is removed with a version or a feature.
///
/// A feature keeps everything of its own below <c>{data}/features/{id}/{feature}</c>:
/// <c>files.json</c> as it is being worked on, <c>preview.json</c> as its
/// preview was deployed, its copy of the workspace in <c>workspace</c> and
/// the resources its preview serves in <c>resources</c> - so deleting a
/// feature is deleting a folder, and deleting a lambda takes its features
/// along.
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

        RemoveAssets();
    }

    /// <summary>
    /// Removes where the resources were written while they were called
    /// assets: what a lambda serves is written out again as it goes online,
    /// into the folder of today's name, so nothing reads these any more.
    /// </summary>
    private void RemoveAssets()
    {
        Remove(Path.Combine(Options.DataDirectory, "assets"));

        foreach (var (lambda, feature) in ListFeatures())
        {
            Remove(Path.Combine(GetFeatureDirectory(lambda, feature), "assets"));
        }
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

    public IReadOnlyList<LambdaFile>? ReadFiles(long lambdaId, int version) => ReadFilesIfThere(lambdaId, GetFile(lambdaId, version));

    public IResourceStore Resources(long lambdaId) => new ResourceStore(GetResourceStoreDirectory(lambdaId));

    public void DeleteVersion(long lambdaId, int version)
    {
        var file = GetFile(lambdaId, version);

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        // what only this version named
        CollectResources(lambdaId);

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
        Remove(GetResourceDirectory(lambdaId));
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

    public string GetResourceDirectory(long lambdaId, long? featureId = null)
        => featureId is { } feature
         ? Path.Combine(GetFeatureDirectory(lambdaId, feature), "resources")
         : Path.Combine(Options.ResourceDirectory, lambdaId.ToString());

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

    public IReadOnlyList<LambdaFile>? ReadPreviewFiles(long lambdaId, long featureId)
        => ReadFilesIfThere(lambdaId, Path.Combine(GetFeatureDirectory(lambdaId, featureId), "preview.json"));

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

        // what only the feature named, its preview included
        CollectResources(lambdaId);
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

    /// <summary>
    /// Reads stored files straight from their file, the resources from the
    /// lambda's store - without the string an envelope that carries them is.
    /// </summary>
    private IReadOnlyList<LambdaFile>? ReadFilesIfThere(long lambdaId, string file)
    {
        try
        {
            using var stored = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

            return LambdaSource.Read(stored, Resources(lambdaId));
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            // removed between looking and reading, or never there
            return null;
        }
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

    #region Resources

    /// <summary>
    /// How long a resource nothing names is kept all the same.
    /// </summary>
    /// <remarks>
    /// The resources of a save are kept before the version or the feature
    /// that names them is written, and a version is removed under the lambda's
    /// turn while a feature is saved under its own - so what was kept a moment
    /// ago may be named a moment from now. An hour is longer than any save.
    /// </remarks>
    private static readonly TimeSpan ResourceGrace = TimeSpan.FromHours(1);

    /// <summary>
    /// Removes the resources none of the lambda's versions, features and
    /// previews names any more.
    /// </summary>
    /// <remarks>
    /// Anything that cannot be read leaves everything kept: a name that is
    /// missed would take a resource somebody still serves.
    /// </remarks>
    private void CollectResources(long lambdaId)
    {
        var store = GetResourceStoreDirectory(lambdaId);

        if (!Directory.Exists(store))
        {
            return;
        }

        try
        {
            var named = new HashSet<string>(StringComparer.Ordinal);

            foreach (var version in Directory.EnumerateFiles(GetCodeDirectory(lambdaId), "v*.cs"))
            {
                Name(version, named);
            }

            var features = Path.Combine(Options.FeatureDirectory, lambdaId.ToString());

            if (Directory.Exists(features))
            {
                foreach (var feature in Directory.EnumerateDirectories(features))
                {
                    Name(Path.Combine(feature, "files.json"), named);
                    Name(Path.Combine(feature, "preview.json"), named);
                }
            }

            var (files, bytes) = new ResourceStore(store).Collect(named, DateTime.UtcNow - ResourceGrace);

            if (files > 0)
            {
                Logger.LogInformation("Removed {Files} resources of lambda #{LambdaId} no version or feature names any more ({Bytes} bytes)", files, lambdaId, bytes);
            }
        }
        catch (Exception e) when (e is IOException or System.Text.Json.JsonException)
        {
            Logger.LogWarning(e, "Kept the resources of lambda #{LambdaId}: what names them could not be read", lambdaId);
        }
    }

    private static void Name(string file, HashSet<string> named)
    {
        try
        {
            using var stored = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

            named.UnionWith(LambdaSource.References(stored));
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            // not there, so it names nothing
        }
    }

    #endregion

    #region Helpers

    private string GetCodeDirectory(long lambdaId) => Path.Combine(Options.CodeDirectory, lambdaId.ToString());

    private string GetResourceStoreDirectory(long lambdaId) => Path.Combine(GetCodeDirectory(lambdaId), "resources");

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
