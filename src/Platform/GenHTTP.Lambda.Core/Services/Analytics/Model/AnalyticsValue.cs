namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// One value of a dimension and how often it came up.
/// </summary>
/// <param name="Value">The path, the host, the country code or the name</param>
/// <param name="Hits">Requests it was part of</param>
/// <param name="Views">Pages opened it was part of</param>
/// <param name="BotViews">Of those, the pages programs opened rather than people</param>
public sealed record AnalyticsValue(string Value, long Hits, long Views, long BotViews);
