using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Protection;

using GenHTTP.Modules.DependencyInjection;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Marks a request as belonging to a lambda, so that whatever it prints can be
/// told from whatever anything else prints.
/// </summary>
/// <remarks>
/// Sits inside the lookup, so there is a lambda to name, and outside the error
/// handler and the execution timeout, so a lambda that printed three lines and
/// then hung still has its three lines. The mark flows with the request rather
/// than with the thread: work the lambda starts and awaits stays attributed to
/// it however many thread pool hops it takes.
/// </remarks>
public sealed class LambdaOutputConcern(LogBook book, LambdaOptions options) : IDependentConcern
{

    public async ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request)
    {
        var lambda = request.GetLambda();

        if (lambda == null)
        {
            return await content.HandleAsync(request);
        }

        using (LambdaOutput.Enter(new OutputScope(lambda.PublicKey, book, options.MaxOutputLines, lambda.Id, lambda.Feature?.Id)))
        {
            return await content.HandleAsync(request);
        }
    }

}
