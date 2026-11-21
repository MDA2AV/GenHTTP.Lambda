using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Showcase;

namespace GenHTTP.Lambda.Web;

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
