using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Sends whoever asked into the editor with a template already chosen, so a
/// page elsewhere can carry a "try this online" link.
/// </summary>
/// <remarks>
/// This used to create the lambda itself. A GET that creates something is an
/// anti-pattern for a reason that is not theoretical here: every crawler that
/// follows links would leave a lambda behind, and a link in a search result or
/// a chat preview would do the same without anybody clicking anything.
///
/// So nothing is created now. The link opens the editor with the template
/// filled in, and the lambda comes into existence when the visitor submits
/// that screen - which is also the only point at which they have been shown
/// the terms to accept.
///
/// It lives beside the lambdas rather than under them: every single segment
/// there is already read as a private key, and a route that means something
/// else would be ambiguous with every lambda that exists.
/// </remarks>
public sealed class InvitationResource
{

    /// <summary>
    /// Redirects to the editor, with the template chosen.
    /// </summary>
    /// <param name="template">The example to start from, the default one if omitted</param>
    /// <remarks>
    /// An unknown template is not an error: the link is likely to outlive the
    /// name it mentions, and dropping somebody on an error page because an
    /// example was renamed is worse than opening the editor without it.
    /// </remarks>
    [ResourceMethod]
    public Result<string> Start(string? template)
    {
        var chosen = TemplateCatalog.Find(template);

        var destination = chosen == null ? "/editor/create" : $"/editor/create?template={chosen.Id}";

        // see other rather than found: what follows is a page, and the browser
        // should ask for it with GET however it arrived here
        var separator = chosen == null ? "?" : "&";

        return new Result<string>(string.Empty).Status(ResponseStatus.SeeOther)
                                               .Header("Location", $"{destination}{separator}invited=1");
    }

}
