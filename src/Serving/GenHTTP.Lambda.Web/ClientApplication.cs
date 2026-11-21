using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Branding;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.SinglePageApplications;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// A bundle whose pages are all drawn in the browser - the editor, the panel -
/// served below the path its component is mounted at.
/// </summary>
/// <remarks>
/// Every path the bundle has no file for is one of its client routes and gets
/// the index page, uncached so a new build is picked up; its scripts and
/// styles are cached for good. Nothing about such a page is for a search
/// engine, so none of them is named on the server: the bundle names them in
/// the browser, and says they are not to be indexed. Given the installation's
/// brand, its index page names the installation as its operator did, and
/// shows their icon (see <see cref="PageBranding"/>).
/// </remarks>
public static class ClientApplication
{

    /// <summary>
    /// Serves the given bundle.
    /// </summary>
    public static ClientApplicationBuilder From(Bundle bundle) => new(bundle);

}
