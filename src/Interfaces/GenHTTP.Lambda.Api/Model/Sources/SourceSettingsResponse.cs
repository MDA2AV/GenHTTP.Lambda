using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What a lambda's source is published under.")]
public sealed record SourceSettingsResponse(
    [Summary("Whether anybody may read it right now - it may have been taken down since")]
    bool Published,
    [Summary("The license")]
    LicenseResponse License,
    [Summary("Who holds the copyright, where the owner named somebody")]
    string? Author,
    [Summary("Who the license names: the author, or the authors of the lambda")]
    string Holder,
    [Summary("How many visitors starred it - kept while it is taken down")]
    int Stars,
    [Summary("When it was published")]
    DateTime PublishedAt,
    [Summary("When what it is published under last changed")]
    DateTime Updated,
    [Summary("Where it is read on this site: `/source/{publicKey}`")]
    string Path
)
{

    public static SourceSettingsResponse Of(SourceSettings settings) => new(
        settings.Published,
        LicenseResponse.Of(settings.License),
        settings.Author,
        SourceLicenses.Holder(settings.Author, settings.PublicKey),
        settings.Stars,
        settings.PublishedAt,
        settings.Updated,
        PlatformRoutes.SourceOf(settings.PublicKey)
    );

}
