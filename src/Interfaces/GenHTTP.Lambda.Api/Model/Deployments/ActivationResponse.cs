using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("One stretch of time a version was online.")]
public sealed record ActivationResponse(
    [Summary("The version that was online")]
    int Version,
    [Summary("When it went online")]
    DateTime Started,
    [Summary("Who put it online: `api`, `agent`, `git`, `admin` (the operator) or `system`")]
    string? Origin,
    [Summary("When it went offline; absent while it is still online")]
    DateTime? Ended,
    [Summary("Why it went offline: `replaced` by another deployment, `stopped` by its owner, `expired` because nobody used it, or taken down by the operator (`admin`)")]
    string? EndedBy,
    [Summary("How long it was online, or has been so far, in seconds")]
    long Seconds
);
