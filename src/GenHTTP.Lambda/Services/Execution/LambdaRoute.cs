using GenHTTP.Api.Content;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Analytics;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Protection;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Visits;

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
/// A lambda can be reached in more than one way - by its key, as a
/// subdomain of the hosting domain, or at a domain of its own, and as the
/// preview of a feature below <c>/features/</c> - and each way is a chain built
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

        var execution = new LambdaExecutionHandler(services.GetRequiredService<IDeploymentService>(), services.GetRequiredService<ILimitsService>());

        return Concerns.Chain([
            Dependent.Concern<ThrottleConcern>(),
            ErrorHandler.From(new LambdaErrorMapper(services.GetRequiredService<ILogger<LambdaErrorMapper>>())),
            // outside the error handler and the timeout, so a lambda that
            // printed and then failed keeps what it printed, and the warning
            // the error handler writes is filed under it as well
            .. options.CaptureLambdaOutput
               ? new IConcernBuilder[] { Dependent.Concern<LambdaOutputConcern>() }
               : [],
            // the visit the request was, made once: outside the error handler
            // and the throttle, inside the lookup - so it is the answer the
            // visitor actually got, filed under the lambda that gave it.
            // Inside the throttle it saw neither the timeout nor the
            // rejection - a lambda that timed out on every request was
            // recorded as succeeding, slowly.
            Dependent.Concern<LambdaVisitConcern>(),
            // what reads the visit, outside what makes it, so it is on the
            // request as the answer passes: the figures, and the owner's
            // analytics, which only queue it - nothing is written while the
            // visitor waits. The request log reads it at the outside of the
            // host.
            Dependent.Concern<LambdaActivityConcern>(),
            Dependent.Concern<AnalyticsConcern>(),
            new LambdaResolutionConcernBuilder(locator),
            Dependent.Concern<RateLimitConcern>()
        ], execution);
    }

}
