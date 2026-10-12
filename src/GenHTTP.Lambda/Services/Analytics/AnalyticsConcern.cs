using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Visits;

using GenHTTP.Modules.DependencyInjection;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// Hands the visit a request was to the owner's analytics, where the owner
/// counts visits.
/// </summary>
/// <remarks>
/// Outside the concern that makes the visit (<see cref="LambdaVisitConcern"/>),
/// so it is on the request by the time the answer passes here - whether the
/// lambda answered or threw. All it does is put it on the queue: what the
/// visit is counted as is worked out, and written, by the job that empties it
/// (<see cref="AnalyticsJob"/>).
/// </remarks>
public sealed class AnalyticsConcern(AnalyticsQueue queue) : IDependentConcern
{

    public async ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request)
    {
        try
        {
            return await content.HandleAsync(request);
        }
        finally
        {
            // a preview is somebody trying a change, never a visitor
            if (request.GetVisit() is { Preview: false, Lambda.AnalyticsSince: not null } visit)
            {
                queue.Add(visit);
            }
        }
    }

}
