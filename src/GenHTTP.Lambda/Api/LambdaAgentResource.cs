using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The agent of this installation, working on one lambda for whoever holds
/// its editor key: the Change section of the control center.
/// </summary>
/// <remarks>
/// Behind the editor key like everything else about a lambda, and refused for
/// a demo like everything else that would change one. The agent is the same
/// one the build page uses, in the same kind of container and on the same
/// queue; what differs is that it is handed the key, told to change rather
/// than create, and cannot create anything.
///
/// A process rather than a thing, so it is read at <c>agent</c> and driven
/// with verbs, the way a deployment is.
/// </remarks>
public sealed class LambdaAgentResource(BuildService builds, IMetaService meta, IFeatureService features, ILogger<LambdaAgentResource> logger)
{

    /// <summary>
    /// Whether a change can be asked for, how many this caller has left
    /// today, and the change under way or the last one.
    /// </summary>
    /// <remarks>
    /// Polled while a change runs. The agent remembers a change for an hour
    /// after it was asked for; after that there is nothing to report, and the
    /// versions it made are in the history.
    /// </remarks>
    [ResourceMethod("lambdas/:privateKey/agent")]
    public async ValueTask<AgentState> Get(string privateKey, IRequest request)
    {
        var lambda = await meta.RequireEditableAsync(privateKey);

        return await builds.StateAsync(lambda, request.Client.Address);
    }

    /// <summary>
    /// Asks the agent to change the lambda.
    /// </summary>
    /// <remarks>
    /// One change of a lambda at a time: a second is refused with 409 while
    /// the first is queued or running, since two agents writing the same files
    /// would each undo the other. Counted against the same daily allowance as
    /// the build page.
    ///
    /// The agent works in a feature: a new one, or the one named - which is
    /// looked up here, so a feature that does not exist is refused before
    /// anything is spent on it.
    /// </remarks>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/agent/start")]
    public async ValueTask<Result<AgentState>> Start(string privateKey, ChangeRequest body, IRequest request)
    {
        var lambda = await meta.RequireEditableAsync(privateKey);

        var current = await meta.RequireAsync(privateKey);

        var feature = body?.Feature is { Length: > 0 } key ? (await features.GetAsync(privateKey, key)).Feature.Key : null;

        var state = await builds.ChangeAsync(lambda, privateKey, current.ActiveVersion, body?.Prompt, body?.Deploy ?? true,
                                             body?.Model, body?.Password, body?.Language, feature, request.Client.Address);

        // logged in full, as the prompts of the build page are
        logger.LogInformation("Asked the agent to change lambda {Lambda} with the {Model} model: {Prompt}", current.PublicKey, body?.Model ?? "default", body?.Prompt?.Trim());

        return new Result<AgentState>(state).Status(ResponseStatus.Accepted);
    }

    /// <summary>
    /// Stops the change under way. What it saved so far stays saved - in its
    /// feature, or as a version - and whatever is online stays online.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/agent/stop")]
    public async ValueTask<AgentState> Stop(string privateKey, IRequest request)
    {
        var lambda = await meta.RequireEditableAsync(privateKey);

        var state = await builds.StopAsync(lambda, request.Client.Address);

        logger.LogInformation("Stopped the agent changing lambda {Lambda}", await meta.PublicKeyOfAsync(privateKey));

        return state;
    }

}
