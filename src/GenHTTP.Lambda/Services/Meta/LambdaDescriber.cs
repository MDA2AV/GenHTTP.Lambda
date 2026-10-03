using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta.Model;

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
public sealed class LambdaDescriber(LambdaLifetime lifetime)
{

    /// <summary>
    /// What the editor is told about a lambda.
    /// </summary>
    public LambdaInfo Describe(LambdaDbContext database, LambdaEntity lambda)
    {
        var latest = database.Deployments.Where(d => d.LambdaId == lambda.Id)
                             .Max(d => (int?)d.Version);

        return new LambdaInfo(lambda.PublicKey, lambda.PrivateKey, lambda.Tier.ToString(), lambda.Created, lambda.Modified,
                              lambda.ActiveVersion, latest, lambda.Deployed, lifetime.DeployedUntil(lambda), lifetime.KeptUntil(lambda),
                              lambda.Domain, lambda.View.ToString());
    }

    /// <summary>
    /// What the panel lists of a lambda.
    /// </summary>
    /// <param name="latest">Its newest version, where it has one</param>
    /// <param name="versions">How many versions it keeps</param>
    public LambdaOverview Overview(LambdaEntity lambda, int? latest, int versions)
        => new(lambda.PublicKey,
               lambda.PrivateKey,
               lambda.Tier.ToString(),
               lambda.Created,
               lambda.Modified,
               lambda.ActiveVersion,
               latest,
               versions,
               lifetime.DeployedUntil(lambda),
               lifetime.KeptUntil(lambda),
               lambda.Domain);

}
