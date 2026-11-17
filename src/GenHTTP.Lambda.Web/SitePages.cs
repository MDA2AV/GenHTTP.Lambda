using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Branding;
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
/// it - with the manifest naming it too.
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

    private readonly Dictionary<string, string> _moved = [];

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

    /// <summary>
    /// A page that is somewhere else now: asked for, in any language or
    /// none, it is sent on to where it went, in the same language.
    /// </summary>
    /// <param name="from">Where it was, without a language: "/platform"</param>
    /// <param name="to">Where it is, without a language: "/"</param>
    /// <remarks>
    /// Permanently, so a search engine moves what it knew of the page onto
    /// the page it went to; a page simply removed is not found instead.
    /// </remarks>
    public SitePagesBuilder Moved(string from, string to)
    {
        _moved[SiteMeta.Normalize(from)] = SiteMeta.Normalize(to);
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

        var pages = new SitePagesHandler(bundle, meta, prerender, services.GetRequiredService<ISourceService>(), services.GetRequiredService<IBrandingService>(),
                                         files.Build(), _head, _catalog, _moved);

        return Concerns.Chain(_concerns, pages);
    }

    #endregion

}

/// <summary>
/// Answers the paths of a site.
/// </summary>
internal sealed class SitePagesHandler(Bundle bundle, SiteMeta meta, SitePrerender prerender, ISourceService sources, IBrandingService branding,
                                       IHandler files, Func<SitePage, string?>? head, bool catalog, IReadOnlyDictionary<string, string> moved) : IHandler
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

        var brand = branding.Current;

        switch (path)
        {
            case "/robots.txt":
                return Answer(request, meta.Robots(brand), "text/plain; charset=utf-8");

            case "/sitemap.xml":
                var sitemap = meta.Sitemap(sources.ListAddresses());

                // without a public address there is nothing to list pages
                // under, and the index page is not a sitemap either
                return sitemap == null ? null : Answer(request, sitemap, "application/xml; charset=utf-8");

            case "/llms.txt":
                return Answer(request, meta.LlmsText(brand), "text/markdown; charset=utf-8");

            case "/.well-known/ai-catalog.json" when catalog:
                var described = meta.AiCatalog(brand);
                return described == null ? null : Answer(request, described, "application/json; charset=utf-8");

            // the bundle's, naming the installation as its operator did
            case "/manifest.json" when brand.Own && bundle.Manifest() is { } built && PageBranding.Manifest(built, brand) is { } named:
                return Answer(request, named, "application/manifest+json; charset=utf-8");
        }

        if (Moved(request, path) is { } elsewhere)
        {
            return elsewhere;
        }

        if (meta.Find(path) is { } page)
        {
            var markup = meta.Render(bundle.Index(), page, brand);

            if (head?.Invoke(page) is { } more)
            {
                markup = SiteMeta.InHead(markup, more);
            }

            markup = prerender.Render(markup, page, request, brand);

            return Answer(request, PageBranding.Apply(markup, brand), "text/html; charset=utf-8");
        }

        if (ToLanguage(request, path) is { } redirect)
        {
            return redirect;
        }

        // the root of a site that names no page there - one without a build
        if (path == "/")
        {
            return Revalidated(bundle.Answer(request, brand: brand));
        }

        // a file of the bundle
        if (await files.HandleAsync(request) is { } file)
        {
            return file;
        }

        // a person is shown the application's own page for it, and anything
        // else - a crawler looking for a file, a checker - learns it is not here
        return PrefersMarkup(request) ? Revalidated(bundle.Answer(request, ResponseStatus.NotFound, brand)) : null;
    }

    /// <summary>
    /// A page that went somewhere else, sent on there: in the language it was
    /// asked for in, or without one, which is then sent on to the visitor's.
    /// </summary>
    private IResponse? Moved(IRequest request, string path)
    {
        if (moved.Count == 0)
        {
            return null;
        }

        var normalized = SiteMeta.Normalize(path);
        var language = SiteLanguages.Of(normalized);
        var bare = language != null ? SiteLanguages.Without(normalized) : normalized;

        if (!moved.TryGetValue(bare, out var to))
        {
            return null;
        }

        var location = language != null ? SiteLanguages.In(language, to) : to;

        return request.Respond()
                      .Status(ResponseStatus.MovedPermanently)
                      .Header("Location", location + RequestOrigin.Query(request))
                      .Build();
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
