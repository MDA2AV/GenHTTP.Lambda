using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("How often one value of an option ran, and for how long.")]
public sealed record ValueUsage(
    [Summary("The value, by name")]
    string Value,
    [Summary("How many runs ran on it")]
    int Runs,
    [Summary("How long they worked, together, in seconds")]
    long Seconds
);
