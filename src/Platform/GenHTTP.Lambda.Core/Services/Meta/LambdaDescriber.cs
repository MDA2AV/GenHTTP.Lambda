using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// A lambda as the outside is told about it: the editor, an agent, the panel.
/// </summary>
/// <remarks>
/// Mapped by hand from the row, so that what leaves the service is chosen
/// field by field rather than whatever the row happens to carry. Includes the
/// two deadlines the sweep will act on (see <see cref="LambdaLifetime"/>), so
/// the editor can say when rather than leaving it to be discovered.
/// </remarks>
public sealed class LambdaDescriber(LambdaLifetime lifetime, ILambdaAddresses addresses, ILimitsService limits)
{

    /// <summary>
    /// What the editor is told about a lambda.
    /// </summary>
    public async ValueTask<LambdaInfo> DescribeAsync(LambdaDbContext database, LambdaEntity lambda, CancellationToken cancellation = default)
    {
        var latest = await database.Deployments.Where(d => d.LambdaId == lambda.Id)
                                   .MaxAsync(d => (int?)d.Version, cancellation);

        var policy = limits.PolicyOf(lambda.Tier);

        return new LambdaInfo(lambda.PublicKey, lambda.PrivateKey, LambdaTierNames.Name(lambda.Tier), policy, limits.Above(lambda.Tier), lambda.Created, lambda.Modified,
                              lambda.ActiveVersion, latest, lambda.Deployed, lifetime.DeployedUntil(lambda), lifetime.KeptUntil(lambda),
                              lambda.Domain, EditorViewNames.Name(lambda.View), addresses.Of(lambda.PublicKey),
                              addresses.Of(lambda.PublicKey, lambda.Tier, lambda.Domain), lambda.Blocked, lambda.BlockReason,
                              ConnectivityRule.Describe(lambda, policy));
    }

    /// <summary>
    /// What the panel lists of a lambda.
    /// </summary>
    /// <param name="latest">Its newest version, where it has one</param>
    /// <param name="versions">How many versions it keeps</param>
    public LambdaOverview Overview(LambdaEntity lambda, int? latest, int versions)
        => new(lambda.PublicKey,
               lambda.PrivateKey,
               LambdaTierNames.Name(lambda.Tier),
               limits.PolicyOf(lambda.Tier),
               lambda.Created,
               lambda.Modified,
               lambda.ActiveVersion,
               latest,
               versions,
               lifetime.DeployedUntil(lambda),
               lifetime.KeptUntil(lambda),
               lambda.Domain,
               lambda.Blocked,
               ConnectivityRule.Describe(lambda, limits.PolicyOf(lambda.Tier)));

}
