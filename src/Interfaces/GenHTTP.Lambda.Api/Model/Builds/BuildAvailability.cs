using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Building;

namespace GenHTTP.Lambda.Api.Model;

/// <param name="Offer">
/// Which models need the password is said, but never the password, nor
/// which identifier a model runs as: the page needs to know what to offer,
/// not what the runner does with it.
/// </param>
[Summary("Whether this installation builds things, how often it lets one caller, and on which models.")]
public sealed record BuildAvailability(
    [Summary("Whether builds can be asked for: there is an agent, and the operator has not switched building off")]
    bool Available,
    [Summary("How many builds and changes one address may ask for in a day")]
    int PerDay,
    [Summary("The models and efforts a build or a change may be asked for, as the operator offers them")]
    AgentOffer Offer
);
