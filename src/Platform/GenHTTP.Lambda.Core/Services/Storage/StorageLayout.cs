using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Where the files of a lambda are on disk, and how what was kept there
/// before the store is read.
/// </summary>
/// <remarks>
/// <code>
/// code/{lambda}/v{n}.manifest.json             a version
/// code/{lambda}/blobs/                         what the versions and features hold (see ContentStore)
/// features/{lambda}/{feature}/files.manifest.json    a feature as it is worked on
/// features/{lambda}/{feature}/preview.manifest.json  what its preview was deployed with
/// </code>
/// Before there was a store, each was a file holding every file whole -
/// <c>v{n}.cs</c>, <c>files.json</c>, <c>preview.json</c> - which is left
/// where it is: as the platform starts it is read once and moved into the
/// store with a manifest beside it (see <see cref="StoreMigration"/>), and
/// where it has none it is read as it is.
/// </remarks>
internal static class StorageLayout
{

    public static string CodeDirectory(LambdaOptions options, long lambdaId) => Path.Combine(options.CodeDirectory, lambdaId.ToString());

    public static string FeatureDirectory(LambdaOptions options, long lambdaId, long featureId)
        => Path.Combine(options.FeatureDirectory, lambdaId.ToString(), featureId.ToString());

    public static string Version(LambdaOptions options, long lambdaId, int version) => Path.Combine(CodeDirectory(options, lambdaId), $"v{version}.manifest.json");

    public static string Feature(LambdaOptions options, long lambdaId, long featureId) => Path.Combine(FeatureDirectory(options, lambdaId, featureId), "files.manifest.json");

    public static string Preview(LambdaOptions options, long lambdaId, long featureId) => Path.Combine(FeatureDirectory(options, lambdaId, featureId), "preview.manifest.json");

    #region Before the store

    public static string FormerVersion(LambdaOptions options, long lambdaId, int version) => Path.Combine(CodeDirectory(options, lambdaId), $"v{version}.cs");

    public static string FormerFeature(LambdaOptions options, long lambdaId, long featureId) => Path.Combine(FeatureDirectory(options, lambdaId, featureId), "files.json");

    public static string FormerPreview(LambdaOptions options, long lambdaId, long featureId) => Path.Combine(FeatureDirectory(options, lambdaId, featureId), "preview.json");

    /// <summary>
    /// The files a file of before holds, whole and as they were: in the
    /// layout of today, whichever it was written in.
    /// </summary>
    /// <returns>The files, or nothing where there is no such file</returns>
    public static IReadOnlyList<VersionFile>? ReadFormer(string file)
    {
        string stored;

        try
        {
            stored = File.ReadAllText(file);
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            return null;
        }

        return VersionFile.FromSent(LambdaSource.Parse(stored));
    }

    #endregion

}
