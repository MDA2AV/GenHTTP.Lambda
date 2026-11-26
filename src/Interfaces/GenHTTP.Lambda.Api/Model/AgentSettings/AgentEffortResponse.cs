using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("One value of the option `effort`, and whether it is offered - as the panel read it before options were the agent's.")]
public sealed record AgentEffortResponse(
    [Summary("What it is asked for by, the least first")]
    string Name,
    [Summary("Whether it may be chosen")]
    bool Offered
);
