namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// How the agent system took a run.
/// </summary>
/// <param name="Position">Its place in the agent system's queue, one being next; zero where it works on it already</param>
/// <param name="Reference">What the agent system knows the run by, where that is not the platform's id: the platform keeps it with the run and hands it back (<see cref="AgentRunHandle"/>)</param>
public sealed record AgentAcceptance(int Position, string? Reference = null);
