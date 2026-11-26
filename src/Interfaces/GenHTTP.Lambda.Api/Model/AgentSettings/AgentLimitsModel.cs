using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What one kind of run may take.")]
public sealed record AgentLimitsModel(
    [Summary("How long it may work, in seconds: zero for no limit, left out for what the agent holds a run to by default")]
    int? Seconds = null,
    [Summary("How many steps it may take: zero for no limit, left out for what the agent holds a run to by default")]
    int? Steps = null
);
