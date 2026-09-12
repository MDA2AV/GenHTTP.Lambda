using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// Bounds what the lambdas as a whole may consume: how many requests run at the
/// same time, and how long a single one may take.
/// </summary>
/// <remarks>
/// The timeout is cooperative - a lambda that ignores its cancellation token and
/// spins forever still occupies its slot. Containing that needs process
/// isolation, which is where the deployment service is headed.
/// </remarks>
public sealed class ThrottleConcern(IHandler content, LambdaOptions options) : IConcern
{
    private readonly SemaphoreSlim _slots = new(options.MaxConcurrency, options.MaxConcurrency);

    #region Get-/Setters

    public IHandler Content => content;

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (!await _slots.WaitAsync(TimeSpan.FromSeconds(5)))
        {
            throw new ProviderException(ResponseStatus.ServiceUnavailable, "The server is currently busy, please try again.");
        }

        try
        {
            var execution = content.HandleAsync(request).AsTask();

            var completed = await Task.WhenAny(execution, Task.Delay(options.ExecutionTimeout));

            if (completed != execution)
            {
                throw new ProviderException(ResponseStatus.GatewayTimeout, $"The lambda did not respond within {options.ExecutionTimeout.TotalSeconds:0} seconds.");
            }

            return await execution;
        }
        finally
        {
            _slots.Release();
        }
    }

    #endregion

}

public sealed class ThrottleConcernBuilder(LambdaOptions options) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new ThrottleConcern(content, options);
}
