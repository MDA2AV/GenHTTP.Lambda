using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A logo, as it was uploaded.")]
public sealed record BrandingLogoResponse(
    [Summary("What it is, as read from its content: `image/png`, `image/jpeg`, `image/gif`, `image/webp` or `image/svg+xml`")]
    string Type,
    [Summary("How large it is, in bytes")]
    int Bytes,
    [Summary("When it was uploaded")]
    DateTime Updated
);
