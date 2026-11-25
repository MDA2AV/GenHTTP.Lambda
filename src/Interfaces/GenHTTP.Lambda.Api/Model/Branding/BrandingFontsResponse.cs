using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.SharedFiles;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The fonts the pages are set in, each the shared files it is made of; none where they are set in the system's.")]
public sealed record BrandingFontsResponse(
    [Summary("The files of the font of the pages")]
    IReadOnlyList<SharedFileResponse> Text,
    [Summary("The files of the font of their headings; none for the same as the text")]
    IReadOnlyList<SharedFileResponse> Headings,
    [Summary("The languages of the pages, by their codes, the font has too few letters for: what it lacks is drawn in the system's, in the middle of a word. Only of those in Latin letters")]
    IReadOnlyList<string> TextLacks,
    [Summary("The same of the font of the headings")]
    IReadOnlyList<string> HeadingLacks
)
{

    internal static BrandingFontsResponse Of(BrandLook look)
        => new([.. look.TextFont.Select(SharedFileResponse.Of)], [.. look.HeadingFont.Select(SharedFileResponse.Of)],
               FontLetters.Lacking(look.TextFont), FontLetters.Lacking(look.HeadingFont));

}
