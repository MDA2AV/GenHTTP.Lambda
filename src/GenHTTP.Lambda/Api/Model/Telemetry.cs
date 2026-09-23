using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// What is running, and on what.
/// </summary>
public sealed record ServerDescription(
    string Engine,
    string Version,
    string Runtime,
    string Platform,
    bool ServerGarbageCollection,
    int Processors,
    DateTime Started,
    long UptimeSeconds
);

/// <summary>
/// What the server has answered since it came up.
/// </summary>
public sealed record TrafficDescription(long Requests, long Failed, long Upgrades, int OpenSockets);

/// <summary>
/// What the platform is holding.
/// </summary>
public sealed record PlatformDescription(int Lambdas, int Deployed, int Versions);

/// <summary>
/// The telemetry page in one document: what is running, what it has done, and
/// the readings taken while it did.
/// </summary>
public sealed record TelemetryResponse(
    ServerDescription Server,
    TrafficDescription Traffic,
    PlatformDescription Platform,
    TelemetrySample Latest,
    int IntervalSeconds,
    IReadOnlyList<TelemetrySample> Samples,
    EventHistory Events
);

/// <summary>
/// What every lambda has been doing since the server came up.
/// </summary>
public sealed record ActivityResponse(IReadOnlyList<LambdaActivity> Lambdas, long Requests, long Upgrades);
