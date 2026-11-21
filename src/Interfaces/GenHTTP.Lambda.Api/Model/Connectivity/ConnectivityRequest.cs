using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Connectivity;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Asks for a lambda to connect to other servers.")]
public sealed record ConnectivityRequest(
    [Summary("What it connects to and why, for the operator who decides: the addresses or services, and what the lambda does with them - `api.open-meteo.com, for the forecast on the front page`. Required, up to 1000 characters.")]
    string? Purpose
);
