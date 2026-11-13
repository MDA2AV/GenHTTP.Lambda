using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Lambda.Configuration;

using GenHTTP.Modules.DependencyInjection;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// Bounds what the lambdas as a whole may consume: how many requests run at the
/// same time, and how long a single one may take.
/// </summary>
/// <remarks>
/// The slots are those of <see cref="LambdaThrottle"/>, shared by every route
/// a lambda can be reached through.
///
/// The timeout is cooperative - a lambda that ignores its cancellation token and
/// spins forever still occupies its slot. Containing that needs process
/// isolation, which is where the deployment service is headed.
/// </remarks>
public sealed class ThrottleConcern(LambdaThrottle throttle, LambdaOptions options) : IDependentConcern
{

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request)
    {
        if (!await throttle.EnterAsync())
        {
            throw new ProviderException(ResponseStatus.ServiceUnavailable, "The server is currently busy, please try again.");
        }

        try
        {
            var execution = content.HandleAsync(request);

            /*
             * Most lambdas answer before they ever wait, and an answer that is
             * already there cannot be late. Only one that is still under way
             * gets a clock - which used to be started for every request, a
             * timer per request left running for the whole timeout after the
             * answer had long gone out.
             */
            if (execution.IsCompletedSuccessfully)
            {
                // finished, so awaiting it hands the answer straight back
                return await execution;
            }

            var pending = execution.AsTask();

            try
            {
                // stops its clock once the lambda answers, unlike a delay raced against it
                return await pending.WaitAsync(options.ExecutionTimeout);
            }
            catch (TimeoutException timeout) when (!ReferenceEquals(timeout, pending.Exception?.InnerException))
            {
                // the clock's, and not one the lambda threw itself
                throw new ProviderException(ResponseStatus.GatewayTimeout, $"The lambda did not respond within {options.ExecutionTimeout.TotalSeconds:0} seconds.");
            }
        }
        finally
        {
            throttle.Leave();
        }
    }

    #endregion

}
