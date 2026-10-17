using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Building;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The text box on the front page: a sentence in, a new application out.
/// </summary>
/// <remarks>
/// Public and unauthenticated on purpose - the point of it is that somebody
/// who has never written a line of C# can type what they want and get a link.
/// What keeps that from being reckless is not on this side: the agent it
/// hands the sentence to runs in a container with no route off the machine
/// and no tools except this server's own MCP, so the worst a prompt can talk
/// it into is creating a lambda, which is what it is for.
///
/// What is on this side is the counting: a few builds per address per day,
/// because each one spends somebody's subscription.
/// </remarks>
[Tag(ApiDocumentation.Builds)]
[Summary("The build agent of the installation: a sentence in, a new lambda out - for anybody, without a key.")]
[Remarks("""
         What `/build` on this site does. Somebody says what they would like; the agent of the installation creates a lambda
         for it, writes it, and puts it online, and the build's result names the address and the editor key. It only builds
         applications: a request that is not one, or is about the platform rather than an app on it, or is meant to do harm,
         is declined with a sentence saying why.

         Whether this installation builds anything at all is in `GET /system` (`build`), with how many builds one address
         may ask for a day. Each build is counted against that allowance, which starts again at midnight UTC and is shared
         with the changes asked for in the editor. An agent may also be reached directly through MCP at `/mcp`.

         Errors here carry `status` and `message` only.
         """)]
public sealed class BuildResource(IBuildService builds, ILogger<BuildResource> logger)
{

    /// <remarks>
    /// Whether this installation builds anything at all is part of
    /// <c>/system</c>, so a page can decide whether to offer the box rather
    /// than offering one that answers with an error.
    /// </remarks>
    [ResourceMethod(Method.Post)]
    [Summary("Asks the build agent to build something")]
    [Remarks("""
             The build is queued and runs in the background; `GET /builds/{id}` follows it. The prompt is logged in full, so the
             operator knows what people ask for - put nothing into it that should not be read. It is cut past 2000 characters.
             """)]
    [Response(ResponseStatus.Accepted, "The build was taken on. Follow it by its `id`.")]
    [Response(ResponseStatus.BadRequest, "The prompt says too little, or there is no such model.")]
    [Response(ResponseStatus.Forbidden, "The password for the second model is not right.")]
    [Response(ResponseStatus.NotFound, "This installation builds nothing: it has no agent, or the operator switched building off.")]
    [Response(ResponseStatus.TooManyRequests, "This address asked for as many builds today as one may.")]
    [Response(ResponseStatus.ServiceUnavailable, "The agent is not answering. Nothing was counted.")]
    public async ValueTask<Result<BuildStarted>> Create(BuildRequest body, IRequest request)
    {
        var started = await builds.StartAsync(body?.Prompt, body?.Model, body?.Password, request.Client.Address);

        // the one argument logged in full however long it is: what people ask
        // to have built is what the operator most needs to know
        logger.LogInformation("Started build {Build} model {Model} prompt {Prompt}", started.Id, body?.Model ?? "default", body?.Prompt?.Trim());

        return new Result<BuildStarted>(started).Status(ResponseStatus.Accepted);
    }

    [ResourceMethod(":id")]
    [Summary("Reads how a build is getting on")]
    [Remarks("""
             Asked again while it runs: its place in the queue, the steps the agent took so far, and once it is done how it
             ended. The result of a build that worked holds the editor key of the new lambda, so whoever has the id of a build
             can take the lambda it made - keep it as private as the key. The agent forgets a build some time after it ended.
             """)]
    [Response(ResponseStatus.BadRequest, "That is not the id of a build.")]
    [Response(ResponseStatus.NotFound, "The agent knows no build of this id, any more - or this installation has no agent.")]
    [return: Summary("Its state, its steps, and how it ended once it has")]
    public async ValueTask<BuildProgress> Get([Summary("The `id` of the build, as starting it returned")] string id) => await builds.ProgressAsync(id);

}
