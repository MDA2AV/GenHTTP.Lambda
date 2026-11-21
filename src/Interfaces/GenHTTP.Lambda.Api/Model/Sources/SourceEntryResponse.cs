using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A published source, as the listing shows it.")]
public sealed record SourceEntryResponse(
    [Summary("The public key of the lambda")]
    string PublicKey,
    [Summary("What its owner calls it in the showcase, where it is in the showcase")]
    string? Title,
    [Summary("What its newest version says it is: the first paragraph of its `docs/product.md`")]
    string? About,
    [Summary("What its owner says about it in the showcase, where it is there")]
    string? Description,
    [Summary("The license it is published under")]
    LicenseResponse License,
    [Summary("How many visitors starred it")]
    int Stars,
    [Summary("Whether the app answers right now")]
    bool Online,
    [Summary("Where the app answers - its own domain while it has one, its public address otherwise")]
    string Address,
    [Summary("Where its source is read on this site: `/source/{publicKey}`")]
    string Path,
    [Summary("Its picture in the showcase, where it has one")]
    string? ImagePath,
    [Summary("Its newest version")]
    int? LatestVersion,
    [Summary("When its newest version was saved")]
    DateTime? Updated,
    [Summary("When its source was published")]
    DateTime PublishedAt
)
{

    public static SourceEntryResponse Of(SourceEntry entry, ILambdaAddresses addresses) => new(
        entry.PublicKey,
        entry.Title,
        entry.About,
        entry.Description,
        LicenseResponse.Of(entry.License),
        entry.Stars,
        entry.Online,
        addresses.Of(entry.PublicKey, entry.Tier, entry.Domain),
        PlatformRoutes.SourceOf(entry.PublicKey),
        entry.Picture is { } picture ? $"/api/v1/showcases/{entry.PublicKey}/image?v={picture.Ticks}" : null,
        entry.LatestVersion,
        entry.Updated,
        entry.PublishedAt
    );

}
