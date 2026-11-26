namespace GenHTTP.Lambda.Services.UsageReports;

/// <param name="Runs">How many runs ran on a value</param>
/// <param name="Seconds">How long they worked together, in seconds</param>
public sealed record UsageRunTime(int Runs, long Seconds);
