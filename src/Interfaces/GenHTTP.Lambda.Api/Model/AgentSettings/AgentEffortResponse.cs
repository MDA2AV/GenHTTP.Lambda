using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("One effort of the build agent, and whether it is offered.")]
public sealed record AgentEffortResponse(
    [Summary("What it is asked for by: `low`, `medium`, `high`, `xhigh` or `max`, the least first")]
    string Name,
    [Summary("Whether it may be chosen")]
    bool Offered
);
