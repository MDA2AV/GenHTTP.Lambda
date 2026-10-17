using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// One answer for both questions a public key gets asked: whether it could be
/// claimed for a new lambda, and whether something is answering there. They
/// were two endpoints once, answering overlapping halves of the same lookup.
///
/// Lives beside <c>/lambdas</c> rather than below it, where every single
/// segment is read as an editor key.
/// </remarks>
[Tag(ApiDocumentation.Keys)]
[Summary("Public keys, and what anybody may know about one: whether it is free, and whether something answers there.")]
[Remarks("Public. A key names the address of a lambda, so what can be learned here is what anybody could learn by visiting it.")]
public sealed class KeyResource(IMetaService meta)
{

    [ResourceMethod(":publicKey")]
    [Summary("Describes a public key")]
    [Remarks("""
             Never refused: a key that cannot be claimed is answered with `valid: false` and the `reason`, as a page checking a
             key while it is typed needs. A key that is free now may be taken by the time a lambda is created with it.
             """)]
    [return: Summary("What anybody may know about the key")]
    public KeyResponse Get([Summary("The key to look up, in any case - it is answered the way it would be stored")] string publicKey)
    {
        var status = meta.DescribeKey(publicKey);

        return new KeyResponse(status.PublicKey, status.Valid, status.Available, status.Exists, status.Deployed, status.Reason);
    }

}
