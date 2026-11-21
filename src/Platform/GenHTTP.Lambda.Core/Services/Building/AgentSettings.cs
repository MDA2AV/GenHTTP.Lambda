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

}

/// <summary>
/// Who may have one model of the build agent.
/// </summary>
public sealed record ModelSetting(AgentModel Model, ModelAccess Access);

/// <summary>
/// The model and the effort a job runs on, as the caller may have them.
/// </summary>
/// <param name="Unbounded">
/// Whether it runs without a clock and without a limit on its steps: only a
/// model behind the password, and only once the password was checked.
/// </param>
public sealed record AgentChoice(AgentModel Model, AgentEffort Effort, bool Unbounded)
{

    public string ModelName => AgentCatalog.NameOf(Model);

    public string EffortName => AgentCatalog.NameOf(Effort);

}
