using GenHTTP.Api.Content;

using GenHTTP.Lambda.Api;

using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.DependencyInjection.Infrastructure;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Webservices.Provider;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// The versioned API the single page application talks to, described by an
/// OpenAPI document and browsable at <c>/api/v1/scalar/</c>.
/// </summary>
public static class ApiLayout
{

    public static IHandlerBuilder Create()
    {
        var version = Layout.Create()
                            .AddDependentService<KeyResource>("keys")
                            .AddDependentService<ExampleResource>("examples")
                            .AddDependentService<BuildResource>("builds")
                            .AddDependentService<SystemResource>("system")
                            .AddDependentService<TelemetryResource>("telemetry")
                            .AddDependentService<LogResource>("logs")
                            .AddDependentService<AdminResource>("admin")
                            .Add(Resource<LambdaResource>())
                            .Add(Resource<VersionResource>())
                            .Add(Resource<DeploymentResource>())
                            .Add(Resource<FileResource>())
                            .Add(Resource<CodeResource>())
                            .Add(Resource<MonitoringResource>())
                            .AddScalar(title: "GenHTTP Lambda API")
                            .AddOpenApi()
                            .Add(ErrorHandler.From(new ApiErrorMapper()));

        return Layout.Create().Add("v1", version);
    }

    /// <summary>
    /// A webservice resolved from the service provider, without a path of its
    /// own.
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
    private static ServiceResourceBuilder Resource<T>() where T : class
        => new ServiceResourceBuilder().Type(typeof(T))
                                       .InstanceProvider(async r => await InstanceProvider.ProvideAsync<T>(r));

}
