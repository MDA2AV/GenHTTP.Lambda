namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// An agent system did not take a run - and nothing of it was spent.
/// </summary>
/// <param name="reason">Why</param>
/// <param name="message">What it said, in English, for the person who asked</param>
public sealed class AgentRefusedException(AgentRefusal reason, string message, Exception? inner = null) : Exception(message, inner)
{

    public AgentRefusal Reason { get; } = reason;

}
