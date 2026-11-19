using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Telemetry;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// When a lambda is taken offline and when it is removed: said in advance,
/// and carried out by the maintenance sweep.
/// </summary>
/// <remarks>
/// A lambda stays online for as long as it is used. One that nobody visits
/// and nobody edits for <see cref="ProductLimits.OfflineAfter"/> is taken
/// offline, and one left alone for <see cref="ProductLimits.RemovedAfter"/>
/// is removed - both as the operator set them in the panel - where its tier
/// says it is not kept while unused (<see cref="TierPolicy.ExpiresWhenUnused"/>),
/// a sandbox. A lambda moved to such a tier is measured from the move at the
/// earliest, never from a visit before it.
///
/// The deadlines the editor shows and the sweep that acts on them are kept
/// together, so that what an owner is told is what happens.
/// </remarks>
public sealed class LambdaLifetime(IDbContextFactory<LambdaDbContext> databases, ITelemetryService telemetry, IDeploymentService deployments,
                                   LambdaHistory history, LambdaRemoval removal, ILimitsService limits, ILogger<LambdaLifetime> logger)
{

    #region Deadlines

    /// <summary>
    /// When the sweep takes the lambda offline unless it is used before then.
    /// </summary>
    public DateTime? DeployedUntil(LambdaEntity lambda)
        => lambda.ActiveVersion != null && Expires(lambda) ? Quiet(lambda) + limits.Get().OfflineAfter : null;

    /// <summary>
    /// When the sweep removes the lambda unless it is used before then.
    /// </summary>
    public DateTime? KeptUntil(LambdaEntity lambda)
        => Expires(lambda) && lambda.Blocked == null ? Quiet(lambda) + limits.Get().RemovedAfter : null;

    /// <summary>
    /// Whether the sweeps decide about the lambda, rather than its tier keeping it.
    /// </summary>
    /// <remarks>
    /// A demo or a system lambda is the installation's own; a lambda in
    /// production is an app people use, which going offline because it had a
    /// quiet month would break.
    /// </remarks>
    private bool Expires(LambdaEntity lambda) => limits.PolicyOf(lambda.Tier).ExpiresWhenUnused;

    /// <summary>
    /// When a lambda last had any attention of either kind.
    /// </summary>
    /// <remarks>
    /// Both clocks run from here, so what a visitor is told about how long
    /// something has left is measured the same way the sweep measures it.
    /// Anything else would show a date that passes without anything happening.
    /// Never before its tier last changed: until then nothing measured it.
    /// </remarks>
    private static DateTime Quiet(LambdaEntity lambda)
    {
        var used = lambda.LastSeen > lambda.Modified ? lambda.LastSeen.Value : lambda.Modified;

        return lambda.TierChanged > used ? lambda.TierChanged.Value : used;
    }

    #endregion

    #region Sweep

    /// <summary>
    /// Takes offline what has been quiet for too long, and removes what has
    /// been abandoned.
    /// </summary>
    public async ValueTask<MaintenanceReport> SweepAsync(DateTime now, CancellationToken cancellation = default)
    {
        using var database = databases.CreateDbContext();

        /*
         * What counts as use, written down before anything is decided by it.
         *
         * Requests are counted in memory, because a database write per request
         * to move a timestamp would be absurd; this is where the last of them
         * reaches the row. It runs first so that a lambda busy right up to
         * this moment is not swept by figures taken before its traffic was
         * recorded.
         */
        RecordUse(database);

        var abandoned = now - limits.Get().RemovedAfter;

        // only the tiers that are not kept while unused (see Expires): for a
        // demo or a system lambda, being untouched is its normal state rather
        // than a sign that nobody wants it. A blocked lambda is the
        // operator's to delete or let go: removed by itself, its key would be
        // free again for whoever it was blocked for
        var expiring = limits.TiersWhere(p => p.ExpiresWhenUnused);

        var expired = database.Lambdas
                              .Where(l => expiring.Contains(l.Tier) && l.Blocked == null && l.Modified < abandoned
                                             && (l.LastSeen == null || l.LastSeen < abandoned)
                                             && (l.TierChanged == null || l.TierChanged < abandoned))
                              .ToList();

        foreach (var lambda in expired)
        {
            // stopped between two lambdas, never within one
            cancellation.ThrowIfCancellationRequested();

            await removal.RemoveAsync(database, lambda);
        }

        var quiet = now - limits.Get().OfflineAfter;

        var running = database.Lambdas.Where(l => expiring.Contains(l.Tier) && l.ActiveVersion != null)
                              .ToList();

        var undeployed = 0;

        foreach (var lambda in running)
        {
            // a lambda deployed before the column existed has no date; it is
            // treated as deployed now rather than swept on the next pass
            if (lambda.Deployed is null)
            {
                lambda.Deployed = now;
                continue;
            }

            /*
             * The later of the two kinds of attention a lambda can get.
             *
             * Being edited counts and being visited counts, and it only goes
             * offline once neither has happened for the whole window. The
             * deployment's own age is deliberately not in here: how long ago
             * something was put online says nothing about whether anybody
             * wants it, and using it as the test is what used to take working
             * lambdas down overnight.
             */
            if (Quiet(lambda) > quiet)
            {
                continue;
            }

            lambda.ActiveVersion = null;
            lambda.Deployed = null;

            history.Close(database, lambda.Id, now, ActivationEndings.Expired);

            deployments.Evict(lambda.Id);

            undeployed++;
        }

        if (undeployed > 0)
        {
            database.SaveChanges();
        }

        if (undeployed > 0 || expired.Count > 0)
        {
            logger.LogInformation("Undeployed {Undeployed} and deleted {Deleted} lambda(s) by maintenance", undeployed, expired.Count);
        }

        return new MaintenanceReport(undeployed, expired.Count);
    }

    /// <summary>
    /// Writes what the counters in memory know about who has been called.
    /// </summary>
    /// <remarks>
    /// Only forwards, and only for rows that would move: a restart empties the
    /// counters, and a lambda that has had no traffic since should keep the
    /// date it already had rather than be pushed back to the epoch.
    /// </remarks>
    private void RecordUse(LambdaDbContext database)
    {
        var seen = telemetry.DescribeLambdas()
                           .Where(a => a.LastSeen != null)
                           .ToDictionary(a => a.PublicKey, a => a.LastSeen!.Value, StringComparer.Ordinal);

        if (seen.Count == 0)
        {
            return;
        }

        var keys = seen.Keys.ToList();

        var rows = database.Lambdas.Where(l => keys.Contains(l.PublicKey)).ToList();

        var moved = 0;

        foreach (var row in rows)
        {
            if (seen.TryGetValue(row.PublicKey, out var last) && (row.LastSeen == null || last > row.LastSeen))
            {
                row.LastSeen = last;
                moved++;
            }
        }

        if (moved > 0)
        {
            database.SaveChanges();
        }
    }

    #endregion

}
