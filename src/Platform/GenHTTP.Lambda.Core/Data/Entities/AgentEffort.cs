namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// The efforts the build agent ran at before what a run is chosen on became
/// the options of the agent integration.
/// </summary>
/// <remarks>
/// Kept to read what was stored then, as <see cref="AgentModel"/> is; named
/// by <see cref="AgentModelNames"/>.
/// </remarks>
public enum AgentEffort
{
    Low = 0,
    Medium = 1,
    High = 2,
    ExtraHigh = 3,
    Max = 4
}
