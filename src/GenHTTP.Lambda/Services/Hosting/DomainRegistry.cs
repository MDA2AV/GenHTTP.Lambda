using System.Collections.Frozen;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Which domains the server answers for on behalf of a lambda, and which
/// lambda each of them belongs to.
/// </summary>
/// <remarks>
/// Asked on every request the server receives, so it is held in memory as an
/// immutable map and swapped as a whole: a lookup never takes a lock and never
/// sees half of a change.
///
/// The database stays the only record. Whatever changes a domain or a tier
/// reloads this afterwards rather than editing it in place, so there is no
/// second copy of the rules about which lambdas are served where - only
/// premium lambdas with a domain are, and that is decided in the one query
/// below.
/// </remarks>
public sealed class DomainRegistry(IDbContextFactory<LambdaDbContext> databases, ILogger<DomainRegistry> logger)
{
    private volatile FrozenDictionary<string, long> _domains = FrozenDictionary<string, long>.Empty;

    /// <summary>
    /// One reload at a time: two running side by side could finish in either
    /// order, and the one that read the database first would win.
    /// </summary>
    private readonly SemaphoreSlim _reloading = new(1, 1);

    #region Get-/Setters

    /// <summary>
    /// How many domains are being served.
    /// </summary>
    public int Count => _domains.Count;

    #endregion

    #region Functionality

    /// <summary>
    /// The lambda a domain is served by, if it is served by one.
    /// </summary>
    /// <param name="domain">A domain in its normalized form, see <see cref="DomainNames.Normalize"/></param>
    public bool TryFind(string domain, out long lambdaId) => _domains.TryGetValue(domain, out lambdaId);

    /// <summary>
    /// Reads the served domains again. Called once on startup and after
    /// anything that changes a domain or a tier.
    /// </summary>
    public async ValueTask ReloadAsync(CancellationToken cancellation = default)
    {
        await _reloading.WaitAsync(cancellation);

        try
        {
            await using var database = await databases.CreateDbContextAsync(cancellation);

            var served = await database.Lambdas.AsNoTracking()
                                       .Where(l => l.Tier == LambdaTier.Premium && l.Domain != null)
                                       .Select(l => new { l.Id, Domain = l.Domain! })
                                       .ToListAsync(cancellation);

            var previous = _domains.Count;

            _domains = served.ToFrozenDictionary(l => l.Domain, l => l.Id, StringComparer.Ordinal);

            if (previous != _domains.Count)
            {
                logger.LogInformation("Serving {Count} custom domain(s)", _domains.Count);
            }
        }
        finally
        {
            _reloading.Release();
        }
    }

    #endregion

}
