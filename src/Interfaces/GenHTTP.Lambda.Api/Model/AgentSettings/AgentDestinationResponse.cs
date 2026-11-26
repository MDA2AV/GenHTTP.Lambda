using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("One place a run of the agent sends something to.")]
public sealed record AgentDestinationResponse(
    [Summary("Whose it is")]
    string Name,
    [Summary("Where it is, or how it is chosen")]
    string Address,
    [Summary("What goes there")]
    string What
);
