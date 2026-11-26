namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// Something the agent does beyond the platform, which only the agent system
/// sees: reading a web page, searching the web, a tool of its own.
/// </summary>
/// <remarks>
/// Told once as it begins and again, by the same <see cref="Id"/>, once it is
/// done. What it does on the platform is the platform's to record and is not
/// told here.
/// </remarks>
/// <param name="Id">What tells the beginning and the end of the same activity apart from others</param>
/// <param name="Kind">What it is</param>
/// <param name="Subject">What it is about, as a page may show it: the site of a page read, by its host name and nothing else of its address; the name of a tool of its own</param>
/// <param name="Done">Whether it is done</param>
/// <param name="Failed">Whether it did not work, once it is done</param>
public sealed record AgentActivity(long Sequence, DateTimeOffset At, string Id, AgentActivityKind Kind, string? Subject = null, bool Done = false,
                                   bool Failed = false) : AgentEvent(Sequence, At);
