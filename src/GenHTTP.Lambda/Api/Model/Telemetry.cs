using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What is running, and on what.")]
public sealed record ServerDescription(
    [Summary("The engine serving the requests: `ioxide` or `kestrel`")]
    string Engine,
    [Summary("The version of GenHTTP")]
    string Version,
    [Summary("The .NET runtime")]
    string Runtime,
    [Summary("The platform the runtime runs on: `linux-x64`, say")]
    string Platform,
    [Summary("Whether the server garbage collector is used")]
    bool ServerGarbageCollection,
    [Summary("How many processors the process may use")]
    int Processors,
    [Summary("When the server came up")]
    DateTime Started,
    [Summary("How long it has been up, in seconds")]
    long UptimeSeconds
);

[Summary("What the server has answered since it came up.")]
public sealed record TrafficDescription(
    [Summary("Requests answered")]
    long Requests,
    [Summary("Of those, answered with a server error")]
    long Failed,
    [Summary("Websocket upgrades")]
    long Upgrades,
    [Summary("Connections open right now")]
    int OpenSockets
);

[Summary("What the platform holds.")]
public sealed record PlatformDescription(
    [Summary("Lambdas there are")]
    int Lambdas,
    [Summary("Of those, online")]
    int Deployed,
    [Summary("Versions kept, of all lambdas")]
    int Versions
);

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

[Summary("What every lambda has been doing since the server came up.")]
public sealed record ActivityResponse(
    [Summary("Every lambda that was called")]
    IReadOnlyList<LambdaActivity> Lambdas,
    [Summary("Their requests, together")]
    long Requests,
    [Summary("Their websocket upgrades, together")]
    long Upgrades
);
