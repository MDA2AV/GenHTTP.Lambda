namespace GenHTTP.Lambda.Services.UsageReports;

/// <summary>
/// How the installation's agent was used: counts and the time its runs took,
/// never what anybody asked, attached or got back.
/// </summary>
/// <param name="Count">How many runs there were</param>
/// <param name="Builds">How many of them built a new app; the rest were messages about one</param>
/// <param name="Seconds">How long they worked together, in seconds</param>
/// <param name="Options">How many ran, and for how long, on each value of each option, by option and value name</param>
public sealed record UsageRuns(int Count, int Builds, long Seconds, IReadOnlyDictionary<string, IReadOnlyDictionary<string, UsageRunTime>> Options);
