namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// What the run has spent so far, as the service behind the agent counts it.
/// </summary>
/// <remarks>
/// Told as often as the agent system likes, each time the whole of it so
/// far: the platform keeps the last one. It is recorded with the run for the
/// operator; how many runs one person may ask for is still the platform's.
/// </remarks>
public sealed record AgentSpent(long Sequence, DateTimeOffset At, AgentUsage Usage) : AgentEvent(Sequence, At);
