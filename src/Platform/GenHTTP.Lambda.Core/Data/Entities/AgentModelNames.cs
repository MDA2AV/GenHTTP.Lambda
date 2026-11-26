namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What the models and efforts stored before the options of the agent
/// integration were called: the values of its <c>model</c> and <c>effort</c>
/// options they are read as.
/// </summary>
/// <remarks>
/// So a version the build agent wrote on Opus at high before an upgrade is
/// marked and counted as one written on <c>model=opus</c>, <c>effort=high</c>
/// after it, beside those written since.
/// </remarks>
public static class AgentModelNames
{

    /// <summary>The option the model was, as the agent integration of the platform's own names it.</summary>
    public const string ModelOption = "model";

    /// <summary>The option the effort was.</summary>
    public const string EffortOption = "effort";

    public static string? Name(AgentModel? model) => model switch
    {
        AgentModel.Opus => "opus",
        AgentModel.Sonnet => "sonnet",
        AgentModel.Haiku => "haiku",
        AgentModel.Fable => "fable",
        _ => null
    };

    public static string? Name(AgentEffort? effort) => effort switch
    {
        AgentEffort.Low => "low",
        AgentEffort.Medium => "medium",
        AgentEffort.High => "high",
        AgentEffort.ExtraHigh => "xhigh",
        AgentEffort.Max => "max",
        _ => null
    };

    /// <summary>
    /// The options a model and an effort stored in the old columns stand for,
    /// or none where neither was stored.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Options(AgentModel? model, AgentEffort? effort)
    {
        var options = new SortedDictionary<string, string>(StringComparer.Ordinal);

        if (Name(model) is { } named)
        {
            options[ModelOption] = named;
        }

        if (Name(effort) is { } level)
        {
            options[EffortOption] = level;
        }

        return options;
    }

}
