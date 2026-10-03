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
/// is removed - both as the operator set them in the panel. Demos and premium
/// lambdas are kept by their tier.
///
/// The deadlines the editor shows and the sweep that acts on them are kept
/// together, so that what an owner is told is what happens.
/// </remarks>
public sealed class LambdaLifetime(IDbContextFactory<LambdaDbContext> databases, LambdaTelemetry activity, IDeploymentService deployments,
                                   LambdaHistory history, LambdaRemoval removal, LimitsService limits, ILogger<LambdaLifetime> logger)
{

    #region Deadlines

    /// <summary>
    /// When the sweep takes the lambda offline unless it is used before then.
    /// </summary>
    public DateTime? DeployedUntil(LambdaEntity lambda)
        => lambda.ActiveVersion != null && !Kept(lambda) ? Quiet(lambda) + limits.Get().OfflineAfter : null;

    /// <summary>
    /// When the sweep removes the lambda unless it is used before then.
    /// </summary>
    public DateTime? KeptUntil(LambdaEntity lambda)
        => !Kept(lambda) ? Quiet(lambda) + limits.Get().RemovedAfter : null;

    /// <summary>
    /// Whether the tier of a lambda keeps it, rather than the sweeps deciding.
    /// </summary>
    /// <remarks>
    /// Demos because they are the installation's own; premium lambdas
    /// because they answer at somebody's domain, and a site going offline
    /// because it had a quiet month is not something anybody would pay for.
    /// </remarks>
    private static bool Kept(LambdaEntity lambda) => lambda.Tier is LambdaTier.Demo or LambdaTier.Premium;

    /// <summary>
    /// When a lambda last had any attention of either kind.
    /// </summary>
    /// <remarks>
    /// Both clocks run from here, so what a visitor is told about how long
    /// something has left is measured the same way the sweep measures it.
    /// Anything else would show a date that passes without anything happening.
    /// </remarks>
    private static DateTime Quiet(LambdaEntity lambda)
        => lambda.LastSeen > lambda.Modified ? lambda.LastSeen.Value : lambda.Modified;

    #endregion

    #region Sweep

    /// <summary>
    /// Takes offline what has been quiet for too long, and removes what has
    /// been abandoned.
    /// </summary>
    public MaintenanceReport Sweep(DateTime now)
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

        // demos are the installation's own, and being untouched is their
        // normal state rather than a sign that nobody wants them; premium
        // lambdas are kept by their tier (see Kept)
        var expired = database.Lambdas
                              .Where(l => l.Tier != LambdaTier.Demo && l.Tier != LambdaTier.Premium && l.Modified < abandoned
                                             && (l.LastSeen == null || l.LastSeen < abandoned))
                              .ToList();

        foreach (var lambda in expired)
        {
            removal.Remove(database, lambda);
        }

        var quiet = now - limits.Get().OfflineAfter;

        var running = database.Lambdas.Where(l => l.Tier != LambdaTier.Demo && l.Tier != LambdaTier.Premium && l.ActiveVersion != null)
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
        var seen = activity.Describe()
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
