using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// One way a request can name the lambda it is for.
/// </summary>
/// <remarks>
/// The only part of serving a lambda that differs between the routes it can be
/// reached through. Whatever is left of the path once the lambda is found is
/// what the lambda routes on, so a locator that reads part of the path steps
/// past it.
/// </remarks>
public interface ILambdaLocator
{

    /// <summary>
    /// The deployed lambda the request is for, or nothing if there is none.
    /// </summary>
    ValueTask<ResolvedLambda?> LocateAsync(IRequest request);

    /// <summary>
    /// What a request is told when there is no lambda to answer it.
    /// </summary>
    ValueTask<IResponse> UnavailableAsync(IRequest request);

}
