using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// Turns away a client that is over its budget of lambda requests. The
/// budget itself is kept by <see cref="LambdaRateLimiter"/>.
/// </summary>
public sealed class RateLimitConcern(IHandler content, LambdaRateLimiter limiter) : IConcern
{

    #region Get-/Setters

    public IHandler Content => content;

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public ValueTask<IResponse?> HandleAsync(IRequest request)
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

public sealed class RateLimitConcernBuilder(LambdaRateLimiter limiter) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new RateLimitConcern(content, limiter);
}
