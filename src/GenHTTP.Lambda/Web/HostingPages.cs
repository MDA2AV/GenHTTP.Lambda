using System.Text.Encodings.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Hosting;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// The two pages of the hosting domain that are nobody's lambda: the one at
/// its root, and the one at a subdomain nothing answers at. Both say where
/// the apps here come from and lead to the site.
/// </summary>
/// <remarks>
/// Rendered here, plain and on their own, rather than taken from the bundle of
/// the site: the hosting domain is where strangers' code runs, so none of the
/// site's scripts are served there, and nothing of what the site keeps in a
/// browser is ever on it.
///
/// Their words are the build's, in <c>pages.json</c> beside the names of the
/// site's pages, so they are translated with the rest; in the language the
/// browser prefers, since a cookie of the site is never sent to this domain.
/// Without a build they are in English.
/// </remarks>
public sealed class HostingPages(SiteMeta meta, LambdaOptions options)
{
    private const string Site = "GenHTTP Lambda";

    private static readonly SiteText HomeFallback = new("Apps made with GenHTTP Lambda",
        "The apps on this domain were built with GenHTTP Lambda, each at an address of its own. To build one of yours, go to {site}.");

    private static readonly SiteText MissingFallback = new("Nothing is running here",
        "No app is online at {address}. It may have been taken offline or removed, or there never was one. To build one of yours, go to {site}.");

    private static readonly HtmlEncoder Html = HtmlEncoder.Create(System.Text.Unicode.UnicodeRanges.All);

    #region Functionality

    /// <summary>
    /// What answers at the root of the hosting domain, and below it.
    /// </summary>
    /// <remarks>
    /// Any other path there is not found, but says the same: whoever typed it
    /// was looking for an app here.
    /// </remarks>
    public IHandler CreateHome() => new Home(this);

    /// <summary>
    /// What a request is told at a subdomain of the hosting domain no lambda
    /// is online at - a page for a browser, a line of JSON for anything else.
    /// </summary>
    public IResponse Missing(IRequest request, string host)
    {
        var accepted = request.Header.Headers.GetEntry("Accept");

        if (accepted == null || !accepted.Contains("text/html", StringComparison.OrdinalIgnoreCase))
        {
            return request.Respond()
                          .Status(ResponseStatus.NotFound)
                          .Content($$"""{"error":{{System.Text.Json.JsonSerializer.Serialize($"There is no lambda online at {host}.")}}}""", ContentType.ApplicationJson)
                          .Build();
        }

        return Render(request, ResponseStatus.NotFound, SiteMeta.HostingMissingTemplate, MissingFallback, host);
    }

    private IResponse Render(IRequest request, ResponseStatus status, string template, SiteText fallback, string host)
    {
        var preferred = SiteLanguages.Negotiate(null, request.Header.Headers.GetEntry("Accept-Language"));

        var (language, text) = meta.Words(template, preferred, fallback);

        return request.Respond()
                      .Status(status)
                      .Content(Markup(language, text, host), ContentType.TextHtml)
                      .Header("Content-Language", SiteLanguages.TagOf(language))
                      .Header("Vary", "Accept-Language")
                      .Header("Cache-Control", "no-cache")
                      .Build();
    }

    private string Markup(string language, SiteText text, string host)
    {
        // the site as a link where the installation names its address, and
        // by its name where it does not
        var site = options.PublicUrl is { } url
                       ? $"<a href=\"{Html.Encode(url + "/")}\">{Html.Encode(new Uri(url).Authority)}</a>"
                       : Site;

        var title = Html.Encode(text.Title);

        var description = Html.Encode(text.Description).Replace(Html.Encode("{address}"), $"<strong>{Html.Encode(host)}</strong>", StringComparison.Ordinal)
                                                       .Replace(Html.Encode("{site}"), site, StringComparison.Ordinal);

        var summary = Html.Encode(text.Description.Replace("{address}", host, StringComparison.Ordinal)
                                                  .Replace("{site}", options.PublicUrl is { } named ? new Uri(named).Authority : Site, StringComparison.Ordinal));

        return $$"""
            <!doctype html>
            <html lang="{{SiteLanguages.TagOf(language)}}">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>{{title}}</title>
                <meta name="description" content="{{summary}}">
                <style>
                    :root { color-scheme: dark light; }
                    body { margin: 0; min-height: 100vh; display: grid; place-items: center;
                           font: 16px/1.6 ui-sans-serif, system-ui, sans-serif; }
                    main { max-width: 36rem; padding: 2rem; }
                    h1 { font-size: 1.5rem; margin: 0 0 .75rem; }
                    p { margin: 0; opacity: .85; }
                    a { color: #1765cc; }
                    @media (prefers-color-scheme: dark) { a { color: #8ab4f8; } }
                </style>
            </head>
            <body>
            <main>
                <h1>{{title}}</h1>
                <p>{{description}}</p>
            </main>
            </body>
            </html>
            """;
    }

    #endregion

    #region Types

    /// <summary>
    /// The root of the hosting domain.
    /// </summary>
    private sealed class Home(HostingPages pages) : IHandler
    {

        public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var status = request.Header.Path.ToString() == "/" ? ResponseStatus.Ok : ResponseStatus.NotFound;

            return new(pages.Render(request, status, SiteMeta.HostingHomeTemplate, HomeFallback, request.GetHost()?.Name ?? string.Empty));
        }

    }

    #endregion

}
