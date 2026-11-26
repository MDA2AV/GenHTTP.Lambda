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
    IReadOnlyList<LambdaFile> Files,
    [Summary("Who wrote it, where the platform knows it: `build-agent`, the installation's own agent, on a job it was given; absent for anybody else's work, an agent of somebody's own included")]
    string? Writer = null,
    [Summary("The value of the agent's option `model` it was written on (`opus`), where it had one; absent unless `writer` is there")]
    string? Model = null,
    [Summary("The value of the agent's option `effort` it was written at (`high`), where it had one; absent unless `writer` is there")]
    string? Effort = null,
    [Summary("The value of every option of the agent's it was written on, by option, as `build.offer.options` of `GET /system` names them; absent unless `writer` is there")]
    IReadOnlyDictionary<string, string>? Options = null
);
