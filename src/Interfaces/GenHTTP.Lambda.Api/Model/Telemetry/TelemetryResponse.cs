using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The state of the server, and the readings taken while it ran.")]
public sealed record TelemetryResponse(
    [Summary("What is running, and on what")]
    ServerDescription Server,
    [Summary("What it answered since it came up")]
    TrafficDescription Traffic,
    [Summary("What the platform holds")]
    PlatformDescription Platform,
    [Summary("The newest reading")]
    TelemetrySample Latest,
    [Summary("How often a reading is taken, in seconds")]
    int IntervalSeconds,
    [Summary("The readings of the window asked for, oldest first")]
    IReadOnlyList<TelemetrySample> Samples,
    [Summary("What happened on the platform per day")]
    EventHistory Events
);
