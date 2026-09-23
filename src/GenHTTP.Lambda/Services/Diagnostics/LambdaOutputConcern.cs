using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Protection;

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
public sealed class LambdaOutputConcern(IHandler content, LogBook book, int most) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var lambda = request.GetLambda();

        if (lambda == null)
        {
            return await content.HandleAsync(request);
        }

        using (LambdaOutput.Enter(new OutputScope(lambda.PublicKey, book, most, lambda.Id)))
        {
            return await content.HandleAsync(request);
        }
    }

}

public sealed class LambdaOutputConcernBuilder(LogBook book, int most) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new LambdaOutputConcern(content, book, most);
}
