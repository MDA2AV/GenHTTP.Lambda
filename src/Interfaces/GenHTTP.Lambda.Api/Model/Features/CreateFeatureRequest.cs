using GenHTTP.Api.Content.Documentation;

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
