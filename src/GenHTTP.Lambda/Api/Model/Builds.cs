using GenHTTP.Api.Content.Services;

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
    [Summary("Which of the offered models builds it: left out or `opus` for the default one, `fable` for the second, where one is offered")]
    string? Model = null,
    [Summary("The password for the second model")]
    string? Password = null
);

/// <param name="SecondModel">
/// Said, but never which one: the page needs to know whether to offer the
/// choice, not what the answer is.
/// </param>
[Summary("Whether this installation builds things, and how often it lets one caller.")]
public sealed record BuildAvailability(
    [Summary("Whether builds can be asked for: there is an agent, and the operator has not switched building off")]
    bool Available,
    [Summary("How many builds and changes one address may ask for in a day")]
    int PerDay,
    [Summary("Whether a second model is on offer, behind a password")]
    bool SecondModel
);

[Summary("What should be different about a lambda.")]
public sealed record ChangeRequest(
    [Summary("What to change, in the words of whoever wants it, in any language - at least three characters, cut past 2000")]
    string? Prompt,
    [Summary("Whether to merge the change and put it online once it works (the default), rather than leave it in its feature for the owner to try first")]
    bool Deploy = true,
    [Summary("Which of the offered models makes it: left out or `opus` for the default one, `fable` for the second, where one is offered")]
    string? Model = null,
    [Summary("The password for the second model")]
    string? Password = null,
    [Summary("The language of whoever asks, as a code like `de` or `pt-pt`, for the agent to answer in where the prompt does not tell")]
    string? Language = null,
    [Summary("The key of a feature to go on with; left out, the agent starts a new one")]
    string? Feature = null
);
