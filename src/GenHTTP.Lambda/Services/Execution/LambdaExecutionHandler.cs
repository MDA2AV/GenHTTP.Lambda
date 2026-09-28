using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Protection;

namespace GenHTTP.Lambda.Services.Execution;

/// <summary>
/// Runs the handler a user defined. The lambda to run has already been resolved
/// by the protection layer, so all that is left is to fetch the compiled
/// handler and let it answer the request.
/// </summary>
public sealed class LambdaExecutionHandler(IDeploymentService deployments, LambdaOptions options) : IHandler
{

    public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var lambda = request.RequireLambda();

        var handler = await deployments.ResolveAsync(lambda.Id, lambda.ActiveVersion, lambda.ActiveRevision,
                                                     options.WorkspaceOf(lambda.Tier, lambda.WorkspaceEnabled));

        return await handler.HandleAsync(request);
    }

}
