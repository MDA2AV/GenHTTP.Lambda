using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Showcase;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A lambda in the showcase, as the showcase lists it.")]
public sealed record ShowcaseResponse(
    [Summary("The public key of the lambda")]
    string PublicKey,
    [Summary("What its owner calls it")]
    string Title,
    [Summary("What its owner says it is, and what a visitor can do with it")]
    string Description,
    [Summary("Where it answers - its own domain while it has one, its address below the hosting domain otherwise")]
    string Address,
    [Summary("Where its picture is on this site; it changes with the picture, so it may be cached for good")]
    string ImagePath,
    [Summary("The media type of the picture: `image/png`, `image/jpeg`, `image/gif` or `image/webp`")]
    string ImageType,
    [Summary("How large the picture is, in bytes")]
    int ImageBytes,
    [Summary("Whether the lambda answers right now - the listing only has those")]
    bool Online,
    [Summary("When it was put into the showcase")]
    DateTime Created,
    [Summary("When its entry last changed")]
    DateTime Updated
)
{

    public static ShowcaseResponse Of(ShowcaseInfo info, ILambdaAddresses addresses) => new(
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

[Summary("The showcase entry of a lambda as its owner sees it, with what may go into one.")]
public sealed record OwnShowcaseResponse(
    [Summary("The entry; absent while the lambda is not in the showcase")]
    ShowcaseResponse? Showcase,
    [Summary("What an entry may hold")]
    ShowcaseLimitsResponse Limits
);

[Summary("What a showcase entry may hold.")]
public sealed record ShowcaseLimitsResponse(
    [Summary("How many characters the title may have")]
    int Title,
    [Summary("How many characters the description may have")]
    int Description,
    [Summary("How large the picture may be, in bytes, as the operator set it")]
    int ImageBytes,
    [Summary("The media types a picture may have")]
    IReadOnlyList<string> ImageTypes,
    [Summary("How an entry should read")]
    string Tone
);

[Summary("What a showcase entry should say.")]
public sealed record ShowcaseRequest(
    [Summary("What it is called, on one line")]
    string? Title,
    [Summary("What it is, and what a visitor can do with it")]
    string? Description,
    [Summary("The picture, base64 encoded: a PNG, JPEG, GIF or WebP image. Required for a new entry; left out, the one there is kept.")]
    string? Image
);
