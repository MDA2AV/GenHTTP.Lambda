using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("How to merge a feature into the lambda.")]
public sealed record MergeFeatureRequest(
    [Summary("Whether to put the version it becomes online as well")]
    bool? Deploy = null,
    [Summary("What the version keeps as what the user wanted; left out, the feature's")]
    string? Specification = null,
    [Summary("What the version says it changes; left out, the feature's, or that it merges the feature")]
    string? Change = null
);
