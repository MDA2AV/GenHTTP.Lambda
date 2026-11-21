using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Api.Model;

[Summary("How the installation presents itself on its pages.")]
public sealed record BrandResponse(
    [Summary("The name its pages show: `GenHTTP Lambda`, or what its operator named it")]
    string Name,
    [Summary("Where the picture beside the name is - the operator's logo, or the first letter of the name on a plate where they named it without one; absent where the pages draw the platform's own mark")]
    string? Logo,
    [Summary("Whether its operator may give it a name and a logo of its own (`PUT /admin/branding`); the host of the installation decides")]
    bool Changeable
)
{

    internal static BrandResponse Of(IBrandingService branding)
    {
        var brand = branding.Current;

        return new(brand.Name, brand.PicturePath, branding.Changeable);
    }

}

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

[Summary("A logo, as it was uploaded.")]
public sealed record BrandingLogoResponse(
    [Summary("What it is, as read from its content: `image/png`, `image/jpeg`, `image/gif`, `image/webp` or `image/svg+xml`")]
    string Type,
    [Summary("How large it is, in bytes")]
    int Bytes,
    [Summary("When it was uploaded")]
    DateTime Updated
);

[Summary("What a name and a logo may be.")]
public sealed record BrandingLimitsResponse(
    [Summary("How many characters a name may have")]
    int Name,
    [Summary("How large a logo may be, in bytes")]
    int LogoBytes,
    [Summary("The kinds of picture a logo may be")]
    IReadOnlyList<string> Types
)
{

    internal static readonly BrandingLimitsResponse Fixed = new(BrandChecks.MaxName, BrandChecks.MaxLogoBytes, BrandChecks.Types);

}

[Summary("What the installation is called.")]
public sealed record BrandingRequest(
    [Summary("The name its pages show, one line of at most 40 characters; empty or absent gives it back the platform's own name")]
    string? Name = null
);
