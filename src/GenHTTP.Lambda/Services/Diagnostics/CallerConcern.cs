using System.Diagnostics;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
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
public sealed class CallerConcern(IHandler content, LogBook book, StringPool pool, LambdaOptions options) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var caller = CallerInfo.From(request, pool, options.LogClientAddress);

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

    private void Record(CallerInfo caller, IRequest request, IResponse? response, TimeSpan took)
    {
        // nothing answered is a not found by the time the client sees it
        var status = response != null ? (int)response.Status : 404;

        var bytes = response?.Content?.Length ?? 0;

        var level = status >= 500 ? "error" : status >= 400 ? "warn" : "info";

        // resolved by a concern below this one, so by the time the answer is
        // on its way back the request knows which lambda it reached
        book.Append(level, "Requests", request.GetLambda()?.PublicKey,
                    $"{caller.Method} {caller.Path} — {status} · {bytes:N0} B · {took.TotalMilliseconds:N2} ms",
                    null, caller.Client, caller.Agent);
    }

}

public sealed class CallerConcernBuilder(LogBook book, StringPool pool, LambdaOptions options) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new CallerConcern(content, book, pool, options);
}
