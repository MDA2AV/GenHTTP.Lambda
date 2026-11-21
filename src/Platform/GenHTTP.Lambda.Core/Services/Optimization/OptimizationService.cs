using System.Diagnostics;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.QuietHours;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Optimization;

/// <summary>
/// Optimizes the platform's own records and analytics, and the database of
/// every lambda that keeps one, when each is due and quiet.
/// </summary>
/// <remarks>
/// <para>
/// A lambda's database is looked at only where it is switched on and the
/// lambda was visited since its database was last optimized - nothing else
/// writes to it, so one nobody uses stays as it was left. The copies the
/// features work on are not: they are thrown away when the feature is merged.
/// The longest unoptimized go first, one database at a time, and a run that
/// took long leaves the rest to the next, a quarter of an hour later and
/// still in the quiet hours.
/// </para>
/// <para>
/// What each came to is written once the run is over, in one write: every
/// write to the platform's records makes what is held in memory about the
/// lambdas stale. A database that failed is written down as failed, and
/// tried again the next day rather than the next run.
/// </para>
/// <para>
/// Called by the background job, on a thread of its own: an integration that
/// optimizes on the thread it is asked on - SQLite does - holds no request up.
/// </para>
/// </remarks>
public sealed class OptimizationService(IDbContextFactory<LambdaDbContext> databases, IDatabaseProvider provider, ISettingsService settings,
                                        IQuietHoursService quietHours, ILogger<OptimizationService> logger) : IOptimizationService
{

    /// <summary>
    /// How long one run optimizes before it leaves the rest to the next.
    /// </summary>
    private static readonly TimeSpan Budget = TimeSpan.FromMinutes(5);

    private readonly DateTime _started = DateTime.UtcNow;

    /// <summary>
    /// Whether a run is going: one at a time, and a run that finds one going
    /// leaves it to it.
    /// </summary>
    private readonly SemaphoreSlim _running = new(1, 1);

    #region Running

    public async ValueTask RunAsync(DateTime now, CancellationToken cancellation = default)
    {
        if (!settings.Get().OptimizeDatabases || now - _started < OptimizationSchedule.Settling)
        {
            return;
        }

        if (!await _running.WaitAsync(0, cancellation))
        {
            return;
        }

        try
        {
            await OptimizeDueAsync(now, cancellation);
        }
        finally
        {
            _running.Release();
        }
    }

    private async ValueTask OptimizeDueAsync(DateTime now, CancellationToken cancellation)
    {
        var installation = await quietHours.OfInstallationAsync(now, cancellation);

        var (platform, candidates) = await DueAsync(now, cancellation);

        var days = await quietHours.OfLambdasAsync([.. candidates.Select(c => c.Id)], now, cancellation);

        var done = new List<DatabaseOptimizationEntity>();

        var clock = Stopwatch.StartNew();

        try
        {
            if (platform && OptimizationSchedule.IsQuiet(now, installation))
            {
                done.Add(await OptimizePlatformAsync(now + clock.Elapsed, cancellation));
            }

            foreach (var candidate in candidates)
            {
                if (clock.Elapsed > Budget)
                {
                    break;
                }

                if (OptimizationSchedule.IsQuiet(now, installation, days.GetValueOrDefault(candidate.Id)))
                {
                    done.Add(await OptimizeLambdaAsync(candidate, now + clock.Elapsed, cancellation));
                }
            }
        }
        finally
        {
            // what was done is written whether or not the run was stopped
            // halfway, so it is not done again as the server comes back
            if (done.Count > 0)
            {
                await RecordAsync(done);

                logger.LogInformation("Optimized databases count {Count} millis {Millis} freed {Freed} failed {Failed}",
                                      done.Count, clock.ElapsedMilliseconds, done.Sum(d => Math.Max(0, d.SizeBefore - d.SizeAfter)), done.Count(d => d.Failure != null));
            }
        }
    }

    /// <summary>
    /// Whether the platform's own stores are due, and the lambdas whose
    /// databases are - the longest unoptimized first.
    /// </summary>
    private async ValueTask<(bool Platform, List<Candidate> Lambdas)> DueAsync(DateTime now, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var platform = await database.DatabaseOptimizations.AsNoTracking()
                                     .Where(o => o.LambdaId == DatabaseOptimizationEntity.Platform)
                                     .Select(o => (DateTime?)o.Optimized)
                                     .FirstOrDefaultAsync(cancellation);

        var cutoff = now - OptimizationSchedule.Apart;

        var due = await (from store in database.DataStores.AsNoTracking()
                         where store.Kind == DataKinds.DatabaseId && store.Enabled
                         join lambda in database.Lambdas.AsNoTracking() on store.LambdaId equals lambda.Id
                         where lambda.LastSeen != null
                         join optimization in database.DatabaseOptimizations.AsNoTracking() on lambda.Id equals optimization.LambdaId into optimizations
                         from last in optimizations.DefaultIfEmpty()
                         // a database that failed is tried again, visited or not
                         where last == null || (last.Optimized <= cutoff && (lambda.LastSeen > last.Optimized || last.Failure != null))
                         select new Candidate(lambda.Id, lambda.PublicKey, last == null ? null : last.Optimized))
                        .ToListAsync(cancellation);

        // never first, then the longest ago - ordered here, since databases
        // disagree about where an empty value sorts
        due.Sort((a, b) => Nullable.Compare(a.Optimized, b.Optimized));

        return (OptimizationSchedule.IsDue(platform, now), due);
    }

    /// <summary>
    /// Optimizes the platform's records and its analytics, written down as
    /// one: they are kept by the same integration, and looked at together.
    /// </summary>
    private async ValueTask<DatabaseOptimizationEntity> OptimizePlatformAsync(DateTime now, CancellationToken cancellation)
    {
        var clock = Stopwatch.StartNew();

        long before = 0, after = 0;

        var steps = new List<string>();
        var skipped = new List<string>();

        foreach (var store in Enum.GetValues<DatabaseStore>())
        {
            try
            {
                var result = await provider.OptimizeAsync(store, cancellation);

                before += result.Before;
                after += result.After;

                steps.AddRange(result.Steps.Select(s => $"{NameOf(store)}: {s}"));
                skipped.AddRange(result.Skipped.Select(s => $"{NameOf(store)}: {s}"));
            }
            catch (Exception e) when (e is not OperationCanceledException || !cancellation.IsCancellationRequested)
            {
                logger.LogWarning(e, "Failed to optimize the platform's {Store}", NameOf(store));

                return Row(DatabaseOptimizationEntity.Platform, now, clock, before, after, steps, skipped, $"{NameOf(store)}: {e.Message}");
            }
        }

        logger.LogDebug("Optimized the platform's records millis {Millis} freed {Freed}", clock.ElapsedMilliseconds, before - after);

        return Row(DatabaseOptimizationEntity.Platform, now, clock, before, after, steps, skipped, null);
    }

    private async ValueTask<DatabaseOptimizationEntity> OptimizeLambdaAsync(Candidate candidate, DateTime now, CancellationToken cancellation)
    {
        var clock = Stopwatch.StartNew();

        try
        {
            var result = await provider.OptimizeAsync(new DataScope(candidate.Id), cancellation);

            logger.LogDebug("Optimized database of lambda {Lambda} millis {Millis} freed {Freed}", candidate.PublicKey, clock.ElapsedMilliseconds,
                            result.Before - result.After);

            return Row(candidate.Id, now, clock, result.Before, result.After, result.Steps, result.Skipped, null);
        }
        catch (Exception e) when (e is not OperationCanceledException || !cancellation.IsCancellationRequested)
        {
            logger.LogWarning(e, "Failed to optimize database of lambda {Lambda}", candidate.PublicKey);

            return Row(candidate.Id, now, clock, 0, 0, [], [], e.Message);
        }
    }

    /// <summary>
    /// Replaces what was written down about the databases optimized, in one write.
    /// </summary>
    /// <remarks>
    /// Not cancelled with the run: what was done stays done, and is written
    /// down even while the server stops. A lambda removed meanwhile is left
    /// out, its row having gone with it.
    /// </remarks>
    private async ValueTask RecordAsync(List<DatabaseOptimizationEntity> done)
    {
        try
        {
            await using var database = await databases.CreateDbContextAsync();

            var ids = done.Select(d => d.LambdaId).ToList();

            var rows = await database.DatabaseOptimizations.Where(o => ids.Contains(o.LambdaId)).ToDictionaryAsync(o => o.LambdaId);

            var lambdas = (await database.Lambdas.AsNoTracking().Where(l => ids.Contains(l.Id)).Select(l => l.Id).ToListAsync()).ToHashSet();

            foreach (var row in done)
            {
                if (rows.TryGetValue(row.LambdaId, out var existing))
                {
                    database.Entry(existing).CurrentValues.SetValues(row);
                }
                else if (row.LambdaId == DatabaseOptimizationEntity.Platform || lambdas.Contains(row.LambdaId))
                {
                    database.DatabaseOptimizations.Add(row);
                }
            }

            await database.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to record optimized databases count {Count}", done.Count);
        }
    }

    private static DatabaseOptimizationEntity Row(long lambdaId, DateTime now, Stopwatch clock, long before, long after, IReadOnlyList<string> steps,
                                                  IReadOnlyList<string> skipped, string? failure)
        => new()
        {
            LambdaId = lambdaId,
            Optimized = now,
            Millis = clock.ElapsedMilliseconds,
            SizeBefore = before,
            SizeAfter = after,
            Steps = OptimizationRecord.Join(steps) ?? string.Empty,
            Skipped = OptimizationRecord.Join(skipped),
            Failure = failure
        };

    /// <summary>
    /// What a store of the platform is called where its steps are listed.
    /// </summary>
    /// <remarks>
    /// Spelled out rather than its name in the code, which is not what the
    /// operator reads.
    /// </remarks>
    private static string NameOf(DatabaseStore store) => store switch
    {
        DatabaseStore.Platform => "records",
        DatabaseStore.Analytics => "analytics",
        _ => throw new ArgumentOutOfRangeException(nameof(store), store, "The platform keeps no such store.")
    };

    /// <summary>
    /// A lambda whose database is due.
    /// </summary>
    private sealed record Candidate(long Id, string PublicKey, DateTime? Optimized);

    #endregion

    #region Reading

    public async ValueTask<OptimizationOverview> OverviewAsync(DateTime now, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var platform = await database.DatabaseOptimizations.AsNoTracking()
                                     .FirstOrDefaultAsync(o => o.LambdaId == DatabaseOptimizationEntity.Platform, cancellation);

        var day = now.AddDays(-1);

        var lambdas = await database.DatabaseOptimizations.AsNoTracking()
                                    .Where(o => o.LambdaId != DatabaseOptimizationEntity.Platform && o.Optimized > day)
                                    .Select(o => new { o.SizeBefore, o.SizeAfter, Failed = o.Failure != null })
                                    .ToListAsync(cancellation);

        var last = await database.DatabaseOptimizations.AsNoTracking()
                                 .Where(o => o.LambdaId != DatabaseOptimizationEntity.Platform)
                                 .OrderByDescending(o => o.Optimized)
                                 .Select(o => (DateTime?)o.Optimized)
                                 .FirstOrDefaultAsync(cancellation);

        return new OptimizationOverview(
            settings.Get().OptimizeDatabases,
            platform != null ? OptimizationRecord.Of(platform) : null,
            lambdas.Count,
            lambdas.Count(l => l.Failed),
            lambdas.Sum(l => Math.Max(0, l.SizeBefore - l.SizeAfter)),
            last,
            OptimizationSchedule.FallbackInUtc(now)
        );
    }

    public async ValueTask<OptimizationRecord?> OfLambdaAsync(long lambdaId, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var row = await database.DatabaseOptimizations.AsNoTracking().FirstOrDefaultAsync(o => o.LambdaId == lambdaId, cancellation);

        return row != null ? OptimizationRecord.Of(row) : null;
    }

    public async ValueTask ForgetAsync(long lambdaId, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        await database.DatabaseOptimizations.Where(o => o.LambdaId == lambdaId).ExecuteDeleteAsync(cancellation);
    }

    #endregion

}
