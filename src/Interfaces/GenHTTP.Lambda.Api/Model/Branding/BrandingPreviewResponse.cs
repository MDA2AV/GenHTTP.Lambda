using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What the pages would be drawn with.")]
public sealed record BrandingPreviewResponse(
    [Summary("The colours, and what they would come to")]
    BrandingColorsResponse Colors,
    [Summary("The fonts")]
    BrandingFontsResponse Fonts,
    [Summary("The style sheet the pages would be sent with; absent where they would be drawn as the platform's")]
    string? StyleSheet
)
{

    internal static BrandingPreviewResponse Of(BrandLook look)
        => new(BrandingColorsResponse.Of(look), BrandingFontsResponse.Of(look), look.StyleSheet);

}
