using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Asks for a new lambda.")]
public sealed record CreateLambdaRequest(
    [Summary("The public key to claim, which names the address: 3 to 40 characters of a-z, 0-9 and dashes. Left out, a random one is chosen.")]
    string? PublicKey,
    [Summary("Whether the terms of the installation (`terms` of `GET /system`) were accepted. Nothing is created unless this is true.")]
    bool AcceptedTerms,
    [Summary("What the lambda starts as: the `id` of a demo, as `GET /demos` and the starters of `GET /system` list them, or `empty`. Left out, empty.")]
    string? Template = null,
    [Summary("How its editor opens: `Full` (the default) with the code, files and versions, or `Simple` - the app, how it is doing and where to ask for a change, for somebody who is not going to read the code. Case does not matter.")]
    string? View = null
);

[Summary("The changes to make to a lambda. What is left out stays as it is.")]
public sealed record UpdateLambdaRequest(
    [Summary("The public key to move the lambda to, under the same rules as when it is created")]
    string? PublicKey,
    [Summary("How its editor opens for somebody who has not chosen a view of their own: `Full` or `Simple`")]
    string? View = null
);

[Summary("A lambda, as whoever holds its editor key sees it.")]
public sealed record LambdaResponse(
    [Summary("Names its address below the hosting domain. Not a secret; its owner may change it.")]
    string PublicKey,
    [Summary("The editor key, which authorizes every call about the lambda. Treat it like a password.")]
    string PrivateKey,
    [Summary("`Free`, `Premium` or `Demo`, assigned by the operator. Decides its limits, whether it is kept online while nobody uses it, and whether it may answer at a domain of its own.")]
    string Tier,
    [Summary("When it was created")]
    DateTime Created,
    [Summary("When something about it last changed: a save, a deployment, a setting")]
    DateTime Modified,
    [Summary("The version that is online; absent while nothing is")]
    int? ActiveVersion,
    [Summary("The newest version saved, which may be ahead of the one online")]
    int? LatestVersion,
    [Summary("Its address below the hosting domain, named after its public key: `https://quiz.genhttp.run/`. While it answers at a domain of its own, this address sends its visitors on to the domain.")]
    string PublicUrl,
    [Summary("Where its editor is on this site: `/editor/{privateKey}`. Holds the editor key.")]
    string EditorPath,
    [Summary("When the version online went online; absent while nothing is")]
    DateTime? DeployedAt,
    [Summary("When it is taken offline unless it is visited or edited before then - each visit and each edit moves this on. Absent while it is offline, and for a tier that keeps it online.")]
    DateTime? DeployedUntil,
    [Summary("When it is deleted unless it is visited or edited before then. Absent for a tier that keeps it.")]
    DateTime? KeptUntil,
    [Summary("The domain of its own it is configured to answer at, whether or not its tier lets it")]
    string? Domain,
    [Summary("Whether it answers at that domain: it has one, and its tier includes it")]
    bool DomainServed,
    [Summary("Where to link to it: its domain while that is served, `publicUrl` otherwise")]
    string Address,
    [Summary("How its editor opens for somebody who has not chosen a view of their own: `Full` or `Simple`")]
    string View,
    [Summary("Where it is cloned with git and pushed to, on this site. The versions are the commits of `main`, the features the other branches. Holds the editor key, like the editor's path.")]
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

[Summary("Everything anybody may know about a public key.")]
public sealed record KeyResponse(
    [Summary("The key as it would be stored: trimmed and lower cased")]
    string PublicKey,
    [Summary("Whether it is spelled as a key may be: 3 to 40 characters of a-z, 0-9 and dashes, not starting or ending with a dash")]
    bool Valid,
    [Summary("Whether a new lambda could be created with it right now")]
    bool Available,
    [Summary("Whether a lambda has it")]
    bool Exists,
    [Summary("Whether that lambda is online")]
    bool Deployed,
    [Summary("Why it cannot be claimed, where it cannot - spelling, a reserved name, a key kept for the demos")]
    string? Reason
);
