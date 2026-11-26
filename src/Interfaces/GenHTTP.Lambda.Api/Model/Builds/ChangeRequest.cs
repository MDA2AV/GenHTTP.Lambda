using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Services.Agents;

namespace GenHTTP.Lambda.Api.Model;

/// <remarks>
/// <c>model</c> and <c>effort</c> are the options of those names, as pages
/// sent them before the options were the agent integration's.
/// </remarks>
[Summary("What the owner writes to the agent about a lambda: a question, a setting to change, or what should be different.")]
public sealed record ChangeRequest(
    [Summary("What to do, in the words of whoever wants it, in any language - at least three characters, cut past 2000; may be left out with `continues`")]
    string? Prompt,
    [Summary("Whether to merge the change and put it online once it works (the default), rather than leave it in its feature for the owner to try first")]
    bool Deploy = true,
    [Summary("The value of the option `model` - the same as `options.model`")]
    string? Model = null,
    [Summary("The installation's password, for a value that needs it (`password` of the value)")]
    string? Password = null,
    [Summary("The language of whoever asks, as a code like `de` or `pt-pt`, for the agent to answer in where the prompt does not tell")]
    string? Language = null,
    [Summary("The key of a feature to go on with; left out, the agent starts a new one where it changes the app")]
    string? Feature = null,
    [Summary("The conversation so far, oldest first, as the page keeps it - taken where the platform keeps no earlier message about the app")]
    IReadOnlyList<ConversationTurn>? History = null,
    [Summary("The value of the option `effort` - the same as `options.effort`")]
    string? Effort = null,
    [Summary("A value for each option of the agent's to run on, by option, as `offer.options` of `GET …/agent` names them; an option left out takes its default")]
    IReadOnlyDictionary<string, string>? Options = null,
    [Summary("The files the message comes with, by the `id` uploading each answered (`POST /attachments`)")]
    IReadOnlyList<string>? Attachments = null,
    [Summary("The words the owner reads: `simple` (no developer words - the workspace and the simple view) or `full`; left out, the view the app's editor opens in")]
    string? View = null,
    [Summary("The `id` of an earlier message about the app this one carries on from: the one whose question the prompt answers, the one whose wait is over, or one to try again")]
    string? Continues = null
)
{

    internal AgentAsk Ask(string? feature) => new(Prompt, AgentRequests.OptionsOf(Options, Model, Effort), Password, Language, Attachments, History, View, Continues,
                                                  Deploy, feature);

}
