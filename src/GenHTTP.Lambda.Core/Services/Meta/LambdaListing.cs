using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The lambdas of the installation as the operator's panel lists them: a page
/// at a time, narrowed and ordered as asked, each with the features it uses.
/// </summary>
/// <remarks>
/// Every filter is a condition of the one query, so a page is counted and cut
/// by the database rather than in memory. What a lambda uses is read for the
/// lambdas of the page only - a handful of small lookups by their ids - since
/// the panel shows it on every row and would otherwise send the operator to
/// the editor of each.
/// </remarks>
public sealed class LambdaListing(IDbContextFactory<LambdaDbContext> databases, LambdaDescriber describer, ILimitsService limits)
{

    #region Functionality

    public LambdaPage List(LambdaQuery query, int skip, int take)
    {
        using var database = databases.CreateDbContext();

        var all = database.Lambdas.AsNoTracking();

        var total = all.Count();

        var deployed = all.Count(l => l.ActiveVersion != null);

        var narrowed = Narrow(database, all, query, DateTime.UtcNow);

        var matched = narrowed.Count();

        var lambdas = Order(narrowed, query.Order).Skip(skip)
                                                  .Take(take)
                                                  .ToList();

        var ids = lambdas.Select(l => l.Id).ToList();

        var versions = database.Deployments.AsNoTracking()
                               .Where(d => ids.Contains(d.LambdaId))
                               .GroupBy(d => d.LambdaId)
                               .Select(g => new { LambdaId = g.Key, Count = g.Count(), Latest = g.Max(d => d.Version) })
                               .ToDictionary(g => g.LambdaId);

        var uses = UsesOf(database, lambdas);

        return new LambdaPage([.. lambdas.Select(l => describer.Overview(l, versions.GetValueOrDefault(l.Id)?.Latest, versions.GetValueOrDefault(l.Id)?.Count ?? 0) with
                              {
                                  Uses = uses[l.Id],
                                  Visited = l.LastSeen
                              })],
                              matched, total, deployed);
    }

    /// <summary>
    /// What each of these lambdas uses, by its id.
    /// </summary>
    public IReadOnlyDictionary<long, LambdaUses> UsesOf(LambdaDbContext database, IReadOnlyCollection<LambdaEntity> lambdas)
    {
        var ids = lambdas.Select(l => l.Id).ToList();

        var switched = database.DataStores.AsNoTracking()
                               .Where(s => ids.Contains(s.LambdaId))
                               .Select(s => new { s.LambdaId, s.Kind, s.Enabled })
                               .ToList()
                               .ToDictionary(s => (s.LambdaId, s.Kind), s => s.Enabled);

        var showcased = database.Showcases.AsNoTracking()
                                .Where(s => ids.Contains(s.LambdaId))
                                .Select(s => s.LambdaId)
                                .ToHashSet();

        var published = database.Sources.AsNoTracking()
                                .Where(s => s.Published && ids.Contains(s.LambdaId))
                                .Select(s => s.LambdaId)
                                .ToHashSet();

        var drafts = database.Features.AsNoTracking()
                             .Where(f => ids.Contains(f.LambdaId))
                             .GroupBy(f => f.LambdaId)
                             .Select(g => new { LambdaId = g.Key, Count = g.Count() })
                             .ToDictionary(g => g.LambdaId, g => g.Count);

        // as its owner switched it, or as the kind comes where they never did
        bool Keeps(long id, DataKind kind) => switched.TryGetValue((id, kind.Id), out var on) ? on : kind.Default;

        return lambdas.ToDictionary(l => l.Id, l => new LambdaUses(
            Keeps(l.Id, DataKinds.Database),
            Keeps(l.Id, DataKinds.Workspace),
            Keeps(l.Id, DataKinds.Secrets),
            l.AnalyticsSince,
            showcased.Contains(l.Id),
            published.Contains(l.Id),
            drafts.GetValueOrDefault(l.Id),
            l.View
        ));
    }

    #endregion

    #region Helpers

    private IQueryable<LambdaEntity> Narrow(LambdaDbContext database, IQueryable<LambdaEntity> query, LambdaQuery wanted, DateTime now)
    {
        if (!string.IsNullOrWhiteSpace(wanted.Search))
        {
            var term = wanted.Search.Trim();

            // the key and the domain are what an administrator has to go on
            // that is not the code itself, and what the panel shows
            query = query.Where(l => EF.Functions.Like(l.PublicKey, $"%{term}%")
                                  || (l.Domain != null && EF.Functions.Like(l.Domain, $"%{term}%")));
        }

        if (wanted.Tier is { } tier)
        {
            query = query.Where(l => l.Tier == tier);
        }

        if (wanted.View is { } view)
        {
            query = query.Where(l => l.View == view);
        }

        if (wanted.CreatedWithin is { } within)
        {
            var since = now - within;

            query = query.Where(l => l.Created >= since);
        }

        query = wanted.State switch
        {
            LambdaState.Online => query.Where(l => l.ActiveVersion != null),
            LambdaState.Offline => query.Where(l => l.ActiveVersion == null && l.Blocked == null),
            LambdaState.Blocked => query.Where(l => l.Blocked != null),
            LambdaState.Expiring => Expiring(query, now),
            _ => query
        };

        foreach (var feature in wanted.Uses ?? [])
        {
            query = Using(database, query, feature);
        }

        return query;
    }

    /// <summary>
    /// The lambdas that go offline or are removed soon unless they are used:
    /// quiet - not changed, visited or moved - since a moment that is the
    /// deadline less the time it allows (see <c>LambdaLifetime</c>).
    /// </summary>
    private IQueryable<LambdaEntity> Expiring(IQueryable<LambdaEntity> query, DateTime now)
    {
        var product = limits.Get();

        var expiring = limits.TiersWhere(p => p.ExpiresWhenUnused);

        var horizon = now + LambdaQuery.ExpiringWithin;

        var removed = horizon - product.RemovedAfter;

        var offline = horizon - product.OfflineAfter;

        return query.Where(l => expiring.Contains(l.Tier) && l.Blocked == null
                                && ((l.Modified < removed && (l.LastSeen == null || l.LastSeen < removed) && (l.TierChanged == null || l.TierChanged < removed))
                                    || (l.ActiveVersion != null && l.Modified < offline && (l.LastSeen == null || l.LastSeen < offline)
                                        && (l.TierChanged == null || l.TierChanged < offline))));
    }

    private IQueryable<LambdaEntity> Using(LambdaDbContext database, IQueryable<LambdaEntity> query, LambdaFeature feature)
    {
        // switched on, or never switched for a kind that comes switched on
        IQueryable<LambdaEntity> Keeping(DataKind kind)
        {
            var id = kind.Id;

            return kind.Default
                ? query.Where(l => !database.DataStores.Any(s => s.LambdaId == l.Id && s.Kind == id && !s.Enabled))
                : query.Where(l => database.DataStores.Any(s => s.LambdaId == l.Id && s.Kind == id && s.Enabled));
        }

        switch (feature)
        {
            case LambdaFeature.Database:
                return Keeping(DataKinds.Database);
            case LambdaFeature.Workspace:
                return Keeping(DataKinds.Workspace);
            case LambdaFeature.Secrets:
                return Keeping(DataKinds.Secrets);
            case LambdaFeature.Analytics:
                return query.Where(l => l.AnalyticsSince != null);
            case LambdaFeature.Showcase:
                return query.Where(l => database.Showcases.Any(s => s.LambdaId == l.Id));
            case LambdaFeature.Source:
                return query.Where(l => database.Sources.Any(s => s.LambdaId == l.Id && s.Published));
            case LambdaFeature.Domain:
                return query.Where(l => l.Domain != null);
            case LambdaFeature.Drafts:
                return query.Where(l => database.Features.Any(f => f.LambdaId == l.Id));
            case LambdaFeature.Requested:
                return query.Where(l => l.Connectivity == ConnectivityStatus.Requested);
            case LambdaFeature.Connects:
            {
                // the operator's decision first, the tier's default where there
                // is none - as ConnectivityRule.Allows reads it
                var included = limits.TiersWhere(p => p.Connectivity);

                return query.Where(l => l.Connectivity == ConnectivityStatus.Allowed || (l.Connectivity == null && included.Contains(l.Tier)));
            }
            default:
                return query;
        }
    }

    private static IQueryable<LambdaEntity> Order(IQueryable<LambdaEntity> query, LambdaOrder order) => order switch
    {
        LambdaOrder.Oldest => query.OrderBy(l => l.Created),
        LambdaOrder.Changed => query.OrderByDescending(l => l.Modified),
        // never visited sorts last whichever way, as a lambda nobody opened is
        // the least recently visited of all
        LambdaOrder.Visited => query.OrderBy(l => l.LastSeen == null).ThenByDescending(l => l.LastSeen),
        LambdaOrder.Key => query.OrderBy(l => l.PublicKey),
        _ => query.OrderByDescending(l => l.Created)
    };

    #endregion

}
