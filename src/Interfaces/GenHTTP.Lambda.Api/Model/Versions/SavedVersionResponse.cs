using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A version that was just saved.")]
public sealed record SavedVersionResponse(
    [Summary("Its number")]
    int Version,
    [Summary("When it was saved")]
    DateTime Created,
    [Summary("What the user wanted and why, as it was kept")]
    string? Specification,
    [Summary("What it changes, as it was kept")]
    string? Change,
    [Summary("How it was saved")]
    string? Origin,
    [Summary("What came of putting it online, where that was asked for (`?deploy=true`); absent otherwise")]
    DeploymentOutcomeResponse? Deployment
);
