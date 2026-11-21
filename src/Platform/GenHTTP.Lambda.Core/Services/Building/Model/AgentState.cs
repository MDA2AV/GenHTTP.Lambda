using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Building;

[Summary("Whether a change can be asked for, and the change under way or the last one.")]
public sealed record AgentState(
    [Summary("Whether changes can be asked for: there is an agent, and the operator has not switched the box off")]
    bool Available,
    [Summary("How many builds and changes one address may ask for in a day")]
    int PerDay,
    [Summary("How many of those this caller has left today")]
    int Left,
    [Summary("The models and efforts a change may be asked for on, as the operator offers them")]
    AgentOffer Offer,
    [Summary("The change under way, or the last one, while the agent still remembers it")]
    ChangeProgress? Job
);
