using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Where a lambda answers and on what terms: its public key, its domain, and
/// the tier that decides what it may have.
/// </summary>
/// <remarks>
/// None of it touches the program: a lambda keeps its versions and its data
/// whatever it is called, wherever it answers and whichever tier it is in.
/// What each change does reach is what serves the lambda - the hosts
/// answered, and the limits its database is opened with.
/// </remarks>
public sealed class LambdaHosting(IDbContextFactory<LambdaDbContext> databases, LambdaDescriber describer, IDatabaseVault databaseVault,
                                  IDomainRegistry domains, ILimitsService limits, LambdaOptions options, ILogger<LambdaHosting> logger)
{

    #region Keys

    /// <summary>
    /// Whether a public key could be claimed, and whether a lambda already
    /// answers there.
    /// </summary>
    public KeyStatus DescribeKey(string? publicKey)
    {
        // looked up even when it could not be claimed: a key that is refused
        // today may still belong to a lambda from before the rule was made
        var valid = LambdaKeys.TryNormalize(publicKey, out var normalized, out var reason);

        using var database = databases.CreateDbContext();

        var lambda = database.Lambdas.AsNoTracking()
                             .Where(l => l.PublicKey == normalized)
                             .Select(l => new { l.ActiveVersion })
                             .FirstOrDefault();

        if (valid && lambda != null)
        {
            reason = "This key is already in use.";
        }
        else if (valid && LambdaKeys.IsDemo(normalized))
        {
            // said here as well as refused on creation, so the page that
            // checks a key while it is typed says why before anybody submits
            valid = false;
            reason = LambdaKeys.DemoReason;
        }

        return new KeyStatus(normalized, valid, lambda != null, lambda?.ActiveVersion != null, reason);
    }

    /// <summary>
    /// Moves the lambda to another public key.
    /// </summary>
    public LambdaInfo ChangeKey(string privateKey, string? publicKey)
    {
        if (!LambdaKeys.TryNormalize(publicKey, out var normalized, out var reason))
        {
            throw LambdaException.Invalid(reason!);
        }

        using var database = databases.CreateDbContext();

        var lambda = LambdaGuard.Require(database, privateKey);

        LambdaGuard.EnsureEditable(lambda, limits.PolicyOf(lambda.Tier));

        if (lambda.PublicKey == normalized)
        {
            return describer.Describe(database, lambda);
        }

        if (LambdaKeys.IsDemo(normalized))
        {
            throw LambdaException.Invalid(LambdaKeys.DemoReason);
        }

        if (database.Lambdas.Any(l => l.PublicKey == normalized))
        {
            throw LambdaException.Conflict("This key is already in use.");
        }

        var previous = lambda.PublicKey;

        lambda.PublicKey = normalized;
        lambda.Modified = DateTime.UtcNow;

        database.SaveChanges();

        // the registry knows a served domain by the key of its lambda as
        // well, for the subdomain that sends visitors on to it
        if (lambda.Domain != null)
        {
            domains.Reload();
        }

        logger.LogInformation("Changed public key of lambda #{LambdaId} from {Previous} to {Lambda}", lambda.Id, previous, normalized);

        return describer.Describe(database, lambda);
    }

    #endregion

    #region Domains

    /// <summary>
    /// Sets the domain the lambda answers at, or removes it when nothing is given.
    /// </summary>
    public LambdaInfo ChangeDomain(string privateKey, string? domain)
    {
        string? normalized = null;

        if (!string.IsNullOrWhiteSpace(domain) && !DomainNames.TryNormalize(domain, options, out normalized, out var reason))
        {
            throw LambdaException.Invalid(reason!);
        }

        using var database = databases.CreateDbContext();

        var lambda = LambdaGuard.Require(database, privateKey);

        LambdaGuard.EnsureEditable(lambda, limits.PolicyOf(lambda.Tier));

        if (lambda.Domain == normalized)
        {
            return describer.Describe(database, lambda);
        }

        if (normalized != null)
        {
            if (!limits.PolicyOf(lambda.Tier).OwnDomain)
            {
                throw LambdaException.Forbidden(NoDomain(lambda.Tier));
            }

            if (database.Lambdas.Any(l => l.Domain == normalized && l.Id != lambda.Id))
            {
                throw LambdaException.Conflict("This domain is already used by another lambda.");
            }
        }

        var previous = lambda.Domain;

        lambda.Domain = normalized;
        lambda.Modified = DateTime.UtcNow;

        try
        {
            database.SaveChanges();
        }
        catch (DbUpdateException)
        {
            // claimed by somebody else between the check above and now
            throw LambdaException.Conflict("This domain is already used by another lambda.");
        }

        domains.Reload();

        logger.LogInformation("Changed domain of lambda {Lambda} #{LambdaId} from {Previous} to {Domain}",
                              lambda.PublicKey, lambda.Id, previous ?? "(none)", normalized ?? "(none)");

        return describer.Describe(database, lambda);
    }

    #endregion

    #region Tiers

    /// <summary>
    /// Moves the lambda to another tier. Only ever done by an administrator.
    /// </summary>
    /// <remarks>
    /// Only the policy changes: nothing is compiled again and nothing of its
    /// data is moved. What it may have is checked from then on - a version
    /// larger than the new tier allows stays and can still be put online, a
    /// database fuller than its new room is read as before and refuses what
    /// would make it grow - and its domain is served or not. Moved to a tier
    /// that takes unused lambdas offline, it is measured from now on: what came
    /// before, when nothing would have taken it offline, does not count.
    /// Whether it connects is compiled in, so where the new tier's default
    /// changes that, it is built again on its next request.
    /// </remarks>
    public LambdaInfo ChangeTier(string privateKey, LambdaTier tier)
    {
        using var database = databases.CreateDbContext();

        var lambda = LambdaGuard.Require(database, privateKey);

        var current = limits.PolicyOf(lambda.Tier);
        var wanted = limits.PolicyOf(tier);

        if (lambda.Tier != tier && (!wanted.Assignable || !current.Assignable))
        {
            // the seeder retires every demo it no longer knows, so a lambda
            // moved in by hand would be deleted on the next start; one moved
            // out would be moved back
            throw LambdaException.Forbidden("The demo tier belongs to the installation's own demos. Nothing is moved into it or out of it by hand.");
        }

        if (lambda.Tier != tier && !wanted.Blockable && lambda.Blocked != null)
        {
            // it would be blocked and could not be unblocked, as nothing in
            // that tier is
            throw LambdaException.Conflict($"'{lambda.PublicKey}' is blocked, and a lambda in the tier '{wanted.Name}' cannot be: lift the block first.");
        }

        if (lambda.Tier != tier)
        {
            var previous = lambda.Tier;
            var now = DateTime.UtcNow;

            lambda.Tier = tier;
            lambda.TierChanged = now;
            lambda.Modified = now;

            // a request that waits for what the new tier allows by default
            // has nothing left to wait for (see ConnectivityRule.Allows)
            if (lambda.Connectivity == ConnectivityStatus.Requested && wanted.Connectivity)
            {
                lambda.Connectivity = null;
                lambda.ConnectivityChanged = null;
            }

            database.SaveChanges();

            // how large its database may grow, and how much its workspace
            // may write, is its tier's to say, from the next connection and
            // the next write on
            databaseVault.Invalidate(lambda.Id);

            // a domain is served or not by the tier, so the tier moving can
            // take one on or off the air without the domain itself changing
            domains.Reload();

            logger.LogInformation("Changed tier of lambda {Lambda} #{LambdaId} from {Previous} to {Tier}",
                                  lambda.PublicKey, lambda.Id, previous, tier);
        }

        return describer.Describe(database, lambda);
    }

    /// <summary>
    /// What an owner is told who asks for a domain where the tier of their
    /// lambda has none: which tier has, as the installation calls it.
    /// </summary>
    private string NoDomain(LambdaTier tier)
    {
        var current = limits.PolicyOf(tier);

        var with = limits.Above(tier) is { OwnDomain: true } above ? above : null;

        return with == null
            ? $"A domain of its own is not part of the tier '{current.Name}', which this lambda is in."
            : $"A domain of its own is part of the tier '{with.Name}', which this lambda is not in: its tier is '{current.Name}'. Whoever runs this installation decides the tier.";
    }

    #endregion

}
