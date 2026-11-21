namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// How much the agent thinks and how many steps it takes over a job - the
/// effort the model is asked for.
/// </summary>
/// <remarks>
/// Stored as its number with what the agent wrote, as the model is; named by
/// <see cref="AgentCatalog"/>.
/// </remarks>
public enum AgentEffort
{
    Low = 0,
    Medium = 1,
    High = 2,
    ExtraHigh = 3,
    Max = 4
}
