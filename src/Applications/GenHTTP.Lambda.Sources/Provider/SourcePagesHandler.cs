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
/// Answers the paths of the published sources.
/// </summary>
internal sealed class SourcePagesHandler(Bundle bundle, SiteMeta meta, ISourceService sources, ILambdaAddresses addresses, IBrandingService branding,
                                         IHandler repositories, IHandler files)
    : IHandler
{

    #region Functionality

    public async ValueTask PrepareAsync(IServer server)
    {
        await repositories.PrepareAsync(server);
        await files.PrepareAsync(server);
    }

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.Header.Target;

        var first = target.Current?.Decode();

        // below /source/: a repository, a file of the bundle, or a page asked
        // for without a language
        if (first == PlatformRoutes.Source)
        {
            target.Advance();

            if (await repositories.HandleAsync(request) is { } git)
            {
                return git;
            }

            if (!IsRead(request))
            {
                return null;
            }

            // a file of the bundle that is not there is not a page either: a
            // stale asset learns it is gone rather than being sent on to a
            // source named "assets"
            if (target.Current?.Decode() == "assets")
            {
                return await files.HandleAsync(request) ?? request.Respond().Status(ResponseStatus.NotFound).Build();
            }

            if (target.Current != null && await files.HandleAsync(request) is { } file)
            {
                return file;
            }

            return Revalidated(ToLanguage(request));
        }

        // a page of the sources in a language
        if (first != null && SiteLanguages.IsLanguage(first) && target.Next(1)?.Decode() == PlatformRoutes.Source && IsRead(request))
        {
            return Revalidated(await PageAsync(request, first));
        }

        return null;
    }

    private static bool IsRead(IRequest request) => request.Header.Method == RequestMethod.Get || request.Header.Method == RequestMethod.Head;

    /// <summary>
    /// A page of the sources: the catalog, named as the bundle's table names
    /// it, or the page of a source, named after its lambda.
    /// </summary>
    private async ValueTask<IResponse> PageAsync(IRequest request, string language)
    {
        var path = request.Header.Path.ToString();

        var brand = branding.Current;

        if (meta.Find(path) is { } catalog)
        {
            return Answer(request, PageBranding.Apply(meta.Render(bundle.Index(), catalog, brand), brand));
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 3)
        {
            return bundle.Answer(request, ResponseStatus.NotFound, brand);
        }

        var key = segments[2];
        var below = "/" + string.Join('/', segments.Skip(1));

        // an unpublished source is not found, the same as a key nobody has
        if (await sources.GetProjectAsync(key) is not { } project)
        {
            return bundle.Answer(request, ResponseStatus.NotFound, brand);
        }

        var entry = project.Entry;

        var name = entry.Title ?? entry.PublicKey;
        var about = entry.About ?? entry.Description;

        // its picture on the showcase, where it has one, and the page's own otherwise
        var image = entry.Picture is { } picture ? $"/api/v1/showcases/{entry.PublicKey}/image?v={picture.Ticks}" : null;

        var page = meta.Source(language, below, name, about, image, brand) with { ImageType = image != null ? entry.PictureType : null };

        var schema = new SourceSchema(name, about, SourceLicenses.Find(entry.License)?.Url ?? entry.License, project.Author, entry.Updated,
                                      entry.Online ? addresses.Of(entry.PublicKey, entry.Tier, entry.Domain) : null);

        return Answer(request, PageBranding.Apply(meta.RenderSource(bundle.Index(), page, schema, brand), brand));
    }

    /// <summary>
    /// A page of the sources asked for without a language, sent on to the one
    /// the visitor prefers. Not cached, since it depends on who is asking.
    /// </summary>
    private static IResponse ToLanguage(IRequest request)
    {
        var headers = request.Header.Headers;

        var preferred = SiteLanguages.Negotiate(headers.GetCookie(SiteLanguages.Cookie), headers.GetEntry("Accept-Language"));

        var path = SiteMeta.Normalize(request.Header.Path.ToString());

        return request.Respond()
                      .Status(ResponseStatus.Found)
                      .Header("Location", SiteLanguages.In(preferred, path) + RequestOrigin.Query(request))
                      .Header("Vary", "Accept-Language, Cookie")
                      .Build();
    }

    /// <summary>
    /// A page names the files of the bundle it was built with, which change
    /// their names with every build - so it is asked for again each time.
    /// </summary>
    private static IResponse Revalidated(IResponse response)
    {
        response.Rebuild().Header("Cache-Control", "no-cache");
        return response;
    }

    private static IResponse Answer(IRequest request, string markup)
        => request.Respond()
                  .Content(Resource.FromString(markup).Type(new ContentType("text/html; charset=utf-8")).Build())
                  .Build();

    #endregion

}
