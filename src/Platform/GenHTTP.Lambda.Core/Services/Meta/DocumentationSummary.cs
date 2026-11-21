using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// What a version says about itself: the pages of its code kept in
/// <c>docs/</c> and <c>tests/</c>.
/// </summary>
[Summary("What a version says about itself, in `docs/` and `tests/`.")]
public sealed record DocumentationSummary(
    [Summary("The first paragraph of `docs/product.md`, as plain text - what the app is, in a sentence or two")]
    string? About,
    [Summary("Whether it has `docs/product.md`: what the app is, for whom, and why")]
    bool Product,
    [Summary("Whether it has `docs/decisions.md`: which technical decisions were made, and why")]
    bool Decisions,
    [Summary("Whether it has `tests/README.md`: how it is tested")]
    bool Tests,
    [Summary("How many files its documentation and tests come to")]
    int Files,
    [Summary("What those weigh, which counts towards what a version may come to like the rest of its code")]
    long Bytes);
