using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Api.Model;

[Summary("How the operator had the installation present itself: the name and the logo its pages show in place of GenHTTP Lambda's, and the colours and fonts they are drawn with.")]
public sealed record BrandingResponse(
    [Summary("What the operator named it; absent where it goes by the platform's own name")]
    string? Name,
    [Summary("The name its pages show: this, or `GenHTTP Lambda`")]
    string Shown,
    [Summary("The logo the operator uploaded; absent where there is none")]
    BrandingLogoResponse? Logo,
    [Summary("Where the pages load the picture beside the name from, which is also the icon of the browser tab: the logo, or the first letter of the name on a plate; absent while the installation goes by the platform's own name and mark")]
    string? Picture,
    [Summary("The colours its pages are drawn in")]
    BrandingColorsResponse Colors,
    [Summary("The fonts they are set in")]
    BrandingFontsResponse Fonts,
    [Summary("The style sheet the pages are sent with to be drawn this way; absent while neither colour nor font is the operator's")]
    string? StyleSheet,
    [Summary("What a name, a logo and a font may be")]
    BrandingLimitsResponse Limits
)
{

    internal static BrandingResponse Of(Brand brand)
        => new(brand.Given,
               brand.Name,
               brand.Logo is { } logo ? new BrandingLogoResponse(logo.Type, logo.Content.Length, logo.Updated) : null,
               brand.PicturePath,
               BrandingColorsResponse.Of(brand.Look),
               BrandingFontsResponse.Of(brand.Look),
               brand.Look.StyleSheet,
               BrandingLimitsResponse.Fixed);

}
