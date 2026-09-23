using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Asks for a new lambda. The key is optional, a short random one is generated
/// if none is given.
/// </summary>
public sealed record CreateLambdaRequest(string? PublicKey, bool AcceptedTerms, string? Template = null);

/// <summary>
/// The key a lambda should move to.
/// </summary>
public sealed record ChangeKeyRequest(string PublicKey);

/// <summary>
/// A lambda as the editor sees it.
/// </summary>
public sealed record LambdaResponse(
    string PublicKey,
    string PrivateKey,
    string Tier,
    DateTime Created,
    DateTime Modified,
    int? ActiveVersion,
    int? LatestVersion,
    string PublicPath,
    string EditorPath,
    DateTime? DeployedAt,
    DateTime? DeployedUntil,
    DateTime KeptUntil
);

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
        lambda.Created,
        lambda.Modified,
        lambda.ActiveVersion,
        lambda.LatestVersion,
        $"/lambda/{lambda.PublicKey}/",
        $"/editor/{lambda.PrivateKey}",
        lambda.DeployedAt,
        lambda.DeployedUntil,
        lambda.KeptUntil
    );
}

/// <summary>
/// Whether a key could be claimed.
/// </summary>
public sealed record AvailabilityResponse(string PublicKey, bool Available, string? Reason);

/// <summary>
/// What is known publicly about a key.
/// </summary>
public sealed record StatusResponse(string PublicKey, bool Exists, bool Deployed);
