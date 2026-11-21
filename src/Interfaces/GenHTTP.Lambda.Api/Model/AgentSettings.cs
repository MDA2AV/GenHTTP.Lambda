using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Meta;

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

[Summary("One effort of the build agent, and whether it is offered.")]
public sealed record AgentEffortResponse(
    [Summary("What it is asked for by: `low`, `medium`, `high`, `xhigh` or `max`, the least first")]
    string Name,
    [Summary("Whether it may be chosen")]
    bool Offered
);

/// <remarks>
/// What is left out stays as it is, so a script that only switches building
/// off does not have to repeat the models.
/// </remarks>
[Summary("What the operator decides about the build agent. What is left out stays as it is.")]
public sealed record AgentSettingsRequest(
    [Summary("Whether the workspace on /build builds new apps")]
    bool? Builds = null,
    [Summary("Whether owners may ask the agent about their apps")]
    bool? Changes = null,
    [Summary("Who may have each model; a model of the platform's this does not name is offered to nobody")]
    IReadOnlyList<AgentModelRequest>? Models = null,
    [Summary("The model a job runs on where nobody chose one: one offered to everybody, without the password")]
    string? Model = null,
    [Summary("The efforts that may be chosen, by name: at least one")]
    IReadOnlyList<string>? Efforts = null,
    [Summary("The effort a job runs at where nobody chose one: one of those offered")]
    string? Effort = null
)
{

    /// <summary>
    /// The settings this asks for, over those that stand.
    /// </summary>
    /// <exception cref="LambdaException">It names a model, an access or an effort there is none of</exception>
    public AgentSettings Over(AgentSettings current)
    {
        var models = current.Models;

        if (Models != null)
        {
            models =
            [
                .. Models.Select(m => new ModelSetting(
                    AgentCatalog.TryModel(m.Name, out var model) ? model : throw LambdaException.Invalid($"There is no model '{m.Name}' here."),
                    AgentCatalog.TryAccess(m.Access, out var access) ? access : throw LambdaException.Invalid($"Who may have a model is 'open', 'password' or 'off', not '{m.Access}'.")))
            ];
        }

        var efforts = current.Efforts;

        if (Efforts != null)
        {
            efforts = [.. Efforts.Select(e => AgentCatalog.TryEffort(e, out var effort) ? effort : throw LambdaException.Invalid($"There is no effort '{e}' here."))];
        }

        var defaultModel = current.DefaultModel;

        if (Model != null && !AgentCatalog.TryModel(Model, out defaultModel))
        {
            throw LambdaException.Invalid($"There is no model '{Model}' here.");
        }

        var defaultEffort = current.DefaultEffort;

        if (Effort != null && !AgentCatalog.TryEffort(Effort, out defaultEffort))
        {
            throw LambdaException.Invalid($"There is no effort '{Effort}' here.");
        }

        return new AgentSettings(Builds ?? current.Builds, Changes ?? current.Changes, models, defaultModel, efforts, defaultEffort);
    }

}

[Summary("Who may have one model of the build agent.")]
public sealed record AgentModelRequest(
    [Summary("The model, by name: `opus`, `sonnet`, `haiku` or `fable`")]
    string Name,
    [Summary("`open` for everybody, `password` for whoever knows the installation's password, `off` for nobody")]
    string Access
);
