using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// What has been happening, counted by day.
/// </summary>
/// <remarks>
/// Reads the append-only events table, which is the only place that can answer
/// this: the lambdas table holds the present and overwrites it, so it knows how
/// many are deployed but not how many were deployed on Tuesday.
/// </remarks>
public sealed class EventReader(IDbContextFactory<LambdaDbContext> databases)
{

    /// <summary>
    /// A day-by-day count of each kind of event, oldest first.
    /// </summary>
    /// <param name="days">How far back to go</param>
    public EventHistory History(int days)
    {
        var span = Math.Clamp(days, 1, 365);

        // from midnight, so the first and last buckets are whole days rather
        // than whatever fraction the clock happens to be at
        var from = DateTime.UtcNow.Date.AddDays(-(span - 1));

        using var database = databases.CreateDbContext();

        var rows = database.Events
                           .Where(e => e.Occurred >= from)
                           .Select(e => new { e.Kind, e.Occurred })
                           .ToList();

        var totals = database.Events
                             .GroupBy(e => e.Kind)
                             .Select(g => new { Kind = g.Key, Count = g.Count() })
                             .ToList();

        var buckets = new Dictionary<string, int[]>();

        foreach (var kind in LambdaEvents.All)
        {
            buckets[kind] = new int[span];
        }

        foreach (var row in rows)
        {
            if (!buckets.TryGetValue(row.Kind, out var bucket))
            {
                continue;
            }

            var day = (int)(row.Occurred.Date - from).TotalDays;

            if (day >= 0 && day < span)
            {
                bucket[day]++;
            }
        }

        var labels = new List<string>(span);

        for (var i = 0; i < span; i++)
        {
            labels.Add(from.AddDays(i).ToString("yyyy-MM-dd"));
        }

        return new EventHistory(
            labels,
            [.. LambdaEvents.All.Select(kind => new EventSeries(
                kind,
                buckets[kind],
                totals.FirstOrDefault(t => t.Kind == kind)?.Count ?? 0))]
        );
    }

}

/// <summary>Counts per day, one series per kind of event.</summary>
[Summary("What happened on the platform per day, one series per kind of event.")]
public sealed record EventHistory(
    [Summary("The days covered, as `yyyy-MM-dd` in UTC, oldest first")]
    IReadOnlyList<string> Days,
    [Summary("One series per kind of event, its counts in the order of `days`")]
    IReadOnlyList<EventSeries> Series);

[Summary("How often one kind of event happened, day by day.")]
public sealed record EventSeries(
    [Summary("`created`, `saved`, `deployed`, `undeployed` or `deleted` - of lambdas")]
    string Kind,
    [Summary("How many happened on each day of the window")]
    IReadOnlyList<int> Counts,
    [Summary("How many have ever happened, beyond the window too")]
    int Total);
