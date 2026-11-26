namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// The agent works on the run - first, or again after it waited.
/// </summary>
/// <param name="Limits">What it holds the run to, where it holds it to less than it was asked or to nothing it was asked</param>
public sealed record AgentStarted(long Sequence, DateTimeOffset At, AgentLimits? Limits = null) : AgentEvent(Sequence, At);
