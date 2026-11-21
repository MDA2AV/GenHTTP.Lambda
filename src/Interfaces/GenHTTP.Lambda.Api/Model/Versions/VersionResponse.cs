using GenHTTP.Api.Content.Documentation;

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
    string? Origin,
    [Summary("Who wrote it, where the platform knows it: `build-agent`, the installation's own agent, on a job it was given; absent for anybody else's work, an agent of somebody's own included")]
    string? Writer = null,
    [Summary("The model the build agent wrote it on, by its name in `build.offer.models` of `GET /system` (`opus`); absent unless `writer` is there")]
    string? Model = null,
    [Summary("The effort the build agent wrote it at: `low`, `medium`, `high`, `xhigh` or `max`; absent unless `writer` is there")]
    string? Effort = null
);
