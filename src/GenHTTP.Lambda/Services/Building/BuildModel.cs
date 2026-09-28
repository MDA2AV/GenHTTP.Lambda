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
/// <param name="Events">What it has done so far, one line each</param>
/// <param name="Result">How it ended, once it has</param>
/// <param name="Waiting">Its place in the queue, zero once it runs</param>
public sealed record BuildProgress(string State, IReadOnlyList<string> Events, BuildResult? Result, int Waiting);

/// <summary>
/// How a build ended.
/// </summary>
/// <param name="Keep">What the person needs to be told about the editor key</param>
/// <param name="Detail">What went wrong, in the words of whatever it went wrong in</param>
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
    string? Detail
);

/// <summary>
/// One thing the agent did while changing a lambda, as the control center
/// draws it.
/// </summary>
/// <remarks>
/// Facts rather than sentences, so the control center can say them in the
/// owner's language: the kind of step, what it was about, and - once the tool
/// has answered - how it went. The only prose is what the agent itself said
/// between steps, which it was asked to write in the owner's language.
/// </remarks>
/// <param name="At">Seconds into the run</param>
/// <param name="Kind">
/// say, guide, demos, read, logs, write, check, deploy, upload, delete, list or other
/// </param>
/// <param name="Text">What the agent said, for a step of kind say</param>
/// <param name="Tool">The tool, for a step of a kind this list does not name</param>
/// <param name="Files">The files written or read</param>
/// <param name="Removed">The files a write removed</param>
/// <param name="Whole">Whether a write replaced every file rather than some</param>
/// <param name="Path">The workspace path of an upload or a removal</param>
/// <param name="Done">Whether the tool has answered</param>
/// <param name="Version">The version read, saved, or put online</param>
/// <param name="Online">Whether a write or a deployment went online</param>
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
    string? Problem = null
);

/// <summary>
/// How a change ended, from what the tools answered rather than from what the
/// model said about them.
/// </summary>
/// <param name="Ok">Whether it saved a version</param>
/// <param name="Version">The newest version it saved</param>
/// <param name="Online">The version it put online, if it put one there</param>
/// <param name="Deployed">Whether it put a version online</param>
/// <param name="Compiles">Whether the last version it compiled compiles, where it is known</param>
/// <param name="Before">The version that was online when it started</param>
/// <param name="Unchanged">It saved nothing</param>
/// <param name="Cancelled">Somebody stopped it</param>
/// <param name="Reason">
/// What cut it short - timeout, turns (it used up its steps) or unauthorised - or,
/// with nothing saved and nothing said, nothing
/// </param>
/// <param name="Summary">What the agent said at the end, for the owner</param>
/// <param name="Error">What went wrong, in English, for a client without words of its own</param>
/// <param name="Detail">What went wrong, in the words of whatever it went wrong in</param>
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
    string? Detail = null
);

/// <summary>
/// A change the agent was asked for, and how it is getting on.
/// </summary>
/// <param name="State">queued, running, done, failed or cancelled</param>
/// <param name="Prompt">What was asked for</param>
/// <param name="Deploy">Whether it was asked to put the change online</param>
/// <param name="Model">Which of the offered models is doing it</param>
/// <param name="Before">The version that was online when it was asked for</param>
/// <param name="Waiting">Its place in the queue, zero once it runs</param>
/// <param name="Seconds">How long it has been running</param>
/// <param name="Limit">How long it may run, in seconds; null when there is no clock on it</param>
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
    int? Limit
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
