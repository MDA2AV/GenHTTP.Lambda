using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Egress;

/// <summary>
/// Keeps where each lambda's egress stands, and changes it as its owner asks
/// and the operator decides.
/// </summary>
/// <remarks>
/// Written through Entity Framework like every change to a lambda, so serving
/// finds out at once (see <c>DatabaseChanges</c>): the lambda it resolves
/// next carries the new answer, and the build compiled under the old one is
/// replaced on that request.
/// </remarks>
public sealed class EgressService(IDbContextFactory<LambdaDbContext> databases, IMetaService meta, ILogger<EgressService> logger) : IEgressService
{
    private const string Missing = "This lambda does not exist (or has been deleted).";

    #region Owner

    public EgressInfo Get(string privateKey)
    {
        using var database = databases.CreateDbContext();

        return EgressRule.Describe(LambdaGuard.Require(database, privateKey));
    }

    public EgressInfo Request(string privateKey, string? purpose)
    {
        var said = VersionInput.Tidy(purpose?.Replace("\r\n", "\n"), EgressRule.MaxPurpose)
                ?? throw LambdaException.Invalid("Say what the lambda connects to and why - the addresses or services, and what for. The operator decides on that.");

        // refuses a demo and a blocked lambda, and notes where the change came from
        var id = meta.RequireEditable(privateKey);

        using var database = databases.CreateDbContext();

        var lambda = database.Lambdas.FirstOrDefault(l => l.Id == id) ?? throw LambdaException.NotFound(Missing);

        if (EgressRule.Allows(lambda.Tier, lambda.Egress))
        {
            // there is nothing to wait for: the answer says it may
            return EgressRule.Describe(lambda);
        }

        if (lambda.Egress != EgressStatus.Requested)
        {
            // asked anew: it waits from now, and what was said about the
            // last request does not answer this one
            lambda.Egress = EgressStatus.Requested;
            lambda.EgressChanged = DateTime.UtcNow;
            lambda.EgressNote = null;
        }

        lambda.EgressPurpose = said;

        database.SaveChanges();

        logger.LogInformation("Requested egress for lambda {Lambda} #{LambdaId}", lambda.PublicKey, lambda.Id);

        return EgressRule.Describe(lambda);
    }

    public EgressInfo Withdraw(string privateKey)
    {
        var id = meta.RequireEditable(privateKey);

        using var database = databases.CreateDbContext();

        var lambda = database.Lambdas.FirstOrDefault(l => l.Id == id) ?? throw LambdaException.NotFound(Missing);

        if (lambda.Egress is EgressStatus.Requested or EgressStatus.Allowed)
        {
            var previous = lambda.Egress;

            lambda.Egress = null;
            lambda.EgressChanged = null;
            lambda.EgressPurpose = null;
            lambda.EgressNote = null;

            database.SaveChanges();

            logger.LogInformation(previous == EgressStatus.Allowed ? "Gave up egress of lambda {Lambda} #{LambdaId}" : "Withdrew egress request of lambda {Lambda} #{LambdaId}",
                                  lambda.PublicKey, lambda.Id);
        }

        return EgressRule.Describe(lambda);
    }

    #endregion

    #region Operator

    public EgressInfo Decide(string privateKey, bool allow, string? note)
    {
        using var database = databases.CreateDbContext();

        var lambda = LambdaGuard.Require(database, privateKey);

        if (lambda.Tier == LambdaTier.Demo)
        {
            throw LambdaException.Forbidden($"'{lambda.PublicKey}' is a demo, which the installation keeps: demos do not connect to other servers, and there is nothing to decide.");
        }

        // nothing about a blocked lambda changes but its block
        LambdaGuard.EnsureUnblocked(lambda);

        var wanted = allow ? EgressStatus.Allowed : EgressStatus.Declined;

        if (!allow && lambda.Egress is not (EgressStatus.Requested or EgressStatus.Allowed))
        {
            throw LambdaException.Conflict($"Nothing was asked for '{lambda.PublicKey}', and nothing is allowed: there is nothing to decline.");
        }

        var said = VersionInput.Tidy(note?.Replace("\r\n", "\n"), EgressRule.MaxNote);

        if (lambda.Egress != wanted || lambda.EgressNote != said)
        {
            if (lambda.Egress != wanted)
            {
                lambda.Egress = wanted;
                lambda.EgressChanged = DateTime.UtcNow;
            }

            lambda.EgressNote = said;

            database.SaveChanges();

            logger.LogInformation(allow ? "Allowed egress for lambda {Lambda} #{LambdaId}" : "Declined egress for lambda {Lambda} #{LambdaId}",
                                  lambda.PublicKey, lambda.Id);
        }

        return EgressRule.Describe(lambda);
    }

    public IReadOnlyList<EgressListing> List()
    {
        using var database = databases.CreateDbContext();

        var lambdas = database.Lambdas.AsNoTracking()
                              .Where(l => l.Egress == EgressStatus.Requested || l.Egress == EgressStatus.Allowed)
                              .ToList();

        var ids = lambdas.Select(l => l.Id).ToList();

        var latest = database.Deployments.AsNoTracking()
                             .Where(d => ids.Contains(d.LambdaId))
                             .GroupBy(d => d.LambdaId)
                             .Select(g => new { LambdaId = g.Key, Version = g.Max(d => d.Version) })
                             .ToDictionary(g => g.LambdaId, g => (int?)g.Version);

        // the ones waiting first, the longest waiting at the top; then what
        // was allowed, the newest first, to be looked at again
        return [.. lambdas.Where(l => l.Egress == EgressStatus.Requested).OrderBy(l => l.EgressChanged)
                          .Concat(lambdas.Where(l => l.Egress == EgressStatus.Allowed).OrderByDescending(l => l.EgressChanged))
                          .Select(l => new EgressListing(l.PublicKey, l.Tier.ToString(), l.ActiveVersion, l.Blocked, EgressRule.Describe(l),
                                                         Connects(l.Id, l.ActiveVersion, latest.GetValueOrDefault(l.Id))))];
    }

    public bool Connects(long lambdaId, int? live, int? latest)
    {
        // read off each version once and kept, since a version never changes
        return meta.GetFacts(lambdaId, live).UsesNetwork || (latest != live && meta.GetFacts(lambdaId, latest).UsesNetwork);
    }

    #endregion

}
