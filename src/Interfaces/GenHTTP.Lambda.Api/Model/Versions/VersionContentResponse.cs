using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A version with its files.")]
public sealed record VersionContentResponse(
    [Summary("Its number")]
    int Version,
    [Summary("When it was saved")]
    DateTime Created,
    [Summary("What the user wanted and why, where whoever saved it said")]
    string? Specification,
    [Summary("What it changed, where whoever saved it said")]
    string? Change,
    [Summary("How it was saved: `template`, `api`, `agent`, `git`, `admin` or `system`")]
    string? Origin,
    [Summary("Its files, `lambda.cs` first - all of them, or those of the folder asked for")]
    IReadOnlyList<LambdaFile> Files
);
