using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("A build the agent has taken on.")]
public sealed record BuildStarted(
    [Summary("What to follow it by: `GET /builds/{id}`")]
    string Id,
    [Summary("How many builds are waiting ahead of it")]
    int Queued,
    [Summary("How many builds and changes this caller has left today, this one counted")]
    int? Left = null,
    [Summary("The value of the option `model` it runs on, where the agent has one")]
    string? Model = null,
    [Summary("The value of the option `effort` it runs at, where the agent has one")]
    string? Effort = null,
    [Summary("The value of each option it runs on, by option, as `build.offer.options` of `GET /system` names them")]
    IReadOnlyDictionary<string, string>? Options = null
);
