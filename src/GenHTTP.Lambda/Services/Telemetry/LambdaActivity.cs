namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// What one lambda has been doing.
/// </summary>
/// <remarks>
/// Counters and a rolling average rather than a series per lambda: the number
/// of lambdas is not bounded by anything this process controls, and a ring
/// buffer each would grow with them.
/// </remarks>
/// <param name="BytesOut">
/// What the responses carried, counting only the ones that declared a length.
/// A response streamed without one adds nothing here, so this is a floor rather
/// than a total.
/// </param>
public sealed record LambdaActivity(
    string PublicKey,
    long Requests,
    long Failed,
    long Upgrades,
    double AverageMillis,
    double SlowestMillis,
    long BytesOut,
    DateTime? FirstSeen,
    DateTime? LastSeen
);
