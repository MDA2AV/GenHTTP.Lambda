using GenHTTP.Api.Content.Services;

namespace GenHTTP.Lambda.Services.Building;

[Summary("A build the agent has taken on.")]
public sealed record BuildStarted(
    [Summary("What to follow it by: `GET /builds/{id}`")]
    string Id,
    [Summary("How many builds are waiting ahead of it")]
    int Queued
);

[Summary("How a build is getting on.")]
public sealed record BuildProgress(
    [Summary("`queued`, `running`, `done` or `failed`")]
    string State,
    [Summary("What it has done so far, as facts a page can say in the visitor's language - the same steps the editor shows of a change")]
    IReadOnlyList<AgentStep> Steps,
    [Summary("How it ended, once it has")]
    BuildResult? Result,
    [Summary("Its place in the queue; zero once it runs")]
    int Waiting,
    [Summary("How long it has been running, in seconds, counted by the agent")]
    int Seconds = 0,
    [Summary("How long it may run, in seconds; absent when there is no clock on it")]
    int? Limit = null
);

[Summary("How a build ended.")]
public sealed record BuildResult(
    [Summary("Whether a lambda was made")]
    bool Ok,
    [Summary("Whether it is online")]
    bool? Deployed,
    [Summary("The public key of the lambda that was made")]
    string? PublicKey,
    [Summary("Its editor key - handed out here, to whoever asked for the build, and nowhere else")]
    string? PrivateKey,
    [Summary("Where it answers")]
    string? Url,
    [Summary("Where its editor is")]
    string? EditorUrl,
    [Summary("What the agent said it made")]
    string? Summary,
    [Summary("What the person needs to be told about keeping the editor key")]
    string? Keep,
    [Summary("What went wrong, or - where it was declined - why, in the language of the request")]
    string? Error,
    [Summary("What went wrong, in the words of whatever it went wrong in")]
    string? Detail,
    [Summary("Whether the agent would not build what was asked for, because it is not an application or not one this platform is for; `error` says why")]
    bool? Declined = null
);

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
    [Summary("What it did: `say`, `guide`, `demos`, `read`, `logs`, `create`, `write`, `check`, `deploy`, `upload`, `delete`, `list`, `feature` (started one), `update` (changed a feature's notes or base), `merge`, `discard` (deleted a feature), `data` (switched a kind of data on), `records` (read the database), `secrets` (listed the secrets) or `other`")]
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
    bool? Declined = null
);

[Summary("A change the agent was asked for, and how it is getting on.")]
public sealed record ChangeProgress(
    [Summary("What the agent knows it by")]
    string Id,
    [Summary("`queued`, `running`, `done`, `failed` or `cancelled`")]
    string State,
    [Summary("What was asked for")]
    string Prompt,
    [Summary("Whether it was asked to merge the change and put it online")]
    bool Deploy,
    [Summary("Which of the offered models is making it")]
    string Model,
    [Summary("The version that was online when it was asked for")]
    int? Before,
    [Summary("What it has done so far")]
    IReadOnlyList<AgentStep> Steps,
    [Summary("How it ended, once it has")]
    ChangeResult? Result,
    [Summary("Its place in the queue; zero once it runs")]
    int Waiting,
    [Summary("How long it has been running, in seconds")]
    int Seconds,
    [Summary("How long it may run, in seconds; absent when there is no clock on it")]
    int? Limit,
    [Summary("The key of the feature it was asked to go on with; absent when it starts one of its own")]
    string? Feature = null
);

[Summary("Whether a change can be asked for, and the change under way or the last one.")]
public sealed record AgentState(
    [Summary("Whether changes can be asked for: there is an agent, and the operator has not switched the box off")]
    bool Available,
    [Summary("How many builds and changes one address may ask for in a day")]
    int PerDay,
    [Summary("How many of those this caller has left today")]
    int Left,
    [Summary("Whether a second model is on offer, behind a password")]
    bool SecondModel,
    [Summary("The change under way, or the last one, while the agent still remembers it")]
    ChangeProgress? Job
);
