using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Api.Model;

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
