namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// A short line the agent wrote for the person as it works, in their
/// language: what it is about to do for them.
/// </summary>
/// <param name="Text">What it said; the platform clips a long one</param>
public sealed record AgentSaid(long Sequence, DateTimeOffset At, string Text) : AgentEvent(Sequence, At);
