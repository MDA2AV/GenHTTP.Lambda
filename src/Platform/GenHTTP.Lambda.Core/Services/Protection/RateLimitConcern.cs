using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.DependencyInjection;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// Turns away a client that is over its budget of lambda requests. The
/// budget itself is kept by <see cref="LambdaRateLimiter"/>.
/// </summary>
public sealed class RateLimitConcern(LambdaRateLimiter limiter) : IDependentConcern
{

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request)
    {
        var client = request.Client.Address;

        if (client != null && !limiter.Allow(client))
        {
            throw new ProviderException(ResponseStatus.TooManyRequests, $"Lambdas accept at most {limiter.Limit} requests per second and client.",
                response => response.Header("Retry-After", "1"));
        }

        return content.HandleAsync(request);
    }

    #endregion

}
