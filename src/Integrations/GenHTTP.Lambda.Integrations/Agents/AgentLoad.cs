namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// How an agent system is, as the operator's panel shows it.
/// </summary>
/// <remarks>
/// Of the runs it has, only this installation's are counted: an agent system
/// shared with other installations says how many runs it takes at once, never
/// what the others ask of it.
/// </remarks>
/// <param name="Reachable">Whether it answered</param>
/// <param name="Capacity">How many runs it works on at once, every installation's together, where it says</param>
/// <param name="Running">How many of this installation's runs it works on now</param>
/// <param name="Queued">How many of this installation's runs wait for their turn</param>
/// <param name="Facts">What else it says of itself, as label and value - what it signs in with, the version it runs - never a credential</param>
public sealed record AgentLoad(bool Reachable, int? Capacity, int Running, int Queued, IReadOnlyList<IntegrationDetail> Facts)
{

    /// <summary>
    /// An agent system that did not answer.
    /// </summary>
    public static readonly AgentLoad Unreachable = new(false, null, 0, 0, []);

}
