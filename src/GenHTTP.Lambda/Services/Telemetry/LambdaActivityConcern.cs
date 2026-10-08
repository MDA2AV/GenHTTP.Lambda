using System.Diagnostics;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Protection;

using GenHTTP.Modules.DependencyInjection;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Counts what a single lambda does.
/// </summary>
/// <remarks>
/// Sits outside the throttle and the error handler, so what it records is the
/// answer the visitor was given - a timeout is a timeout here and not the
/// success the lambda eventually got round to - and what it times is the whole
/// wait rather than only the part spent running.
/// </remarks>
public sealed class LambdaActivityConcern(LambdaTelemetry telemetry) : IDependentConcern
{

    public async ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request)
    {
        var lambda = request.GetLambda();

        // a preview is somebody trying a change: counting it would make the
        // lambda look used, and keep a free one online, while nobody visits it
        if (lambda == null || lambda.Feature != null)
        {
            return await content.HandleAsync(request);
        }

        // read before the lambda routes it: the target is a pointer that the
        // handlers below move along, and afterwards it points at nothing
        var path = PathOf(request);

        var domain = request.GetHost()?.Name;

        var started = Stopwatch.GetTimestamp();

        try
        {
            var response = await content.HandleAsync(request);

            var status = response != null ? (int)response.Status : 404;

            telemetry.Record(lambda.Id, lambda.PublicKey, Stopwatch.GetElapsedTime(started), status,
                             (long)(response?.Content?.Length ?? 0), path, domain);

            return response;
        }
        catch (Exception)
        {
            // the error handler above turns this into a response for the
            // visitor, but as far as the lambda is concerned it failed
            telemetry.Record(lambda.Id, lambda.PublicKey, Stopwatch.GetElapsedTime(started), 500, 0, path, domain);
            throw;
        }
    }

    /// <summary>
    /// What was asked for, relative to the lambda and cut to a length worth
    /// showing - the lookup above has already stepped past the key, where
    /// there was one.
    /// </summary>
    private static string PathOf(IRequest request)
    {
        var remaining = request.Header.Target.AsString(decode: false, remainingOnly: true);

        var path = remaining.Length == 0 ? "/" : remaining[0] == '/' ? remaining : "/" + remaining;

        return path.Length <= 100 ? path : string.Concat(path.AsSpan(0, 99), "…");
    }

}
