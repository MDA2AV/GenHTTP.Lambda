using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Building;

[Summary("A build the agent has taken on.")]
public sealed record BuildStarted(
    [Summary("What to follow it by: `GET /builds/{id}`")]
    string Id,
    [Summary("How many builds are waiting ahead of it")]
    int Queued,
    [Summary("How many builds and changes this caller has left today, this one counted")]
    int? Left = null,
    [Summary("The model it runs on, by its name in `build.offer.models` of `GET /system`")]
    string? Model = null,
    [Summary("The effort it runs at, by its name in `build.offer.efforts` of `GET /system`")]
    string? Effort = null
);
