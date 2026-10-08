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
/// for a broken link.
/// </remarks>
public sealed class ShowcaseResource(IShowcaseService showcases, ILambdaAddresses addresses)
{

    public const int PageSize = 12;

    /// <summary>
    /// One page of the showcase, the most active lambdas first.
    /// </summary>
    /// <param name="skip">How many to leave out, from the start</param>
    /// <param name="take">How many to answer with, at most 48</param>
    [ResourceMethod]
    public Page<ShowcaseResponse> List(int skip = 0, int take = PageSize)
    {
        var from = Math.Max(0, skip);

        var page = showcases.List(from, take);

        return Page<ShowcaseResponse>.Of([.. page.Entries.Select(e => ShowcaseResponse.Of(e, addresses))], from, page.Total);
    }

    /// <summary>
    /// The picture of an entry.
    /// </summary>
    /// <remarks>
    /// Cached for good, because the address it is linked with carries the time
    /// the entry last changed: a new picture is a new address.
    /// </remarks>
    [ResourceMethod(":publicKey/image")]
    public IResponse Image(string publicKey, IRequest request)
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
