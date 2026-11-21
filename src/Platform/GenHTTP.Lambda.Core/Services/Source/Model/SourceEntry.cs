using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// A published source, as the listing shows it.
/// </summary>
/// <param name="Title">What its owner calls it on the showcase, if it is on the showcase</param>
/// <param name="About">What its newest version says it is: the first paragraph of its product page</param>
/// <param name="Description">What its owner says about it on the showcase, if it is there</param>
/// <param name="Online">Whether the app answers right now</param>
/// <param name="Updated">When its newest version was saved</param>
/// <param name="Picture">When its showcase picture last changed, if it has one - which names the version of it</param>
/// <param name="PictureType">What that picture is, as it was sniffed</param>
public sealed record SourceEntry(
    string PublicKey,
    string? Title,
    string? About,
    string? Description,
    string License,
    int Stars,
    bool Online,
    LambdaTier Tier,
    string? Domain,
    int? LatestVersion,
    DateTime? Updated,
    DateTime PublishedAt,
    DateTime? Picture,
    string? PictureType
);
