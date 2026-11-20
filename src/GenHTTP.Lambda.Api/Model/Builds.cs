using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Building;

namespace GenHTTP.Lambda.Api.Model;

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
[Summary("What should be built.")]
public sealed record BuildRequest(
    [Summary("What to build, in the words of whoever wants it, in any language - at least three characters, cut past 2000")]
    string? Prompt,
    [Summary("Which of the offered models builds it, by its name in `build.offer.models` of `GET /system`; left out, the default one (`build.offer.model`)")]
    string? Model = null,
    [Summary("The installation's password, for a model that needs it (`password` of the model)")]
    string? Password = null,
    [Summary("Which of the offered efforts it runs at, as `build.offer.efforts` names them; left out, the default one (`build.offer.effort`)")]
    string? Effort = null
);

/// <param name="Offer">
/// Which models need the password is said, but never the password, nor
/// which identifier a model runs as: the page needs to know what to offer,
/// not what the runner does with it.
/// </param>
[Summary("Whether this installation builds things, how often it lets one caller, and on which models.")]
public sealed record BuildAvailability(
    [Summary("Whether builds can be asked for: there is an agent, and the operator has not switched building off")]
    bool Available,
    [Summary("How many builds and changes one address may ask for in a day")]
    int PerDay,
    [Summary("The models and efforts a build or a change may be asked for, as the operator offers them")]
    AgentOffer Offer
);

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
