using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What every name in a file means.")]
public sealed record SemanticsResponse(
    [Summary("The names, in the order they appear")]
    IReadOnlyList<SemanticToken> Tokens
);
