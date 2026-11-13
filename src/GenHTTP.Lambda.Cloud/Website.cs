using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Git;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.Layouting;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Cloud;

/// <summary>
/// The pages of genhttp.dev that are this host's own, and the single page
/// application that answers every other path of the site.
/// </summary>
public static class Website
{

    public static WebsiteBuilder Create(LambdaPlatform platform) => new(platform);

}

public sealed class WebsiteBuilder(LambdaPlatform platform) : IHandlerBuilder<WebsiteBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    public WebsiteBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var services = platform.Services;

        var meta = ActivatorUtilities.CreateInstance<SiteMeta>(services);

        var prerender = ActivatorUtilities.CreateInstance<SitePrerender>(services);

        var spa = ActivatorUtilities.CreateInstance<SpaResources>(services, meta, prerender);

        // the repositories below the editor and the published sources, asked
        // before the pages that answer every other path there
        var layout = Layout.Create()
                           .Add(ActivatorUtilities.CreateInstance<GitRoutesBuilder>(services));

        // Ahead of the application, so a miss here is a 404 rather than the
        // index page: a named route answers for itself and never falls through
        // to the handler that catches everything else.
        if (spa.HasAssets)
        {
            layout = layout.Add("assets", spa.CreateAssetHandler());
        }

        return Concerns.Chain(_concerns, layout.Add(spa.CreateHandler()).Build());
    }

}
