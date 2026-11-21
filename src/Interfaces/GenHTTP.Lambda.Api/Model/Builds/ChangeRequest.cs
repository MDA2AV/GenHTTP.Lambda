using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Building;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What should be different about a lambda.")]
public sealed record ChangeRequest(
    [Summary("What to change, in the words of whoever wants it, in any language - at least three characters, cut past 2000")]
    string? Prompt,
    [Summary("Whether to merge the change and put it online once it works (the default), rather than leave it in its feature for the owner to try first")]
    bool Deploy = true,
    [Summary("Which of the offered models makes it, by its name in `offer.models` of `GET …/agent`; left out, the default one")]
    string? Model = null,
    [Summary("The installation's password, for a model that needs it (`password` of the model)")]
    string? Password = null,
    [Summary("The language of whoever asks, as a code like `de` or `pt-pt`, for the agent to answer in where the prompt does not tell")]
    string? Language = null,
    [Summary("The key of a feature to go on with; left out, the agent starts a new one")]
    string? Feature = null,
    [Summary("The conversation so far, oldest first, as the page keeps it - the agent is handed the newest turns that fit, each cut, to understand the message by")]
    IReadOnlyList<ConversationTurn>? History = null,
    [Summary("Which of the offered efforts it runs at, as `offer.efforts` of `GET …/agent` names them; left out, the default one")]
    string? Effort = null
);
