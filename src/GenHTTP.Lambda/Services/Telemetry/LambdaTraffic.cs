using GenHTTP.Api.Content.Services;

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

[Summary("The traffic of one interval.")]
public sealed record TrafficPoint(
    [Summary("When the interval started")]
    DateTime At,
    [Summary("Requests answered in it")]
    int Requests,
    [Summary("Of those, answered with a server error (5xx)")]
    int Failed,
    [Summary("Of those, answered with a client error (4xx), not found included")]
    int Rejected,
    [Summary("Connections upgraded to a websocket")]
    int Upgrades,
    [Summary("The mean time to answer, over its requests, in milliseconds")]
    double AverageMillis,
    [Summary("What the responses carried, where they said how much")]
    long Bytes
);

[Summary("Requests by the class of status they were answered with.")]
public sealed record StatusClasses(
    [Summary("2xx")]
    long Success,
    [Summary("3xx")]
    long Redirect,
    [Summary("4xx, not found included")]
    long ClientError,
    [Summary("5xx")]
    long ServerError
);

[Summary("One path of a lambda, and how it was answered.")]
public sealed record PathTraffic(
    [Summary("The path, relative to the lambda - the same wherever it is reached")]
    string Path,
    [Summary("How often it was asked for")]
    long Requests,
    [Summary("Of those, answered with a server error")]
    long Failed,
    [Summary("The mean time to answer it, in milliseconds")]
    double AverageMillis
);

[Summary("How often a lambda was reached through one of its addresses.")]
public sealed record EntranceTraffic(
    [Summary("The host it was reached at - its subdomain of the hosting domain, or its own domain")]
    string? Domain,
    [Summary("Requests and websocket upgrades alike - every time somebody came in this way")]
    long Requests
);
