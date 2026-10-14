using System.Diagnostics;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Protection;

using GenHTTP.Modules.DependencyInjection;

namespace GenHTTP.Lambda.Services.Visits;

/// <summary>
/// Makes the visit a request to a lambda was, and leaves it on the request for
/// whatever reads it.
/// </summary>
/// <remarks>
/// It only makes the visit: the concerns outside it read it as the answer
/// passes them on the way back - the figures (<c>LambdaActivityConcern</c>), the
/// owner's analytics (<c>AnalyticsConcern</c>) - and so does the request log at
/// the outside of the host (<c>CallerConcern</c>). None of them measures the
/// request again.
///
/// Outside the throttle and the error handler, so the visit is the answer the
/// visitor was given - a timeout is a timeout here and not the success the
/// lambda eventually got round to - and its time is the whole wait rather than
/// only the part spent running. Inside the lookup, so it knows which lambda it
/// was.
/// </remarks>
public sealed class LambdaVisitConcern : IDependentConcern
{

    public async ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request)
    {
        var lambda = request.GetLambda();

        if (lambda == null)
        {
            return await content.HandleAsync(request);
        }

        // read before the lambda routes it: the target is a pointer that the
        // handlers below move along, and afterwards it points at nothing
        var target = TargetOf(request);

        var path = target.Length <= 100 ? target : string.Concat(target.AsSpan(0, 99), "…");

        var domain = request.GetHost()?.Name;

        /*
         * What only the owner's analytics read, and only where they count
         * visits - never for a preview. Read before the lambda is asked,
         * because the headers of a request are let go of once its body was read.
         */
        var counted = lambda.AnalyticsSince != null && lambda.Feature == null;

        var headers = request.Header.Headers;

        var referrer = counted ? headers.GetEntry("Referer") : null;

        var destination = counted ? headers.GetEntry("Sec-Fetch-Dest") : null;

        var hints = counted ? HintsOf(headers) : null;

        var method = request.Header.Method;

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

            request.SetVisit(new LambdaVisit(lambda, DateTime.UtcNow, path, domain, status, Stopwatch.GetElapsedTime(started),
                                             (long)(response?.Content?.Length ?? 0), caller?.Client, caller?.Agent, caller?.Country,
                                             referrer, counted && IsPage(method, target, status, destination, response), hints));
        }
    }

    /// <summary>
    /// What was asked for, relative to the lambda - the lookup above has
    /// already stepped past the key, where there was one.
    /// </summary>
    /// <remarks>
    /// In full, so its ending is there to be read; the visit keeps it cut to
    /// a length worth showing.
    /// </remarks>
    private static string TargetOf(IRequest request)
    {
        var remaining = request.Header.Target.AsString(decode: false, remainingOnly: true);

        return remaining.Length == 0 ? "/" : remaining[0] == '/' ? remaining : "/" + remaining;
    }

    /// <summary>
    /// The client hints of a Chromium browser, or nothing from anything else.
    /// </summary>
    private static ClientHints? HintsOf(IKeyValueList headers)
    {
        var brands = headers.GetEntry("Sec-CH-UA");
        var mobile = headers.GetEntry("Sec-CH-UA-Mobile");
        var platform = headers.GetEntry("Sec-CH-UA-Platform");

        return brands == null && mobile == null && platform == null ? null : new ClientHints(Cut(brands), Cut(mobile), Cut(platform));
    }

    private static string? Cut(string? value) => value == null || value.Length <= 200 ? value : value[..200];

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
    /// they got back was HTML - unless they asked for a file that is no page
    /// (<see cref="Files"/>): a single page app answers every path with its
    /// page, <c>/favicon.ico</c> included, and a browser fetching its icons
    /// sends no <c>Sec-Fetch-Dest</c>. A redirect is not a page: the page it
    /// sends to is counted when it is opened.
    /// </remarks>
    private static bool IsPage(RequestMethod method, string target, int status, string? destination, IResponse? response)
    {
        if (method != RequestMethod.Get || status is not (>= 200 and < 300 or 304))
        {
            return false;
        }

        if (destination != null)
        {
            return string.Equals(destination, "document", StringComparison.OrdinalIgnoreCase);
        }

        return status != 304 && response?.Content?.Type is { } type && type.Bytes.Span.StartsWith("text/html"u8) && !NamesAFile(target);
    }

    /// <summary>
    /// The endings of files a page is made of or comes with, which nobody
    /// opens as a page.
    /// </summary>
    /// <remarks>
    /// A known few rather than every ending: a path with a dot in it may well
    /// be a page (<c>/user/jane.doe</c>, <c>/release/1.2</c>).
    /// </remarks>
    private static readonly HashSet<string> Files = new(StringComparer.OrdinalIgnoreCase)
    {
        "ico", "png", "jpg", "jpeg", "gif", "webp", "avif", "svg", "bmp",
        "js", "mjs", "map", "css", "json", "webmanifest", "xml", "txt",
        "woff", "woff2", "ttf", "otf", "eot",
        "mp3", "mp4", "webm", "ogg", "wav",
        "pdf", "zip", "wasm"
    };

    private static readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> FileEndings = Files.GetAlternateLookup<ReadOnlySpan<char>>();

    private static bool NamesAFile(string target)
    {
        var name = target.AsSpan(target.LastIndexOf('/') + 1);

        var dot = name.LastIndexOf('.');

        return dot >= 0 && FileEndings.Contains(name[(dot + 1)..]);
    }

}
