using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Building;

/// <remarks>
/// Kept by the page that holds the conversation - the workspace on /build -
/// and sent along with the next message, so the agent understands "and last
/// month?". The agent keeps nothing between runs.
/// </remarks>
[Summary("One earlier turn of a conversation with the agent about a lambda.")]
public sealed record ConversationTurn(
    [Summary("What was asked, as it was asked")]
    string? Prompt,
    [Summary("What the agent answered, where it did")]
    string? Reply = null
);
