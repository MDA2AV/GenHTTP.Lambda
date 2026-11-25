using GenHTTP.Api.Content;
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

        // one for every platform of the process that serves this bundle
        var rendered = platform.Runtime.Shared(bundle.PrerenderFile, () => new PrerenderFile(bundle.PrerenderFile));

        var prerender = new SitePrerender(rendered, publicUrl, services.GetRequiredService<ILimitsService>(), services.GetRequiredService<IShowcaseService>(),
                                          services.GetRequiredService<ILambdaAddresses>(), platform.Loggers.CreateLogger<SitePrerender>());

        // ranges because a front page may show a video, and Safari will not
        // play one from a server that answers a range with the whole file
        IHandlerBuilder files = GenHTTP.Modules.Files.Assets.From(bundle.Files()).Add(RangeSupport.Create()).Add(CacheControl.NoCache());

        if (bundle.HasAssets)
        {
            files = Layout.Create().Add("assets", bundle.Assets()).Add(files);
        }

        var pages = new SitePagesHandler(bundle, meta, prerender, services.GetRequiredService<ISourceService>(), services.GetRequiredService<IBrandingService>(),
                                         services.GetRequiredService<ISettingsService>(), files.Build(), _head, _catalog, _moved);

        return Concerns.Chain(_concerns, pages);
    }

    #endregion

}
