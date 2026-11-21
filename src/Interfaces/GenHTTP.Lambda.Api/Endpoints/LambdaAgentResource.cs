using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

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
[Tag(ApiDocumentation.Changes)]
[Summary("The build agent of the installation, changing a lambda - and answering questions about it - for whoever holds its editor key.")]
[Remarks("""
         What the Change section of the editor does: the owner says what should be different, and the agent changes the lambda
         in a feature - a new one, or one named - and tries it at its preview. With `deploy` (the default) it merges the feature
         and puts the new version online once it works; without, it leaves the feature for the owner to try and merge. What is
         online stays online until then.

         One change of a lambda runs at a time. Changes count against the same daily allowance as builds, per address, which
         starts again at midnight UTC. Demos cannot be changed. Errors that come from the agent carry `status` and `message`
         only.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
[Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class LambdaAgentResource(IBuildService builds, IMetaService meta, IFeatureService features, ILogger<LambdaAgentResource> logger)
{

    [ResourceMethod("lambdas/:privateKey/agent")]
    [Summary("Reads how a change is getting on")]
    [Remarks("""
             Asked again while a change runs. The agent remembers a change for an hour after it was asked for; after that there
             is nothing to report, and what it saved is in the history of the lambda and its features.
             """)]
    [return: Summary("Whether there is an agent, how many changes this caller has left today, and the change under way or the last one")]
    public async ValueTask<AgentState> Get([Summary(ApiDocumentation.PrivateKey)] string privateKey, IRequest request)
    {
        var lambda = await meta.RequireEditableAsync(privateKey);

        return await builds.StateAsync(lambda, request.Client.Address);
    }

    /// <remarks>
    /// One change of a lambda at a time, since two agents writing the same
    /// files would each undo the other. The feature is looked up here, so one
    /// that does not exist is refused before anything is spent on it.
    /// </remarks>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/agent/start")]
    [Summary("Asks the agent to change a lambda")]
    [Remarks("""
             The change is queued and runs in the background; `GET …/agent` follows it. The prompt is logged in full, so the
             operator knows what people ask for, and cut past 2000 characters. A question about the application is answered
             rather than made a change of (`result.answered`), and a setting the agent may change - analytics, a kind of data,
             an earlier version back online, a feature merged or deleted, the source published - is changed without a
             feature (`result.settings`). A request that is not about the application is declined, and the result says why.

             `history` is the conversation so far, as the page that holds it sent it: the newest turns that fit, each cut, are
             handed to the agent as context and are not logged. The agent keeps nothing between messages.
             """)]
    [Response(ResponseStatus.Accepted, "The change was taken on.")]
    [Response(ResponseStatus.BadRequest, "The prompt says too little, or the model or the effort is not one offered here.")]
    [Response(ResponseStatus.Forbidden, "The model asked for needs the installation's password, and this is not it - or the lambda is a demo.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "A change of this lambda is under way already. Wait for it, or stop it first.")]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, it has no such feature to go on with, or this installation takes no changes from here.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.TooManyRequests, "This address asked for as many builds and changes today as one may.")]
    [Response(ResponseStatus.ServiceUnavailable, "The agent is not answering. Nothing was counted.")]
    public async ValueTask<Result<AgentState>> Start([Summary(ApiDocumentation.PrivateKey)] string privateKey, ChangeRequest body, IRequest request)
    {
        var lambda = await meta.RequireEditableAsync(privateKey);

        var current = await meta.RequireAsync(privateKey);

        var feature = body?.Feature is { Length: > 0 } key ? (await features.GetAsync(privateKey, key)).Feature.Key : null;

        var state = await builds.ChangeAsync(lambda, privateKey, current.ActiveVersion, body?.Prompt, body?.Deploy ?? true,
                                             body?.Model, body?.Effort, body?.Password, body?.Language, feature, body?.History,
                                             request.Client.Address);

        // logged in full, as the prompts of the build page are - the message
        // only: what was said before was logged when it was asked
        logger.LogInformation("Started change {Change} of lambda {Lambda} model {Model} effort {Effort} prompt {Prompt}",
                              state.Job?.Id, current.PublicKey, state.Job?.Model, state.Job?.Effort, body?.Prompt?.Trim());

        return new Result<AgentState>(state).Status(ResponseStatus.Accepted);
    }

    [ResourceMethod(Method.Post, "lambdas/:privateKey/agent/stop")]
    [Summary("Stops the change under way")]
    [Remarks("What it saved so far stays saved - in its feature, or as a version - and whatever is online stays online. A change that is stopped is not counted back.")]
    [return: Summary("The state afterwards")]
    public async ValueTask<AgentState> Stop([Summary(ApiDocumentation.PrivateKey)] string privateKey, IRequest request)
    {
        var lambda = await meta.RequireEditableAsync(privateKey);

        var state = await builds.StopAsync(lambda, request.Client.Address);

        logger.LogInformation("Stopped change of lambda {Lambda}", await meta.PublicKeyOfAsync(privateKey));

        return state;
    }

}
