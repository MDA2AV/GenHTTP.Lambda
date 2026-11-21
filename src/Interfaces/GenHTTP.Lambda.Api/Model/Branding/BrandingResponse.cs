using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The name and the logo the operator gave the installation, which its pages show in place of GenHTTP Lambda's.")]
public sealed record BrandingResponse(
    [Summary("What the operator named it; absent where it goes by the platform's own name")]
    string? Name,
    [Summary("The name its pages show: this, or `GenHTTP Lambda`")]
    string Shown,
    [Summary("The logo the operator uploaded; absent where there is none")]
    BrandingLogoResponse? Logo,
    [Summary("Where the pages load the picture beside the name from, which is also the icon of the browser tab: the logo, or the first letter of the name on a plate; absent while the installation goes by the platform's own name and mark")]
    string? Picture,
    [Summary("What a name and a logo may be")]
    BrandingLimitsResponse Limits
)
{

    internal static BrandingResponse Of(Brand brand)
        => new(brand.Given,
               brand.Name,
               brand.Logo is { } logo ? new BrandingLogoResponse(logo.Type, logo.Content.Length, logo.Updated) : null,
               brand.PicturePath,
               BrandingLimitsResponse.Fixed);

}
