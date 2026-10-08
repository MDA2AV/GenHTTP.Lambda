using System.Collections.Frozen;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Which hosts the server answers for on behalf of a lambda, and which lambda
/// each of them belongs to: the domains of their own, and the subdomains of
/// the hosting domain.
/// </summary>
/// <remarks>
/// Asked on every request the server receives, so the domains are held in
/// memory as immutable maps and swapped as a whole: a lookup never takes a
/// lock and never sees half of a change. A subdomain of the hosting domain
/// needs no map - it is named after the key of its lambda - and is read off
/// the host by <see cref="LambdaAddresses"/>.
///
/// The database stays the only record. Whatever changes a domain, a tier or
/// the key of a lambda with a domain reloads this afterwards rather than
/// editing it in place, so there is no second copy of the rules about which
/// lambdas are served where - only premium lambdas with a domain are, and
/// that is decided in the one query below.
/// </remarks>
public sealed class DomainRegistry(IDbContextFactory<LambdaDbContext> databases, LambdaAddresses addresses, ILogger<DomainRegistry> logger)
    : IDomainRegistry
{
    private volatile Served _served = new(FrozenDictionary<string, long>.Empty, FrozenDictionary<string, string>.Empty);

    /// <summary>
    /// One reload at a time: two running side by side could finish in either
    /// order, and the one that read the database first would win.
    /// </summary>
    private readonly Lock _reloading = new();

    #region Get-/Setters

    /// <summary>
    /// How many domains are being served.
    /// </summary>
    public int Count => _served.Domains.Count;

    #endregion

    #region Functionality

    public LambdaHost? Find(string host)
        => _served.Domains.TryGetValue(host, out var lambdaId) ? new CustomDomain(host, lambdaId) : addresses.Resolve(host);

    public string? DomainOf(string publicKey) => _served.ByKey.GetValueOrDefault(publicKey);

    /// <summary>
    /// Reads the served domains again. Called once on startup and after
    /// anything that changes a domain, a tier or the key of a lambda.
    /// </summary>
    public void Reload()
    {
        lock (_reloading)
        {
            using var database = databases.CreateDbContext();

            var served = database.Lambdas.AsNoTracking()
                                 .Where(l => l.Tier == LambdaTier.Premium && l.Domain != null)
                                 .Select(l => new { l.Id, l.PublicKey, Domain = l.Domain! })
                                 .ToList();

            var previous = _served.Domains.Count;

            _served = new Served(served.ToFrozenDictionary(l => l.Domain, l => l.Id, StringComparer.Ordinal),
                                 served.ToFrozenDictionary(l => l.PublicKey, l => l.Domain, StringComparer.Ordinal));

            if (previous != _served.Domains.Count)
            {
                logger.LogInformation("Serving {Count} custom domain(s)", _served.Domains.Count);
            }
        }
    }

    #endregion

    #region Types

    /// <summary>
    /// The served domains, by name and by the key of their lambda - swapped
    /// together, so the two never tell different stories.
    /// </summary>
    private sealed record Served(FrozenDictionary<string, long> Domains, FrozenDictionary<string, string> ByKey);

    #endregion

}
