using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

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
