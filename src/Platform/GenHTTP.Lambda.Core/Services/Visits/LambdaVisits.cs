using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Visits;

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
