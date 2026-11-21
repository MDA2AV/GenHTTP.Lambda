using GenHTTP.Api.Protocol;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// Carries the lambda a request has been resolved to from the protection layer
/// down to the handler that executes it.
/// </summary>
public static class LambdaRequest
{
    private const string Key = "__LAMBDA_TARGET";

    public static void SetLambda(this IRequest request, ResolvedLambda lambda) => request.Properties[Key] = lambda;

    public static ResolvedLambda RequireLambda(this IRequest request)
        => request.Properties.TryGet<ResolvedLambda>(Key, out var lambda)
         ? lambda
         : throw new InvalidOperationException("The request has not been resolved to a lambda.");

    public static ResolvedLambda? GetLambda(this IRequest request)
        => request.Properties.TryGet<ResolvedLambda>(Key, out var lambda) ? lambda : null;

}
