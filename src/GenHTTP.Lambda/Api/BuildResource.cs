using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Building;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

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
public sealed class BuildResource(BuildService builds)
{

    /// <summary>
    /// Asks for something to be built.
    /// </summary>
    /// <remarks>
    /// Whether this installation builds anything at all is part of
    /// <c>/system</c>, so a page can decide whether to offer the box rather
    /// than offering one that answers with an error.
    /// </remarks>
    [ResourceMethod(Method.Post)]
    public async ValueTask<Result<BuildStarted>> Create(BuildRequest body, IRequest request)
    {
        var started = await builds.StartAsync(body?.Prompt, body?.Model, body?.Password, request.Client.Address);

        return new Result<BuildStarted>(started).Status(ResponseStatus.Accepted);
    }

    /// <summary>
    /// How a build is getting on, polled while it runs.
    /// </summary>
    [ResourceMethod(":id")]
    public async ValueTask<BuildProgress> Get(string id) => await builds.ProgressAsync(id);

}
