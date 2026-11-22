using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Telemetry;

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
