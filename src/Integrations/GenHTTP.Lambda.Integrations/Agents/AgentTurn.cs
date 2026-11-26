namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// One earlier message of the conversation, and what the agent answered.
/// </summary>
/// <param name="Message">What was asked, as it was asked</param>
/// <param name="Reply">What the agent answered - or asked back - where it did</param>
/// <param name="Attachments">The names of the files it came with</param>
public sealed record AgentTurn(string Message, string? Reply = null, IReadOnlyList<string>? Attachments = null);
