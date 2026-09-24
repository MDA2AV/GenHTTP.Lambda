using GenHTTP.Lambda.Services.Showcase;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// A lambda in the showcase, as the page draws it.
/// </summary>
/// <param name="Path">Where it answers</param>
/// <param name="ImagePath">Its picture, versioned so it can be cached for good</param>
/// <param name="Online">Whether it answers right now - the page only lists those</param>
public sealed record ShowcaseResponse(
    string PublicKey,
    string Title,
    string Description,
    string Path,
    string ImagePath,
    string ImageType,
    int ImageBytes,
    bool Online,
    DateTime Created,
    DateTime Updated
)
{

    public static ShowcaseResponse Of(ShowcaseInfo info) => new(
        info.PublicKey,
        info.Title,
        info.Description,
        $"/lambda/{info.PublicKey}/",
        $"/api/v1/showcases/{info.PublicKey}/image?v={info.Updated.Ticks}",
        info.ImageType,
        info.ImageBytes,
        info.Online,
        info.Created,
        info.Updated
    );

}

/// <summary>
/// One page of the showcase, the most active first.
/// </summary>
/// <param name="Total">How many are listed altogether</param>
/// <param name="Next">Where the next page starts, or nothing when this was the last</param>
public sealed record ShowcaseListingResponse(IReadOnlyList<ShowcaseResponse> Entries, int Total, int? Next);

/// <summary>
/// The entry of a lambda as its owner sees it, with what may go into one.
/// </summary>
/// <param name="Showcase">The entry, or nothing while the lambda is not in the showcase</param>
public sealed record OwnShowcaseResponse(ShowcaseResponse? Showcase, ShowcaseLimitsResponse Limits);

/// <param name="Tone">How an entry should read</param>
public sealed record ShowcaseLimitsResponse(int Title, int Description, int ImageBytes, IReadOnlyList<string> ImageTypes, string Tone);

/// <summary>
/// What the entry should say.
/// </summary>
/// <param name="Image">The picture, base64 encoded. Left out, the one already there is kept.</param>
public sealed record ShowcaseRequest(string? Title, string? Description, string? Image);
