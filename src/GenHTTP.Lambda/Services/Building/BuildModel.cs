namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// A build the agent has taken on.
/// </summary>
/// <param name="Id">What to ask for its progress by</param>
/// <param name="Queued">How many builds are waiting ahead of it</param>
public sealed record BuildStarted(string Id, int Queued);

/// <summary>
/// How a build is getting on.
/// </summary>
/// <param name="State">queued, running, done or failed</param>
/// <param name="Steps">
/// What it has done so far, as facts the build page says in the visitor's
/// language - the same steps the control center shows of a change
/// </param>
/// <param name="Result">How it ended, once it has</param>
/// <param name="Waiting">Its place in the queue, zero once it runs</param>
public sealed record BuildProgress(string State, IReadOnlyList<AgentStep> Steps, BuildResult? Result, int Waiting);

/// <summary>
/// How a build ended.
/// </summary>
/// <param name="Keep">What the person needs to be told about the editor key</param>
/// <param name="Detail">What went wrong, in the words of whatever it went wrong in</param>
/// <param name="Declined">
/// The agent would not build what was asked for, because it is not an application
/// or not one this platform is for; <c>Error</c> is what it said, in the language
/// of the request
/// </param>
public sealed record BuildResult(
    bool Ok,
    bool? Deployed,
    string? PublicKey,
    string? PrivateKey,
    string? Url,
    string? EditorUrl,
    string? Summary,
    string? Keep,
    string? Error,
    string? Detail,
    bool? Declined = null
);

/// <summary>
/// One thing the agent did while building or changing a lambda, as the build
/// page and the control center draw it.
/// </summary>
/// <remarks>
/// Facts rather than sentences, so the pages can say them in the visitor's or
/// the owner's language: the kind of step, what it was about, and - once the
/// tool has answered - how it went. The only prose is what the agent itself
/// said between steps, which it was asked to write in the owner's language.
/// </remarks>
/// <param name="At">Seconds into the run</param>
/// <param name="Kind">
/// say, guide, demos, read, logs, create, write, check, deploy, upload, delete,
/// list, feature (started one), update (changed one's notes or base), merge,
/// discard (deleted one), data (switched a kind of data on), records (read the
/// database), secrets (listed the secrets) or other
/// </param>
/// <param name="Data">The kind of data a step of kind data switched on: database, secrets or workspace</param>
/// <param name="Text">What the agent said, for a step of kind say</param>
/// <param name="Tool">The tool, for a step of a kind this list does not name</param>
/// <param name="Files">The files written or read</param>
/// <param name="Removed">The files a write removed</param>
/// <param name="Whole">Whether a write replaced every file rather than some</param>
/// <param name="Path">The workspace path of an upload or a removal</param>
/// <param name="Done">Whether the tool has answered</param>
/// <param name="Version">The version read, saved, merged into, put online, or made a feature's base</param>
/// <param name="Online">Whether a write or a deployment went online - at the preview address, for a feature</param>
/// <param name="Feature">The name of the feature the step was about</param>
/// <param name="Preview">Whether the step was about a feature rather than the lambda itself</param>
/// <param name="Errors">How many errors the compiler found</param>
/// <param name="Problems">How many errors the log holds</param>
/// <param name="Problem">Why the platform refused the step, in its words</param>
public sealed record AgentStep(
    int At,
    string Kind,
    string? Text = null,
    string? Tool = null,
    IReadOnlyList<string>? Files = null,
    IReadOnlyList<string>? Removed = null,
    bool? Whole = null,
    string? Path = null,
    bool? Done = null,
    int? Version = null,
    bool? Online = null,
    int? Errors = null,
    int? Problems = null,
    string? Problem = null,
    string? Feature = null,
    bool? Preview = null,
    string? Data = null
);

/// <summary>
/// How a change ended, from what the tools answered rather than from what the
/// model said about them.
/// </summary>
/// <param name="Ok">Whether it saved anything - into a feature, or as a version</param>
/// <param name="Version">The newest version it saved, merging included</param>
/// <param name="Online">The version it put online, if it put one there</param>
/// <param name="Deployed">Whether it put a version online</param>
/// <param name="Compiles">Whether the last version it compiled compiles, where it is known</param>
/// <param name="Before">The version that was online when it started</param>
/// <param name="Unchanged">It saved nothing</param>
/// <param name="Cancelled">Somebody stopped it</param>
/// <param name="Reason">
/// What cut it short - timeout, turns (it used up its steps) or unauthorised - or,
/// with nothing saved and nothing said, nothing; declined when the agent would
/// not make the change at all
/// </param>
/// <param name="Declined">The agent would not make the change; <c>Summary</c> says why</param>
/// <param name="Summary">What the agent said at the end, for the owner</param>
/// <param name="Error">What went wrong, in English, for a client without words of its own</param>
/// <param name="Detail">What went wrong, in the words of whatever it went wrong in</param>
/// <param name="Feature">The feature the change was left in, by its key, when it was not merged</param>
/// <param name="FeatureName">What that feature is called</param>
/// <param name="Preview">Whether that feature's preview is online, to be tried</param>
public sealed record ChangeResult(
    bool Ok,
    int? Version = null,
    int? Online = null,
    bool? Deployed = null,
    bool? Compiles = null,
    int? Before = null,
    bool? Unchanged = null,
    bool? Cancelled = null,
    string? Reason = null,
    string? Summary = null,
    string? Error = null,
    string? Detail = null,
    string? Feature = null,
    string? FeatureName = null,
    bool? Preview = null,
    bool? Declined = null
);

/// <summary>
/// A change the agent was asked for, and how it is getting on.
/// </summary>
/// <param name="State">queued, running, done, failed or cancelled</param>
/// <param name="Prompt">What was asked for</param>
/// <param name="Deploy">Whether it was asked to merge the change and put it online</param>
/// <param name="Model">Which of the offered models is doing it</param>
/// <param name="Before">The version that was online when it was asked for</param>
/// <param name="Waiting">Its place in the queue, zero once it runs</param>
/// <param name="Seconds">How long it has been running</param>
/// <param name="Limit">How long it may run, in seconds; null when there is no clock on it</param>
/// <param name="Feature">The feature it was asked to go on with, by its key; null when it starts one of its own</param>
public sealed record ChangeProgress(
    string Id,
    string State,
    string Prompt,
    bool Deploy,
    string Model,
    int? Before,
    IReadOnlyList<AgentStep> Steps,
    ChangeResult? Result,
    int Waiting,
    int Seconds,
    int? Limit,
    string? Feature = null
);

/// <summary>
/// What the Change section of the control center needs to draw itself.
/// </summary>
/// <param name="Available">Whether this installation has an agent at all</param>
/// <param name="PerDay">How many builds and changes one address may ask for in a day</param>
/// <param name="Left">How many of those this caller has left today</param>
/// <param name="SecondModel">Whether a second model is on offer, behind a password</param>
/// <param name="Job">The change under way, or the last one, while the agent still remembers it</param>
public sealed record AgentState(bool Available, int PerDay, int Left, bool SecondModel, ChangeProgress? Job);
