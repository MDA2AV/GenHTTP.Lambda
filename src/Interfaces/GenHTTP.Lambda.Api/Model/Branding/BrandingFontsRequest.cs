using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The fonts of the pages, each the names of the shared files it is made of. The two are set together: one that is empty or left out is given back to the system's.")]
public sealed record BrandingFontsRequest(
    [Summary("The font of the pages: one variable font, or a file for each weight and slant - at most 12")]
    IReadOnlyList<string>? Text = null,
    [Summary("The font of their headings, and of the installation's name; none for the same as the text")]
    IReadOnlyList<string>? Headings = null
);
