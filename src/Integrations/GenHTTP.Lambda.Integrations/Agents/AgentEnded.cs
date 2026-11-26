namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// How a run ended. Nothing follows it.
/// </summary>
/// <remarks>
/// It says how the agent's work ended, not what came of it: what the run
/// saved, put online or changed is what the platform's records say, so a run
/// that ended in an error with a draft half done shows the draft, and one
/// that said it put something online when it did not shows that it did not.
/// </remarks>
/// <param name="Ending">How it ended</param>
/// <param name="Message">What the agent wrote for the person at the end, in their language: its closing note, the answer to their question, the question it asks them back, or why it declined</param>
/// <param name="Detail">What went wrong, in the words of whatever it went wrong in - for the operator and for a page with no words of its own</param>
public sealed record AgentEnded(long Sequence, DateTimeOffset At, AgentEnding Ending, string? Message = null, string? Detail = null)
    : AgentEvent(Sequence, At);
