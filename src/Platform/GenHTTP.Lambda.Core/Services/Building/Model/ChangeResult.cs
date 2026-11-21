using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Building;

[Summary("How a change ended, from what the tools answered rather than from what the model said about them.")]
public sealed record ChangeResult(
    [Summary("Whether it saved anything - into a feature, or as a version")]
    bool Ok,
    [Summary("The newest version it saved, merging included")]
    int? Version = null,
    [Summary("The version it put online, if it put one there")]
    int? Online = null,
    [Summary("Whether it put a version online")]
    bool? Deployed = null,
    [Summary("Whether the last code it compiled compiles, where it is known")]
    bool? Compiles = null,
    [Summary("The version that was online when it started - to put back, if the change is not wanted")]
    int? Before = null,
    [Summary("Whether it saved nothing")]
    bool? Unchanged = null,
    [Summary("Whether somebody stopped it")]
    bool? Cancelled = null,
    [Summary("What cut it short: `timeout`, `turns` (it used up its steps) or `unauthorised`; `declined` where the agent would not make the change at all")]
    string? Reason = null,
    [Summary("What the agent said at the end, for the owner - or, where it declined, why")]
    string? Summary = null,
    [Summary("What went wrong, in English")]
    string? Error = null,
    [Summary("What went wrong, in the words of whatever it went wrong in")]
    string? Detail = null,
    [Summary("The key of the feature the change was left in, where it was not merged")]
    string? Feature = null,
    [Summary("What that feature is called")]
    string? FeatureName = null,
    [Summary("Whether that feature's preview is online, to be tried")]
    bool? Preview = null,
    [Summary("Whether the agent would not make the change; `summary` says why")]
    bool? Declined = null,
    [Summary("Whether the agent answered a question rather than change anything - it started no feature and saved nothing; `summary` is the answer")]
    bool? Answered = null,
    [Summary("The settings it changed: `database`, `secrets` or `workspace` switched on, `analytics` switched on, `source` published, `connectivity` asked for, `discarded` (it deleted a feature)")]
    IReadOnlyList<string>? Settings = null
);
