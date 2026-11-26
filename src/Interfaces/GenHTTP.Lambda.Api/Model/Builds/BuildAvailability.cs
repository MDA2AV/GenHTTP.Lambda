using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Agents;

namespace GenHTTP.Lambda.Api.Model;

/// <param name="Offer">
/// Which values need the password is said, but never the password, nor what
/// the agent system runs a value as: the page needs to know what to offer,
/// not what the agent does with it.
/// </param>
[Summary("Whether this installation builds things, how often it lets one caller, on which options and with which files.")]
public sealed record BuildAvailability(
    [Summary("Whether builds can be asked for: there is an agent, and the operator has not switched building off")]
    bool Available,
    [Summary("How many builds and changes one address may ask for in a day")]
    int PerDay,
    [Summary("The options a build or a message may be asked for on, as the operator offers them, the files it may come with and what the agent does")]
    AgentOffer Offer
);
