using System.Text;
using System.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// Sits in front of the single page application and answers what is there for
/// crawlers: <c>robots.txt</c>, <c>sitemap.xml</c>, and the index page named
/// as the public page that was asked for, in the language its address names,
/// with that page's content already in it. A public page asked for without a
/// language is sent on to the one the visitor prefers. Everything else goes
/// through.
/// </summary>
public sealed class SiteMetaConcern : IConcern
{

    /// <summary>
    /// The page that used to be here, and the one it became.
    /// </summary>
    private static readonly Dictionary<string, string> Moved = new() { ["/agentic-coding"] = "/showcase" };

    #region Get-/Setters

    public IHandler Content { get; }

    private SiteMeta Meta { get; }

    private SitePrerender Prerender { get; }

    private Func<ValueTask<string>> Index { get; }

    #endregion

    #region Initialization

    public SiteMetaConcern(IHandler content, SiteMeta meta, SitePrerender prerender, Func<ValueTask<string>> index)
    {
        Content = content;
        Meta = meta;
        Prerender = prerender;
        Index = index;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Header.Method != RequestMethod.Get)
        {
            return await Content.HandleAsync(request);
        }

        var path = request.Header.Path.ToString();

        switch (path)
        {
            case "/robots.txt":
                return Answer(request, Meta.Robots(), "text/plain; charset=utf-8");

            case "/sitemap.xml":
                var sitemap = Meta.Sitemap();

                // without a public address there is nothing to list pages
                // under, and the index page is not a sitemap either
                return sitemap == null ? null : Answer(request, sitemap, "application/xml; charset=utf-8");
        }

        var page = Meta.Find(path);

        if (page == null)
        {
            return ToLanguage(request, path) ?? await Content.HandleAsync(request);
        }

        var markup = Meta.Render(await Index(), page);

        markup = await Prerender.RenderAsync(markup, page, request);

        return Answer(request, markup, "text/html; charset=utf-8");
    }

    /// <summary>
    /// A public page asked for without a language, sent on to the language
    /// the visitor chose here or their browser prefers - or nothing, for any
    /// other path.
    /// </summary>
    /// <remarks>
    /// Found rather than moved: the answer depends on who is asking, which the
    /// Vary header says as well, so no cache hands one visitor's language to
    /// the next. A crawler asks in no language and is sent to the default.
    /// </remarks>
    private IResponse? ToLanguage(IRequest request, string path)
    {
        var normalized = SiteMeta.Normalize(path);

        if (!Moved.TryGetValue(normalized, out var target))
        {
            if (!Meta.IsPage(normalized))
            {
                return null;
            }

            target = normalized;
        }

        var headers = request.Header.Headers;

        var language = SiteLanguages.Negotiate(headers.GetCookie(SiteLanguages.Cookie), headers.GetEntry("Accept-Language"));

        return request.Respond()
                      .Status(ResponseStatus.Found)
                      .Header("Location", SiteLanguages.In(language, target) + Query(request))
                      .Header("Vary", "Accept-Language, Cookie")
                      .Build();
    }

    /// <summary>
    /// The query of the request, to be passed along with it.
    /// </summary>
    private static string Query(IRequest request)
    {
        var query = request.Header.Query;

        if (query.Count == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder();

        for (var i = 0; i < query.Count; i++)
        {
            var entry = query.GetStringEntry(i);

            result.Append(i == 0 ? '?' : '&')
                  .Append(Uri.EscapeDataString(entry.Key.ToString()))
                  .Append('=')
                  .Append(Uri.EscapeDataString(HttpUtility.UrlDecode(entry.Value.ToString())));
        }

        return result.ToString();
    }

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    private static IResponse Answer(IRequest request, string body, string type)
        => request.Respond()
                  .Content(Resource.FromString(body).Type(new ContentType(type)).Build())
                  .Build();

    #endregion

}

/// <summary>
/// Builds a <see cref="SiteMetaConcern" /> for a handler.
/// </summary>
public sealed class SiteMetaConcernBuilder(SiteMeta meta, SitePrerender prerender, Func<ValueTask<string>> index) : IConcernBuilder
{

    public IConcern Build(IHandler content) => new SiteMetaConcern(content, meta, prerender, index);

}
