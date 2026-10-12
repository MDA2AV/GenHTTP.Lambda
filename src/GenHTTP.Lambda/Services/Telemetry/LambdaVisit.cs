using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// One request a lambda answered, as everything that counts requests needs to
/// know it.
/// </summary>
/// <remarks>
/// Made once per request, by <see cref="LambdaActivityConcern"/>, and left on
/// the request (<see cref="LambdaVisits.GetVisit"/>) for whoever counts it -
/// the figures the overview and the stats show, and the owner's analytics. So
/// the path is cut, the clock read and the status decided once, rather than by
/// every one of them in its own way, and they never disagree about what the
/// visitor was given.
///
/// Holds what is cheap to have at hand and nothing worked out from it: what
/// only analytics make of a visit - the host of the referrer, the kind of
/// device - is worked out by them, off the request, when they write it down.
/// </remarks>
/// <param name="Lambda">The lambda that answered</param>
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
    bool Page
);

/// <summary>
/// Carries the visit a request was to the concerns that count it.
/// </summary>
public static class LambdaVisits
{
    private const string Key = "__LAMBDA_VISIT";

    public static void SetVisit(this IRequest request, LambdaVisit visit) => request.Properties[Key] = visit;

    /// <summary>
    /// The visit the request was, once a lambda answered it - nothing before,
    /// and nothing for a preview of a feature, which is nobody's visit.
    /// </summary>
    public static LambdaVisit? GetVisit(this IRequest request)
        => request.Properties.TryGet<LambdaVisit>(Key, out var visit) ? visit : null;

}
