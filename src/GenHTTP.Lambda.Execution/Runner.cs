using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Layouting;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Execution;

/// <summary>
/// Serves the lambdas of the platform, and hands every other request to the
/// site it is given.
/// </summary>
/// <remarks>
/// It goes around the site rather than into it: a lambda at a host of its own
/// - its own domain, its subdomain of the hosting domain - owns every path
/// there, so <c>shop.example.com/api</c> is the shop's and never the
/// platform's API. On the platform's own host it takes the previews of
/// features, below <c>/features/</c>, and the lambdas below <c>/lambda/</c>
/// where they answer by path, and leaves the rest to the site.
/// </remarks>
/// <example>
/// <code>
/// Host.Create().Handler(Runner.Create(platform).Site(site));
/// </code>
/// </example>
public static class Runner
{

    /// <summary>
    /// The lambdas of the given platform.
    /// </summary>
    public static RunnerBuilder Create(LambdaPlatform platform) => new(platform);

}

/// <summary>
/// Configures the runner.
/// </summary>
public sealed class RunnerBuilder(LambdaPlatform platform) : IHandlerBuilder<RunnerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private IHandlerBuilder? _site;

    #region Functionality

    /// <summary>
    /// What answers on the platform's own host: the API, the editor, the
    /// pages - everything that is not a lambda.
    /// </summary>
    public RunnerBuilder Site(IHandlerBuilder site)
    {
        _site = site;
        return this;
    }

    public RunnerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var site = _site ?? throw new BuilderMissingPropertyException("Site");

        var services = platform.Services;

        var meta = services.GetRequiredService<IMetaService>();

        var pages = new HostingPages(platform.Options);

        var domains = LambdaRoute.Create(services, new DomainLocator(meta));

        var hosted = LambdaRoute.Create(services, new SubdomainLocator(meta, pages));

        // the previews of features, served like the lambdas they belong to
        var previews = LambdaRoute.Create(services, new FeatureLocator(services.GetRequiredService<IFeatureService>()));

        var own = Layout.Create()
                        .Add(PlatformRoutes.Features, previews);

        // where the lambdas answer below a path of this host rather than at
        // hosts of their own
        if (services.GetRequiredService<ILambdaAddresses>().ByPath)
        {
            var lambdas = LambdaRoute.Create(services, new PathLocator(meta, pages));

            own.Add(PlatformRoutes.Lambdas, Concerns.Chain([new TrailingSlashConcernBuilder()], lambdas));
        }

        own.Add(site);

        var router = new DomainRouter(services.GetRequiredService<IDomainRegistry>(), domains, hosted, pages.CreateHome(), pages.CreateUnknown(), own.Build());

        return Concerns.Chain(_concerns, router);
    }

    #endregion

}
