using GenHTTP.Api.Content;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Protection;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Telemetry;

using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.ErrorHandling;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Execution;

/// <summary>
/// Assembles everything that answers a request for a lambda: the handler that
/// runs the user code, wrapped in the layers that keep it in line.
/// </summary>
/// <remarks>
/// A lambda can be reached in more than one way - by its key below
/// <c>/lambda/</c>, or at a domain of its own - and each way is a chain built
/// here, differing only in the locator that finds the lambda. So a lambda is
/// throttled, timed, counted and logged the same whichever door it was
/// reached through.
///
/// Building a chain more than once is safe because none of its layers keep
/// anything of their own: what has to be shared between the chains - the
/// concurrency slots, the per-client budgets, the counters, the log - lives in
/// services. The layers that need nothing but those services are taken from
/// the container as they are asked (<see cref="Dependent"/>), the same
/// instance for every chain; the lookup is the one that is built here, since
/// its locator is what tells the chains apart.
///
/// Concerns are applied inside out, so the list below reads from the code of
/// the user outwards to the client.
/// </remarks>
public static class LambdaRoute
{

    public static IHandler Create(IServiceProvider services, ILambdaLocator locator)
    {
        var options = services.GetRequiredService<LambdaOptions>();

        var execution = new LambdaExecutionHandler(services.GetRequiredService<IDeploymentService>(), services.GetRequiredService<LimitsService>());

        return Concerns.Chain([
            Dependent.Concern<ThrottleConcern>(),
            ErrorHandler.From(new LambdaErrorMapper(services.GetRequiredService<ILogger<LambdaErrorMapper>>())),
            // outside the error handler and the timeout, so a lambda that
            // printed and then failed keeps what it printed, and the warning
            // the error handler writes is filed under it as well
            .. options.CaptureLambdaOutput
               ? new IConcernBuilder[] { Dependent.Concern<LambdaOutputConcern>() }
               : [],
            // outside the error handler and the throttle, inside the lookup: so
            // it records the answer the visitor actually got, and knows which
            // lambda to file it under. Inside the throttle it saw neither the
            // timeout nor the rejection - a lambda that timed out on every
            // request was recorded as succeeding, slowly.
            Dependent.Concern<LambdaActivityConcern>(),
            new LambdaResolutionConcernBuilder(locator),
            Dependent.Concern<RateLimitConcern>()
        ], execution);
    }

}
