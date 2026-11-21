using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

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
