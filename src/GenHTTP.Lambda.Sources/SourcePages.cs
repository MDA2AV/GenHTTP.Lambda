using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Git;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Sources;

/// <summary>
/// The published sources: their catalog at <c>/{language}/source</c>, the page
/// of each at <c>/{language}/source/{publicKey}</c>, and each as a git
/// repository at <c>/source/{publicKey}.git</c> for anybody to clone.
/// </summary>
/// <remarks>
/// Added to a site's layout without a path of its own, since its pages are
/// below the language they are in: it answers the paths that are its own and
/// passes on every other. A bundle of its own (Frontend/), which draws the
/// pages in the browser; the server names each after its lambda and marks it
/// up as source code, so a link to one reads as what it is.
/// </remarks>
/// <example>
/// <code>
/// Layout.Create().Add(SourcePages.Create(platform)).Add(site);
/// </code>
/// </example>
public static class SourcePages
{

    /// <summary>
    /// The published sources of the given platform.
    /// </summary>
    public static SourcePagesBuilder Create(LambdaPlatform platform) => new(platform);

}

/// <summary>
/// Configures the pages of the published sources.
/// </summary>
public sealed class SourcePagesBuilder(LambdaPlatform platform) : IHandlerBuilder<SourcePagesBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private Web.Bundle _bundle = Web.Bundle.Beside("sources");

    #region Functionality

    /// <summary>
    /// Where the bundle of the pages was built to, where it is not beside the host.
    /// </summary>
    public SourcePagesBuilder Bundle(string directory)
    {
        _bundle = Web.Bundle.In(directory);
        return this;
    }

    public SourcePagesBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var services = platform.Services;

        var repositories = ActivatorUtilities.CreateInstance<GitRoutesBuilder>(services, GitArea.Published).Build();

        IHandler files = Assets.From(_bundle.Files()).Add(CacheControl.NoCache()).Build();

        if (_bundle.HasAssets)
        {
            files = GenHTTP.Modules.Layouting.Layout.Create().Add("assets", _bundle.Assets()).Add(files).Build();
        }

        var handler = new SourcePagesHandler(_bundle, new SiteMeta(_bundle, platform.Options.PublicUrl), services.GetRequiredService<ISourceService>(),
                                             services.GetRequiredService<ILambdaAddresses>(), repositories, files);

        return Concerns.Chain(_concerns, handler);
    }

    #endregion

}

/// <summary>
/// Answers the paths of the published sources.
/// </summary>
internal sealed class SourcePagesHandler(Bundle bundle, SiteMeta meta, ISourceService sources, ILambdaAddresses addresses, IHandler repositories, IHandler files)
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

            if (target.Current != null && await files.HandleAsync(request) is { } file)
            {
                return file;
            }

            return ToLanguage(request);
        }

        // a page of the sources in a language
        if (first != null && SiteLanguages.IsLanguage(first) && target.Next(1)?.Decode() == PlatformRoutes.Source && IsRead(request))
        {
            return Page(request, first);
        }

        return null;
    }

    private static bool IsRead(IRequest request) => request.Header.Method == RequestMethod.Get || request.Header.Method == RequestMethod.Head;

    /// <summary>
    /// A page of the sources: the catalog, named as the bundle's table names
    /// it, or the page of a source, named after its lambda.
    /// </summary>
    private IResponse Page(IRequest request, string language)
    {
        var path = request.Header.Path.ToString();

        if (meta.Find(path) is { } catalog)
        {
            return Answer(request, meta.Render(bundle.Index(), catalog));
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 3)
        {
            return bundle.Answer(request, ResponseStatus.NotFound);
        }

        var key = segments[2];
        var below = "/" + string.Join('/', segments.Skip(1));

        // an unpublished source is not found, the same as a key nobody has
        if (sources.GetProject(key) is not { } project)
        {
            return bundle.Answer(request, ResponseStatus.NotFound);
        }

        var entry = project.Entry;

        var name = entry.Title ?? entry.PublicKey;
        var about = entry.About ?? entry.Description;

        // its picture on the showcase, where it has one, and the page's own otherwise
        var image = entry.Picture is { } picture ? $"/api/v1/showcases/{entry.PublicKey}/image?v={picture.Ticks}" : null;

        var page = meta.Source(language, below, name, about, image) with { ImageType = image != null ? entry.PictureType : null };

        var schema = new SourceSchema(name, about, SourceLicenses.Find(entry.License)?.Url ?? entry.License, project.Author, entry.Updated,
                                      entry.Online ? addresses.Of(entry.PublicKey, entry.Tier, entry.Domain) : null);

        return Answer(request, meta.RenderSource(bundle.Index(), page, schema));
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

    private static IResponse Answer(IRequest request, string markup)
        => request.Respond()
                  .Content(Resource.FromString(markup).Type(new ContentType("text/html; charset=utf-8")).Build())
                  .Build();

    #endregion

}
