using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Building;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What the operator decided about the build agent, and what its runner says of itself.")]
public sealed record AgentSettingsResponse(
    [Summary("Whether the workspace on /build builds new apps, where there is an agent")]
    bool Builds,
    [Summary("Whether owners may ask the agent about their apps - in the workspace and the Change section of the editor - where there is an agent")]
    bool Changes,
    [Summary("Every model the platform knows, in the order the pages list them, with who may have it")]
    IReadOnlyList<AgentModelResponse> Models,
    [Summary("The model a job runs on where nobody chose one, by its name: offered to everybody")]
    string Model,
    [Summary("Every effort, the least first, with whether it is offered")]
    IReadOnlyList<AgentEffortResponse> Efforts,
    [Summary("The effort a job runs at where nobody chose one")]
    string Effort,
    [Summary("Whether the installation has a password for the models behind one (`LAMBDA_AGENT_PASSWORD`); without, they are offered to nobody")]
    bool PasswordSet,
    [Summary("How many builds and changes one address may ask for in a day - a limit, set with `PUT /admin/limits`")]
    int PerDay,
    [Summary("Where the runner of the agent listens (`LAMBDA_AGENT_URL`); absent where the installation has none, and nothing is built from a request")]
    string? Url,
    [Summary("What the runner says of itself, asked as this was read; absent without a runner, and in the answer to a change")]
    AgentRunner? Runner
)
{

    public static AgentSettingsResponse Of(AgentSettings settings, AgentOffer offer, bool passwordSet, int perDay, string? url, AgentRunner? runner)
        => new(
            settings.Builds,
            settings.Changes,
            [.. AgentCatalog.Models.Select(m => new AgentModelResponse(m.Name, m.Label, AgentCatalog.NameOf(settings.AccessOf(m.Model)),
                                                                       offer.Models.Any(o => o.Name == m.Name)))],
            AgentCatalog.NameOf(settings.DefaultModel),
            [.. AgentCatalog.Efforts.Select(e => new AgentEffortResponse(e.Name, settings.Efforts.Contains(e.Effort)))],
            AgentCatalog.NameOf(settings.DefaultEffort),
            passwordSet,
            perDay,
            url,
            runner
        );

}
