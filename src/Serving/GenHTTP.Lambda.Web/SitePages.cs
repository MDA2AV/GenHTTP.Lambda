namespace GenHTTP.Lambda.Web;

/// <summary>
/// The public pages of a site - its front page, its showcase - each
/// named in its language before it is sent, and with the content the build
/// rendered for it, so a crawler that runs no script reads the page it asked
/// for.
/// </summary>
/// <remarks>
/// What a host's site is built from, at the root of the platform's host, behind
/// every component mounted at a path of its own. It answers what a site
/// answers for the host as well: <c>robots.txt</c>, the sitemap of its pages
/// and of every published source, and <c>llms.txt</c>.
///
/// Every page is the installation's as it presents itself (see
/// <see cref="PageBranding"/>): GenHTTP Lambda, or what its operator named
/// it - with the manifest naming it too - and speaks to whom it is for, as
/// its operator chose it (see <see cref="SitePrerender"/>).
/// </remarks>
public static class SitePages
{

    /// <summary>
    /// The pages of the given bundle, over the platform.
    /// </summary>
    public static SitePagesBuilder Create(LambdaPlatform platform, Bundle bundle) => new(platform, bundle);

}
