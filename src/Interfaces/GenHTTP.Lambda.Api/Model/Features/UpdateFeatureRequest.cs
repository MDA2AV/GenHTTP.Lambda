using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

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
