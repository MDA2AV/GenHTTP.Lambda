using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;

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
public sealed class ConnectivityService(IDbContextFactory<LambdaDbContext> databases, IMetaService meta, ILogger<ConnectivityService> logger) : IConnectivityService
{
    private const string Missing = "This lambda does not exist (or has been deleted).";

    #region Owner

    public ConnectivityInfo Get(string privateKey)
    {
        using var database = databases.CreateDbContext();

        return ConnectivityRule.Describe(LambdaGuard.Require(database, privateKey));
    }

    public ConnectivityInfo Request(string privateKey, string? purpose)
    {
        var said = VersionInput.Tidy(purpose?.Replace("\r\n", "\n"), ConnectivityRule.MaxPurpose)
                ?? throw LambdaException.Invalid("Say what the lambda connects to and why - the addresses or services, and what for. The operator decides on that.");

        // refuses a demo and a blocked lambda, and notes where the change came from
        var id = meta.RequireEditable(privateKey);

        using var database = databases.CreateDbContext();

        var lambda = database.Lambdas.FirstOrDefault(l => l.Id == id) ?? throw LambdaException.NotFound(Missing);

        if (ConnectivityRule.Allows(lambda.Tier, lambda.Connectivity))
        {
            // there is nothing to wait for: the answer says it may
            return ConnectivityRule.Describe(lambda);
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

        database.SaveChanges();

        logger.LogInformation("Requested connectivity for lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);

        return ConnectivityRule.Describe(lambda);
    }

    public ConnectivityInfo Withdraw(string privateKey)
    {
        var id = meta.RequireEditable(privateKey);

        using var database = databases.CreateDbContext();

        var lambda = database.Lambdas.FirstOrDefault(l => l.Id == id) ?? throw LambdaException.NotFound(Missing);

        if (lambda.Connectivity is ConnectivityStatus.Requested or ConnectivityStatus.Allowed)
        {
            var previous = lambda.Connectivity;

            lambda.Connectivity = null;
            lambda.ConnectivityChanged = null;
            lambda.ConnectivityPurpose = null;
            lambda.ConnectivityNote = null;

            database.SaveChanges();

            logger.LogInformation(previous == ConnectivityStatus.Allowed ? "Gave up connectivity of lambda {Lambda} #{LambdaId}" : "Withdrew connectivity request of lambda {Lambda} #{LambdaId}",
                                  lambda.PublicKey, lambda.Id);
        }

        return ConnectivityRule.Describe(lambda);
    }

    #endregion

    #region Operator

    public ConnectivityInfo Decide(string privateKey, bool allow, string? note)
    {
        using var database = databases.CreateDbContext();

        var lambda = LambdaGuard.Require(database, privateKey);

        if (lambda.Tier == LambdaTier.Demo)
        {
            throw LambdaException.Forbidden($"'{lambda.PublicKey}' is a demo, which the installation keeps: demos do not connect to other servers, and there is nothing to decide.");
        }

        // nothing about a blocked lambda changes but its block
        LambdaGuard.EnsureUnblocked(lambda);

        var wanted = allow ? ConnectivityStatus.Allowed : ConnectivityStatus.Declined;

        if (!allow && lambda.Connectivity is not (ConnectivityStatus.Requested or ConnectivityStatus.Allowed))
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

            database.SaveChanges();

            logger.LogInformation(allow ? "Allowed connectivity for lambda {Lambda} #{LambdaId}" : "Declined connectivity for lambda {Lambda} #{LambdaId}",
                                  lambda.PublicKey, lambda.Id);
        }

        return ConnectivityRule.Describe(lambda);
    }

    public IReadOnlyList<ConnectivityListing> List()
    {
        using var database = databases.CreateDbContext();

        var lambdas = database.Lambdas.AsNoTracking()
                              .Where(l => l.Connectivity == ConnectivityStatus.Requested || l.Connectivity == ConnectivityStatus.Allowed)
                              .ToList();

        var ids = lambdas.Select(l => l.Id).ToList();

        var latest = database.Deployments.AsNoTracking()
                             .Where(d => ids.Contains(d.LambdaId))
                             .GroupBy(d => d.LambdaId)
                             .Select(g => new { LambdaId = g.Key, Version = g.Max(d => d.Version) })
                             .ToDictionary(g => g.LambdaId, g => (int?)g.Version);

        // the ones waiting first, the longest waiting at the top; then what
        // was allowed, the newest first, to be looked at again
        return [.. lambdas.Where(l => l.Connectivity == ConnectivityStatus.Requested).OrderBy(l => l.ConnectivityChanged)
                          .Concat(lambdas.Where(l => l.Connectivity == ConnectivityStatus.Allowed).OrderByDescending(l => l.ConnectivityChanged))
                          .Select(l => new ConnectivityListing(l.PublicKey, l.Tier.ToString(), l.ActiveVersion, l.Blocked, ConnectivityRule.Describe(l),
                                                         Connects(l.Id, l.ActiveVersion, latest.GetValueOrDefault(l.Id))))];
    }

    public bool Connects(long lambdaId, int? live, int? latest)
    {
        // read off each version once and kept, since a version never changes
        return meta.GetFacts(lambdaId, live).UsesNetwork || (latest != live && meta.GetFacts(lambdaId, latest).UsesNetwork);
    }

    #endregion

}
