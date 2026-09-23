using GenHTTP.Api.Content;

using GenHTTP.Lambda.Api;

using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;

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
                            .AddDependentService<LambdaResource>("lambdas")
                            .AddDependentService<SystemResource>("system")
                            .AddDependentService<TelemetryResource>("telemetry")
                            .AddDependentService<ExampleResource>("examples")
                            .AddDependentService<BuildResource>("build")
                            .AddDependentService<InvitationResource>("start")
                            .AddDependentService<AdminResource>("admin")
                            .AddDependentService<LogResource>("logs")
                            .AddScalar(title: "GenHTTP Lambda API")
                            .AddOpenApi()
                            .Add(ErrorHandler.From(new ApiErrorMapper()));

        return Layout.Create().Add("v1", version);
    }

}
