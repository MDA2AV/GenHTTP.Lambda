using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

/// <remarks>
/// Read from the platform's records once a run ended - the lambda waits for
/// the operator's decision on a request to connect, its code reads a key
/// nobody entered - not from what the agent said.
/// </remarks>
[Summary("Something the app waits for after a run, and whom it waits for.")]
public sealed record RunBlocker(
    [Summary("Whom it waits for: `operator` or `owner`")]
    string Who,
    [Summary("What: `connectivity` (a decision on connecting to other servers) or `keys` (keys and passwords to enter)")]
    string What,
    [Summary("The names of the keys, for `keys`")]
    IReadOnlyList<string>? Names = null
);
