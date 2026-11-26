namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// Something that happened in a run, as the agent system tells it.
/// </summary>
/// <remarks>
/// <para>
/// What the agent does on the platform is not told here: the platform sees
/// every call that carries the run's credential and records it itself, in
/// words of its own that every page can say in its visitor's language. What
/// is told here is what only the agent system knows - whether the run waits
/// or works, what the agent said to the person, what it did beyond the
/// platform, what it handed back, what it spent, and how it ended.
/// </para>
/// <para>
/// The kinds are a closed set, so that a page can say every one of them in
/// its own language, whichever integration told it; the only prose is what
/// the agent itself wrote for the person (<see cref="AgentSaid"/>, the
/// message of <see cref="AgentEnded"/>), which it writes in their language.
/// </para>
/// </remarks>
/// <param name="Sequence">Its position in the run, increasing: what a platform asks again from</param>
/// <param name="At">When it happened</param>
public abstract record AgentEvent(long Sequence, DateTimeOffset At);
