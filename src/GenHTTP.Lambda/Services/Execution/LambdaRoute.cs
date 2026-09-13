using GenHTTP.Api.Content;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Protection;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.ErrorHandling;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Execution;

/// <summary>
/// Assembles everything that answers <c>/lambda/:publicKey</c>: the handler that
/// runs the user code, wrapped in the layers that keep it in line.
/// </summary>
/// <remarks>
/// Concerns are applied inside out, so the list below reads from the code of the
/// user outwards to the client.
/// </remarks>
public static class LambdaRoute
{

    public static IHandler Create(IMetaService meta, IDeploymentService deployments, SpaResources spa, LambdaOptions options,
                                  LambdaTelemetry telemetry, ILoggerFactory loggers)
    {
        var execution = new LambdaExecutionHandler(deployments);

        return Concerns.Chain([
            // innermost, so what it times is the lambda and not the wait in
            // front of it - and by here the lambda is known
            new LambdaActivityConcernBuilder(telemetry),
            new ThrottleConcernBuilder(options),
            ErrorHandler.From(new LambdaErrorMapper(loggers.CreateLogger<LambdaErrorMapper>())),
            new LambdaResolutionConcernBuilder(meta, spa),
            new RateLimitConcernBuilder(options)
        ], execution);
    }

}
