using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What every lambda has been doing since the server came up.")]
public sealed record ActivityResponse(
    [Summary("Every lambda that was called")]
    IReadOnlyList<LambdaActivity> Lambdas,
    [Summary("Their requests, together")]
    long Requests,
    [Summary("Their websocket upgrades, together")]
    long Upgrades
);
