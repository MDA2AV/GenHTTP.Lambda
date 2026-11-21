using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

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
