using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Api.Model;

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
