using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What the platform holds.")]
public sealed record PlatformDescription(
    [Summary("Lambdas there are")]
    int Lambdas,
    [Summary("Of those, online")]
    int Deployed,
    [Summary("Versions kept, of all lambdas")]
    int Versions
);
