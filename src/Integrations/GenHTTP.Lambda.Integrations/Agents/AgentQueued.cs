namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// The run waits for its turn at the agent system, which works on others.
/// </summary>
/// <param name="Position">Its place in the queue, one being next</param>
public sealed record AgentQueued(long Sequence, DateTimeOffset At, int Position) : AgentEvent(Sequence, At);
