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
    [Summary("The model the build agent wrote it on, by its name in `build.offer.models` of `GET /system` (`opus`); absent unless `writer` is there")]
    string? Model = null,
    [Summary("The effort the build agent wrote it at: `low`, `medium`, `high`, `xhigh` or `max`; absent unless `writer` is there")]
    string? Effort = null
);
