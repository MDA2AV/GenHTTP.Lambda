using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Agents;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// Behind the editor key like everything else about a lambda, and refused for
/// a demo like everything else that would change one. The agent is the same
/// one the build page uses, on the same allowance; what differs is that it is
/// handed the key and the app it works on.
///
/// A process rather than a thing, so it is read at <c>agent</c> and driven
/// with verbs, the way a deployment is.
/// </remarks>
[Tag(ApiDocumentation.Changes)]
[Summary("The agent of the installation, changing a lambda - and answering questions about it - for whoever holds its editor key.")]
[Remarks("""
         What the workspace on `/build` and the Change section of the editor do: the owner writes what they would like to know
         or have different, and the agent answers it, changes a setting, or changes the lambda in a feature - a new one, or one
         named - and tries it at its preview. With `deploy` (the default) it merges the feature and puts the new version online
         once it works; without, it leaves the feature for the owner to try and merge. What is online stays online until then.

         What a message came to - what it saved, put online, switched on, what the app waits for afterwards - is read from what
         the platform recorded of its calls, never from what the agent said. It may end with a question (`result.asked`), which the
         next message answers (`continues`). The version that was online before (`result.before`) is put back online with
         `POST …/deployment/start`.

         One message of a lambda runs at a time. Messages count against the same daily allowance as builds, per address, which
         starts again at midnight UTC. Demos cannot be changed. Errors that come from the agent carry `status` and `message`
         only.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
[Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class LambdaAgentResource(IAgentService agents, IMetaService meta, IFeatureService features, ILogger<LambdaAgentResource> logger)
{

    [ResourceMethod("lambdas/:privateKey/agent")]
    [Summary("Reads how a message is getting on")]
    [Remarks("""
             The message under way, or the last one, while the platform keeps it - a while after it ended; after that, what it
             saved is in the history of the lambda and its features. `GET …/agent/events` pushes the same each time it changes.
             """)]
    [return: Summary("Whether there is an agent, how many messages this caller has left today, and the one under way or the last one")]
    public async ValueTask<AgentState> Get([Summary(ApiDocumentation.PrivateKey)] string privateKey, IRequest request)
    {
        // reading stays where changing does not - a blocked lambda among them
        var lambda = await ReadableAsync(privateKey);

        return await agents.StateAsync(lambda, request.Client.Address);
    }

    [ResourceMethod("lambdas/:privateKey/agent/events")]
    [Summary("Follows the messages about a lambda as they go on")]
    [Remarks("""
             A stream of server-sent events: a `state` event with what `GET …/agent` answers, sent at once and again each time it
             changes - a message started here or anywhere else, a step it took, how it ended - for as long as the page listens. A
             browser's `EventSource` reconnects by itself.
             """)]
    [Response(ResponseStatus.Ok, "The events", ContentType = "text/event-stream")]
    public async ValueTask<IHandlerBuilder> Events([Summary(ApiDocumentation.PrivateKey)] string privateKey, IRequest request)
    {
        var lambda = await ReadableAsync(privateKey);

        var caller = request.Client.Address;

        return PushedStream.Of("state", cancellation => agents.FollowAsync(lambda, caller, cancellation));
    }

    /// <remarks>
    /// One message of a lambda at a time, since two agents writing the same
    /// files would each undo the other. The feature is looked up here, so one
    /// that does not exist is refused before anything is spent on it.
    /// </remarks>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/agent/start")]
    [Summary("Writes to the agent about a lambda")]
    [Remarks("""
             The message is queued and runs in the background; `GET …/agent` and `GET …/agent/events` follow it. The prompt is
             logged in full, so the operator knows what people ask for, and cut past 2000 characters; the files it comes with are
             named in the log as they are uploaded, and may be opened by the operator. A question about the application is answered
             rather than made a change of (`result.answered`), and a setting the agent may change - analytics, a kind of data, an
             earlier version back online, a feature merged or deleted, the source published - is changed without a feature
             (`result.settings`). A request that is not about the application is declined, and the result says why.

             The agent is handed the newest earlier messages about the app and their answers as the platform keeps them, as context;
             `history`, the conversation as the page keeps it, is handed on only where the platform keeps none. Neither is logged.
             With `continues` the prompt may be left out: the message carries on with what the earlier one was asked.
             """)]
    [Response(ResponseStatus.Accepted, "The message was taken on.")]
    [Response(ResponseStatus.BadRequest, "The prompt says too little, an option or a value is not one offered here, a file is not there any more, or the message to carry on from is not.")]
    [Response(ResponseStatus.Forbidden, "A value asked for needs the installation's password, and this is not it - or the lambda is a demo.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "A message about this lambda is under way already. Wait for it, or stop it first.")]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, it has no such feature to go on with, or this installation takes no messages from here.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.TooManyRequests, "This address asked for as many builds and messages today as one may.")]
    [Response(ResponseStatus.ServiceUnavailable, "The agent is not answering, or too busy to take another. Nothing was counted.")]
    public async ValueTask<Result<AgentState>> Start([Summary(ApiDocumentation.PrivateKey)] string privateKey, ChangeRequest body, IRequest request)
    {
        var lambda = await meta.RequireEditableAsync(privateKey);

        var current = await meta.RequireAsync(privateKey);

        var feature = body?.Feature is { Length: > 0 } key ? (await features.GetAsync(privateKey, key)).Feature.Key : null;

        var state = await agents.ChangeAsync(lambda, privateKey, current.ActiveVersion, (body ?? new ChangeRequest(null)).Ask(feature), request.Client.Address);

        // logged in full, as the prompts of the build page are - the message
        // only: what was said before was logged when it was asked
        logger.LogInformation("Started change {Change} of lambda {Lambda} options {Options} attachments {Attachments} prompt {Prompt}",
                              state.Job?.Id, current.PublicKey, BuildResource.OptionsOf(state.Job?.Options), body?.Attachments?.Count ?? 0, body?.Prompt?.Trim());

        return new Result<AgentState>(state).Status(ResponseStatus.Accepted);
    }

    [ResourceMethod(Method.Post, "lambdas/:privateKey/agent/stop")]
    [Summary("Stops the message under way")]
    [Remarks("What it saved so far stays saved - in its feature, or as a version - and whatever is online stays online. A message that is stopped is not counted back.")]
    [Response(ResponseStatus.Conflict, "The agent of this installation cannot be stopped (`offer.cancels` is false); the run ends by itself.")]
    [return: Summary("The state afterwards")]
    public async ValueTask<AgentState> Stop([Summary(ApiDocumentation.PrivateKey)] string privateKey, IRequest request)
    {
        var lambda = await meta.RequireEditableAsync(privateKey);

        // the service logs what it stopped, where there was something
        return await agents.StopAsync(lambda, request.Client.Address);
    }

    [ResourceMethod("lambdas/:privateKey/agent/files")]
    [Summary("Lists the files attached to messages about a lambda")]
    [Remarks("The newest first, while the platform keeps the messages they came with. `GET /attachments/{id}` answers with one.")]
    [return: Summary("The files")]
    public async ValueTask<IReadOnlyList<RunFile>> Files([Summary(ApiDocumentation.PrivateKey)] string privateKey)
        => await agents.AttachedToAsync(await ReadableAsync(privateKey));

    private ValueTask<long> ReadableAsync(string privateKey) => meta.RequireOwnAsync(privateKey);

}
