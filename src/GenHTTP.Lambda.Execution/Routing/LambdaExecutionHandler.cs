using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using Microsoft.Extensions.Logging;

using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Protection;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Execution;

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
///
/// A lambda answering on the platform's own host - a preview always, a live
/// lambda where the lambdas answer by path - may not widen the scope of a
/// service worker beyond its own path: a worker for <c>/</c> would see every
/// page of the platform that browser opens afterwards, the editor and its
/// key, the panel and its token. Without the header a worker stays below
/// the lambda's path, which is all a lambda needs.
/// </remarks>
public sealed class LambdaExecutionHandler(IDeploymentService deployments, ILimitsService limits, ILogger<LambdaExecutionHandler> logger) : IHandler
{

    private static readonly byte[] WorkerScope = "Service-Worker-Allowed"u8.ToArray();

    public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var lambda = request.RequireLambda();

        var workspace = limits.WorkspaceOf(lambda.Tier, lambda.WorkspaceEnabled);

        if (lambda.Feature is not { } feature)
        {
            var handler = await deployments.ResolveAsync(lambda.Id, lambda.ActiveVersion, workspace);

            var answer = await handler.HandleAsync(request);

            // at a host of its own, its scope is its own business
            return request.GetHost() == null ? Confined(request, answer, lambda.PublicKey) : answer;
        }

        var preview = await deployments.ResolvePreviewAsync(lambda.Id, feature.Id, feature.Preview, workspace);

        var response = Confined(request, await preview.HandleAsync(request), lambda.PublicKey);

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

    /// <summary>
    /// The answer, unless it would let a service worker take over paths of
    /// the platform - which is refused instead, and said in the log.
    /// </summary>
    /// <remarks>
    /// Refused rather than answered without the header: a response is
    /// added to, not taken from, and a worker that silently failed to
    /// register would be harder to understand than a refusal saying why.
    /// </remarks>
    private IResponse? Confined(IRequest request, IResponse? response, string publicKey)
    {
        if (response == null || !Names(response, WorkerScope))
        {
            return response;
        }

        logger.LogWarning("Refused Service-Worker-Allowed from lambda {Lambda} path {Path}, which answers on the platform's host", publicKey, request.Header.Path.ToString());

        return request.Respond()
                      .Status(ResponseStatus.InternalServerError)
                      .Content("Refused: a lambda answering on the platform's address may not widen the scope of a service worker (Service-Worker-Allowed). Register the worker without it - its scope is then the folder of its script.")
                      .Build();
    }

    private static bool Names(IResponse response, byte[] name)
    {
        for (var i = 0; i < response.Headers.Count; i++)
        {
            var key = response.Headers.GetMemoryEntry(i).Key.Span;

            if (key.Length == name.Length && System.Text.Ascii.EqualsIgnoreCase(key, name))
            {
                return true;
            }
        }

        return false;
    }

}
