using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.QuietHours;

/// <summary>
/// Keeps a typical day of the installation and of every lambda visited, and
/// reads the quiet hours off it.
/// </summary>
/// <remarks>
/// <para>
/// The requests are counted in memory as they are answered
/// (<see cref="QuietHoursConcern"/> into <see cref="HourlyCounts"/>), without a
/// lock and without a write; once an hour the job folds what was counted into
/// the rows of the installation and of the lambdas that were asked for
/// something, in one write. What a lambda's day looks like when it is read is
/// worked out from its row then (<see cref="QuietProfile"/>), so a lambda
/// nobody visits costs nothing.
/// </para>
/// <para>
/// Every write to the platform's records makes what is held in memory about
/// the lambdas stale, so there is one an hour and no more: the installation's
/// row is written every time, since the hours the server counted move on with
/// it.
/// </para>
/// </remarks>
public sealed class QuietHoursService(IDbContextFactory<LambdaDbContext> databases, HourlyCounts counts, ILogger<QuietHoursService> logger) : IQuietHoursService
{

    /// <summary>
    /// Who is writing the counts down: one at a time, across the read and the write.
    /// </summary>
    private readonly SemaphoreSlim _turn = new(1, 1);

    /// <summary>
    /// When the counts were last written down, which is where what is written
    /// next was counted from - nothing before the first write.
    /// </summary>
    private DateTime? _written;

    #region Reading

    public async ValueTask<TypicalDay> OfInstallationAsync(DateTime now, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var row = await database.QuietHours.AsNoTracking().FirstOrDefaultAsync(q => q.LambdaId == QuietHoursEntity.Installation, cancellation);

        return row == null ? TypicalDay.Unknown : QuietProfile.Read(row).Describe(now, row.Covered);
    }

    public async ValueTask<TypicalDay> OfLambdaAsync(long lambdaId, DateTime now, CancellationToken cancellation = default)
        => (await OfLambdasAsync([lambdaId], now, cancellation)).GetValueOrDefault(lambdaId) ?? TypicalDay.Unknown;

    /// <remarks>
    /// Read with the installation's row, whose hours counted are the lambdas'
    /// as well.
    /// </remarks>
    public async ValueTask<IReadOnlyDictionary<long, TypicalDay>> OfLambdasAsync(IReadOnlyCollection<long> lambdaIds, DateTime now, CancellationToken cancellation = default)
    {
        if (lambdaIds.Count == 0)
        {
            return new Dictionary<long, TypicalDay>();
        }

        var wanted = lambdaIds.Where(id => id != QuietHoursEntity.Installation).Append(QuietHoursEntity.Installation).ToList();

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var rows = await database.QuietHours.AsNoTracking().Where(q => wanted.Contains(q.LambdaId)).ToListAsync(cancellation);

        var covered = rows.FirstOrDefault(r => r.LambdaId == QuietHoursEntity.Installation)?.Covered ?? 0;

        return rows.Where(r => r.LambdaId != QuietHoursEntity.Installation)
                   .ToDictionary(r => r.LambdaId, r => QuietProfile.Read(r).Describe(now, covered));
    }

    #endregion

    #region Writing

    /// <remarks>
    /// The counts of a lambda removed since are let go of: its row went with
    /// it, and is not made again.
    /// </remarks>
    public async ValueTask FlushAsync(DateTime now, CancellationToken cancellation = default)
    {
        await _turn.WaitAsync(cancellation);

        try
        {
            var installation = counts.TakeInstallation();

            var lambdas = counts.TakeLambdas();

            await using var database = await databases.CreateDbContextAsync(cancellation);

            var counted = lambdas.Select(l => l.Lambda).ToList();

            var existing = counted.Count == 0
                ? []
                : (await database.Lambdas.AsNoTracking().Where(l => counted.Contains(l.Id)).Select(l => l.Id).ToListAsync(cancellation)).ToHashSet();

            var wanted = existing.Append(QuietHoursEntity.Installation).ToList();

            var rows = await database.QuietHours.Where(q => wanted.Contains(q.LambdaId)).ToDictionaryAsync(q => q.LambdaId, cancellation);

            // a day begun now begins where what is written was counted from:
            // the hour before the write was counted too, by the job that
            // writes in the middle of an hour
            var from = _written ?? counts.Since;

            from = from < now ? from : now;

            // the hours this process counted are the installation's to know
            Fold(database, rows, QuietHoursEntity.Installation, installation, from, now, counts.Since);

            foreach (var (lambda, hours) in lambdas.Where(l => existing.Contains(l.Lambda)))
            {
                Fold(database, rows, lambda, hours, from, now);
            }

            await database.SaveChangesAsync(cancellation);

            _written = now;

            logger.LogDebug("Wrote quiet hours of lambdas count {Count}", existing.Count);
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Adds what was counted to the day of a row, making the row where there
    /// is none, and writes the day back into it.
    /// </summary>
    /// <param name="from">Where what was counted was counted from, which a row made now begins at</param>
    /// <param name="counting">Since when the server has been counting, for the row that keeps which hours were</param>
    private static void Fold(LambdaDbContext database, Dictionary<long, QuietHoursEntity> rows, long lambdaId, long[] counted, DateTime from, DateTime now,
                             DateTime? counting = null)
    {
        if (!rows.TryGetValue(lambdaId, out var row))
        {
            row = new QuietHoursEntity { LambdaId = lambdaId, Hours = string.Empty, Since = from, Updated = now };

            database.QuietHours.Add(row);

            rows[lambdaId] = row;
        }

        var profile = row.Hours.Length == 0 ? QuietProfile.Start(from, now) : QuietProfile.Read(row);

        profile.Add(counted, now);

        if (counting is { } since)
        {
            profile.Cover(since, now);
        }

        profile.WriteTo(row);
    }

    public async ValueTask ForgetAsync(long lambdaId, CancellationToken cancellation = default)
    {
        counts.Forget(lambdaId);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        await database.QuietHours.Where(q => q.LambdaId == lambdaId).ExecuteDeleteAsync(cancellation);
    }

    #endregion

}
