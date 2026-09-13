using System.Diagnostics;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Protection;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Counts what a single lambda does. Sits innermost, directly around the code
/// of the user, so what it times is the lambda rather than the queue in front
/// of it.
/// </summary>
public sealed class LambdaActivityConcern(IHandler content, LambdaTelemetry telemetry) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var lambda = request.GetLambda();

        if (lambda == null)
        {
            return await content.HandleAsync(request);
        }

        var started = Stopwatch.GetTimestamp();

        try
        {
            var response = await content.HandleAsync(request);

            var status = response != null ? (int)response.Status : 404;

            telemetry.Record(lambda.Id, lambda.PublicKey, Stopwatch.GetElapsedTime(started), status,
                             (long)(response?.Content?.Length ?? 0));

            return response;
        }
        catch (Exception)
        {
            // the error handler above turns this into a response for the
            // visitor, but as far as the lambda is concerned it failed
            telemetry.Record(lambda.Id, lambda.PublicKey, Stopwatch.GetElapsedTime(started), 500, 0);
            throw;
        }
    }

}

public sealed class LambdaActivityConcernBuilder(LambdaTelemetry telemetry) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new LambdaActivityConcern(content, telemetry);
}
