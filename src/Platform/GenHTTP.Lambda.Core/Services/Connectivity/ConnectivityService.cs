using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Connectivity;

/// <summary>
/// Keeps where each lambda's connectivity stands, and changes it as its owner asks
/// and the operator decides.
/// </summary>
/// <remarks>
/// Written through Entity Framework like every change to a lambda, so serving
/// finds out at once (see <c>DatabaseChanges</c>): the lambda it resolves
/// next carries the new answer, and the build compiled under the old one is
/// replaced on that request.
/// </remarks>
public sealed class ConnectivityService(IDbContextFactory<LambdaDbContext> databases, IMetaService meta, ILimitsService limits,
                                        ILogger<ConnectivityService> logger) : IConnectivityService
{
    private const string Missing = "This lambda does not exist (or has been deleted).";

    #region Owner

    public async ValueTask<ConnectivityInfo> GetAsync(string privateKey, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return Describe(await LambdaGuard.RequireAsync(database, privateKey, cancellation));
    }

    public async ValueTask<ConnectivityInfo> RequestAsync(string privateKey, string? purpose, CancellationToken cancellation = default)
    {
        var said = VersionInput.Tidy(purpose?.Replace("\r\n", "\n"), ConnectivityRule.MaxPurpose)
                ?? throw LambdaException.Invalid("Say what the lambda connects to and why - the addresses or services, and what for. The operator decides on that.");

        // refuses a demo and a blocked lambda, and notes where the change came from
        var id = await meta.RequireEditableAsync(privateKey, cancellation);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.FirstOrDefaultAsync(l => l.Id == id, cancellation) ?? throw LambdaException.NotFound(Missing);

        if (ConnectivityRule.Allows(limits.PolicyOf(lambda.Tier), lambda.Connectivity))
        {
            // there is nothing to wait for: the answer says it may
            return Describe(lambda);
        }

        if (lambda.Connectivity != ConnectivityStatus.Requested)
        {
            // asked anew: it waits from now, and what was said about the
            // last request does not answer this one
            lambda.Connectivity = ConnectivityStatus.Requested;
            lambda.ConnectivityChanged = DateTime.UtcNow;
            lambda.ConnectivityNote = null;
        }

        lambda.ConnectivityPurpose = said;

        await database.SaveChangesAsync(cancellation);

        logger.LogInformation("Requested connectivity for lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);

        return Describe(lambda);
    }

    public async ValueTask<ConnectivityInfo> WithdrawAsync(string privateKey, CancellationToken cancellation = default)
    {
        var id = await meta.RequireEditableAsync(privateKey, cancellation);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.FirstOrDefaultAsync(l => l.Id == id, cancellation) ?? throw LambdaException.NotFound(Missing);

        if (lambda.Connectivity is ConnectivityStatus.Requested or ConnectivityStatus.Allowed)
        {
            var previous = lambda.Connectivity;

            lambda.Connectivity = null;
            lambda.ConnectivityChanged = null;
            lambda.ConnectivityPurpose = null;
            lambda.ConnectivityNote = null;

            await database.SaveChangesAsync(cancellation);

            logger.LogInformation(previous == ConnectivityStatus.Allowed ? "Gave up connectivity of lambda {Lambda} #{LambdaId}" : "Withdrew connectivity request of lambda {Lambda} #{LambdaId}",
                                  lambda.PublicKey, lambda.Id);
        }

        return Describe(lambda);
    }

    #endregion

    #region Operator

    public async ValueTask<ConnectivityInfo> DecideAsync(string privateKey, bool allow, string? note, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await LambdaGuard.RequireAsync(database, privateKey, cancellation);

        var policy = limits.PolicyOf(lambda.Tier);

        if (policy.ReadOnly)
        {
            throw LambdaException.Forbidden($"'{lambda.PublicKey}' is a demo, which the installation keeps: demos do not connect to other servers, and there is nothing to decide.");
        }

        // nothing about a blocked lambda changes but its block
        LambdaGuard.EnsureUnblocked(lambda);

        var wanted = allow ? ConnectivityStatus.Allowed : ConnectivityStatus.Declined;

        // what its tier allows by default may be declined as well: it is a
        // grant of the lambda's, which the tier only starts it with
        if (!allow && lambda.Connectivity != ConnectivityStatus.Requested && !ConnectivityRule.Allows(policy, lambda.Connectivity))
        {
            throw LambdaException.Conflict($"Nothing was asked for '{lambda.PublicKey}', and nothing is allowed: there is nothing to decline.");
        }

        var said = VersionInput.Tidy(note?.Replace("\r\n", "\n"), ConnectivityRule.MaxNote);

        if (lambda.Connectivity != wanted || lambda.ConnectivityNote != said)
        {
            if (lambda.Connectivity != wanted)
            {
                lambda.Connectivity = wanted;
                lambda.ConnectivityChanged = DateTime.UtcNow;
            }

            lambda.ConnectivityNote = said;

            await database.SaveChangesAsync(cancellation);

            logger.LogInformation(allow ? "Allowed connectivity for lambda {Lambda} #{LambdaId}" : "Declined connectivity for lambda {Lambda} #{LambdaId}",
                                  lambda.PublicKey, lambda.Id);
        }

        return Describe(lambda);
    }

    public async ValueTask<IReadOnlyList<ConnectivityListing>> ListAsync(CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambdas = await database.Lambdas.AsNoTracking()
                                    .Where(l => l.Connectivity == ConnectivityStatus.Requested || l.Connectivity == ConnectivityStatus.Allowed)
                                    .ToListAsync(cancellation);

        var ids = lambdas.Select(l => l.Id).ToList();

        var latest = await database.Deployments.AsNoTracking()
                                   .Where(d => ids.Contains(d.LambdaId))
                                   .GroupBy(d => d.LambdaId)
                                   .Select(g => new { LambdaId = g.Key, Version = g.Max(d => d.Version) })
                                   .ToDictionaryAsync(g => g.LambdaId, g => (int?)g.Version, cancellation);

        // the ones waiting first, the longest waiting at the top; then what
        // was allowed, the newest first, to be looked at again
        return [.. lambdas.Where(l => l.Connectivity == ConnectivityStatus.Requested).OrderBy(l => l.ConnectivityChanged)
                          .Concat(lambdas.Where(l => l.Connectivity == ConnectivityStatus.Allowed).OrderByDescending(l => l.ConnectivityChanged))
                          .Select(l => new ConnectivityListing(l.PublicKey, l.Tier.ToString(), l.ActiveVersion, l.Blocked, Describe(l),
                                                         Connects(l.Id, l.ActiveVersion, latest.GetValueOrDefault(l.Id))))];
    }

    private ConnectivityInfo Describe(LambdaEntity lambda) => ConnectivityRule.Describe(lambda, limits.PolicyOf(lambda.Tier));

    public bool Connects(long lambdaId, int? live, int? latest)
    {
        // read off each version once and kept, since a version never changes
        return meta.GetFacts(lambdaId, live).UsesNetwork || (latest != live && meta.GetFacts(lambdaId, latest).UsesNetwork);
    }

    #endregion

}
