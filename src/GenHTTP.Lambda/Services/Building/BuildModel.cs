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
    bool Changed,
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
