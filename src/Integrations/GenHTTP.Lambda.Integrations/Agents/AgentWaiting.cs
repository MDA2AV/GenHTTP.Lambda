namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// The run has begun but is held up, and goes on by itself once that passes:
/// what follows it, an <see cref="AgentStarted"/> again or the end.
/// </summary>
/// <param name="Reason">What holds it up</param>
/// <param name="Until">When it expects to go on, where it knows</param>
/// <param name="Detail">What the system behind it said, in its words</param>
public sealed record AgentWaiting(long Sequence, DateTimeOffset At, AgentWaitReason Reason, DateTimeOffset? Until = null, string? Detail = null)
    : AgentEvent(Sequence, At);
