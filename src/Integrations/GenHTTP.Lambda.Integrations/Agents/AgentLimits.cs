namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// What one run may take, as the operator decided it for the options it runs
/// on.
/// </summary>
/// <remarks>
/// An agent holds a run to those it supports (<see cref="AgentLimitSupport"/>)
/// and tells the agent what it has, so it aims at something that works within
/// them. A run without a limit is told to take the time it needs.
/// </remarks>
/// <param name="Time">How long it may work, or nothing for as long as it takes</param>
/// <param name="Steps">How many steps it may take, or nothing for as many as it takes</param>
public sealed record AgentLimits(TimeSpan? Time, int? Steps)
{

    /// <summary>
    /// Whether nothing limits the run.
    /// </summary>
    public bool Unbounded => Time == null && Steps == null;

}
