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
    public async ValueTask<EventHistory> HistoryAsync(int days, CancellationToken cancellation = default)
    {
        var span = Math.Clamp(days, 1, 365);

        // from midnight, so the first and last buckets are whole days rather
        // than whatever fraction the clock happens to be at
        var from = DateTime.UtcNow.Date.AddDays(-(span - 1));

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var rows = await database.Events
                                 .Where(e => e.Occurred >= from)
                                 .Select(e => new { e.Kind, e.Occurred })
                                 .ToListAsync(cancellation);

        var totals = await database.Events
                                   .GroupBy(e => e.Kind)
                                   .Select(g => new { Kind = g.Key, Count = g.Count() })
                                   .ToListAsync(cancellation);

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

        var days_ = new List<string>(span);

        for (var i = 0; i < span; i++)
        {
            days_.Add(from.AddDays(i).ToString("yyyy-MM-dd"));
        }

        return new EventHistory(
            days_,
            [.. LambdaEvents.All.Select(kind => new EventSeries(
                kind,
                buckets[kind],
                totals.FirstOrDefault(t => t.Kind == kind)?.Count ?? 0))]
        );
    }

}

/// <summary>Counts per day, one series per kind of event.</summary>
/// <param name="Days">The days covered, as yyyy-MM-dd, oldest first</param>
/// <param name="Series">One entry per kind, aligned to Days</param>
public sealed record EventHistory(IReadOnlyList<string> Days, IReadOnlyList<EventSeries> Series);

/// <param name="Kind">created, saved, deployed, undeployed or deleted</param>
/// <param name="Counts">How many happened on each day of the window</param>
/// <param name="Total">How many have ever happened, beyond the window</param>
public sealed record EventSeries(string Kind, IReadOnlyList<int> Counts, int Total);
