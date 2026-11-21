using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A version, without its files.")]
public sealed record VersionResponse(
    [Summary("Its number, counting from 1")]
    int Version,
    [Summary("When it was saved")]
    DateTime Created,
    [Summary("What the user wanted and why, where whoever saved it said")]
    string? Specification,
    [Summary("What it changed, where whoever saved it said")]
    string? Change,
    [Summary("How it was saved: `template` (what the lambda started as), `api`, `agent`, `git`, `admin` or `system`")]
    string? Origin
);
