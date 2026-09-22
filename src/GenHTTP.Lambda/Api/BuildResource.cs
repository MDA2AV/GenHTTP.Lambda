using System.Text.Json.Nodes;

using GenHTTP.Api.Protocol;

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
    /// Whether there is an agent here at all, so a page can decide whether to
    /// offer the box rather than offering one that answers with an error.
    /// </summary>
    [ResourceMethod]
    public JsonObject Get() => new()
    {
        ["available"] = builds.Available,
        ["perDay"] = builds.PerDay,
        // said, but never what it is: the page needs to know whether to offer
        // the choice, not what the answer is
        ["secondModel"] = builds.HasSecondModel
    };

    /// <summary>
    /// Asks for something to be built.
    /// </summary>
    [ResourceMethod(Method.Post)]
    public async ValueTask<JsonObject> Start(BuildRequest body, IRequest request)
        => await builds.StartAsync(body?.Prompt, body?.Model, body?.Password, request.Client.Address);

    /// <summary>
    /// How a build is getting on, polled while it runs.
    /// </summary>
    [ResourceMethod(":id")]
    public async ValueTask<JsonObject> Progress(string id) => await builds.ProgressAsync(id);

}

/// <summary>
/// What the browser sends: one sentence, and nothing else that identifies
/// anything already made.
/// </summary>
/// <remarks>
/// There is deliberately no way to name an existing lambda here. This endpoint
/// creates, and only creates; changing something that exists belongs to the
/// editor, which already holds its key, or to an agent over MCP. Accepting a
/// key here made one door do two jobs and blurred what the box is for.
/// </remarks>
public sealed record BuildRequest(string? Prompt, string? Model = null, string? Password = null);
