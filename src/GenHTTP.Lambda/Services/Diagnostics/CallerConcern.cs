using System.Diagnostics;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Protection;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Names the caller on every line a request causes, and writes the line that
/// says how the request itself went.
/// </summary>
/// <remarks>
/// The engine logs a line per request already, but it has only the parts it
/// needs - the method, the path, the status - and not who was asking, which is
/// the first thing wanted when a path is being hit five hundred times a minute
/// and nobody knows by what. So the book takes this one instead and skips the
/// engine's; both still reach stdout, because that is the engine's to write.
///
/// Outermost, so the mark is in place before anything below can log under it,
/// and so the duration covers the whole answer rather than the part after the
/// throttle let it through.
/// </remarks>
public sealed class CallerConcern(IHandler content, LogBook book, StringPool pool, GeoTable geo, GeoPlaces places, DomainRegistry domains,
                                  LambdaOptions options) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        // decided here, before anything below can read a body and take the
        // headers with it, and remembered on the request for the router
        var domain = request.ResolveDomain(domains);

        var caller = CallerInfo.From(request, pool, options.LogClientAddress, options.Geo ? geo : null,
                                     options.GeoPlaces ? places : null, domain?.Name);

        var started = Stopwatch.GetTimestamp();

        using (Caller.Enter(caller))
        {
            IResponse? response = null;

            try
            {
                response = await content.HandleAsync(request);

                return response;
            }
            finally
            {
                Record(caller, request, response, Stopwatch.GetElapsedTime(started));
            }
        }
    }

    /// <summary>
    /// Whether this is an owner's control center asking how their lambda is
    /// doing, which it does every few seconds for as long as it is open.
    /// </summary>
    /// <remarks>
    /// The same reasoning as the panel's tail, for the same reason: a page
    /// that polls would otherwise become most of what the log holds.
    /// </remarks>
    private static bool Watching(CallerInfo caller)
        => caller.Domain == null
        && caller.Method == "GET"
        && caller.Path.StartsWith("/api/v1/lambdas/", StringComparison.Ordinal)
        && (caller.Path.EndsWith("/logs", StringComparison.Ordinal)
         || caller.Path.EndsWith("/traffic", StringComparison.Ordinal)
         || caller.Path.EndsWith("/summary", StringComparison.Ordinal));

    private void Record(CallerInfo caller, IRequest request, IResponse? response, TimeSpan took)
    {
        /*
         * Reading the log does not fill the log.
         *
         * The panel asks for the tail every second and a half and each ask is
         * a request like any other, so left in it becomes most of what there
         * is to read - measured at four in every five lines - and pushes what
         * somebody opened the page for out of the ring. It was suppressed
         * when these lines came from the engine and the suppression was lost
         * when they started coming from here.
         */
        if ((caller.Domain == null && caller.Path.StartsWith("/api/v1/logs", StringComparison.Ordinal)) || Watching(caller))
        {
            return;
        }

        // nothing answered is a not found by the time the client sees it
        var status = response != null ? (int)response.Status : 404;

        var bytes = response?.Content?.Length ?? 0;

        var level = status >= 500 ? "error" : status >= 400 ? "warn" : "info";

        // resolved by a concern below this one, so by the time the answer is
        // on its way back the request knows which lambda it reached
        var lambda = request.GetLambda();

        // a lambda's own domain is named in front of the path: its paths are
        // its own, and "/api/v1/logs" there is not the platform's
        var target = caller.Domain != null ? caller.Domain + caller.Path : caller.Path;

        book.Append(level, "Requests", lambda?.PublicKey,
                    $"{caller.Method} {target} — {status} · {bytes:N0} B · {took.TotalMilliseconds:N2} ms",
                    null, caller.Client, caller.Agent, caller.Country, caller.Place,
                    // what it asked for and what it got back identifies the
                    // line; how many microseconds it took measures it
                    $"{caller.Method} {target} {status}",
                    lambda?.Id, caller.Domain);
    }

}

public sealed class CallerConcernBuilder(LogBook book, StringPool pool, GeoTable geo, GeoPlaces places, DomainRegistry domains,
                                         LambdaOptions options) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new CallerConcern(content, book, pool, geo, places, domains, options);
}
