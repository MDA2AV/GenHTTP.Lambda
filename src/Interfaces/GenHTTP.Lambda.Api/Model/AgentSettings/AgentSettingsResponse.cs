using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Services.Agents;

namespace GenHTTP.Lambda.Api.Model;

/// <remarks>
/// <c>models</c>, <c>model</c>, <c>efforts</c> and <c>effort</c> are the
/// options <c>model</c> and <c>effort</c>, as the panel read them before
/// options were the agent integration's - empty where it has no such option.
/// </remarks>
[Summary("What the operator decided about the agent, what the agent is and offers, and what its system says of itself.")]
public sealed record AgentSettingsResponse(
    [Summary("Whether the workspace on /build builds new apps, where there is an agent")]
    bool Builds,
    [Summary("Whether owners may ask the agent about their apps - in the workspace and the Change section of the editor - where there is an agent")]
    bool Changes,
    [Summary("The values of the option `model`, with who may have each")]
    IReadOnlyList<AgentModelResponse> Models,
    [Summary("The default of the option `model`")]
    string? Model,
    [Summary("The values of the option `effort`, the least first, with whether each is offered")]
    IReadOnlyList<AgentEffortResponse> Efforts,
    [Summary("The default of the option `effort`")]
    string? Effort,
    [Summary("Whether the installation has a password for the values behind one (`LAMBDA_AGENT_PASSWORD`); without, they are offered to nobody")]
    bool PasswordSet,
    [Summary("How many builds and messages one address may ask for in a day - a limit, set with `PUT /admin/limits`")]
    int PerDay,
    [Summary("Where the runner of the platform's own agent listens (`LAMBDA_AGENT_URL`), where the installation names one")]
    string? Url,
    [Summary("What the agent system says of itself, asked as this was read; absent without an agent, and in the answer to a change")]
    AgentSystem? Runner,
    [Summary("Every option of the agent's, with who may choose each value and the default")]
    IReadOnlyList<AgentOptionResponse>? Options = null,
    [Summary("What a run may take, as the operator set it")]
    AgentLimitsModel? Limits = null,
    [Summary("What a run on a value behind the password may take, as the operator set it")]
    AgentLimitsModel? GuardedLimits = null,
    [Summary("Which limits the agent holds a run to, and to what by default")]
    AgentLimitSupportResponse? LimitSupport = null,
    [Summary("What the agent is, where it sends what, and what it does; absent without an agent")]
    AgentDescriptionResponse? Agent = null
)
{

    public static AgentSettingsResponse Of(IAgentService agents, string? url, AgentSystem? runner)
    {
        var settings = agents.GetSettings();
        var capabilities = agents.Capabilities;

        var offer = agents.Offer();

        var options = (capabilities?.Options ?? [])
                      .Select(o => new AgentOptionResponse(o.Name, o.Label,
                                                           [.. o.Values.Select(v => new AgentValueResponse(v.Name, v.Label, v.Description,
                                                                                                           OptionAccessNames.Name(settings.AccessOf(o, v)),
                                                                                                           offer.Options.Any(x => x.Name == o.Name && x.Values.Any(y => y.Name == v.Name))))],
                                                           AgentGate.DefaultOf(settings, o)))
                      .ToList();

        var models = options.FirstOrDefault(o => o.Name == "model");
        var efforts = options.FirstOrDefault(o => o.Name == "effort");

        return new AgentSettingsResponse(
            settings.Builds,
            settings.Changes,
            [.. models?.Values.Select(v => new AgentModelResponse(v.Name, v.Label, v.Access, v.Offered)) ?? []],
            models?.Default,
            [.. efforts?.Values.Select(v => new AgentEffortResponse(v.Name, v.Offered)) ?? []],
            efforts?.Default,
            agents.PasswordSet,
            agents.PerDay,
            url,
            runner,
            options,
            new AgentLimitsModel(settings.Limits.Seconds, settings.Limits.Steps),
            new AgentLimitsModel(settings.GuardedLimits.Seconds, settings.GuardedLimits.Steps),
            capabilities is { Limits: var support }
                ? new AgentLimitSupportResponse(support.Time, support.Steps, Seconds(support.Default.Time), support.Default.Steps)
                : null,
            capabilities is { } known ? Describe(known) : null
        );
    }

    private static AgentDescriptionResponse Describe(AgentCapabilities capabilities)
        => new(capabilities.Disclosure.Agent, capabilities.Disclosure.Vendor,
               [.. capabilities.Disclosure.Destinations.Select(d => new AgentDestinationResponse(d.Name, d.Address, d.What))],
               capabilities.Web.Reads switch
               {
                   AgentWebScope.NamedSites => "named",
                   AgentWebScope.Any => "any",
                   _ => "none"
               },
               capabilities.Web.Searches, capabilities.Web.Policy, capabilities.Attachments?.MediaTypes, capabilities.ReturnsFiles, capabilities.Asks,
               capabilities.Cancels);

    private static int? Seconds(TimeSpan? time) => time is { } value ? (int)value.TotalSeconds : null;

}
