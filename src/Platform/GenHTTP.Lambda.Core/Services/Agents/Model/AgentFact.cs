using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// One thing the agent system says of itself, as the panel lists it.
/// </summary>
[Summary("One thing the agent system says of itself.")]
public sealed record AgentFact(
    [Summary("What it is about: Signs in with, Version")]
    string Label,
    [Summary("What it says")]
    string Value
);
