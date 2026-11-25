using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The colours of the pages. The two are set together: one that is empty or left out is given back to the platform's.")]
public sealed record BrandingColorsRequest(
    [Summary("What links, buttons and marks are drawn in, as `#rrggbb`")]
    string? Accent = null,
    [Summary("What is drawn in the platform's purple otherwise, and the second light of the moving background, as `#rrggbb`")]
    string? Second = null
);
