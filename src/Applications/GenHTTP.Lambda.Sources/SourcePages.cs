using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Git;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Sources;

/// <summary>
/// The published sources: their catalog at <c>/{language}/source</c>, the page
/// of each at <c>/{language}/source/{publicKey}</c>, and each as a git
/// repository at <c>/source/{publicKey}.git</c> for anybody to clone.
/// </summary>
/// <remarks>
/// Added to a site's layout without a path of its own, since its pages are
/// below the language they are in: it answers the paths that are its own and
/// passes on every other. A bundle of its own (Frontend/), which draws the
/// pages in the browser; the server names each after its lambda and marks it
/// up as source code, so a link to one reads as what it is.
/// </remarks>
/// <example>
/// <code>
/// Layout.Create().Add(SourcePages.Create(platform)).Add(site);
/// </code>
/// </example>
public static class SourcePages
{

    /// <summary>
    /// The published sources of the given platform.
    /// </summary>
    public static SourcePagesBuilder Create(LambdaPlatform platform) => new(platform);

}
