namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// A run, as the platform asks after it: by its id, and by what the agent
/// system answered it was taken as.
/// </summary>
/// <param name="Id">The platform's id (<see cref="AgentRun.Id"/>)</param>
/// <param name="Reference">What the agent system knows it by (<see cref="AgentAcceptance.Reference"/>), where it said</param>
public sealed record AgentRunHandle(string Id, string? Reference = null);
