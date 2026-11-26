using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

/// <remarks>
/// For the operator, who reads the prompts in the log in full already: the
/// files people attached are theirs to open as well, by the same rule.
/// </remarks>
[Summary("A run of the agent, as the operator sees it.")]
public sealed record RunListing(
    [Summary("What it is known by")]
    string Id,
    [Summary("`build` or `change`")]
    string Kind,
    [Summary("The lambda it worked on, by public key, once it had one")]
    string? Lambda,
    [Summary("`queued`, `running`, `waiting` or `ended`")]
    string State,
    [Summary("How it ended: `completed`, `asked`, `declined`, `cancelled`, `timed-out`, `out-of-steps`, `sign-in-failed`, `failed` or `lost`")]
    string? Ending,
    [Summary("The value of each option it ran on")]
    IReadOnlyDictionary<string, string> Options,
    [Summary("When it was asked for")]
    DateTime Created,
    [Summary("How long it worked, in seconds")]
    int Seconds,
    [Summary("What was asked, as it was asked")]
    string Prompt,
    [Summary("The files it was asked with - `GET /admin/agent/files/{id}` answers with one")]
    IReadOnlyList<RunFile> Attachments,
    [Summary("The files the agent handed back")]
    IReadOnlyList<RunFile> Files,
    [Summary("The tokens it read, where the agent says")]
    long? InputTokens = null,
    [Summary("The tokens it wrote, where the agent says")]
    long? OutputTokens = null,
    [Summary("What it cost, where the agent says")]
    double? Cost = null,
    [Summary("What the cost is in, as an ISO 4217 code")]
    string? Currency = null,
    [Summary("What went wrong, in the words of whatever it went wrong in")]
    string? Detail = null
);
