using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Egress;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// Behind the editor key like everything else about a lambda: whoever owns
/// it asks, and says what for. The decision is the operator's, in
/// <see cref="AdminResource"/> - the owner can only ask, wait, take the
/// request back or give up what was allowed.
/// </remarks>
[Tag(ApiDocumentation.Egress)]
[Summary("Whether a lambda's code may connect to other servers - asked for by its owner, decided by the operator.")]
[Remarks("""
         A lambda shares the server with every other, so it may only connect to other servers - call an API with
         `HttpClient`, open a socket, a WebSocket client or a mail client, look up a name - where it was allowed: the premium
         tier includes it, and any other lambda asks the operator of the installation, saying what it connects to and why.
         Without it, code that connects is refused as it compiles, with `LAMBDA0003` and a message that says so.

         A decision takes effect from the next request, without a deploy: the lambda and the previews of its features are
         built again with it. Code that connects and is online when what was allowed is taken back stops compiling, and its
         requests are answered with an error until it changes or egress is allowed again. Whatever the page of a lambda
         fetches in the visitor's browser is the browser's business, not the server's, and needs none of this.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class LambdaEgressResource(IEgressService egress, IMetaService meta, ILogger<LambdaEgressResource> logger)
{

    [ResourceMethod("lambdas/:privateKey/egress")]
    [Summary("Reads whether a lambda may connect to other servers")]
    [return: Summary("Whether it may, and where its request stands")]
    public EgressResponse Get([Summary(ApiDocumentation.PrivateKey)] string privateKey)
        => EgressResponse.Of(egress.Get(privateKey));

    [ResourceMethod(Method.Put, "lambdas/:privateKey/egress")]
    [Summary("Asks for a lambda to connect to other servers")]
    [Remarks("""
             The request waits for the operator of the installation, who reads what it is for and allows or declines it - the
             answer says where it stands, and `note` what they said. Asked again while it waits, the purpose is replaced and it
             keeps its place; asked again after it was declined, it waits anew. Nothing is asked for a lambda that may connect
             already - a premium one, or one allowed - and the answer says so.

             Say what it connects to and why: the addresses or services, and what the lambda does with them. A lambda that
             passes on whatever it is asked to fetch, a proxy, is the kind of thing this is here to keep off the server.
             """)]
    [Response(ResponseStatus.BadRequest, "No purpose was given.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo + " Or it is blocked.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("Where it stands afterwards")]
    public EgressResponse Put([Summary(ApiDocumentation.PrivateKey)] string privateKey, EgressRequest request)
    {
        var asked = egress.Request(privateKey, request.Purpose);

        logger.LogInformation("Requested egress for lambda {Lambda} status {Status}", meta.PublicKeyOf(privateKey), Said(asked));

        return EgressResponse.Of(asked);
    }

    [ResourceMethod(Method.Delete, "lambdas/:privateKey/egress")]
    [Summary("Takes a request back, or gives up egress")]
    [Remarks("""
             Takes back a request that waits, or gives up what the operator allowed - from the next request on, code that connects
             is refused as it compiles, and a version online that does stops working. A premium lambda may connect whatever is
             asked here. Nothing happens when nothing was asked, or it was declined.
             """)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo + " Or it is blocked.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("Where it stands afterwards")]
    public EgressResponse Delete([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var withdrawn = egress.Withdraw(privateKey);

        logger.LogInformation("Withdrew egress of lambda {Lambda}", meta.PublicKeyOf(privateKey));

        return EgressResponse.Of(withdrawn);
    }

    /// <summary>
    /// Where a request stands, for a log line.
    /// </summary>
    internal static string Said(EgressInfo egress) => egress.Status?.ToString() ?? (egress.Included ? "Included" : "None");

}
