using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Starts a feature: a copy of a version's files, and of the lambda's data, to work on without touching the lambda.")]
public sealed record CreateFeatureRequest(
    [Summary("What it is, in a few words - \"Leaderboard\", \"Dark mode\". One line, 80 characters at most.")]
    string? Name,
    [Summary("What the user wants from it and why. The version it is merged into keeps it.")]
    string? Specification = null,
    [Summary("The number of the version to start from; left out, the newest")]
    int? Base = null
);

[Summary("Changes a feature. What is left out stays as it is.")]
public sealed record UpdateFeatureRequest(
    [Summary("What it is, in a few words")]
    string? Name = null,
    [Summary("What the user wants from it and why; empty clears it")]
    string? Specification = null,
    [Summary("What it changes, in one line - what the version it is merged into will say; empty clears it")]
    string? Change = null,
    [Summary("The version it is based on from now on. Move it only once the changes of that version are in the feature's files: only a feature based on the newest version can be merged, and nothing checks that the changes really are in.")]
    int? Base = null
);

[Summary("Every file of a feature, replacing the ones it has.")]
public sealed record FeatureFilesRequest(
    [Summary("Every file, `lambda.cs` first")]
    IReadOnlyList<LambdaFile>? Files,
    [Summary("What the user wants from it and why; left out, the one it has is kept")]
    string? Specification = null,
    [Summary("What it changes, in one line; left out, the one it has is kept")]
    string? Change = null,
    [Summary("The `revision` of the feature these files were made from. Given, the save is refused with 409 if the feature was saved again since, instead of replacing that save without a trace.")]
    int? Revision = null
);

[Summary("How to merge a feature into the lambda.")]
public sealed record MergeFeatureRequest(
    [Summary("Whether to put the version it becomes online as well")]
    bool? Deploy = null,
    [Summary("What the version keeps as what the user wanted; left out, the feature's")]
    string? Specification = null,
    [Summary("What the version says it changes; left out, the feature's, or that it merges the feature")]
    string? Change = null
);

[Summary("A feature: a change being worked on beside the lambda.")]
public sealed record FeatureResponse(
    [Summary("What every call about it takes, and what its preview is reached by")]
    string Key,
    [Summary("What it is, in a few words")]
    string Name,
    [Summary("The branch it is in the lambda's git repository - given when it starts, and kept when it is renamed")]
    string Branch,
    [Summary("What the user wants from it and why")]
    string? Specification,
    [Summary("What it changes, in one line")]
    string? Change,
    [Summary("The version it is based on")]
    int Base,
    [Summary("The newest version of the lambda")]
    int? Newest,
    [Summary("Whether it can be merged: it is based on the newest version")]
    bool Mergeable,
    [Summary("How it was started: `api`, `agent` or `git`")]
    string? Origin,
    [Summary("When it was started")]
    DateTime Created,
    [Summary("When it was last saved or changed")]
    DateTime Modified,
    [Summary("Whether its preview is online")]
    bool Online,
    [Summary("Whether its preview is online and serves what the feature holds now, rather than an earlier save")]
    bool Current,
    [Summary("When its preview was last put online, while it is")]
    DateTime? Previewed,
    [Summary("Where its preview answers on this site, whether it is online or not: `/features/{key}/`")]
    string PreviewPath,
    [Summary("How many times its files were saved, counting the ones it began with. Passed back with a save, it keeps that save from replacing one made elsewhere.")]
    int Revision
);

[Summary("A feature with its files.")]
public sealed record FeatureContentResponse(
    [Summary("The feature")]
    FeatureResponse Feature,
    [Summary("Its files, `lambda.cs` first - all of them, or those of the folder asked for")]
    IReadOnlyList<LambdaFile> Files
);

[Summary("What came of putting the preview of a feature online.")]
public sealed record FeaturePreviewResponse(
    [Summary("Whether it compiled and the preview is online with it")]
    bool Success,
    [Summary("The feature afterwards")]
    FeatureResponse Feature,
    [Summary("What the compiler said: the errors that kept it offline, or the warnings of a preview that went online")]
    IReadOnlyList<CompilationDiagnostic> Diagnostics
);

[Summary("A feature that was just saved.")]
public sealed record FeatureSavedResponse(
    [Summary("The feature afterwards")]
    FeatureResponse Feature,
    [Summary("What came of putting its preview online, where that was asked for (`?deploy=true`); absent otherwise")]
    FeaturePreviewResponse? Preview
);

[Summary("What came of merging a feature.")]
public sealed record FeatureMergeResponse(
    [Summary("Whether its files became a version, and the feature is gone")]
    bool Merged,
    [Summary("The version it became")]
    VersionResponse? Version,
    [Summary("Why it was not merged, where its code does not compile")]
    IReadOnlyList<CompilationDiagnostic> Diagnostics,
    [Summary("What came of putting that version online, where that was asked for")]
    DeploymentOutcomeResponse? Deployment
);
