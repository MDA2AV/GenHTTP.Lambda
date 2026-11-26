using GenHTTP.Api.Content;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Agents;

using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices.Provider;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// The versioned API the single page application talks to, described by an
/// OpenAPI document and browsable at <c>/api/v1/scalar/</c>.
/// </summary>
public static class ApiLayout
{

    public static IHandlerBuilder Create(LambdaPlatform platform, bool compiled)
    {
        var options = platform.Options;

        var services = platform.Services;

        var mode = compiled ? ExecutionMode.Auto : ExecutionMode.Reflection;

        var version = Layout.Create()
                            .Add("keys", Resource<KeyResource>(services, mode))
                            .Add("demos", Resource<DemoResource>(services, mode))
                            .Add("showcases", Resource<ShowcaseResource>(services, mode))
                            .Add("sources", Resource<SourceResource>(services, mode))
                            .Add("builds", Resource<BuildResource>(services, mode))
                            .Add("attachments", Resource<AttachmentResource>(services, mode))
                            .Add("system", Resource<SystemResource>(services, mode))
                            .Add("shared", Resource<SharedFileResource>(services, mode))
                            .Add(Resource<LambdaResource>(services, mode))
                            .Add(Resource<VersionResource>(services, mode))
                            .Add(Resource<DeploymentResource>(services, mode))
                            .Add(Resource<FileResource>(services, mode))
                            .Add(Resource<DataResource>(services, mode))
                            .Add(Resource<SecretResource>(services, mode))
                            .Add(Resource<DatabaseResource>(services, mode))
                            .Add(Resource<FeatureResource>(services, mode))
                            .Add(Resource<FeatureWorkspaceResource>(services, mode))
                            .Add(Resource<CodeResource>(services, mode))
                            .Add(Resource<MonitoringResource>(services, mode))
                            .Add(Resource<LambdaShowcaseResource>(services, mode))
                            .Add(Resource<LambdaSourceResource>(services, mode))
                            .Add(Resource<LambdaAnalyticsResource>(services, mode))
                            .Add(Resource<LambdaDomainResource>(services, mode))
                            .Add(Resource<LambdaConnectivityResource>(services, mode))
                            .Add(Resource<LambdaAgentResource>(services, mode))
                            .AddScalar(title: "GenHTTP Lambda API")
                            .Add(ApiDocumentation.Create())
                            .Add(ErrorHandler.From(new ApiErrorMapper()));

        // what only the operator may see, behind the token - and not there at
        // all on an installation that has none, see AdminAuthentication
        if (options.Administrable)
        {
            // the guide agents read, and the operator's section of it - on
            // every installation that has an operator
            var admin = Layout.Create()
                              .Add("guide", Resource<GuideResource>(services, mode).Add(AdminAuthentication.Create(options)))
                              // the files shared across the installation, which
                              // its branding and its guide use
                              .Add("shared", Resource<SharedFileAdminResource>(services, mode).Add(AdminAuthentication.Create(options)))
                              // whether the build agent builds and changes apps,
                              // and on which models - there with or without one,
                              // so it is decided before the agent is set up
                              .Add("agent", Resource<AgentSettingsResource>(services, mode).Add(AdminAuthentication.Create(options)));

            // the installation's name, logo, colours and fonts, where its
            // host lets the operator give it some - and not there at all
            // where it does not
            if (options.Branding)
            {
                admin.Add("branding", Resource<BrandingResource>(services, mode).Add(AdminAuthentication.Create(options)));
            }

            // on the resource itself rather than a layout of its own, which
            // would redirect /telemetry to /telemetry/ before asking it; the
            // layout around it has no path of its own to answer
            admin.Add(Resource<AdminResource>(services, mode).Add(AdminAuthentication.Create(options)));

            version.Add("admin", admin)
                   .Add("telemetry", Resource<TelemetryResource>(services, mode).Add(AdminAuthentication.Create(options)))
                   .Add("logs", Resource<LogResource>(services, mode).Add(AdminAuthentication.Create(options)));
        }

        // a call of a run of the agent is the run's, whichever route it takes
        version.Add(ServiceConcern.Of<AgentCallerConcern>(services));

        return Layout.Create().Add("v1", version);
    }

    /// <summary>
    /// A webservice made for every request from the services of the platform,
    /// with the execution mode chosen by the configuration.
    /// </summary>
    /// <remarks>
    /// Everything below <c>/lambdas</c> is split into one resource per concern,
    /// but the segment after it is the editor key, which a layout cannot route
    /// on. So those resources are added without a path and asked in turn, each
    /// declaring the whole of its paths and passing on the ones it does not
    /// know. No path may be claimed by two of them - the first would answer a
    /// method it lacks with 405 before the second is asked.
    ///
    /// Not a layout of their own below <c>lambdas</c>: a layout answers its
    /// own path with a redirect to the same path with a trailing slash before
    /// anything in it is asked, which a POST to create a lambda would follow
    /// as a GET.
    /// </remarks>
    private static ServiceResourceBuilder Resource<T>(IServiceProvider services, ExecutionMode mode) where T : class
    {
        var factory = ActivatorUtilities.CreateFactory<T>([]);

        return new ServiceResourceBuilder().Type(typeof(T))
                                           .InstanceProvider(_ => ValueTask.FromResult<object>(factory(services, null)))
                                           .ExecutionMode(mode);
    }

}
