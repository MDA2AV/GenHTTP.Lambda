using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Something that could be written at a position.")]
public sealed record ResolvedCompletionResponse(
    [Summary("What to write")]
    string Label,
    [Summary("What it is: `class`, `interface`, `struct`, `enum`, `method`, `property`, `field`, `parameter`, `variable` or `text`")]
    string Kind,
    [Summary("Its signature, as C# writes it")]
    string Detail,
    [Summary("What its documentation says about it, where it has any")]
    string? Documentation
);
