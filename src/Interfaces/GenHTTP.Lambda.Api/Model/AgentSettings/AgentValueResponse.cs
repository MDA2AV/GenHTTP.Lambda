using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("One value of an option of the agent's, and who may choose it.")]
public sealed record AgentValueResponse(
    [Summary("What it is asked for by, as the agent integration names it")]
    string Name,
    [Summary("What the pages call it")]
    string Label,
    [Summary("A line about it from the agent integration, where it has one")]
    string? Description,
    [Summary("Who may choose it: `open` for everybody, `password` for whoever knows the installation's password - a run with it held to the limits for such runs - or `off` for nobody")]
    string Access,
    [Summary("Whether the pages offer it now: one behind the password is offered only where the installation has a password")]
    bool Offered
);
