using GenHTTP.Api.Content;

using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The versioned API the single page application talks to, described by an
/// OpenAPI document and browsable at <c>/api/v1/swagger/</c>.
/// </summary>
public static class ApiLayout
{

    public static IHandlerBuilder Create()
    {
        var version = Layout.Create()
                            .AddDependentService<LambdaResource>("lambdas")
                            .AddDependentService<SystemResource>("system")
                            .AddSwaggerUi(title: "GenHTTP Lambda API")
                            .AddOpenApi()
                            .Add(ErrorHandler.From(new ApiErrorMapper()));

        return Layout.Create().Add("v1", version);
    }

}
