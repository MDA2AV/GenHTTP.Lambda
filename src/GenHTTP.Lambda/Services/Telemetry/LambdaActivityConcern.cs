using System.Diagnostics;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Protection;

using GenHTTP.Modules.DependencyInjection;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Makes the visit a request to a lambda was, counts it, and leaves it on the
/// request for whatever else counts it.
/// </summary>
/// <remarks>
/// Sits outside the throttle and the error handler, so what it records is the
/// answer the visitor was given - a timeout is a timeout here and not the
/// success the lambda eventually got round to - and what it times is the whole
/// wait rather than only the part spent running.
///
/// The visit is made here once (see <see cref="LambdaVisit"/>). The figures of
/// the overview are counted from it at once, in memory; the concerns outside
/// this one read it off the request as it passes them on the way back (the
/// owner's analytics, see <c>AnalyticsConcern</c>).
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

        /*
         * What only the owner's analytics read, and only where they are on.
         * Read before the lambda is asked, because the headers of a request
         * are let go of once its body was read.
         */
        var counted = lambda.AnalyticsSince != null;

        var referrer = counted ? request.Header.Headers.GetEntry("Referer") : null;

        var destination = counted ? request.Header.Headers.GetEntry("Sec-Fetch-Dest") : null;

        var started = Stopwatch.GetTimestamp();

        IResponse? response = null;

        // what the visitor is told when the lambda throws: the error handler
        // above makes a response of it, but as far as the lambda is concerned
        // it failed
        var status = 500;

        try
        {
            response = await content.HandleAsync(request);

            // nothing answered is a not found by the time the client sees it
            status = response != null ? (int)response.Status : 404;

            return response;
        }
        finally
        {
            // the caller is made once for the whole request, its country and
            // its agent included, before anything below was asked
            var caller = Caller.Ambient;

            var visit = new LambdaVisit(lambda, DateTime.UtcNow, path, domain, status, Stopwatch.GetElapsedTime(started),
                                        (long)(response?.Content?.Length ?? 0), caller?.Client, caller?.Agent, caller?.Country,
                                        referrer, counted && IsPage(request, status, destination, response));

            request.SetVisit(visit);

            telemetry.Record(visit);
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

    /// <summary>
    /// Whether somebody opened a page, rather than the page loading what it
    /// is made of.
    /// </summary>
    /// <remarks>
    /// A browser says so itself: it marks a navigation with
    /// <c>Sec-Fetch-Dest: document</c>, and only that - so a page revalidated
    /// from its cache (Not Modified) is a page opened again, and a script or
    /// a call to an API is not one, whatever it answers with. Programs that
    /// do not send the header - crawlers, mostly - opened a page when what
    /// they got back was HTML. A redirect is not a page: the page it sends to
    /// is counted when it is opened.
    /// </remarks>
    private static bool IsPage(IRequest request, int status, string? destination, IResponse? response)
    {
        if (request.Header.Method != RequestMethod.Get || status is not (>= 200 and < 300 or 304))
        {
            return false;
        }

        if (destination != null)
        {
            return string.Equals(destination, "document", StringComparison.OrdinalIgnoreCase);
        }

        return status != 304 && response?.Content?.Type is { } type && type.Bytes.Span.StartsWith("text/html"u8);
    }

}
