using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A feature: a change being worked on beside the lambda.")]
public sealed record FeatureResponse(
    [Summary("What every call about it takes, and what its preview is reached by")]
    string Key,
    [Summary("What it is, in a few words")]
    string Name,
    [Summary("The branch it is in the lambda's git repository - given when it starts, and kept when it is renamed")]
    string Branch,
    [Summary("What the user wants from it and why")]
    string? Specification,
    [Summary("What it changes, in one line")]
    string? Change,
    [Summary("The version it is based on")]
    int Base,
    [Summary("The newest version of the lambda")]
    int? Newest,
    [Summary("Whether it can be merged: it is based on the newest version")]
    bool Mergeable,
    [Summary("How it was started: `api`, `agent` or `git`")]
    string? Origin,
    [Summary("When it was started")]
    DateTime Created,
    [Summary("When it was last saved or changed")]
    DateTime Modified,
    [Summary("Whether its preview is online")]
    bool Online,
    [Summary("Whether its preview is online and serves what the feature holds now, rather than an earlier save")]
    bool Current,
    [Summary("When its preview was last put online, while it is")]
    DateTime? Previewed,
    [Summary("Where its preview answers on this site, whether it is online or not: `/features/{key}/`")]
    string PreviewPath,
    [Summary("How many times its files were saved, counting the ones it began with. Passed back with a save, it keeps that save from replacing one made elsewhere.")]
    int Revision,
    [Summary("Who wrote what it holds - its last save - where the platform knows it: `build-agent`, the installation's own agent, on a job it was given; absent for anybody else's work, an agent of somebody's own included")]
    string? Writer = null,
    [Summary("The model the build agent wrote it on, by its name in `build.offer.models` of `GET /system` (`opus`); absent unless `writer` is there")]
    string? Model = null,
    [Summary("The effort the build agent wrote it at: `low`, `medium`, `high`, `xhigh` or `max`; absent unless `writer` is there")]
    string? Effort = null
);
