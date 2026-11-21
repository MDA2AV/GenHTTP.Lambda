using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What every name in a file means.")]
public sealed record SemanticsResponse(
    [Summary("The names, in the order they appear")]
    IReadOnlyList<SemanticToken> Tokens
);
