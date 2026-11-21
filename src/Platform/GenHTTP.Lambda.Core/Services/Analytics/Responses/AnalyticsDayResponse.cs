using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Analytics;

[Summary("What was counted on one day.")]
public sealed record AnalyticsDayResponse(
    [Summary("The day, in UTC")]
    DateOnly Date,
    [Summary("What was counted on it")]
    AnalyticsTotalsResponse Totals
);
