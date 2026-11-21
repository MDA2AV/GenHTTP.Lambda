using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Source;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// Answers the paths of a site.
/// </summary>
internal sealed class SitePagesHandler(Bundle bundle, SiteMeta meta, SitePrerender prerender, ISourceService sources, IBrandingService branding,
                                       ISettingsService settings, IHandler files, Func<SitePage, string?>? head, bool catalog,
                                       IReadOnlyDictionary<string, string> moved) : IHandler
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
                var sitemap = meta.Sitemap(await sources.ListAddressesAsync());

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

            markup = await prerender.RenderAsync(markup, page, request, settings.Get().Profile, brand);

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
