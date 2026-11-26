namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// What a page sends with a message to the agent, before it is checked.
/// </summary>
/// <param name="Prompt">What was asked, in the person's words</param>
/// <param name="Options">A value for each option asked for, by option name; one left out takes its default</param>
/// <param name="Password">The installation's password, where a value behind it was asked for</param>
/// <param name="Language">The language of the page, as a code</param>
/// <param name="Attachments">The ids of the files the message comes with, uploaded before</param>
/// <param name="History">What was said before, as the page keeps it - taken where the platform keeps no earlier run of the app</param>
/// <param name="View">The words the person reads: <c>simple</c> or <c>full</c>; left out, what the app's editor opens in - and simple for a build</param>
/// <param name="Continues">The run this one carries on from: the one whose question it answers, or whose wait is over</param>
/// <param name="Deploy">For a change: whether to put it online once it works</param>
/// <param name="Feature">For a change: the feature to go on with, by its key</param>
public sealed record AgentAsk(string? Prompt, IReadOnlyDictionary<string, string?> Options, string? Password = null, string? Language = null,
                              IReadOnlyList<string>? Attachments = null, IReadOnlyList<ConversationTurn>? History = null, string? View = null,
                              string? Continues = null, bool Deploy = true, string? Feature = null);
