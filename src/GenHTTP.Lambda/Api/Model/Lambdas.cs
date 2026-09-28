using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Asks for a new lambda. The key is optional, a short random one is generated
/// if none is given.
/// </summary>
public sealed record CreateLambdaRequest(string? PublicKey, bool AcceptedTerms, string? Template = null);

/// <summary>
/// The changes to make to a lambda. What is left out stays as it is.
/// </summary>
/// <param name="PublicKey">The key the lambda should move to</param>
public sealed record UpdateLambdaRequest(string? PublicKey);

/// <summary>
/// A lambda as the editor sees it.
/// </summary>
/// <param name="DeployedUntil">When it goes offline unless used; absent while offline, or when its tier keeps it online</param>
/// <param name="KeptUntil">When it is removed unless used; absent when its tier keeps it</param>
/// <param name="Domain">The domain it is configured to answer at, whether or not its tier lets it</param>
/// <param name="DomainServed">Whether it actually answers at that domain - it has one, and its tier includes it</param>
/// <param name="Address">Where to link to it: its domain while that is served, its path otherwise</param>
/// <param name="ActiveRevision">Which save of the version online is being served; absent while offline</param>
/// <param name="ActiveChanged">
/// Whether the version online was saved over after it went online. Visitors get what was deployed, so deploying it
/// again is what puts the changes online.
/// </param>
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
    DateTime? KeptUntil,
    string? Domain,
    bool DomainServed,
    string Address,
    int? ActiveRevision,
    bool ActiveChanged
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
        lambda.KeptUntil,
        lambda.Domain,
        Serves(lambda.Tier, lambda.Domain),
        Address(lambda.PublicKey, lambda.Tier, lambda.Domain),
        lambda.ActiveRevision,
        lambda.ActiveChanged
    );

    /// <summary>
    /// Whether a lambda of this tier may answer at a domain of its own.
    /// </summary>
    public static bool AllowsDomain(string tier) => tier == nameof(LambdaTier.Premium);

    /// <summary>
    /// Whether a lambda of this tier with this domain is answering at it.
    /// </summary>
    public static bool Serves(string tier, string? domain) => domain != null && AllowsDomain(tier);

    /// <summary>
    /// Where anything linking to a lambda should point: the root of its own
    /// domain while it answers there, since that is the address its visitors
    /// know, and its path on the platform otherwise.
    /// </summary>
    public static string Address(string publicKey, string tier, string? domain)
        => Serves(tier, domain) ? $"https://{domain}/" : $"/lambda/{publicKey}/";
}

/// <summary>
/// Everything anybody may know about a public key.
/// </summary>
/// <param name="PublicKey">The key as it would be stored</param>
/// <param name="Valid">Whether it is a key a lambda could have</param>
/// <param name="Available">Whether a new lambda could be created with it</param>
/// <param name="Exists">Whether a lambda has it</param>
/// <param name="Deployed">Whether that lambda is online</param>
/// <param name="Reason">Why it cannot be claimed, if it cannot</param>
public sealed record KeyResponse(string PublicKey, bool Valid, bool Available, bool Exists, bool Deployed, string? Reason);
