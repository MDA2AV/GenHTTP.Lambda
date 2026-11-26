using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Agents;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The text box on /build: a sentence in, a new application out.
/// </summary>
/// <remarks>
/// Public and unauthenticated on purpose - the point of it is that somebody
/// who has never written a line of C# can type what they want and get a link.
/// What keeps that sane is the counting here - a few builds per address per
/// day, because each spends what the installation's agent runs on - the
/// options its operator offers, the dearer ones behind a password where they
/// put them there, and what the agent declines.
/// </remarks>
[Tag(ApiDocumentation.Builds)]
[Summary("The agent of the installation: a sentence in, a new lambda out - for anybody, without a key.")]
[Remarks("""
         What `/build` on this site does. Somebody says what they would like; the agent of the installation creates a lambda
         for it, writes it, and puts it online, and the build's result names the address and the editor key - read from what the
         platform recorded of the build's calls, not from what the agent said. It only builds applications: a request that is not
         one, or is about the platform rather than an app on it, or is meant to do harm, is declined with a sentence saying why. It
         may end with a question instead (`result.asked`), which a new build carrying on from this one answers (`continues`).

         Whether this installation builds anything at all is in `GET /system` (`build`), with how many builds one address may ask
         for a day, the options its operator offers (`build.offer.options`) - a value may need the installation's password - and
         the files a request may come with (`build.offer.attachments`), uploaded first with `POST /attachments`. Each build is
         counted against that allowance, which starts again at midnight UTC and is shared with the messages to the agent in the
         editor. An agent of one's own reaches the platform directly through MCP at `/mcp`.

         Errors here carry `status` and `message` only.
         """)]
public sealed class BuildResource(IAgentService agents, ILogger<BuildResource> logger)
{

    /// <remarks>
    /// Whether this installation builds anything at all is part of
    /// <c>/system</c>, so a page can decide whether to offer the box rather
    /// than offering one that answers with an error.
    /// </remarks>
    [ResourceMethod(Method.Post)]
    [Summary("Asks the agent to build something")]
    [Remarks("""
             The build is queued and runs in the background; `GET /builds/{id}` reads how it is getting on, and
             `GET /builds/{id}/events` pushes it each time it changes. The prompt is logged in full, so the operator knows what
             people ask for - put nothing into it that should not be read - and so are the names, types and sizes of the files it
             comes with, which the operator may open. It is cut past 2000 characters. With `continues`, the prompt may be left out:
             the build carries on with what the earlier one was asked.
             """)]
    [Response(ResponseStatus.Accepted, "The build was taken on. Follow it by its `id`.")]
    [Response(ResponseStatus.BadRequest, "The prompt says too little, an option or a value is not one offered here, a file is not there any more, or the build to carry on from is not.")]
    [Response(ResponseStatus.Forbidden, "A value asked for needs the installation's password, and this is not it.")]
    [Response(ResponseStatus.NotFound, "This installation builds nothing: it has no agent, or the operator switched building off.")]
    [Response(ResponseStatus.TooManyRequests, "This address asked for as many builds today as one may.")]
    [Response(ResponseStatus.ServiceUnavailable, "The agent is not answering, or too busy to take another. Nothing was counted.")]
    public async ValueTask<Result<BuildStarted>> Create(BuildRequest body, IRequest request)
    {
        var started = await agents.StartAsync(body.Ask(), request.Client.Address);

        // the one argument logged in full however long it is: what people ask
        // to have built is what the operator most needs to know
        logger.LogInformation("Started build {Build} options {Options} attachments {Attachments} prompt {Prompt}",
                              started.Id, OptionsOf(started.Options), body?.Attachments?.Count ?? 0, body?.Prompt?.Trim());

        return new Result<BuildStarted>(started).Status(ResponseStatus.Accepted);
    }

    [ResourceMethod(":id")]
    [Summary("Reads how a build is getting on")]
    [Remarks("""
             Its place in the queue, what it waits for, the steps it took so far, and once it is done how it ended. The result of a
             build that worked holds the editor key of the new lambda, so whoever has the id of a build can take the lambda it
             made - keep it as private as the key. The platform keeps a build for a while after it ended.
             """)]
    [Response(ResponseStatus.BadRequest, "That is not the id of a build.")]
    [Response(ResponseStatus.NotFound, "There is no build of this id, any more - or this installation has no agent.")]
    [return: Summary("Its state, its steps, and how it ended once it has")]
    public async ValueTask<BuildProgress> Get([Summary("The `id` of the build, as starting it returned")] string id) => await agents.ProgressAsync(id);

    [ResourceMethod(":id/events")]
    [Summary("Follows a build as it goes on")]
    [Remarks("""
             A stream of server-sent events: a `progress` event with what `GET /builds/{id}` answers, sent at once and again each
             time the build changes, until it ended. A browser's `EventSource` reconnects by itself.
             """)]
    [Response(ResponseStatus.Ok, "The events", ContentType = "text/event-stream")]
    [Response(ResponseStatus.BadRequest, "That is not the id of a build.")]
    [Response(ResponseStatus.NotFound, "There is no build of this id, any more - or this installation has no agent.")]
    public async ValueTask<IHandlerBuilder> Events([Summary("The `id` of the build, as starting it returned")] string id)
    {
        // refused as a read would be, before the stream starts
        await agents.ProgressAsync(id);

        return PushedStream.Of("progress", cancellation => agents.FollowBuildAsync(id, cancellation));
    }

    internal static string OptionsOf(IReadOnlyDictionary<string, string>? options)
        => options == null ? "(none)" : string.Join(',', options.Select(o => $"{o.Key}={o.Value}"));

}
