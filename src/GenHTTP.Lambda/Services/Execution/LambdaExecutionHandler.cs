using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Protection;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Execution;

/// <summary>
/// Runs the handler a user defined. The lambda to run has already been resolved
/// by the protection layer, so all that is left is to fetch the compiled
/// handler and let it answer the request.
/// </summary>
/// <remarks>
/// The lambda itself, or the preview of one of its features - which is the
/// same thing built from other files, and the only one of the two a search
/// engine is asked to leave alone: its address is handed around to try a
/// change, not to be found.
/// </remarks>
public sealed class LambdaExecutionHandler(IDeploymentService deployments, LimitsService limits) : IHandler
{

    public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var lambda = request.RequireLambda();

        var workspace = limits.WorkspaceOf(lambda.Tier, lambda.WorkspaceEnabled);

        if (lambda.Feature is not { } feature)
        {
            var handler = await deployments.ResolveAsync(lambda.Id, lambda.ActiveVersion, workspace);

            return await handler.HandleAsync(request);
        }

        var preview = await deployments.ResolvePreviewAsync(lambda.Id, feature.Id, feature.Preview, workspace);

        var response = await preview.HandleAsync(request);

        if (response == null || (int)response.Status == 101)
        {
            return response;
        }

        var rebuilt = response.Rebuild().Header("X-Robots-Tag", "noindex, nofollow");

        // the key in its address is what opens it: not handed to whatever the
        // page links to, unless the code decided otherwise
        if (!response.Headers.ContainsKey("Referrer-Policy"))
        {
            rebuilt.Header("Referrer-Policy", "no-referrer");
        }

        return rebuilt.Build();
    }

}
