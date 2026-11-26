using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Services.Agents;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// What the browser sends: what should be built, and nothing that names
/// anything already made.
/// </summary>
/// <remarks>
/// There is deliberately no way to name an existing lambda here. This endpoint
/// creates, and only creates; changing something that exists belongs to the
/// editor, which already holds its key, or to an agent over MCP. Accepting a
/// key here made one door do two jobs and blurred what the box is for.
///
/// <c>model</c> and <c>effort</c> are the options of those names, as pages
/// sent them before the options were the agent integration's.
/// </remarks>
[Summary("What should be built.")]
public sealed record BuildRequest(
    [Summary("What to build, in the words of whoever wants it, in any language - at least three characters, cut past 2000; may be left out with `continues`")]
    string? Prompt,
    [Summary("The value of the option `model` - the same as `options.model`")]
    string? Model = null,
    [Summary("The installation's password, for a value that needs it (`password` of the value)")]
    string? Password = null,
    [Summary("The value of the option `effort` - the same as `options.effort`")]
    string? Effort = null,
    [Summary("A value for each option of the agent's to run on, by option, as `build.offer.options` of `GET /system` names them; an option left out takes its default")]
    IReadOnlyDictionary<string, string>? Options = null,
    [Summary("The files the request comes with, by the `id` uploading each answered (`POST /attachments`)")]
    IReadOnlyList<string>? Attachments = null,
    [Summary("The language of whoever asks, as a code like `de` or `pt-pt`, for the agent to answer in where the prompt does not tell")]
    string? Language = null,
    [Summary("The `id` of an earlier build this one carries on from: the one whose question the prompt answers, or one to try again")]
    string? Continues = null,
    [Summary("The conversation so far, oldest first, as the page keeps it - for a build that carries on from one the platform has forgotten")]
    IReadOnlyList<ConversationTurn>? History = null
)
{

    internal AgentAsk Ask() => new(Prompt, AgentRequests.OptionsOf(Options, Model, Effort), Password, Language, Attachments, History, "Simple", Continues);

}
