using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Telemetry;

[Summary("How often a lambda was reached through one of its addresses.")]
public sealed record EntranceTraffic(
    [Summary("The host it was reached at - its subdomain of the hosting domain, or its own domain")]
    string? Domain,
    [Summary("Requests and websocket upgrades alike - every time somebody came in this way")]
    long Requests
);
