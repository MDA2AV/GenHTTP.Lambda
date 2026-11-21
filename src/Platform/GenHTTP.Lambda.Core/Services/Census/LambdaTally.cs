using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Census;

/// <summary>
/// Counts the lambdas of the installation: how many, in which state and tier,
/// how recent, and which of the platform's features they use.
/// </summary>
/// <remarks>
/// One read of the lambdas' rows and a few counts beside it, done in memory -
/// a row is a few dozen bytes, and the conditions the counts need (when a
/// sandbox lambda expires, whether one may connect) are read off the tier's
/// policy, which the database does not hold. Awaited, like every query of the
/// platform's records, one after another on the one context.
/// </remarks>
public sealed class LambdaTally(IDbContextFactory<LambdaDbContext> databases, ILimitsService limits)
{

    /// <summary>
    /// How far back what is recent reaches.
    /// </summary>
    private static readonly TimeSpan Month = TimeSpan.FromDays(30);

    #region Functionality

    public async ValueTask<LambdaCensus> CountAsync(DateTime now, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var demos = limits.TiersWhere(p => p.ReadOnly);

        var rows = await database.Lambdas.AsNoTracking()
                                 .Select(l => new Row(l.Id, l.Tier, l.ActiveVersion, l.Blocked, l.Created, l.Modified, l.LastSeen, l.TierChanged, l.View,
                                                      l.Domain, l.AnalyticsSince, l.Connectivity, l.ConnectivityChanged))
                                 .ToListAsync(cancellation);

        var tiers = rows.GroupBy(l => l.Tier).ToDictionary(g => g.Key, g => g.Count());

        var lambdas = rows.Where(l => !demos.Contains(l.Tier)).ToList();

        var ids = lambdas.Select(l => l.Id).ToHashSet();

        var switched = (await database.DataStores.AsNoTracking()
                                      .Select(s => new { s.LambdaId, s.Kind, s.Enabled })
                                      .ToListAsync(cancellation))
                       .Where(s => ids.Contains(s.LambdaId))
                       .ToList();

        // as their owners switched it, or as the kind comes where they never did
        int Keeping(DataKind kind) => kind.Default
            ? ids.Count - switched.Count(s => s.Kind == kind.Id && !s.Enabled)
            : switched.Count(s => s.Kind == kind.Id && s.Enabled);

        int Among(IEnumerable<long> owners) => owners.Distinct().Count(ids.Contains);

        var showcased = Among(await database.Showcases.AsNoTracking().Select(s => s.LambdaId).ToListAsync(cancellation));

        var published = Among(await database.Sources.AsNoTracking().Where(s => s.Published).Select(s => s.LambdaId).ToListAsync(cancellation));

        var drafting = Among(await database.Features.AsNoTracking().Select(f => f.LambdaId).ToListAsync(cancellation));

        var versions = await database.Deployments.CountAsync(d => !demos.Contains(d.Lambda!.Tier), cancellation);

        var recent = now - Month;

        var saved = (await database.Deployments.AsNoTracking()
                                   .Where(d => d.Created >= recent && !demos.Contains(d.Lambda!.Tier))
                                   .GroupBy(d => d.Origin)
                                   .Select(g => new { Origin = g.Key, Count = g.Count() })
                                   .ToListAsync(cancellation))
                    // by name, the unrecorded first, as the panel lists them
                    .GroupBy(g => VersionOriginNames.Name(g.Origin))
                    .OrderBy(g => g.Key != null)
                    .ThenBy(g => g.Key, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key ?? "unknown", g => g.Sum(o => o.Count));

        var product = limits.Get();

        var horizon = now + LambdaQuery.ExpiringWithin;

        bool Expires(Row lambda) => limits.PolicyOf(lambda.Tier).ExpiresWhenUnused && lambda.Blocked == null;

        bool GoingOffline(Row lambda) => Expires(lambda) && lambda.ActiveVersion != null && lambda.Quiet + product.OfflineAfter <= horizon;

        bool Removed(Row lambda) => Expires(lambda) && lambda.Quiet + product.RemovedAfter <= horizon;

        return new LambdaCensus(
            now,
            lambdas.Count,
            rows.Count - lambdas.Count,
            lambdas.Count(l => l.ActiveVersion != null),
            lambdas.Count(l => l.ActiveVersion == null && l.Blocked == null),
            lambdas.Count(l => l.Blocked != null),
            tiers,
            lambdas.Count(l => l.Created >= now.AddDays(-1)),
            lambdas.Count(l => l.Created >= now.AddDays(-7)),
            lambdas.Count(l => l.Created >= recent),
            lambdas.Count(l => l.Modified >= recent || l.LastSeen >= recent),
            lambdas.Count(l => GoingOffline(l)),
            lambdas.Count(l => Removed(l)),
            lambdas.Count(l => GoingOffline(l) || Removed(l)),
            new CensusUses(
                Keeping(DataKinds.Database),
                Keeping(DataKinds.Workspace),
                Keeping(DataKinds.Secrets),
                lambdas.Count(l => l.AnalyticsSince != null),
                showcased,
                published,
                lambdas.Count(l => l.Domain != null),
                drafting,
                // as ConnectivityRule.Allows reads it: the operator's decision
                // first, the tier's default where there is none
                lambdas.Count(l => l.Connectivity == ConnectivityStatus.Allowed || (l.Connectivity == null && limits.PolicyOf(l.Tier).Connectivity)),
                lambdas.Count(l => l.Connectivity == ConnectivityStatus.Requested)
            ),
            lambdas.Count(l => l.View == EditorView.Simple),
            lambdas.Count(l => l.View == EditorView.Full),
            versions,
            saved,
            lambdas.Where(l => l.Connectivity == ConnectivityStatus.Requested).Min(l => l.ConnectivityChanged)
        );
    }

    #endregion

    #region Helpers

    private sealed record Row(long Id, LambdaTier Tier, int? ActiveVersion, DateTime? Blocked, DateTime Created, DateTime Modified,
                              DateTime? LastSeen, DateTime? TierChanged, EditorView View, string? Domain, DateTime? AnalyticsSince,
                              ConnectivityStatus? Connectivity, DateTime? ConnectivityChanged)
    {

        /// <summary>
        /// Since when it was not used: changed, visited or moved into its tier,
        /// as <c>LambdaLifetime</c> reads it.
        /// </summary>
        public DateTime Quiet
        {
            get
            {
                var used = LastSeen > Modified ? LastSeen.Value : Modified;

                return TierChanged > used ? TierChanged.Value : used;
            }
        }

    }

    #endregion

}
