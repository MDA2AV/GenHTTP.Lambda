using GenHTTP.Api.Content.Documentation;

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
    [Summary("Where it answers - its own domain while it has one, its public address otherwise")]
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
