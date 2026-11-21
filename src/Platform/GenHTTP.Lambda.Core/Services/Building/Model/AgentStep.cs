using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Building;

/// <remarks>
/// Facts rather than sentences, so the pages can say them in the visitor's or
/// the owner's language: the kind of step, what it was about, and - once the
/// tool has answered - how it went. The only prose is what the agent itself
/// said between steps, which it was asked to write in the owner's language.
/// </remarks>
[Summary("One thing the agent did while building or changing a lambda.")]
[Remarks("A fact rather than a sentence, so a page can say it in its own language. The only prose is what the agent itself said between steps (`say`).")]
public sealed record AgentStep(
    [Summary("Seconds into the run")]
    int At,
    [Summary("What it did: `say`, `guide`, `demos`, `read`, `logs`, `create`, `write`, `check`, `deploy`, `upload`, `delete`, `list`, `feature` (started one), `update` (changed a feature's notes or base), `merge`, `discard` (deleted a feature), `data` (switched a kind of data on), `records` (read the database), `secrets` (listed the secrets), `connectivity` (asked for the lambda to connect to other servers), `analytics` (read who visits, or switched counting on), `source` (published the source code, or read whether it is) or `other`")]
    string Kind,
    [Summary("What the agent said, for a step of kind `say`")]
    string? Text = null,
    [Summary("The tool, for a step of kind `other`")]
    string? Tool = null,
    [Summary("The files written or read")]
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
    string? Data = null
);
