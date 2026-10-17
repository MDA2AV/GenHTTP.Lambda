using GenHTTP.Api.Content.Services;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// How an error is reported to the single page application.
/// </summary>
[Summary("Why a request was refused.")]
[Remarks("A request refused before it reached the platform's own checks - a body that is not valid JSON, a build agent that cannot be reached - carries `status` and `message` only.")]
public sealed record ErrorResponse(
    [Summary("The status code it was answered with")]
    int Status,
    [Summary("What kind of refusal it is: `Invalid` (400), `Forbidden` (403), `NotFound` (404), `Conflict` (409) or `TooMany` (429)")]
    string Error,
    [Summary("What went wrong, written for a person or an agent to act on - often with what to do instead")]
    string Message
);
