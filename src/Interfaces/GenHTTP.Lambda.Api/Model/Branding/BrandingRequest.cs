using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What the installation is called.")]
public sealed record BrandingRequest(
    [Summary("The name its pages show, one line of at most 40 characters; empty or absent gives it back the platform's own name")]
    string? Name = null
);
