using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What the operator decides about one option of the agent's. What is left out stays as it is.")]
public sealed record AgentOptionRequest(
    [Summary("The option, by name")]
    string Name,
    [Summary("Who may choose which values; a value this does not name stays as it is")]
    IReadOnlyList<AgentModelRequest>? Values = null,
    [Summary("The value a run takes where nobody chose one: one offered to everybody, without the password")]
    string? Default = null
);
