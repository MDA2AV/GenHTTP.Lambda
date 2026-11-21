using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("One model of the build agent, and who may have it.")]
public sealed record AgentModelResponse(
    [Summary("What it is asked for by: `opus`, `sonnet`, `haiku` or `fable`")]
    string Name,
    [Summary("What the pages call it, with the version the runner runs by default")]
    string Label,
    [Summary("Who may have it: `open` for everybody, `password` for whoever knows the installation's password - and, with it, without a clock and a limit on its steps - or `off` for nobody")]
    string Access,
    [Summary("Whether the pages offer it now: one behind the password is offered only where the installation has a password")]
    bool Offered
);
