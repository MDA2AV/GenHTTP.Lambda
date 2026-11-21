using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Describes a lambda for the API.
/// </summary>
/// <remarks>
/// Lives here rather than in the resources that answer with it: there is more
/// than one of those, and a copy each is a copy to forget when the record
/// gains a field - which compiles right up until the copy is in another file.
/// </remarks>
public static class LambdaDescription
{
    public static LambdaResponse Of(LambdaInfo lambda) => new(
        lambda.PublicKey,
        lambda.PrivateKey,
        lambda.Tier,
        TierDescription.Of(lambda.Policy, lambda.Above),
        lambda.Created,
        lambda.Modified,
        lambda.ActiveVersion,
        lambda.LatestVersion,
        lambda.PublicUrl,
        PlatformRoutes.EditorOf(lambda.PrivateKey),
        lambda.DeployedAt,
        lambda.DeployedUntil,
        lambda.KeptUntil,
        lambda.Domain,
        lambda.Domain != null && lambda.Policy.OwnDomain,
        lambda.Address,
        lambda.View,
        PlatformRoutes.RepositoryOf(lambda.PrivateKey, lambda.PublicKey),
        lambda.Blocked,
        lambda.BlockReason,
        ConnectivityResponse.Of(lambda.Connectivity)
    );
}
