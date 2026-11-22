using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// How a lambda's visitors reach it, counted by day for its owner where the
/// owner asked for it.
/// </summary>
/// <remarks>
/// The switch is kept with the lambda in the platform's database, where
/// serving a request reads it from memory (<c>ResolvedLambda.AnalyticsSince</c>);
/// the counts are kept in the analytics database (<see cref="AnalyticsStore"/>).
/// A request puts its visit on a queue (<see cref="AnalyticsConcern"/>) and
/// this service writes the queue down when the job asks it to - so the
/// requests never wait for a database, and the database is written once every
/// few minutes rather than once per request.
/// </remarks>
public sealed class AnalyticsService(IDbContextFactory<LambdaDbContext> databases, AnalyticsStore store, AnalyticsQueue queue, DailyVisitors visitors,
                                     VisitorClassifier classifier, CrawlerRanges ranges, ILimitsService limits, LambdaOptions options, ILambdaAddresses addresses,
                                     ILogger<AnalyticsService> logger) : IAnalyticsService
{
    private const string Missing = "This lambda does not exist (or has been deleted).";

    /// <summary>
    /// How many values of a dimension an answer lists at most.
    /// </summary>
    private const int Listed = 50;

    /// <summary>
    /// Held while visits are written down and while a lambda's counts are
    /// deleted, so a writing that started before analytics were switched off
    /// cannot put back what switching them off removed.
    /// </summary>
    private readonly SemaphoreSlim _turn = new(1, 1);

    private DateTime? _updated;

    private DateOnly _expired;

    #region Owner

    public async ValueTask<LambdaAnalytics> GetAsync(string privateKey, int? days = null, DateTime? now = null, CancellationToken cancellation = default)
    {
        LambdaEntity lambda;

        await using (var database = await databases.CreateDbContextAsync(cancellation))
        {
            lambda = await database.Lambdas.AsNoTracking().FirstOrDefaultAsync(l => l.PrivateKey == privateKey, cancellation)
                  ?? throw LambdaException.NotFound(Missing);
        }

        return await DescribeAsync(lambda, days, now ?? DateTime.UtcNow, cancellation);
    }

    public async ValueTask<LambdaAnalytics> EnableAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireEditableAsync(database, privateKey, cancellation);

        if (lambda.AnalyticsSince == null)
        {
            // whatever a writing left behind while they were off - or of a
            // lambda that had this id before - is not this lambda's
            await _turn.WaitAsync(cancellation);

            try
            {
                await store.ForgetAsync(lambda.Id, cancellation);
            }
            finally
            {
                _turn.Release();
            }

            lambda.AnalyticsSince = DateTime.UtcNow;

            await database.SaveChangesAsync(cancellation);
        }

        return await DescribeAsync(lambda, null, DateTime.UtcNow, cancellation);
    }

    public async ValueTask<string> DisableAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await RequireEditableAsync(database, privateKey, cancellation);

        await _turn.WaitAsync(cancellation);

        try
        {
            if (lambda.AnalyticsSince != null)
            {
                lambda.AnalyticsSince = null;

                await database.SaveChangesAsync(cancellation);
            }

            // also where they were off already: what is deleted is never
            // shown, and nothing is left that could be - whoever cancels,
            // once they are off
            await store.ForgetAsync(lambda.Id, CancellationToken.None);
        }
        finally
        {
            _turn.Release();
        }

        return lambda.PublicKey;
    }

    #endregion

    #region Writing

    public async ValueTask<AnalyticsFlush> FlushAsync(DateTime? now = null, CancellationToken cancellation = default)
    {
        var at = now ?? DateTime.UtcNow;

        var today = DateOnly.FromDateTime(at);

        await _turn.WaitAsync(cancellation);

        try
        {
            var (visits, dropped) = queue.Take();

            var counted = 0;

            if (visits.Count > 0)
            {
                var tally = new VisitTally(visitors, classifier, addresses);

                var since = await CountingAsync(visits.Select(v => v.Lambda.Id).Distinct().ToList(), cancellation);

                foreach (var visit in visits)
                {
                    // switched off since the visit was queued, or off and on
                    // again: then it was made before what is counted now
                    if (since.TryGetValue(visit.Lambda.Id, out var start) && visit.At >= start)
                    {
                        tally.Add(visit);
                        counted++;
                    }
                }

                await store.WriteAsync(tally, at, cancellation);
            }

            visitors.Prune(today);

            if (_expired != today)
            {
                var removed = await store.ExpireAsync(today.AddDays(-(AnalyticsStore.Kept - 1)), cancellation);

                _expired = today;

                if (removed > 0)
                {
                    logger.LogInformation("Expired {Rows} rows of analytics before {Day}", removed, today.AddDays(-(AnalyticsStore.Kept - 1)));
                }
            }

            if (dropped > 0)
            {
                logger.LogWarning("Dropped {Dropped} visits for analytics, the queue holding {Most} at most", dropped, AnalyticsQueue.Most);
            }

            _updated = at;

            return new AnalyticsFlush(visits.Count, counted, dropped);
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Since when each of the lambdas has its visits counted, for those that do.
    /// </summary>
    private async ValueTask<Dictionary<long, DateTime>> CountingAsync(List<long> lambdas, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.Lambdas.AsNoTracking()
                             .Where(l => lambdas.Contains(l.Id) && l.AnalyticsSince != null)
                             .Select(l => new { l.Id, l.AnalyticsSince })
                             .ToDictionaryAsync(l => l.Id, l => l.AnalyticsSince!.Value, cancellation);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// What the owner of a lambda is shown: as many days as were asked for,
    /// and never more than its tier shows.
    /// </summary>
    /// <remarks>
    /// The days before are summed for a comparison only where the tier shows
    /// those as well - a sandbox lambda's week is not compared with the week
    /// before, which it does not show.
    /// </remarks>
    private async ValueTask<LambdaAnalytics> DescribeAsync(LambdaEntity lambda, int? days, DateTime now, CancellationToken cancellation)
    {
        var window = Math.Clamp(limits.PolicyOf(lambda.Tier).AnalyticsDays, 1, AnalyticsStore.Kept);

        var shown = Math.Clamp(days ?? window, 1, window);

        var to = DateOnly.FromDateTime(now);

        var from = to.AddDays(-(shown - 1));

        var tier = LambdaTierNames.Name(lambda.Tier);

        if (lambda.AnalyticsSince == null)
        {
            return new LambdaAnalytics(lambda.PublicKey, tier, null, shown, window, AnalyticsStore.Kept, from, to, null, null, null, _updated,
                                       options.AnalyticsInterval, ranges.Confirmed);
        }

        var report = await store.ReadAsync(lambda.Id, from, to, Listed, cancellation);

        var previous = shown * 2 <= window ? await store.SumAsync(lambda.Id, from.AddDays(-shown), from.AddDays(-1), cancellation) : null;

        return new LambdaAnalytics(lambda.PublicKey, tier, lambda.AnalyticsSince, shown, window, AnalyticsStore.Kept, from, to,
                                   await store.EarliestAsync(lambda.Id, cancellation), report, previous, _updated, options.AnalyticsInterval, ranges.Confirmed);
    }

    /// <summary>
    /// The lambda behind an editor key, as long as its owner may change it.
    /// </summary>
    /// <remarks>
    /// A demo's key is announced, so holding it says nothing about being the
    /// one who decides whether its visitors are counted.
    /// </remarks>
    private async ValueTask<LambdaEntity> RequireEditableAsync(LambdaDbContext database, string privateKey, CancellationToken cancellation)
    {
        var lambda = await database.Lambdas.FirstOrDefaultAsync(l => l.PrivateKey == privateKey, cancellation)
                  ?? throw LambdaException.NotFound(Missing);

        LambdaGuard.EnsureUnblocked(lambda);

        if (limits.PolicyOf(lambda.Tier).ReadOnly)
        {
            throw LambdaException.Forbidden($"'{lambda.PublicKey}' is a demo and read only: its visitors are not counted. A lambda of your own made from it can count them.");
        }

        return lambda;
    }

    #endregion

}
