using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// What one lambda has been doing, in enough detail for its owner to read a
/// trend rather than a total.
/// </summary>
[Summary("What one lambda has been doing since the server came up.")]
[Remarks("Counted in memory, so a restart of the server starts every figure again from nothing (`since`).")]
public sealed record LambdaTraffic(
    [Summary("Everything since the server came up; absent while nobody has called it")]
    LambdaActivity? Totals,
    [Summary("The last hour, a minute at a time, oldest first")]
    IReadOnlyList<TrafficPoint> Minutes,
    [Summary("The last day, a quarter of an hour at a time, oldest first")]
    IReadOnlyList<TrafficPoint> Quarters,
    [Summary("How the requests were answered, by the class of their status")]
    StatusClasses Statuses,
    [Summary("What was asked for, the busiest paths first")]
    IReadOnlyList<PathTraffic> Paths,
    [Summary("Where it was reached - its subdomain of the hosting domain, or its own domain - the busiest first")]
    IReadOnlyList<EntranceTraffic> Entrances,
    [Summary("When the counting started, which is when the server came up")]
    DateTime Since
);
