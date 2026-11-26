using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("One value of the option `model`, and who may have it - as the panel read it before options were the agent's.")]
public sealed record AgentModelResponse(
    [Summary("What it is asked for by")]
    string Name,
    [Summary("What the pages call it")]
    string Label,
    [Summary("Who may have it: `open` for everybody, `password` for whoever knows the installation's password, or `off` for nobody")]
    string Access,
    [Summary("Whether the pages offer it now: one behind the password is offered only where the installation has a password")]
    bool Offered
);
