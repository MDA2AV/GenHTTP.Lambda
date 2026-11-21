using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;

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
