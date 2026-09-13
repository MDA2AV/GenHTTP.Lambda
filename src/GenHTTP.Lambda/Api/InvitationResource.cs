using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Creates a lambda and sends whoever asked straight into its editor, so a page
/// elsewhere can carry a "try this online" link.
/// </summary>
/// <remarks>
/// A link cannot post a body, which is what creating a lambda otherwise needs,
/// and a visitor arriving from one has not been shown anything to accept yet.
/// So the key is generated rather than asked for, and the editor is told the
/// lambda is new, which is where the terms are shown.
///
/// It lives beside the lambdas rather than under them: every single segment
/// there is already read as a private key, and a route that means something
/// else would be ambiguous with every lambda that exists.
/// </remarks>
public sealed class InvitationResource(IMetaService meta)
{

    /// <summary>
    /// Creates a lambda and redirects to its editor.
    /// </summary>
    /// <param name="template">The example to start from, the default one if omitted</param>
    /// <remarks>
    /// A caller asking for JSON is answered with the lambda instead, so a page
    /// that would rather redirect the browser itself can do that.
    /// </remarks>
    [ResourceMethod]
    public async ValueTask<Result<LambdaResponse>> Start(string? template, IRequest request)
    {
        var lambda = await meta.CreateAsync(null, template);

        var described = Describe(lambda);

        if (WantsJson(request))
        {
            return new Result<LambdaResponse>(described).Status(ResponseStatus.Created);
        }

        // see other rather than found: what follows is a page, and the browser
        // should ask for it with GET however it arrived here
        return new Result<LambdaResponse>(described).Status(ResponseStatus.SeeOther)
                                                    .Header("Location", $"{described.EditorPath}?created=1");
    }

    /// <summary>
    /// Whether the caller wants the lambda described rather than to be sent to it.
    /// </summary>
    private static bool WantsJson(IRequest request)
    {
        var accepted = request.Header.Headers.GetEntry("Accept");

        return accepted != null
            && accepted.Contains("application/json", StringComparison.OrdinalIgnoreCase)
            && !accepted.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static LambdaResponse Describe(LambdaInfo lambda) => new(
        lambda.PublicKey,
        lambda.PrivateKey,
        lambda.Tier,
        lambda.Created,
        lambda.Modified,
        lambda.ActiveVersion,
        lambda.LatestVersion,
        $"/lambda/{lambda.PublicKey}/",
        $"/editor/{lambda.PrivateKey}"
    );

}
