using GenHTTP.Lambda.Integrations.Agents;

namespace GenHTTP.Lambda.Integrations.ClaudeCode.Provider;

/// <summary>
/// The options the runner offers - the model a run is on and its effort -
/// with what the pages call them.
/// </summary>
/// <remarks>
/// The names are what the runner takes (<c>opus</c>, <c>high</c>) and are kept
/// for good: a run records them, and what it wrote is marked with them. The
/// labels name the models the runner runs by default; which identifier of
/// Anthropic's each runs as is the runner's (<c>AGENT_MODEL</c> and the
/// like), which its health says and the panel shows. A model the runner
/// names that is not known here is offered by its name.
/// </remarks>
internal static class RunnerCatalog
{

    public const string Model = "model";

    public const string Effort = "effort";

    private static readonly (string Name, string Label, bool Guarded)[] Models =
    [
        ("opus", "Opus 5.5", false),
        ("sonnet", "Sonnet 5.5", false),
        ("haiku", "Haiku 5.5", false),
        ("fable", "Fable 5.2", true)
    ];

    private static readonly (string Name, string Label)[] Efforts =
    [
        ("low", "Low"),
        ("medium", "Medium"),
        ("high", "High"),
        ("xhigh", "Extra high"),
        ("max", "Max")
    ];

    /// <summary>
    /// The options, as a runner offers them that says which models and
    /// efforts it has - or, where it has not said, as every runner has them.
    /// </summary>
    public static IReadOnlyList<AgentOption> Options(IReadOnlyList<string>? models, IReadOnlyList<string>? efforts)
    {
        var offeredModels = (models is { Count: > 0 } ? models : Models.Select(m => m.Name))
            .Select(name => Models.FirstOrDefault(m => m.Name == name) is { Name: not null } known
                ? new AgentOptionValue(known.Name, known.Label, Guarded: known.Guarded)
                : new AgentOptionValue(name, name))
            .ToList();

        var offeredEfforts = (efforts is { Count: > 0 } ? efforts : Efforts.Select(e => e.Name))
            .Select(name => new AgentOptionValue(name, Efforts.FirstOrDefault(e => e.Name == name).Label ?? name))
            .ToList();

        return
        [
            new AgentOption(Model, "Model", offeredModels, offeredModels.Any(m => m.Name == "opus") ? "opus" : offeredModels[0].Name),
            new AgentOption(Effort, "Effort", offeredEfforts, offeredEfforts.Any(e => e.Name == "high") ? "high" : offeredEfforts[0].Name)
        ];
    }

}
