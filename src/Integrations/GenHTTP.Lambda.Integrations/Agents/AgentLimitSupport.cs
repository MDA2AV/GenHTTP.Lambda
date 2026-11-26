namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// Which limits an agent holds a run to (<see cref="AgentLimits"/>), so the
/// operator is offered only those it keeps, and what it holds one to unless
/// it is told otherwise.
/// </summary>
/// <param name="Time">Whether it stops a run once its time is up</param>
/// <param name="Steps">Whether it stops a run once it took as many steps as it may</param>
/// <param name="Default">What it holds a run to where the operator decided nothing</param>
public sealed record AgentLimitSupport(bool Time, bool Steps, AgentLimits Default);
