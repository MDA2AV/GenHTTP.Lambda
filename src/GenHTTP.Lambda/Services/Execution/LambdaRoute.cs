using GenHTTP.Api.Content;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Diagnostics;
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
                                  LambdaTelemetry telemetry, LogBook book, ILoggerFactory loggers)
    {
        var execution = new LambdaExecutionHandler(deployments);

        return Concerns.Chain([
            new ThrottleConcernBuilder(options),
            ErrorHandler.From(new LambdaErrorMapper(loggers.CreateLogger<LambdaErrorMapper>())),
            // outside the error handler and the timeout, so a lambda that
            // printed and then failed keeps what it printed, and the warning
            // the error handler writes is filed under it as well
            .. options.CaptureLambdaOutput
               ? new IConcernBuilder[] { new LambdaOutputConcernBuilder(book, options.MaxOutputLines) }
               : [],
            // outside the error handler and the throttle, inside the lookup: so
            // it records the answer the visitor actually got, and knows which
            // lambda to file it under. Inside the throttle it saw neither the
            // timeout nor the rejection - a lambda that timed out on every
            // request was recorded as succeeding, slowly.
            new LambdaActivityConcernBuilder(telemetry),
            new LambdaResolutionConcernBuilder(meta, spa),
            new RateLimitConcernBuilder(options)
        ], execution);
    }

}
