using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Services.Source;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// The public pages of a site - its front page, its guide, its showcase - each
/// named in its language before it is sent, and with the content the build
/// rendered for it, so a crawler that runs no script reads the page it asked
/// for.
/// </summary>
/// <remarks>
/// What a host's site is built from, at the root of the platform's host, behind
/// every component mounted at a path of its own. It answers what a site
/// answers for the host as well: <c>robots.txt</c>, the sitemap of its pages
/// and of every published source, and <c>llms.txt</c>.
/// </remarks>
public static class SitePages
{

    /// <summary>
    /// The pages of the given bundle, over the platform.
    /// </summary>
    public static SitePagesBuilder Create(LambdaPlatform platform, Bundle bundle) => new(platform, bundle);

}

public sealed class SitePagesBuilder(LambdaPlatform platform, Bundle bundle) : IHandlerBuilder<SitePagesBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private Func<SitePage, string?>? _head;

    private bool _catalog;

    #region Functionality

    /// <summary>
    /// More for the head of a page, beside its names - what a site says about
    /// itself on its front page, for one.
    /// </summary>
    public SitePagesBuilder Head(Func<SitePage, string?> head)
    {
        _head = head;
        return this;
    }

    /// <summary>
    /// Whether the site describes its MCP server and its API to agents that
    /// look for one at <c>/.well-known/ai-catalog.json</c>.
    /// </summary>
    public SitePagesBuilder Catalog(bool catalog = true)
    {
        _catalog = catalog;
        return this;
    }

    public SitePagesBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var services = platform.Services;
        var publicUrl = platform.Options.PublicUrl;

        var meta = new SiteMeta(bundle, publicUrl);

        var prerender = new SitePrerender(bundle, publicUrl, services.GetRequiredService<ILimitsService>(), services.GetRequiredService<IShowcaseService>(),
                                          services.GetRequiredService<ILambdaAddresses>(), platform.Loggers.CreateLogger<SitePrerender>());

        // ranges because a front page may show a video, and Safari will not
        // play one from a server that answers a range with the whole file
        IHandlerBuilder files = GenHTTP.Modules.Files.Assets.From(bundle.Files()).Add(RangeSupport.Create()).Add(CacheControl.NoCache());

        if (bundle.HasAssets)
        {
            files = Layout.Create().Add("assets", bundle.Assets()).Add(files);
        }

        var pages = new SitePagesHandler(bundle, meta, prerender, services.GetRequiredService<ISourceService>(), files.Build(), _head, _catalog);

        return Concerns.Chain(_concerns, pages);
    }

    #endregion

}

/// <summary>
/// Answers the paths of a site.
/// </summary>
internal sealed class SitePagesHandler(Bundle bundle, SiteMeta meta, SitePrerender prerender, ISourceService sources, IHandler files,
                                       Func<SitePage, string?>? head, bool catalog) : IHandler
{

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => files.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Header.Method != RequestMethod.Get && request.Header.Method != RequestMethod.Head)
        {
            return null;
        }

        var path = request.Header.Path.ToString();

        switch (path)
        {
            case "/robots.txt":
                return Answer(request, meta.Robots(), "text/plain; charset=utf-8");

            case "/sitemap.xml":
                var sitemap = meta.Sitemap(sources.ListAddresses());

                // without a public address there is nothing to list pages
                // under, and the index page is not a sitemap either
                return sitemap == null ? null : Answer(request, sitemap, "application/xml; charset=utf-8");

            case "/llms.txt":
                return Answer(request, meta.LlmsText(), "text/markdown; charset=utf-8");

            case "/.well-known/ai-catalog.json" when catalog:
                var described = meta.AiCatalog();
                return described == null ? null : Answer(request, described, "application/json; charset=utf-8");
        }

        if (meta.Find(path) is { } page)
        {
            var markup = meta.Render(bundle.Index(), page);

            if (head?.Invoke(page) is { } more)
            {
                markup = SiteMeta.InHead(markup, more);
            }

            markup = prerender.Render(markup, page, request);

            return Answer(request, markup, "text/html; charset=utf-8");
        }

        if (ToLanguage(request, path) is { } redirect)
        {
            return redirect;
        }

        // the root of a site that names no page there - one without a build
        if (path == "/")
        {
            return Revalidated(bundle.Answer(request));
        }

        // a file of the bundle
        if (await files.HandleAsync(request) is { } file)
        {
            return file;
        }

        // a person is shown the application's own page for it, and anything
        // else - a crawler looking for a file, a checker - learns it is not here
        return PrefersMarkup(request) ? Revalidated(bundle.Answer(request, ResponseStatus.NotFound)) : null;
    }

    private static bool PrefersMarkup(IRequest request)
        => request.Header.Headers.GetEntry("Accept")?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// A public page asked for without a language, sent on to the one the
    /// visitor prefers: the one they chose before, or the one their browser
    /// asks for. Not cached, since it depends on who is asking.
    /// </summary>
    private IResponse? ToLanguage(IRequest request, string path)
    {
        var normalized = SiteMeta.Normalize(path);

        if (!meta.IsPage(normalized))
        {
            return null;
        }

        var headers = request.Header.Headers;

        var language = SiteLanguages.Negotiate(headers.GetCookie(SiteLanguages.Cookie), headers.GetEntry("Accept-Language"));

        return request.Respond()
                      .Status(ResponseStatus.Found)
                      .Header("Location", SiteLanguages.In(language, normalized) + RequestOrigin.Query(request))
                      .Header("Vary", "Accept-Language, Cookie")
                      .Build();
    }

    /// <summary>
    /// What is made of the index page is asked for again every time: it names
    /// the scripts of the build it was made from, which a new build replaces.
    /// </summary>
    private static IResponse Revalidated(IResponse response)
    {
        response.Rebuild().Header("Cache-Control", "no-cache");
        return response;
    }

    private static IResponse Answer(IRequest request, string body, string type)
        => Revalidated(request.Respond()
                              .Content(Resource.FromString(body).Type(new ContentType(type)).Build())
                              .Build());

    #endregion

}
