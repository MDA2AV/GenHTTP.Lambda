using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Visits;

/// <summary>
/// One request a lambda answered, as everything that counts or logs requests
/// needs to know it.
/// </summary>
/// <remarks>
/// Made once per request to a lambda or a preview of one, by
/// <see cref="LambdaVisitConcern"/>, and left on the request
/// (<see cref="LambdaVisits.GetVisit"/>) for whoever reads it: the figures of
/// the overview and the stats, the owner's analytics and the line the request
/// log writes. So the path is cut, the clock read and the status decided once,
/// rather than by each of them in its own way, and they never disagree about
/// what the visitor was given. A reader that wants something of a request to a
/// lambda takes it from here, and what is missing is added here.
///
/// The visit says what happened; who is interested in which visits is the
/// reader's to decide - the figures and the analytics leave previews out, the
/// log keeps them. It holds what is cheap to have at hand and nothing worked
/// out from it: what only analytics make of a visit - the host of the
/// referrer, the kind of device - is worked out by them, off the request, when
/// they write it down.
/// </remarks>
/// <param name="Lambda">The lambda that answered, with the feature whose preview it was, if it was one</param>
/// <param name="At">When the answer was given</param>
/// <param name="Path">What was asked for, relative to the lambda and cut to a length worth showing</param>
/// <param name="Domain">The host it was reached at - its subdomain of the hosting domain, or its own domain</param>
/// <param name="Status">The status the visitor was given - a not found where nothing answered, a server error where the lambda threw</param>
/// <param name="Elapsed">How long the answer took, waiting for a slot included</param>
/// <param name="Bytes">What the response carried, where that is known</param>
/// <param name="Client">Who asked, as the log names them - nothing where the installation does not keep addresses</param>
/// <param name="Agent">The browser or program that asked, as it called itself</param>
/// <param name="Country">The country the address was allocated to, where it is known</param>
/// <param name="Referrer">
/// The page the visitor came from, as the browser said - read only for a lambda whose owner counts visits, since
/// nothing else asks for it
/// </param>
/// <param name="Page">
/// Whether this was somebody opening a page - a browser navigating to it, or a program asking for HTML - rather than
/// the scripts, pictures and calls that page goes on to load. Only worked out for a lambda whose owner counts visits.
/// </param>
/// <param name="Hints">
/// What a Chromium browser says about itself apart from its agent, which it no longer keeps accurate - read only
/// for a lambda whose owner counts visits
/// </param>
public sealed record LambdaVisit(
    ResolvedLambda Lambda,
    DateTime At,
    string Path,
    string? Domain,
    int Status,
    TimeSpan Elapsed,
    long Bytes,
    string? Client,
    string? Agent,
    string? Country,
    string? Referrer,
    bool Page,
    ClientHints? Hints = null
)
{

    /// <summary>
    /// Whether this was somebody trying a feature at its preview address
    /// rather than visiting the lambda.
    /// </summary>
    public bool Preview => Lambda.Feature != null;

}

/// <summary>
/// The client hints a Chromium browser sends with every request by default:
/// its brands, whether it is on a phone, and its operating system.
/// </summary>
/// <remarks>
/// Chromium froze most of its User-Agent - every Android phone is "Android 10;
/// K" there - and says the rest here instead, so where they are sent they are
/// the better answer. Kept as sent; like the agent, they are written by the
/// sender and read with that in mind.
/// </remarks>
/// <param name="Brands">Sec-CH-UA, such as <c>"Google Chrome";v="141", "Not?A_Brand";v="8", "Chromium";v="141"</c></param>
/// <param name="Mobile">Sec-CH-UA-Mobile, <c>?1</c> on a phone</param>
/// <param name="Platform">Sec-CH-UA-Platform, such as <c>"Android"</c></param>
public sealed record ClientHints(string? Brands, string? Mobile, string? Platform);

/// <summary>
/// Carries the visit a request was to whatever reads it.
/// </summary>
public static class LambdaVisits
{
    private const string Key = "__LAMBDA_VISIT";

    public static void SetVisit(this IRequest request, LambdaVisit visit) => request.Properties[Key] = visit;

    /// <summary>
    /// The visit the request was, once a lambda or a preview answered it, or
    /// nothing before that and for a request that reached no lambda.
    /// </summary>
    public static LambdaVisit? GetVisit(this IRequest request)
        => request.Properties.TryGet<LambdaVisit>(Key, out var visit) ? visit : null;

}
