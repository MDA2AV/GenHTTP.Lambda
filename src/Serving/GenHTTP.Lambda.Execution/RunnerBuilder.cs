using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Layouting;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Execution;

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

        var pages = new HostingPages(platform.Options, services.GetRequiredService<IBrandingService>());

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

            own.Add(PlatformRoutes.Lambdas, Concerns.Chain([new TrailingSlashConcernBuilder(services.GetRequiredService<IDomainRegistry>())], lambdas));
        }

        own.Add(site);

        var router = new DomainRouter(services.GetRequiredService<IDomainRegistry>(), domains, hosted, pages.CreateHome(), pages.CreateUnknown(), own.Build());

        return Concerns.Chain(_concerns, router);
    }

    #endregion

}
