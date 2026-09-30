using GenHTTP.Api.Content;

using GenHTTP.Lambda.Configuration;

using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.DependencyInjection.Infrastructure;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices.Provider;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// The versioned API the single page application talks to, described by an
/// OpenAPI document and browsable at <c>/api/v1/scalar/</c>.
/// </summary>
public static class ApiLayout
{

    public static IHandlerBuilder Create(LambdaOptions options)
    {
        var mode = options.CompileApi ? ExecutionMode.Auto : ExecutionMode.Reflection;

        var version = Layout.Create()
                            .Add("keys", Resource<KeyResource>(mode))
                            .Add("demos", Resource<DemoResource>(mode))
                            .Add("showcases", Resource<ShowcaseResource>(mode))
                            .Add("builds", Resource<BuildResource>(mode))
                            .Add("system", Resource<SystemResource>(mode))
                            .Add("telemetry", Resource<TelemetryResource>(mode))
                            .Add("logs", Resource<LogResource>(mode))
                            // the token is checked in front of the resource, see AdminGateConcern
                            .Add("admin", Layout.Create()
                                                .Add(Resource<AdminResource>(mode))
                                                .Add(new AdminGateConcernBuilder(options)))
                            .Add(Resource<LambdaResource>(mode))
                            .Add(Resource<VersionResource>(mode))
                            .Add(Resource<DeploymentResource>(mode))
                            .Add(Resource<FileResource>(mode))
                            .Add(Resource<DataResource>(mode))
                            .Add(Resource<SecretResource>(mode))
                            .Add(Resource<DatabaseResource>(mode))
                            .Add(Resource<FeatureResource>(mode))
                            .Add(Resource<FeatureWorkspaceResource>(mode))
                            .Add(Resource<CodeResource>(mode))
                            .Add(Resource<MonitoringResource>(mode))
                            .Add(Resource<LambdaShowcaseResource>(mode))
                            .Add(Resource<LambdaDomainResource>(mode))
                            .Add(Resource<LambdaAgentResource>(mode))
                            .AddScalar(title: "GenHTTP Lambda API")
                            .AddOpenApi()
                            .Add(ErrorHandler.From(new ApiErrorMapper()));

        return Layout.Create().Add("v1", version);
    }

    /// <summary>
    /// A webservice resolved from the service provider, the way
    /// <c>AddDependentService</c> builds one, but with the execution mode
    /// chosen by the configuration.
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
    private static ServiceResourceBuilder Resource<T>(ExecutionMode mode) where T : class
        => new ServiceResourceBuilder().Type(typeof(T))
                                       .InstanceProvider(async r => await InstanceProvider.ProvideAsync<T>(r))
                                       .Injectors(Injection.Default().Add(new DependencyInjector()))
                                       .ExecutionMode(mode);

}
