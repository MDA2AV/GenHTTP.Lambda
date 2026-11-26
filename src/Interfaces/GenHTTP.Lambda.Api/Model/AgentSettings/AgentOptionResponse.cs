using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("One option of the agent's - such as the model it runs on - and what the operator decided about it.")]
public sealed record AgentOptionResponse(
    [Summary("What it is called, as the agent integration names it")]
    string Name,
    [Summary("What the pages call it")]
    string Label,
    [Summary("Every value the agent has, in the order the pages list them")]
    IReadOnlyList<AgentValueResponse> Values,
    [Summary("The value a run takes where nobody chose one: offered to everybody")]
    string Default
);
