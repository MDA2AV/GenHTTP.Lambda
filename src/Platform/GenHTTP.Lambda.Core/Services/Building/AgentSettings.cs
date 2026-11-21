using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// What the operator decided about the build agent: whether it builds and
/// changes apps here, and on which models and at which efforts.
/// </summary>
/// <remarks>
/// The password a model may be behind is not among them: it is the
/// installation's (<c>LAMBDA_AGENT_PASSWORD</c>), kept where its other
/// secrets are, and the panel says only whether there is one.
/// </remarks>
/// <param name="Builds">
/// Whether the workspace on /build builds new apps. Off, it only explains how
/// to connect an agent of one's own. Either way there is no box without an
/// agent.
/// </param>
/// <param name="Changes">
/// Whether owners may ask the agent about their apps - in the workspace and
/// the Change section of the editor. Off, both only explain how to connect an
/// agent of one's own.
/// </param>
/// <param name="Models">Who may have each model, every model of the catalog in its order</param>
/// <param name="DefaultModel">What a job runs on where nobody chose: offered to everybody</param>
/// <param name="Efforts">The efforts that may be chosen, the least first</param>
/// <param name="DefaultEffort">What a job runs at where nobody chose: one of <paramref name="Efforts"/></param>
public sealed record AgentSettings(bool Builds, bool Changes, IReadOnlyList<ModelSetting> Models, AgentModel DefaultModel,
                                   IReadOnlyList<AgentEffort> Efforts, AgentEffort DefaultEffort)
{

    /// <summary>
    /// What an installation offers until its operator decides otherwise:
    /// building and changing on, Opus, Sonnet and Haiku to everybody and Fable
    /// to whoever has the password, every effort, Opus at high by default.
    /// </summary>
    public static readonly AgentSettings Default = new(
        Builds: true,
        Changes: true,
        Models:
        [
            new(AgentModel.Opus, ModelAccess.Open),
            new(AgentModel.Sonnet, ModelAccess.Open),
            new(AgentModel.Haiku, ModelAccess.Open),
            new(AgentModel.Fable, ModelAccess.Password)
        ],
        DefaultModel: AgentModel.Opus,
        Efforts: [.. AgentCatalog.Efforts.Select(e => e.Effort)],
        DefaultEffort: AgentEffort.High
    );

    /// <summary>Who may have the given model.</summary>
    public ModelAccess AccessOf(AgentModel model)
        => Models.FirstOrDefault(m => m.Model == model)?.Access ?? ModelAccess.Off;

    /// <summary>
    /// These settings with what a caller changed, by the names the platform
    /// gives the models, accesses and efforts - or a refusal that names the
    /// one there is none of. Whether the result may be saved is the service's
    /// to say.
    /// </summary>
    /// <param name="models">Who may have each model; a model this does not name is offered to nobody</param>
    public AgentSettings With(bool? builds = null, bool? changes = null, IReadOnlyList<(string Name, string Access)>? models = null, string? model = null,
                              IReadOnlyList<string>? efforts = null, string? effort = null)
    {
        var offered = Models;

        if (models != null)
        {
            offered =
            [
                .. models.Select(m => new ModelSetting(
                    AgentCatalog.TryModel(m.Name, out var named) ? named : throw LambdaException.Invalid($"There is no model '{m.Name}' here."),
                    AgentCatalog.TryAccess(m.Access, out var access) ? access : throw LambdaException.Invalid($"Who may have a model is 'open', 'password' or 'off', not '{m.Access}'.")))
            ];
        }

        var chosen = Efforts;

        if (efforts != null)
        {
            chosen = [.. efforts.Select(e => AgentCatalog.TryEffort(e, out var named) ? named : throw LambdaException.Invalid($"There is no effort '{e}' here."))];
        }

        var defaultModel = DefaultModel;

        if (model != null && !AgentCatalog.TryModel(model, out defaultModel))
        {
            throw LambdaException.Invalid($"There is no model '{model}' here.");
        }

        var defaultEffort = DefaultEffort;

        if (effort != null && !AgentCatalog.TryEffort(effort, out defaultEffort))
        {
            throw LambdaException.Invalid($"There is no effort '{effort}' here.");
        }

        return new AgentSettings(builds ?? Builds, changes ?? Changes, offered, defaultModel, chosen, defaultEffort);
    }

}
