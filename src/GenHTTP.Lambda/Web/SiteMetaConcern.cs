using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// Sits in front of the single page application and answers what is there for
/// crawlers: <c>robots.txt</c>, <c>sitemap.xml</c>, <c>llms.txt</c> and the
/// AI catalog, and the index page named
/// as the public page that was asked for, in the language its address names,
/// with that page's content already in it. A public page asked for without a
/// language is sent on to the one the visitor prefers. The page of a published
/// source is named after the lambda, and not found where its source is not
/// published. The editor and the administration are handed the index page for
/// the client to draw, the bundle's files go through, and any other address is
/// not found.
/// </summary>
public sealed class SiteMetaConcern : IConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private SiteMeta Meta { get; }

    private SitePrerender Prerender { get; }

    private Func<string> Index { get; }

    private ISourceService Sources { get; }

    private ILambdaAddresses Addresses { get; }

    #endregion

    #region Initialization

    public SiteMetaConcern(IHandler content, SiteMeta meta, SitePrerender prerender, ISourceService sources, ILambdaAddresses addresses,
                           Func<string> index)
    {
        Content = content;
        Meta = meta;
        Prerender = prerender;
        Sources = sources;
        Addresses = addresses;
        Index = index;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Header.Method != RequestMethod.Get && request.Header.Method != RequestMethod.Head)
        {
            return await Content.HandleAsync(request);
        }

        var path = request.Header.Path.ToString();

        switch (path)
        {
            case "/robots.txt":
                return Answer(request, Meta.Robots(), "text/plain; charset=utf-8");

            case "/sitemap.xml":
                var sitemap = Meta.Sitemap(Sources.ListAddresses());

                // without a public address there is nothing to list pages
                // under, and the index page is not a sitemap either
                return sitemap == null ? null : Answer(request, sitemap, "application/xml; charset=utf-8");

            case "/llms.txt":
                return Answer(request, Meta.LlmsText(), "text/markdown; charset=utf-8");

            case "/.well-known/ai-catalog.json":
                var catalog = Meta.AiCatalog();

                return catalog == null ? null : Answer(request, catalog, "application/json; charset=utf-8");
        }

        var page = Meta.Find(path);

        if (page != null)
        {
            var markup = Meta.Render(Index(), page);

            markup = Prerender.Render(markup, page, request);

            return Answer(request, markup, "text/html; charset=utf-8");
        }

        if (SourcePath(path) is { } source)
        {
            return Source(request, source.Language, source.Key, source.Path);
        }

        if (ToLanguage(request, path) is { } redirect)
        {
            return redirect;
        }

        // a file of the bundle, or the index page for the root
        if (await Content.HandleAsync(request) is { } content)
        {
            return content;
        }

        if (IsClientRoute(path))
        {
            return Answer(request, Index(), "text/html; charset=utf-8");
        }

        // a person is shown the application's own page for it, and anything
        // else - a crawler looking for a file, a checker - learns it is not here
        return PrefersMarkup(request)
                   ? request.Respond()
                            .Status(ResponseStatus.NotFound)
                            .Content(Resource.FromString(Index()).Type(new ContentType("text/html; charset=utf-8")).Build())
                            .Build()
                   : null;
    }

    /// <summary>
    /// Whether the client side router draws a page for the path that is not a
    /// public one: the editor and the administration.
    /// </summary>
    /// <remarks>
    /// Without a build naming the public pages there is no telling which
    /// address is one, so every address is.
    /// </remarks>
    private bool IsClientRoute(string path)
        => path == "/admin"
           || path.StartsWith("/admin/", StringComparison.Ordinal)
           || path.StartsWith("/editor/", StringComparison.Ordinal)
           || !Meta.HasPages;

    private static bool PrefersMarkup(IRequest request)
        => request.Header.Headers.GetEntry("Accept")?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true;

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

        if (!Meta.IsPage(normalized))
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
    /// The page of a published source - its files, its documentation and its
    /// history below it - named after the lambda in the language its address
    /// names, or sent on to a language where it names none.
    /// </summary>
    /// <remarks>
    /// Not rendered like the pages of the build, since the build cannot know
    /// which sources there are: named, described and marked up as source code
    /// here, and drawn by the client. A source that is not published is not
    /// found - asked for, it looks the same as a key nobody has.
    /// </remarks>
    private IResponse Source(IRequest request, string? language, string key, string path)
    {
        if (language == null)
        {
            var headers = request.Header.Headers;

            var preferred = SiteLanguages.Negotiate(headers.GetCookie(SiteLanguages.Cookie), headers.GetEntry("Accept-Language"));

            return request.Respond()
                          .Status(ResponseStatus.Found)
                          .Header("Location", SiteLanguages.In(preferred, path) + RequestOrigin.Query(request))
                          .Header("Vary", "Accept-Language, Cookie")
                          .Build();
        }

        var project = Sources.GetProject(key);

        if (project == null)
        {
            return request.Respond()
                          .Status(ResponseStatus.NotFound)
                          .Content(Resource.FromString(Index()).Type(new ContentType("text/html; charset=utf-8")).Build())
                          .Build();
        }

        var entry = project.Entry;

        var name = entry.Title ?? entry.PublicKey;

        var about = entry.About ?? entry.Description;

        var image = entry.Picture is { } picture ? $"/api/v1/showcases/{entry.PublicKey}/image?v={picture.Ticks}" : null;

        var page = Meta.Source(language, path, name, about, image) with { ImageType = image != null ? entry.PictureType : null };

        var schema = new SourceSchema(name, about, SourceLicenses.Find(entry.License)?.Url ?? entry.License, project.Author, entry.Updated,
                                      entry.Online ? Addresses.Of(entry.PublicKey, entry.Tier, entry.Domain) : null);

        return Answer(request, Meta.RenderSource(Index(), page, schema), "text/html; charset=utf-8");
    }

    /// <summary>
    /// The page of a published source a path is below, and the language it
    /// names: "/de/source/quiz/docs" is the German page of quiz, "/source/quiz"
    /// the one in no language - or nothing, for any other path.
    /// </summary>
    private static (string? Language, string Key, string Path)? SourcePath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var offset = segments.Length > 0 && SiteLanguages.IsLanguage(segments[0]) ? 1 : 0;

        if (segments.Length < offset + 2 || segments[offset] != "source")
        {
            return null;
        }

        return (offset == 1 ? segments[0] : null, segments[offset + 1], "/" + string.Join('/', segments.Skip(offset)));
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
public sealed class SiteMetaConcernBuilder(SiteMeta meta, SitePrerender prerender, ISourceService sources, ILambdaAddresses addresses,
                                           Func<string> index)
    : IConcernBuilder
{

    public IConcern Build(IHandler content) => new SiteMetaConcern(content, meta, prerender, sources, addresses, index);

}
