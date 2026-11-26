using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Who may choose one value of an option of the agent's - one model, as `models` names them.")]
public sealed record AgentModelRequest(
    [Summary("The value, by name")]
    string Name,
    [Summary("`open` for everybody, `password` for whoever knows the installation's password, `off` for nobody")]
    string Access
);
