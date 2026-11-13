using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Showcase;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// The first page of the showcase as a rendered page carries it, for the
/// browser to take over.
/// </summary>
/// <remarks>
/// The same shape as the API's listing (<c>GET /api/v1/showcases/</c>), since
/// the page reads either one the same way - made here rather than taken from
/// the API, so that the pages do not depend on the component that answers it.
/// A test holds the two to the same JSON.
/// </remarks>
/// <param name="Entries">The entries of the page, in the order they are shown</param>
/// <param name="Total">How many entries there are altogether</param>
/// <param name="Next">Where the next page starts; absent when this was the last</param>
public sealed record ShowcaseListing(IReadOnlyList<ShowcaseListingEntry> Entries, int Total, int? Next)
{

    /// <summary>
    /// The first page of the showcase.
    /// </summary>
    public static ShowcaseListing First(IShowcaseService showcases, ILambdaAddresses addresses)
    {
        var page = showcases.List(0, ShowcasePage.Size);

        var entries = page.Entries.Select(e => ShowcaseListingEntry.Of(e, addresses)).ToList();

        return new ShowcaseListing(entries, page.Total, entries.Count < page.Total ? entries.Count : null);
    }

}

/// <summary>
/// An entry of the showcase, as the API lists it.
/// </summary>
public sealed record ShowcaseListingEntry(
    string PublicKey,
    string Title,
    string Description,
    string Address,
    string ImagePath,
    string ImageType,
    int ImageBytes,
    bool Online,
    DateTime Created,
    DateTime Updated
)
{

    public static ShowcaseListingEntry Of(ShowcaseInfo info, ILambdaAddresses addresses) => new(
        info.PublicKey,
        info.Title,
        info.Description,
        addresses.Of(info.PublicKey, info.Tier, info.Domain),
        $"/api/v1/showcases/{info.PublicKey}/image?v={info.Updated.Ticks}",
        info.ImageType,
        info.ImageBytes,
        info.Online,
        info.Created,
        info.Updated
    );

}
