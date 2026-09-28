namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Persists the source code of the lambdas. Backed by the file system for now,
/// a git repository would be a natural next step.
/// </summary>
public interface IStorageService
{

    /// <summary>
    /// Stores the code of a newly created version.
    /// </summary>
    ValueTask WriteAsync(long lambdaId, int version, string code, CancellationToken cancellation = default);

    /// <summary>
    /// Reads the code of the given version, if it exists.
    /// </summary>
    ValueTask<string?> ReadAsync(long lambdaId, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a single version of a lambda.
    /// </summary>
    ValueTask DeleteVersionAsync(long lambdaId, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Removes everything stored for the given lambda, including its workspace
    /// and its features.
    /// </summary>
    ValueTask DeleteAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// The directory a deployed lambda - or the preview of one of its features -
    /// may read and write files in. Created on demand.
    /// </summary>
    /// <param name="featureId">The feature whose copy of the workspace is meant, or nothing for the lambda's own</param>
    string GetWorkspace(long lambdaId, long? featureId = null);

    /// <summary>
    /// The directory the assemblies generated for a lambda are written to.
    /// </summary>
    string GetAssemblyDirectory(long lambdaId);

    /// <summary>
    /// The directory the assets shipped with a lambda - or with the preview of
    /// one of its features - are written to.
    /// </summary>
    /// <remarks>
    /// Rewritten from what is being deployed every time something goes online,
    /// so it holds what that shipped and nothing that went before. The lambda
    /// may read it and not write it - what it writes goes in the workspace,
    /// which outlives a deployment.
    /// </remarks>
    /// <param name="featureId">The feature whose preview is meant, or nothing for the lambda itself</param>
    string GetAssetDirectory(long lambdaId, long? featureId = null);

    #region Features

    /// <summary>
    /// Stores the files of a feature as they are now, replacing what they were.
    /// </summary>
    /// <remarks>
    /// Written whole or not at all: a feature is saved over again and again
    /// while it is worked on, and a save cut short must not leave half of one.
    /// </remarks>
    ValueTask WriteFeatureAsync(long lambdaId, long featureId, string code, CancellationToken cancellation = default);

    /// <summary>
    /// Reads the files of a feature, if it has any.
    /// </summary>
    ValueTask<string?> ReadFeatureAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Keeps what the preview of a feature was deployed with, so it serves that
    /// until it is deployed again - a restart included - however much the
    /// feature is changed in the meantime.
    /// </summary>
    ValueTask WritePreviewAsync(long lambdaId, long featureId, string code, CancellationToken cancellation = default);

    /// <summary>
    /// Reads what the preview of a feature was deployed with, if it was.
    /// </summary>
    ValueTask<string?> ReadPreviewAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Replaces the feature's copy of the workspace with a fresh copy of the
    /// lambda's own.
    /// </summary>
    ValueTask CopyWorkspaceAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Removes everything stored for a feature: its files, what its preview
    /// serves and its copy of the data.
    /// </summary>
    ValueTask DeleteFeatureAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// The features there are files of, by lambda and feature.
    /// </summary>
    IEnumerable<(long LambdaId, long FeatureId)> ListFeatures();

    #endregion

}
