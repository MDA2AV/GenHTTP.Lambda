using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Keeps the files of the lambdas: their versions, their features and what
/// the previews of those were deployed with - on the file system, never in
/// the database.
/// </summary>
/// <remarks>
/// A version is a manifest naming what it holds, and what it holds is kept
/// once per lambda however many versions hold it (see <see cref="ContentStore"/>),
/// so a file is handed on as a <see cref="VersionFile"/> and read only by
/// whoever needs it. Content is kept before the turn a change is written in -
/// <see cref="KeepAsync"/> - so writing a version or a feature in its turn is
/// writing its manifest.
/// </remarks>
public interface IStorageService
{

    #region Versions

    /// <summary>
    /// The files of a version, or nothing where it has none.
    /// </summary>
    IReadOnlyList<VersionFile>? Read(long lambdaId, int version);

    /// <summary>
    /// Stores the files as a version: what of their content the lambda does
    /// not keep yet is kept, and the version names it.
    /// </summary>
    ValueTask WriteAsync(long lambdaId, int version, IReadOnlyList<VersionFile> files);

    /// <summary>
    /// Removes a single version of a lambda. Its content stays as long as
    /// another version, a feature or a preview holds it.
    /// </summary>
    void DeleteVersion(long lambdaId, int version);

    /// <summary>
    /// Removes everything stored for the given lambda: its versions, its
    /// features, what it serves and the commits it was read as with git. Its
    /// data is its integrations' to remove.
    /// </summary>
    ValueTask DeleteAsync(long lambdaId);

    /// <summary>
    /// The directory the assemblies generated for a lambda are written to.
    /// </summary>
    string GetAssemblyDirectory(long lambdaId);

    /// <summary>
    /// The directory the resources shipped with a lambda - or with the preview
    /// of one of its features - are written to.
    /// </summary>
    /// <remarks>
    /// Rewritten from what is being deployed every time something goes online,
    /// so it holds what that shipped and nothing that went before. The lambda
    /// may read it and not write it - what it writes goes in the workspace,
    /// which outlives a deployment.
    /// </remarks>
    /// <param name="featureId">The feature whose preview is meant, or nothing for the lambda itself</param>
    string GetResourceDirectory(long lambdaId, long? featureId = null);

    #endregion

    #region Content

    /// <summary>
    /// Keeps the content of the files that the lambda does not keep yet, and
    /// says where each is kept now.
    /// </summary>
    /// <remarks>
    /// Called before the turn a change is written in, so a large file is
    /// written while nobody waits for the turn: what was kept and is not
    /// named by a version or a feature soon after is collected.
    /// </remarks>
    ValueTask<IReadOnlyList<VersionFile>> KeepAsync(long lambdaId, IReadOnlyList<VersionFile> files, CancellationToken cancellation = default);

    /// <summary>
    /// The files the lambda keeps whose git blob ids are among those given,
    /// by their id - without a name, binary where they are not text.
    /// </summary>
    /// <remarks>
    /// For a push, whose files the lambda keeps already are then not read.
    /// </remarks>
    ValueTask<IReadOnlyDictionary<string, VersionFile>> FindAsync(long lambdaId, IReadOnlyCollection<string> gitIds);

    /// <summary>
    /// The git blob id of each of the files, by the SHA-256 of what it holds,
    /// for laying them out as a commit without reading them.
    /// </summary>
    ValueTask<IReadOnlyDictionary<string, string>> GitIdsAsync(long lambdaId, IReadOnlyList<VersionFile> files);

    /// <summary>
    /// Removes the content of the lambdas whose versions, features or
    /// previews changed that nothing names any more.
    /// </summary>
    /// <returns>How many files were removed</returns>
    ValueTask<int> CollectAsync(CancellationToken cancellation = default);

    #endregion

    #region Features

    /// <summary>
    /// Stores the files of a feature as they are now, replacing what they were.
    /// </summary>
    /// <remarks>
    /// Written whole or not at all: a feature is saved over again and again
    /// while it is worked on, and a save cut short must not leave half of one.
    /// </remarks>
    ValueTask WriteFeatureAsync(long lambdaId, long featureId, IReadOnlyList<VersionFile> files);

    /// <summary>
    /// Reads the files of a feature, if it has any.
    /// </summary>
    IReadOnlyList<VersionFile>? ReadFeature(long lambdaId, long featureId);

    /// <summary>
    /// Keeps what the preview of a feature was deployed with, so it serves that
    /// until it is deployed again - a restart included - however much the
    /// feature is changed in the meantime.
    /// </summary>
    ValueTask WritePreviewAsync(long lambdaId, long featureId, IReadOnlyList<VersionFile> files);

    /// <summary>
    /// Reads what the preview of a feature was deployed with, if it was.
    /// </summary>
    IReadOnlyList<VersionFile>? ReadPreview(long lambdaId, long featureId);

    /// <summary>
    /// Removes everything stored for a feature: its files and what its
    /// preview serves. Its copy of the data is its integrations' to remove.
    /// </summary>
    void DeleteFeature(long lambdaId, long featureId);

    /// <summary>
    /// The features there are files of, by lambda and feature.
    /// </summary>
    IEnumerable<(long LambdaId, long FeatureId)> ListFeatures();

    #endregion

}
