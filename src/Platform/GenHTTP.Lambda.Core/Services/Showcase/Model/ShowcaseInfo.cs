using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Showcase;

/// <summary>
/// A lambda as its owner presents it.
/// </summary>
/// <param name="PublicKey">Where it answers</param>
/// <param name="Online">Whether it answers right now - only those are listed</param>
/// <param name="Updated">When the entry last changed, which versions the picture</param>
/// <param name="Tier">The tier of the lambda, which decides whether its domain is served</param>
/// <param name="Domain">The domain the lambda is configured to answer at, if any</param>
public sealed record ShowcaseInfo(
    string PublicKey,
    string Title,
    string Description,
    string ImageType,
    int ImageBytes,
    bool Online,
    DateTime Created,
    DateTime Updated,
    LambdaTier Tier,
    string? Domain
);
