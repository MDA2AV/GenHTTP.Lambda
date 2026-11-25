using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Api.Model;

[Summary("How the installation presents itself on its pages.")]
public sealed record BrandResponse(
    [Summary("The name its pages show: `GenHTTP Lambda`, or what its operator named it")]
    string Name,
    [Summary("Where the picture beside the name is - the operator's logo, or the first letter of the name on a plate where they named it without one; absent where the pages draw the platform's own mark")]
    string? Logo,
    [Summary("Whether its operator may brand it (`/admin/branding`); the host of the installation decides")]
    bool Changeable,
    [Summary("What its pages draw links, buttons and marks in, as `#rrggbb`; absent where that is the platform's blue")]
    string? Accent = null,
    [Summary("Its second accent, as `#rrggbb`; absent where it has none of its own")]
    string? SecondAccent = null
)
{

    internal static BrandResponse Of(IBrandingService branding)
    {
        var brand = branding.Current;

        return new(brand.Name, brand.PicturePath, branding.Changeable, brand.Look.Accent?.Hex, brand.Look.Second?.Hex);
    }

}
