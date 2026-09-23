using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// Sits in front of the single page application and answers what is there for
/// crawlers: <c>robots.txt</c>, <c>sitemap.xml</c>, and the index page named
/// as the public page that was asked for. Everything else goes through.
/// </summary>
public sealed class SiteMetaConcern : IConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private SiteMeta Meta { get; }

    private Func<ValueTask<string>> Index { get; }

    #endregion

    #region Initialization

    public SiteMetaConcern(IHandler content, SiteMeta meta, Func<ValueTask<string>> index)
    {
        Content = content;
        Meta = meta;
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
            return await Content.HandleAsync(request);
        }

        return Answer(request, Meta.Render(await Index(), path, page), "text/html; charset=utf-8");
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
public sealed class SiteMetaConcernBuilder(SiteMeta meta, Func<ValueTask<string>> index) : IConcernBuilder
{

    public IConcern Build(IHandler content) => new SiteMetaConcern(content, meta, index);

}
