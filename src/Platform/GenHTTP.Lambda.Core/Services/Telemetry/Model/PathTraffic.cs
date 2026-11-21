using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Telemetry;

[Summary("One path of a lambda, and how it was answered.")]
public sealed record PathTraffic(
    [Summary("The path, relative to the lambda - the same wherever it is reached")]
    string Path,
    [Summary("How often it was asked for")]
    long Requests,
    [Summary("Of those, answered with a server error")]
    long Failed,
    [Summary("The mean time to answer it, in milliseconds")]
    double AverageMillis
);
