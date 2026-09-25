namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// What one lambda has been doing, in enough detail for its owner to read a
/// trend rather than a total.
/// </summary>
/// <param name="Totals">Everything since the server came up</param>
/// <param name="Minutes">The last hour, a minute at a time, oldest first</param>
/// <param name="Quarters">The last day, fifteen minutes at a time, oldest first</param>
/// <param name="Statuses">How the requests were answered, by class</param>
/// <param name="Paths">What was asked for, busiest first</param>
/// <param name="Entrances">Where it was reached - its path on the platform, or its own domain - busiest first</param>
/// <param name="Since">When the counting started, which is when the server came up</param>
public sealed record LambdaTraffic(
    LambdaActivity? Totals,
    IReadOnlyList<TrafficPoint> Minutes,
    IReadOnlyList<TrafficPoint> Quarters,
    StatusClasses Statuses,
    IReadOnlyList<PathTraffic> Paths,
    IReadOnlyList<EntranceTraffic> Entrances,
    DateTime Since
);

/// <summary>
/// One interval of traffic.
/// </summary>
/// <param name="At">When the interval started</param>
/// <param name="Failed">Answered with a server error</param>
/// <param name="Rejected">Answered with a client error, which includes not found</param>
/// <param name="AverageMillis">The mean time to answer, over the requests of the interval</param>
public sealed record TrafficPoint(DateTime At, int Requests, int Failed, int Rejected, int Upgrades, double AverageMillis, long Bytes);

/// <summary>
/// Requests by the class of status they were answered with.
/// </summary>
public sealed record StatusClasses(long Success, long Redirect, long ClientError, long ServerError);

/// <summary>
/// One path of a lambda and what it was asked.
/// </summary>
/// <param name="Path">Relative to the lambda, so the same wherever it is hosted</param>
public sealed record PathTraffic(string Path, long Requests, long Failed, double AverageMillis);

/// <summary>
/// How often a lambda was reached through one of the ways it can be reached.
/// </summary>
/// <param name="Domain">The lambda's own domain, or nothing for its path on the platform</param>
/// <param name="Requests">Requests and upgrades alike - every time somebody came in this way</param>
public sealed record EntranceTraffic(string? Domain, long Requests);
