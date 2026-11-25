using System.Diagnostics;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Compiles what the lambdas of the tiers kept warm have online as the server
/// starts, so the first visitor after a restart does not wait for it.
/// </summary>
/// <remarks>
/// A lambda is compiled on its first request otherwise, which takes from a
/// moment to seconds - nothing for something being tried, but an app people
/// use answering its first visitor after every restart seconds late. Which
/// tiers are kept warm is their policy (<see cref="TierPolicy.WarmedUp"/>):
/// production and system.
///
/// Run in the background once the server answers, never before: compiling
/// every lambda first would be that long spent refusing connections, and a
/// handler is prepared against the running server anyway. Each lambda is
/// asked for the way a request asks for it - resolved by the meta service,
/// built with the settings it is served with - so the build it leaves is the
/// one the first request finds, and a request arriving meanwhile is served
/// whether or not its lambda's turn has come.
///
/// One lambda after another rather than all at once: the deployment service
/// compiles one snippet at a time, because compiling takes a lot of memory,
/// so asking for all of them together would compile nothing sooner and put
/// every one of them ahead of a visitor whose lambda is not warm yet, and of
/// an owner putting a version online. Asked for one at a time, those wait for
/// one compilation at most. The ones used last go first: whoever was there
/// before the restart is likeliest to be back first.
/// </remarks>
public sealed class LambdaWarmup(IMetaService meta, IDeploymentService deployments, IDbContextFactory<LambdaDbContext> databases,
                                 ILimitsService limits, ILogBook book, LambdaOptions options, ILogger<LambdaWarmup> logger)
{

    #region Functionality

    /// <summary>
    /// Compiles what every lambda of a tier kept warm has online, one after
    /// another, until all are or the server stops.
    /// </summary>
    /// <remarks>
    /// One that does not build is said in the log and left to its first
    /// request, as before - which is told why at once rather than building it
    /// again (see the deployment service) - and the others are warmed all the
    /// same.
    /// </remarks>
    public async ValueTask WarmUpAsync(CancellationToken cancellation = default)
    {
        var watch = Stopwatch.StartNew();

        var warmed = 0;

        foreach (var id in await CandidatesAsync(cancellation))
        {
            if (cancellation.IsCancellationRequested)
            {
                return;
            }

            // asked again rather than taken from the list: one taken offline
            // or blocked since is not served, so it is not compiled either
            if (await meta.ResolveAsync(id) is not { } lambda)
            {
                continue;
            }

            try
            {
                // under the lambda's own name, as a deployment and a request
                // are: its startup runs here, and what it prints is its own
                using (options.CaptureLambdaOutput
                       ? LambdaOutput.Enter(new OutputScope(lambda.PublicKey, book, options.MaxOutputLines, lambda.Id))
                       : null)
                {
                    await deployments.ResolveAsync(lambda.Id, lambda.ActiveVersion, lambda.Settings, cancellation);
                }

                warmed++;
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to warm up lambda {Lambda} version {Version}", lambda.PublicKey, lambda.ActiveVersion);
            }
        }

        if (warmed > 0)
        {
            logger.LogInformation("Warmed up {Count} lambda(s) in {Elapsed} ms", warmed, watch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// The lambdas of the tiers kept warm that have something online and are
    /// not blocked, the one visited last first.
    /// </summary>
    /// <remarks>
    /// Ids only: each is resolved as its turn comes, and what it is served
    /// with may have changed by then.
    /// </remarks>
    private async ValueTask<List<long>> CandidatesAsync(CancellationToken cancellation)
    {
        var warm = limits.TiersWhere(p => p.WarmedUp);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        // SQLite sorts nothing before everything, so the ones never visited
        // since the column existed come last
        return await database.Lambdas.AsNoTracking()
                             .Where(l => warm.Contains(l.Tier) && l.ActiveVersion != null && l.Blocked == null)
                             .OrderByDescending(l => l.LastSeen)
                             .ThenBy(l => l.Id)
                             .Select(l => l.Id)
                             .ToListAsync(cancellation);
    }

    #endregion

}
