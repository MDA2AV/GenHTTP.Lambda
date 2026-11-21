using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

/// <remarks>
/// Counted in memory since the server last started, which is
/// <paramref name="Since"/>: a restart begins these again.
/// </remarks>
[Summary("How much a lambda is used, and how well it answers.")]
public sealed record TrafficSummary(
    [Summary("Requests in the last hour")]
    long HourRequests,
    [Summary("Of those, answered with a server error")]
    long HourFailed,
    [Summary("Requests in the last day")]
    long DayRequests,
    [Summary("Of those, answered with a server error")]
    long DayFailed,
    [Summary("Of those, answered with a client error, not found included")]
    long DayRejected,
    [Summary("The mean time to answer over the last day, in milliseconds")]
    double AverageMillis,
    [Summary("Connections upgraded to a websocket in the last day")]
    long Upgrades,
    [Summary("Requests per hour over the last day, oldest first")]
    IReadOnlyList<int> Hourly,
    [Summary("When the last request arrived")]
    DateTime? LastSeen,
    [Summary("When the counting started: when the server came up")]
    DateTime Since
);
