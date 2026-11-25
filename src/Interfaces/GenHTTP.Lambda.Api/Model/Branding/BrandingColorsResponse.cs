using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The colours the pages are drawn in, and what they come to in each theme.")]
public sealed record BrandingColorsResponse(
    [Summary("What links, buttons and marks are drawn in, as `#rrggbb`; absent for the platform's blue")]
    string? Accent,
    [Summary("What is drawn in the platform's purple otherwise, and the second light of the moving background, as `#rrggbb`; absent for the platform's")]
    string? Second,
    [Summary("What they come to on a light page")]
    BrandingThemeResponse Light,
    [Summary("What they come to on a dark page")]
    BrandingThemeResponse Dark
)
{

    internal static BrandingColorsResponse Of(BrandLook look)
        => new(look.Accent?.Hex, look.Second?.Hex, BrandingThemeResponse.Of(look, p => p.Light), BrandingThemeResponse.Of(look, p => p.Dark));

}
