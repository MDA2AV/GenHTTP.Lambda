using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Analytics;

[Summary("One value visits are counted by, and how often it came up.")]
public sealed record AnalyticsValueResponse(
    [Summary("The path, the host, the country code or the name; `(other)` gathers what did not fit into a day's list")]
    string Value,
    [Summary("Requests it was part of")]
    long Hits,
    [Summary("Pages opened it was part of")]
    long Views,
    [Summary("Of those, the pages programs opened rather than people")]
    long BotViews
);
