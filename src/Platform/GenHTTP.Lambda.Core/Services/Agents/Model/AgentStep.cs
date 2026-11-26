using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

/// <remarks>
/// Facts rather than sentences, so the pages can say them in the visitor's or
/// the owner's language: the kind of step, what it was about, and - once it
/// is done - how it went. The platform records them itself from the calls
/// that carry the run's credential, whatever the agent behind it; what the
/// agent did beyond the platform the integration tells in a set of kinds of
/// its own (<c>web</c>, <c>search</c>, <c>other</c>). The only prose is what
/// the agent itself said between steps, which it writes in the person's
/// language.
/// </remarks>
[Summary("One thing the agent did while building or changing a lambda.")]
[Remarks("A fact rather than a sentence, so a page can say it in its own language. The only prose is what the agent itself said between steps (`say`).")]
public sealed record AgentStep(
    [Summary("Seconds into the run")]
    int At,
    [Summary("What it did: `say`, `guide`, `demos`, `read`, `logs`, `create`, `write`, `check`, `deploy`, `upload`, `delete`, `list`, `feature` (started one), `update` (changed a feature's notes or base), `merge`, `discard` (deleted a feature), `data` (switched a kind of data on), `records` (read the database), `secrets` (listed the secrets), `connectivity` (asked for the lambda to connect to other servers), `analytics` (read who visits, or switched counting on), `source` (published the source code, or read whether it is), `showcase` (listed the app in the showcase, or took it out), `attachment` (read a file the person attached), `file` (handed the person a file), `web` (read a page of a site), `search` (searched the web) or `other`")]
    string Kind,
    [Summary("What the agent said, for a step of kind `say`")]
    string? Text = null,
    [Summary("The tool, for a step of kind `other`, as the agent names it")]
    string? Tool = null,
    [Summary("The files written or read - or, for `attachment` and `file`, the file by its name")]
    IReadOnlyList<string>? Files = null,
    [Summary("The files a write removed")]
    IReadOnlyList<string>? Removed = null,
    [Summary("Whether a write replaced every file rather than some")]
    bool? Whole = null,
    [Summary("The workspace path of an upload or a removal")]
    string? Path = null,
    [Summary("Whether the tool has answered")]
    bool? Done = null,
    [Summary("The version read, saved, merged into, put online, or made a feature's base")]
    int? Version = null,
    [Summary("Whether a write or a deployment went online - at the preview address, for a feature")]
    bool? Online = null,
    [Summary("How many errors the compiler found")]
    int? Errors = null,
    [Summary("How many errors the log holds")]
    int? Problems = null,
    [Summary("Why the platform refused the step, in its words")]
    string? Problem = null,
    [Summary("The name of the feature the step was about")]
    string? Feature = null,
    [Summary("Whether the step was about a feature rather than the lambda itself")]
    bool? Preview = null,
    [Summary("The kind of data a step of kind `data` switched on: `database`, `secrets` or `workspace`")]
    string? Data = null,
    [Summary("The site a step of kind `web` read a page of, by its host name without `www.` - never the rest of the address")]
    string? Site = null
);
