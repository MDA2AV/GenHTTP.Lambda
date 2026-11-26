using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Integrations;

namespace GenHTTP.Lambda.Services.Agents;

/// <remarks>
/// Asked as the panel is drawn, with a few seconds to answer. Where several
/// installations share the agent system, the runs it counts are this
/// installation's; of the others it says only how many it takes at once.
/// </remarks>
[Summary("What the agent system says of itself.")]
public sealed record AgentSystem(
    [Summary("Whether it answered")]
    bool Reachable,
    [Summary("How many of this installation's runs it works on now")]
    int Running = 0,
    [Summary("How many of this installation's runs wait for their turn")]
    int Queued = 0,
    [Summary("How many runs it works on at once, every installation's together, where it says")]
    int? Concurrency = null,
    [Summary("What else it says of itself, as label and value - what it signs in with, the version it runs - never a credential")]
    IReadOnlyList<IntegrationDetail>? Facts = null
);
