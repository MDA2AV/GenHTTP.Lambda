using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Visits;

using GenHTTP.Modules.DependencyInjection;

namespace GenHTTP.Lambda.Services.QuietHours;

/// <summary>
/// Counts every request the server answers by the hour of the day, for the
/// installation's quiet hours - and, from the visit it was, for the quiet
/// hours of the lambda that answered it.
/// </summary>
/// <remarks>
/// <para>
/// At the root of the host, beside the telemetry, so it sees the API, the
/// pages and the lambdas alike; the visit was made further in
/// (<see cref="LambdaVisitConcern"/>) and is on the request by the time the
/// answer passes here. A feature's preview is counted for the installation,
/// whose load it is, and not for the lambda, whose visitors it is not.
/// </para>
/// <para>
/// What it costs a request is two increments of a slot, without a lock, and
/// a look at the clock - and no frame of its own where the answer was there
/// at once, as it mostly is on the reactor of the Ioxide engine.
/// </para>
/// </remarks>
public sealed class QuietHoursConcern(HourlyCounts counts) : IDependentConcern
{

    public ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request)
    {
        ValueTask<IResponse?> answer;

        try
        {
            answer = content.HandleAsync(request);
        }
        catch
        {
            Count(request);
            throw;
        }

        if (answer.IsCompleted)
        {
            Count(request);
            return answer;
        }

        return CountOnceAnsweredAsync(answer, request);
    }

    private async ValueTask<IResponse?> CountOnceAnsweredAsync(ValueTask<IResponse?> answer, IRequest request)
    {
        try
        {
            return await answer;
        }
        finally
        {
            Count(request);
        }
    }

    private void Count(IRequest request)
    {
        counts.CountInstallation(DateTime.UtcNow.Hour);

        if (request.GetVisit() is { Preview: false } visit)
        {
            counts.CountLambda(visit.Lambda.Id, visit.At.Hour);
        }
    }

}
