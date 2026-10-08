using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Asks for a new lambda. The key is optional, a short random one is generated
/// if none is given.
/// </summary>
/// <param name="View">
/// How its editor opens: Full, the default, or Simple - the app, how it is
/// doing and where to ask for a change, for somebody who is not going to
/// read the code
/// </param>
public sealed record CreateLambdaRequest(string? PublicKey, bool AcceptedTerms, string? Template = null, string? View = null);

/// <summary>
/// The changes to make to a lambda. What is left out stays as it is.
/// </summary>
/// <param name="PublicKey">The key the lambda should move to</param>
/// <param name="View">How its editor opens for somebody who has not chosen a view of their own: Full or Simple</param>
public sealed record UpdateLambdaRequest(string? PublicKey, string? View = null);

/// <summary>
/// A lambda as the editor sees it.
/// </summary>
/// <param name="DeployedUntil">When it goes offline unless used; absent while offline, or when its tier keeps it online</param>
/// <param name="KeptUntil">When it is removed unless used; absent when its tier keeps it</param>
/// <param name="Domain">The domain it is configured to answer at, whether or not its tier lets it</param>
/// <param name="PublicUrl">
/// Its address below the hosting domain, named after its key - which sends its visitors on to its domain while it
/// answers at one
/// </param>
/// <param name="DomainServed">Whether it actually answers at that domain - it has one, and its tier includes it</param>
/// <param name="Address">Where to link to it: its domain while that is served, its address below the hosting domain otherwise</param>
/// <param name="View">How its editor opens for somebody who has not chosen a view of their own: Full or Simple</param>
/// <param name="GitPath">
/// Where it is cloned with git, and pushed to: its versions are the commits of main, its features the other
/// branches. Holds the editor key, like the editor's path
/// </param>
public sealed record LambdaResponse(
    string PublicKey,
    string PrivateKey,
    string Tier,
    DateTime Created,
    DateTime Modified,
    int? ActiveVersion,
    int? LatestVersion,
    string PublicUrl,
    string EditorPath,
    DateTime? DeployedAt,
    DateTime? DeployedUntil,
    DateTime? KeptUntil,
    string? Domain,
    bool DomainServed,
    string Address,
    string View,
    string GitPath
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
        lambda.PublicUrl,
        $"/editor/{lambda.PrivateKey}",
        lambda.DeployedAt,
        lambda.DeployedUntil,
        lambda.KeptUntil,
        lambda.Domain,
        Serves(lambda.Tier, lambda.Domain),
        lambda.Address,
        lambda.View,
        GitPath(lambda.PrivateKey, lambda.PublicKey)
    );

    /// <summary>
    /// Where whoever holds the editor key clones the lambda: the editor's
    /// path, with a name for the folder a clone makes.
    /// </summary>
    public static string GitPath(string privateKey, string publicKey) => $"/editor/{privateKey}/{publicKey}.git";

    /// <summary>
    /// Whether a lambda of this tier may answer at a domain of its own.
    /// </summary>
    public static bool AllowsDomain(string tier) => tier == nameof(LambdaTier.Premium);

    /// <summary>
    /// Whether a lambda of this tier with this domain is answering at it, as
    /// <see cref="LambdaAddresses.Serves"/> decides.
    /// </summary>
    public static bool Serves(string tier, string? domain)
        => Enum.TryParse<LambdaTier>(tier, out var parsed) && LambdaAddresses.Serves(parsed, domain);
}

/// <summary>
/// Reads the view a lambda's editor is asked to open in.
/// </summary>
/// <remarks>
/// Shared by the REST API and the MCP, so both accept the same words and
/// refuse the rest with the same sentence.
/// </remarks>
public static class EditorViews
{

    /// <summary>
    /// The view that was asked for, in any case, or nothing when none was.
    /// </summary>
    public static EditorView? Parse(string? view)
    {
        if (string.IsNullOrWhiteSpace(view))
        {
            return null;
        }

        return Enum.TryParse<EditorView>(view.Trim(), true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : throw LambdaException.Invalid($"There is no view called '{view.Trim()}'. It is either 'Full' or 'Simple'.");
    }

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
