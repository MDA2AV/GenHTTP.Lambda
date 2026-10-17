using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Showcase;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The lambdas their owners chose to show, for anybody to browse.
/// </summary>
/// <remarks>
/// Public and read only, like the examples: it names lambdas by their public
/// key and never hands out an editor key. Only lambdas that are online are
/// listed - an entry for something that does not answer is an advertisement
/// for a broken link. Listed under the tag <see cref="LambdaShowcaseResource"/>
/// describes.
/// </remarks>
[Tag(ApiDocumentation.Showcase)]
public sealed class ShowcaseResource(IShowcaseService showcases, ILambdaAddresses addresses)
{

    public const int PageSize = 12;

    [ResourceMethod]
    [Summary("Lists the showcase, a page at a time")]
    [Remarks("""
             Public. Only lambdas that are online are listed, the liveliest first: what was used over the last day, how recently
             it was visited and changed, and a few days of being seen for a new entry. The order changes as they are used, so a
             page read later may overlap the one before.
             """)]
    [return: Summary("A page of entries, and where the next one starts")]
    public Page<ShowcaseResponse> List([Summary("How many entries to leave out, from the start")] int skip = 0,
                                       [Summary("How many entries to answer with, 1 to 48")] int take = PageSize)
    {
        var from = Math.Max(0, skip);

        var page = showcases.List(from, take);

        return Page<ShowcaseResponse>.Of([.. page.Entries.Select(e => ShowcaseResponse.Of(e, addresses))], from, page.Total);
    }

    /// <remarks>
    /// Cached for good, because the address it is linked with carries the time
    /// the entry last changed: a new picture is a new address.
    /// </remarks>
    [ResourceMethod(":publicKey/image")]
    [Summary("Reads the picture of a showcase entry")]
    [Remarks("Public. Link it by the `imagePath` of the entry, which changes with the picture, so it may be cached for good.")]
    [Response(ResponseStatus.Ok, "The picture, as it was uploaded", ContentType = "image/*")]
    [ResponseHeader(ResponseStatus.Ok, "Cache-Control", "`public, max-age=31536000, immutable`")]
    [Response(ResponseStatus.NotFound, "The lambda of this public key has no showcase entry.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public IResponse Image([Summary("The public key of the lambda")] string publicKey, IRequest request)
    {
        var image = showcases.GetImage(publicKey)
                 ?? throw LambdaException.NotFound($"There is no showcase for '{publicKey}'.");

        return request.Respond()
                      .Content(image.Content, new ContentType(image.Type))
                      .Header("Cache-Control", "public, max-age=31536000, immutable")
                      .Header("X-Content-Type-Options", "nosniff")
                      .Build();
    }

}
