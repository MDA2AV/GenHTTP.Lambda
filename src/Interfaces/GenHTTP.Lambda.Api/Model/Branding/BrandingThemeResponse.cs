using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What the colours come to in one theme; absent is what the platform's own are drawn in.")]
public sealed record BrandingThemeResponse(
    [Summary("The shades of the accent")]
    BrandingShadesResponse? Accent,
    [Summary("The shades of the second accent")]
    BrandingShadesResponse? Second,
    [Summary("The first light of the moving background behind the front page, as `#rrggbb`: the accent")]
    string? FirstLight,
    [Summary("Its second light: the second accent, or the colour beside the accent where there is none")]
    string? SecondLight
)
{

    internal static BrandingThemeResponse Of(BrandLook look, Func<BrandPalette, BrandShades> theme)
        => new(BrandingShadesResponse.Of(Shades(look.AccentShades, theme)),
               BrandingShadesResponse.Of(Shades(look.SecondShades, theme)),
               Shades(look.AccentShades, theme)?.Light.Hex,
               Shades(look.SecondLight, theme)?.Light.Hex);

    private static BrandShades? Shades(BrandPalette? palette, Func<BrandPalette, BrandShades> theme) => palette == null ? null : theme(palette);

}
