using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Building;

/// <remarks>
/// Read from the runner's health, which it answers without the queue
/// noticing: what it signs in with and which identifier each model runs as
/// are its own environment, which only the panel shows.
/// </remarks>
[Summary("What the runner of the build agent says of itself.")]
public sealed record AgentRunner(
    [Summary("Whether it answered")]
    bool Reachable,
    [Summary("What its builds sign in to Anthropic with: `subscription` (a token of a Claude subscription) or `api-key`; absent where it has neither, and every job would fail to sign in")]
    string? SignsInWith = null,
    [Summary("Which model of Anthropic's each model runs as, by name")]
    IReadOnlyDictionary<string, string>? Models = null,
    [Summary("The efforts it takes")]
    IReadOnlyList<string>? Efforts = null,
    [Summary("The version of Claude Code its builds run, where it says")]
    string? Cli = null,
    [Summary("How many jobs run now, of at most two: one on the clock and one without")]
    int Running = 0,
    [Summary("How many jobs wait")]
    int Queued = 0,
    [Summary("Whether a build may read the pages of the sites its request names - GET and HEAD, made by its proxy, to public addresses of those sites only; absent where the runner does not say (one from before)")]
    bool? ReadsPages = null
);
