namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// The run a run carries on from: the agent keeps nothing, so what the earlier
/// one left open is said here.
/// </summary>
/// <param name="Kind">Why it carries on</param>
/// <param name="Previous">The id of the run it carries on from</param>
/// <param name="Asked">The message the earlier run was asked</param>
/// <param name="Question">The question the earlier run ended with, for an answer</param>
/// <param name="Done">What the platform's records say the earlier run did, in English, as facts</param>
public sealed record AgentContinuation(AgentContinuationKind Kind, string Previous, string Asked, string? Question = null, string? Done = null);
