using GenHTTP.Api.Content.Services;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// What one lambda has been doing.
/// </summary>
/// <remarks>
/// Counters and a rolling average rather than a series per lambda: the number
/// of lambdas is not bounded by anything this process controls, and a ring
/// buffer each would grow with them.
/// </remarks>
[Summary("What one lambda has been doing since the server came up, in totals.")]
public sealed record LambdaActivity(
    [Summary("The lambda, by its public key")]
    string PublicKey,
    [Summary("Requests it answered")]
    long Requests,
    [Summary("Of those, answered with a server error")]
    long Failed,
    [Summary("Connections upgraded to a websocket")]
    long Upgrades,
    [Summary("The mean time to answer, in milliseconds")]
    double AverageMillis,
    [Summary("The longest time to answer, in milliseconds")]
    double SlowestMillis,
    [Summary("What the responses carried, counting only those that said how much - a floor rather than a total")]
    long BytesOut,
    [Summary("When the first request arrived")]
    DateTime? FirstSeen,
    [Summary("When the last request arrived")]
    DateTime? LastSeen
);
