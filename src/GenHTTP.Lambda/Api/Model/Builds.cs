namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// What the browser sends: one sentence, and nothing else that identifies
/// anything already made.
/// </summary>
/// <remarks>
/// There is deliberately no way to name an existing lambda here. This endpoint
/// creates, and only creates; changing something that exists belongs to the
/// editor, which already holds its key, or to an agent over MCP. Accepting a
/// key here made one door do two jobs and blurred what the box is for.
/// </remarks>
public sealed record BuildRequest(string? Prompt, string? Model = null, string? Password = null);

/// <summary>
/// Whether this installation builds things, and how often it lets one caller.
/// </summary>
/// <param name="SecondModel">
/// Whether a second model is on offer. Said, but never which one: the page
/// needs to know whether to offer the choice, not what the answer is.
/// </param>
public sealed record BuildAvailability(bool Available, int PerDay, bool SecondModel);
