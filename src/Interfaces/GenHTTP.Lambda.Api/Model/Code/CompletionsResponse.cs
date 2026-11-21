using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Everything that could be written at a position.")]
public sealed record CompletionsResponse(
    [Summary("The suggestions")]
    IReadOnlyList<ResolvedCompletionResponse> Completions
);
