using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Showcase;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The lambdas their owners chose to show, for anybody to browse.
/// </summary>
/// <remarks>
/// Public and read only, like the examples: it names lambdas by their public
/// key and never hands out an editor key. Only lambdas that are online are
/// listed - an entry for something that does not answer is an advertisement
/// for a broken link.
/// </remarks>
public sealed class ShowcaseResource(IShowcaseService showcases)
{

    private const int PageSize = 12;

    /// <summary>
    /// One page of the showcase, the most active lambdas first.
    /// </summary>
    /// <param name="skip">How many to leave out, from the start</param>
    /// <param name="take">How many to answer with, at most 48</param>
    [ResourceMethod]
    public async ValueTask<ShowcaseListingResponse> List(int? skip, int? take)
    {
        var from = Math.Max(0, skip ?? 0);

        var page = await showcases.ListAsync(from, take ?? PageSize);

        var next = from + page.Entries.Count;

        return new ShowcaseListingResponse([.. page.Entries.Select(ShowcaseResponse.Of)], page.Total, next < page.Total ? next : null);
    }

    /// <summary>
    /// The picture of an entry.
    /// </summary>
    /// <remarks>
    /// Cached for good, because the address it is linked with carries the time
    /// the entry last changed: a new picture is a new address.
    /// </remarks>
    [ResourceMethod(":publicKey/image")]
    public async ValueTask<IResponse> Image(string publicKey, IRequest request)
    {
        var image = await showcases.GetImageAsync(publicKey)
                 ?? throw LambdaException.NotFound($"There is no showcase for '{publicKey}'.");

        return request.Respond()
                      .Content(new BinaryContent(image.Content, image.Type))
                      .Header("Cache-Control", "public, max-age=31536000, immutable")
                      .Header("X-Content-Type-Options", "nosniff")
                      .Build();
    }

}
