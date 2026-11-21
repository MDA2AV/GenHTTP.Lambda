using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Telemetry;

[Summary("Requests by the class of status they were answered with.")]
public sealed record StatusClasses(
    [Summary("2xx")]
    long Success,
    [Summary("3xx")]
    long Redirect,
    [Summary("4xx, not found included")]
    long ClientError,
    [Summary("5xx")]
    long ServerError
);
