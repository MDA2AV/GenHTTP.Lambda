using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Public keys, and what anybody may know about one.
/// </summary>
/// <remarks>
/// One answer for both questions a public key gets asked: whether it could be
/// claimed for a new lambda, and whether something is answering there. They
/// were two endpoints once, answering overlapping halves of the same lookup.
///
/// Lives beside <c>/lambdas</c> rather than below it, where every single
/// segment is read as an editor key.
/// </remarks>
public sealed class KeyResource(IMetaService meta)
{

    /// <summary>
    /// Whether a key is free, and if it is not, whether its lambda is online.
    /// </summary>
    /// <param name="publicKey">The key, normalized the way it would be stored</param>
    [ResourceMethod(":publicKey")]
    public async ValueTask<KeyResponse> Get(string publicKey)
    {
        var status = await meta.DescribeKeyAsync(publicKey);

        return new KeyResponse(status.PublicKey, status.Valid, status.Available, status.Exists, status.Deployed, status.Reason);
    }

}
