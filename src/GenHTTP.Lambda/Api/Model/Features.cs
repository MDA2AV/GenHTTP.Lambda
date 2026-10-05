using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Starts a feature: a copy of a version's files, and of the lambda's data, to
/// work on without touching the lambda.
/// </summary>
/// <param name="Name">What it is, in a few words - "Leaderboard", "Dark mode"</param>
/// <param name="Specification">What the user wants from it and why; the version it is merged into keeps it</param>
/// <param name="Base">The version to branch off. Left out, the newest.</param>
public sealed record CreateFeatureRequest(string? Name, string? Specification = null, int? Base = null);

/// <summary>
/// Changes a feature. What is left out stays as it is.
/// </summary>
/// <param name="Name">What it is, in a few words</param>
/// <param name="Specification">What the user wants from it and why</param>
/// <param name="Change">What it changes, in a line - what the version it is merged into will say</param>
/// <param name="Base">
/// The version it is now based on. Set once the changes of that version are in the feature's files: only a feature
/// based on the newest version can be merged, and nothing checks that the changes really are in.
/// </param>
public sealed record UpdateFeatureRequest(string? Name = null, string? Specification = null, string? Change = null, int? Base = null);

/// <summary>
/// Every file of a feature, replacing the ones it has.
/// </summary>
/// <param name="Files">Every file, <c>lambda.cs</c> first</param>
/// <param name="Specification">What the user wants from it and why; left out, the one it has is kept</param>
/// <param name="Change">What it changes, in a line; left out, the one it has is kept</param>
/// <param name="Revision">
/// The save of the feature these files were made from; when given, the save is refused with 409 if the
/// feature was saved again since, instead of replacing that save without a trace
/// </param>
public sealed record FeatureFilesRequest(IReadOnlyList<LambdaFile>? Files, string? Specification = null, string? Change = null,
                                         int? Revision = null);

/// <summary>
/// Merges a feature into the lambda.
/// </summary>
/// <param name="Deploy">Whether to put the version it becomes online as well</param>
/// <param name="Specification">What the version keeps as what the user wanted; left out, the feature's</param>
/// <param name="Change">What the version says it changes; left out, the feature's</param>
public sealed record MergeFeatureRequest(bool? Deploy = null, string? Specification = null, string? Change = null);

/// <summary>
/// A feature: a change being worked on beside the lambda.
/// </summary>
/// <param name="Key">What it is addressed by in this API, and where its preview answers</param>
/// <param name="Branch">The branch it is in the lambda's git repository - given when it starts, and kept when it is renamed</param>
/// <param name="Base">The version it is based on</param>
/// <param name="Newest">The newest version of the lambda</param>
/// <param name="Mergeable">Whether it can be merged: it is based on the newest version</param>
/// <param name="Online">Whether its preview is online</param>
/// <param name="Current">Whether its preview is online and serves what the feature holds now, rather than an earlier save</param>
/// <param name="Previewed">When its preview was last put online, while it is</param>
/// <param name="PreviewPath">Where its preview answers on this server, whether it is online or not</param>
/// <param name="Revision">How many times its files were saved, counting the ones it began with - to notice a save made elsewhere</param>
public sealed record FeatureResponse(
    string Key,
    string Name,
    string Branch,
    string? Specification,
    string? Change,
    int Base,
    int? Newest,
    bool Mergeable,
    string? Origin,
    DateTime Created,
    DateTime Modified,
    bool Online,
    bool Current,
    DateTime? Previewed,
    string PreviewPath,
    int Revision
);

/// <summary>
/// A feature with the files it holds.
/// </summary>
/// <param name="Files">Every file, <c>lambda.cs</c> first</param>
public sealed record FeatureContentResponse(FeatureResponse Feature, IReadOnlyList<LambdaFile> Files);

/// <summary>
/// The result of putting the preview of a feature online.
/// </summary>
/// <param name="Feature">The feature afterwards</param>
/// <param name="Diagnostics">What the compiler said on the way</param>
public sealed record FeaturePreviewResponse(bool Success, FeatureResponse Feature, IReadOnlyList<CompilationDiagnostic> Diagnostics);

/// <summary>
/// A feature that was just saved.
/// </summary>
/// <param name="Preview">The outcome of putting its preview online, if that was asked for</param>
public sealed record FeatureSavedResponse(FeatureResponse Feature, FeaturePreviewResponse? Preview);

/// <summary>
/// The result of merging a feature.
/// </summary>
/// <param name="Merged">Whether its files became a version, and the feature is gone</param>
/// <param name="Version">The version it became</param>
/// <param name="Diagnostics">Why it was not merged, where its code does not compile</param>
/// <param name="Deployment">The outcome of putting that version online, if that was asked for</param>
public sealed record FeatureMergeResponse(bool Merged, VersionResponse? Version, IReadOnlyList<CompilationDiagnostic> Diagnostics,
                                          DeploymentOutcomeResponse? Deployment);
