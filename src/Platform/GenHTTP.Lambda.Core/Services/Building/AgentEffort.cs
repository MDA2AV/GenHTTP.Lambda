namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// How much the agent thinks and how many steps it takes over a job - the
/// effort the model is asked for.
/// </summary>
public enum AgentEffort
{
    Low,
    Medium,
    High,
    ExtraHigh,
    Max
}
