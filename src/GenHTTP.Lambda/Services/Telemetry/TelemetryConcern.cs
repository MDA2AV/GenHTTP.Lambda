using System.Diagnostics;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Counts what passes through the server. Sits at the root so it sees the API,
/// the lambdas and the single page application alike.
/// </summary>
/// <remarks>
/// A request that is upgraded is counted apart from one that is answered: it
/// leaves the handler in milliseconds and then lives for as long as the socket
/// does, so averaging it in with the others would say nothing about either.
/// </remarks>
public sealed class TelemetryConcern(IHandler content, TelemetryService telemetry) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        using var tracked = telemetry.Track();

        var started = Stopwatch.GetTimestamp();

        try
        {
            var response = await content.HandleAsync(request);

            var status = response != null ? (int)response.Status : 404;

            if (status == 101)
            {
                telemetry.Upgraded();
            }
            else
            {
                telemetry.Record(Stopwatch.GetElapsedTime(started), status);
            }

            return response;
        }
        catch (Exception)
        {
            // the error handler above turns this into a response, but the
            // request still cost what it cost
            telemetry.Record(Stopwatch.GetElapsedTime(started), 500);
            throw;
        }
    }

}

public sealed class TelemetryConcernBuilder(TelemetryService telemetry) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new TelemetryConcern(content, telemetry);
}
