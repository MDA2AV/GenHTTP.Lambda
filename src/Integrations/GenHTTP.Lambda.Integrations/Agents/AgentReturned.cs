namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// The agent hands the person a file - a document it wrote, a picture it made.
/// </summary>
/// <remarks>
/// The platform keeps it with the run and offers it to the person to look at
/// or download, never as a page or a script of its own address. Only told
/// where <see cref="AgentCapabilities.ReturnsFiles"/> says so.
/// </remarks>
public sealed record AgentReturned(long Sequence, DateTimeOffset At, AgentFile File) : AgentEvent(Sequence, At);
