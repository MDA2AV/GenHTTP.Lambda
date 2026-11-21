using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("That the operator blocked a lambda, and why.")]
public sealed record LambdaBlockResponse(
    [Summary("Since when")]
    DateTime Since,
    [Summary("Why, in the operator's words; never shown to the owner")]
    string? Reason
);
