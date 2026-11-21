using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Who may have one model of the build agent.")]
public sealed record AgentModelRequest(
    [Summary("The model, by name: `opus`, `sonnet`, `haiku` or `fable`")]
    string Name,
    [Summary("`open` for everybody, `password` for whoever knows the installation's password, `off` for nobody")]
    string Access
);
