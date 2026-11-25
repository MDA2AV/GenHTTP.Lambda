using System.Collections.Concurrent;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Deployment.Model;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Keeps the files of the lambdas below the data directory: a manifest per
/// version as <c>{data}/code/{id}/v{version}.manifest.json</c>, and what they
/// hold once per lambda in its store beside them (see <see cref="ContentStore"/>).
/// </summary>
/// <remarks>
/// A feature keeps everything of its own below <c>{data}/features/{id}/{feature}</c>:
/// <c>files.manifest.json</c> as it is being worked on, <c>preview.manifest.json</c>
/// as its preview was deployed and the resources its preview serves in
/// <c>resources</c> - so deleting a feature is deleting a folder, and deleting
/// a lambda takes its features along. What they hold is in the lambda's store
/// with the versions', so starting a feature, deploying its preview and
/// merging it write manifests and copy no content. Its copy of the data is
/// kept by the integrations, which may put it there as well.
///
/// Which lambdas may have content nothing names any more - a version pruned,
/// a feature saved over or gone, a save refused after its content was kept -
/// is remembered, and their stores are collected by the maintenance; after a
/// start, every lambda's once.
/// </remarks>
public sealed class FileSystemStorageService : IStorageService
{

    /// <summary>
    /// The lambdas whose stores may hold content nothing names, to be collected.
    /// </summary>
    private readonly ConcurrentDictionary<long, byte> _changed = [];

    /// <summary>
    /// Whether every lambda's store is collected next, as it is once after a
    /// start: what was changed before is not remembered across it.
    /// </summary>
    private volatile bool _everything = true;

    #region Get-/Setters

    private LambdaOptions Options { get; }

    private ContentStore Store { get; }

    private ILogger Logger { get; }

    #endregion

    #region Initialization

    public FileSystemStorageService(LambdaOptions options, ContentStore store, ILogger<FileSystemStorageService> logger)
    {
        Options = options;
        Store = store;
        Logger = logger;

        Directory.CreateDirectory(options.CodeDirectory);

        // what was on its way into a store when the server stopped, which no
        // manifest names
        Remove(options.StagingDirectory);

        Directory.CreateDirectory(options.StagingDirectory);

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
            Remove(Path.Combine(StorageLayout.FeatureDirectory(Options, lambda, feature), "assets"));
        }
    }

    #endregion

    #region Versions

    /// <remarks>
    /// One kept before the store and not moved into it - which the start of
    /// the platform does, so this is one that failed to move - is read whole,
    /// as it was.
    /// </remarks>
    public IReadOnlyList<VersionFile>? Read(long lambdaId, int version)
        => Store.Read(lambdaId, StorageLayout.Version(Options, lambdaId, version))
        ?? StorageLayout.ReadFormer(StorageLayout.FormerVersion(Options, lambdaId, version));

    public async ValueTask WriteAsync(long lambdaId, int version, IReadOnlyList<VersionFile> files)
    {
        var kept = await KeepAsync(lambdaId, files);

        await Store.WriteAsync(lambdaId, StorageLayout.Version(Options, lambdaId, version), kept);
    }

    public void DeleteVersion(long lambdaId, int version)
    {
        Delete(StorageLayout.Version(Options, lambdaId, version));
        Delete(StorageLayout.FormerVersion(Options, lambdaId, version));

        Changed(lambdaId);

        // and the project its published source was packed into, which no
        // visitor can ask for any more
        var packed = Path.Combine(Options.SourceDirectory, lambdaId.ToString());

        if (Directory.Exists(packed))
        {
            foreach (var project in Directory.EnumerateFiles(packed, $"v{version}-*"))
            {
                Delete(project);
            }
        }
    }

    public async ValueTask DeleteAsync(long lambdaId)
    {
        Remove(StorageLayout.CodeDirectory(Options, lambdaId));
        Remove(GetResourceDirectory(lambdaId));
        Remove(Path.Combine(Options.FeatureDirectory, lambdaId.ToString()));
        // what its published source was packed into, if it was published
        Remove(Path.Combine(Options.SourceDirectory, lambdaId.ToString()));
        // and the commits it was read as with git, if anybody did
        Remove(Path.Combine(Options.GitDirectory, lambdaId.ToString()));

        await Store.ForgetAsync(lambdaId);

        _changed.TryRemove(lambdaId, out _);

        // the generated assembly stays: it is the process's (see CompiledAssemblies)

        Logger.LogInformation("Removed storage of lambda #{LambdaId}", lambdaId);
    }

    public string GetResourceDirectory(long lambdaId, long? featureId = null)
        => featureId is { } feature
         ? Path.Combine(StorageLayout.FeatureDirectory(Options, lambdaId, feature), "resources")
         : Path.Combine(Options.ResourceDirectory, lambdaId.ToString());

    #endregion

    #region Content

    public async ValueTask<IReadOnlyList<VersionFile>> KeepAsync(long lambdaId, IReadOnlyList<VersionFile> files, CancellationToken cancellation = default)
    {
        var (kept, added) = await Store.KeepAsync(lambdaId, files, cancellation);

        // named by the version or feature it was kept for in a moment - or by
        // nothing, where that save is refused
        if (added)
        {
            Changed(lambdaId);
        }

        return kept;
    }

    public ValueTask ReadingAsync(long lambdaId, IReadOnlyList<VersionFile> files) => Store.ReadingAsync(lambdaId, files);

    public ValueTask<IReadOnlyDictionary<string, VersionFile>> FindAsync(long lambdaId, IReadOnlyCollection<string> gitIds) => Store.FindAsync(lambdaId, gitIds);

    public ValueTask<IReadOnlyDictionary<string, string>> GitIdsAsync(long lambdaId, IReadOnlyList<VersionFile> files) => Store.GitIdsAsync(lambdaId, files);

    public async ValueTask<int> CollectAsync(CancellationToken cancellation = default)
    {
        var lambdas = _everything ? Stores() : [.. _changed.Keys];

        _everything = false;

        var removed = 0;

        foreach (var lambda in lambdas)
        {
            if (cancellation.IsCancellationRequested)
            {
                // the rest are collected next time
                Changed(lambda);
                continue;
            }

            _changed.TryRemove(lambda, out _);

            try
            {
                var (count, lingering) = await Store.CollectAsync(lambda, () => Named(lambda));

                removed += count;

                // what nothing names was kept a moment ago: the next round decides
                if (lingering)
                {
                    Changed(lambda);
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to collect the store of lambda #{LambdaId}", lambda);

                // and tried again next time
                Changed(lambda);
            }
        }

        if (removed > 0)
        {
            Logger.LogInformation("Removed {Count} files no version or feature holds any more", removed);
        }

        return removed;
    }

    /// <summary>
    /// The content every manifest of a lambda names: its versions', its features' and their previews'.
    /// </summary>
    /// <remarks>
    /// A manifest that cannot be read stops the collection rather than
    /// letting what it names go.
    /// </remarks>
    private HashSet<string> Named(long lambdaId)
    {
        var named = new HashSet<string>(StringComparer.Ordinal);

        var manifests = new List<string>();

        var code = StorageLayout.CodeDirectory(Options, lambdaId);

        if (Directory.Exists(code))
        {
            manifests.AddRange(Directory.EnumerateFiles(code, "*.manifest.json"));
        }

        var features = Path.Combine(Options.FeatureDirectory, lambdaId.ToString());

        if (Directory.Exists(features))
        {
            foreach (var feature in Directory.EnumerateDirectories(features))
            {
                try
                {
                    manifests.AddRange(Directory.EnumerateFiles(feature, "*.manifest.json"));
                }
                catch (DirectoryNotFoundException)
                {
                    // deleted meanwhile, with what only it named
                }
            }
        }

        foreach (var manifest in manifests)
        {
            string text;

            try
            {
                text = File.ReadAllText(manifest);
            }
            catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
            {
                // deleted meanwhile, with what only it named
                continue;
            }

            foreach (var entry in Manifest.Read(text))
            {
                named.Add(entry.Blob);
            }
        }

        return named;
    }

    /// <summary>
    /// The lambdas that have a store.
    /// </summary>
    private List<long> Stores()
    {
        if (!Directory.Exists(Options.CodeDirectory))
        {
            return [];
        }

        return [.. Directory.EnumerateDirectories(Options.CodeDirectory)
                            .Select(d => long.TryParse(Path.GetFileName(d), out var id) ? id : (long?)null)
                            .OfType<long>()
                            .Where(id => Directory.Exists(Store.Root(id)))];
    }

    private void Changed(long lambdaId) => _changed.TryAdd(lambdaId, 0);

    #endregion

    #region Features

    public async ValueTask WriteFeatureAsync(long lambdaId, long featureId, IReadOnlyList<VersionFile> files)
    {
        var kept = await KeepAsync(lambdaId, files);

        await Store.WriteAsync(lambdaId, StorageLayout.Feature(Options, lambdaId, featureId), kept);

        // what it held before is saved over, and may have been all that named some content
        Delete(StorageLayout.FormerFeature(Options, lambdaId, featureId));

        Changed(lambdaId);
    }

    public IReadOnlyList<VersionFile>? ReadFeature(long lambdaId, long featureId)
        => Store.Read(lambdaId, StorageLayout.Feature(Options, lambdaId, featureId))
        ?? StorageLayout.ReadFormer(StorageLayout.FormerFeature(Options, lambdaId, featureId));

    public async ValueTask WritePreviewAsync(long lambdaId, long featureId, IReadOnlyList<VersionFile> files)
    {
        var kept = await KeepAsync(lambdaId, files);

        await Store.WriteAsync(lambdaId, StorageLayout.Preview(Options, lambdaId, featureId), kept);

        Delete(StorageLayout.FormerPreview(Options, lambdaId, featureId));

        Changed(lambdaId);
    }

    public IReadOnlyList<VersionFile>? ReadPreview(long lambdaId, long featureId)
        => Store.Read(lambdaId, StorageLayout.Preview(Options, lambdaId, featureId))
        ?? StorageLayout.ReadFormer(StorageLayout.FormerPreview(Options, lambdaId, featureId));

    public void DeleteFeature(long lambdaId, long featureId)
    {
        Remove(StorageLayout.FeatureDirectory(Options, lambdaId, featureId));

        Changed(lambdaId);
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

    #endregion

    #region Helpers

    private static void Delete(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch (IOException)
        {
            // being read right now: it is let go of in time
        }
    }

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
