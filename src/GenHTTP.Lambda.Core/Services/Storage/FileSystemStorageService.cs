using GenHTTP.Lambda.Configuration;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Stores the code of a lambda as <c>{data}/code/{id}/v{version}.cs</c>.
/// </summary>
/// <remarks>
/// A feature keeps everything of its own below <c>{data}/features/{id}/{feature}</c>:
/// <c>files.json</c> as it is being worked on, <c>preview.json</c> as its
/// preview was deployed and the resources its preview serves in
/// <c>resources</c> - so deleting a feature is deleting a folder, and deleting
/// a lambda takes its features along. Its copy of the data is kept by the
/// integrations, which may put it there as well.
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
