using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Visits;

using GenHTTP.Modules.DependencyInjection;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Counts what a single lambda does, from the visit each request was.
/// </summary>
/// <remarks>
/// Outside the concern that makes the visit (<see cref="LambdaVisitConcern"/>),
/// so it finds it on the request as the answer passes - whether the lambda
/// answered or threw - and counts what the visitor was given without measuring
/// anything itself.
/// </remarks>
public sealed class LambdaActivityConcern(LambdaTelemetry telemetry) : IDependentConcern
{

    public async ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request)
    {
        try
        {
            return await content.HandleAsync(request);
        }
        finally
        {
            // a preview is somebody trying a change: counting it would make
            // the lambda look used, and keep a free one online, while nobody
            // visits it
            if (request.GetVisit() is { Preview: false } visit)
            {
                telemetry.Record(visit);
            }
        }
    }

}
