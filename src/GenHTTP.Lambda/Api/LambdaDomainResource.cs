using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// Behind the editor key like everything else about a lambda. Whether a lambda
/// may have one at all is decided by its tier, which only an administrator
/// sets - so the owner can read here why the domain is not served, but can
/// only change the domain itself.
/// </remarks>
[Tag(ApiDocumentation.Domain)]
[Summary("A domain of a lambda's own, which it answers at instead of its subdomain of the hosting domain.")]
[Remarks("""
         Part of the premium tier, which only the operator assigns. While a lambda answers at its domain, its subdomain of the
         hosting domain sends its visitors on to the domain with a permanent redirect, and `address` names the domain.

         The domain is entered as it should answer. `www.` in front of a domain is sent on to the domain itself, unless the
         domain was entered with `www.` - then that is where it answers, and the name without is not sent there. The domain has
         to point to this installation (in the DNS) and needs a certificate, which the operator of the installation takes
         care of; the answer says what the domain resolves to, as the server sees it, to check the first.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class LambdaDomainResource(IMetaService meta, ILogger<LambdaDomainResource> logger)
{

    /// <summary>
    /// How long the server waits for the name servers before it says it
    /// could not find out.
    /// </summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(3);

    [ResourceMethod("lambdas/:privateKey/domain")]
    [Summary("Reads the domain of a lambda")]
    [Remarks("The name servers are asked as the request is answered, for up to three seconds, and their answers may be cached for as long as the records say.")]
    [return: Summary("The domain, and what it resolves to")]
    public async ValueTask<DomainResponse> Get([Summary(ApiDocumentation.PrivateKey)] string privateKey)
        => await DescribeAsync(meta.Require(privateKey));

    [ResourceMethod(Method.Put, "lambdas/:privateKey/domain")]
    [Summary("Sets the domain a lambda answers at")]
    [Remarks("""
             Takes effect at once: the next request addressed to the domain reaches the lambda, one addressed to its previous
             domain no longer does, and its subdomain of the hosting domain sends its visitors on to the domain from then on -
             so the domain should point here, with a certificate, first.

             The domain is lower cased, an international one written as it is in the DNS, and a pasted address is forgiven its
             scheme and path. It needs an ending (`example.com`, not `example`), and may not be an IP address or a name of the
             platform. Sending none removes it, as `DELETE` does.
             """)]
    [Response(ResponseStatus.BadRequest, "That is not a domain that can be used. The message says why.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, "The lambda is not in the premium tier - or it is a demo.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "Another lambda answers at that domain.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The domain afterwards, and what it resolves to")]
    public async ValueTask<DomainResponse> Put([Summary(ApiDocumentation.PrivateKey)] string privateKey, DomainChangeRequest request)
    {
        var lambda = meta.ChangeDomain(privateKey, request.Domain);

        logger.LogInformation("Set domain of lambda {Lambda} to {Domain}", lambda.PublicKey, lambda.Domain ?? "(none)");

        return await DescribeAsync(lambda);
    }

    [ResourceMethod(Method.Delete, "lambdas/:privateKey/domain")]
    [Summary("Removes the domain of a lambda")]
    [Remarks("From the next request on it no longer answers at the domain, and its subdomain of the hosting domain answers again instead of sending visitors there. Any tier may remove a domain.")]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda, without a domain")]
    public async ValueTask<DomainResponse> Delete([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var lambda = meta.ChangeDomain(privateKey, null);

        logger.LogInformation("Removed domain of lambda {Lambda}", lambda.PublicKey);

        return await DescribeAsync(lambda);
    }

    internal static async ValueTask<DomainResponse> DescribeAsync(LambdaInfo lambda)
    {
        var allowed = LambdaDescription.AllowsDomain(lambda.Tier);

        var dns = lambda.Domain != null ? await ResolveAsync(lambda.Domain) : null;

        return new DomainResponse(lambda.Domain, lambda.Tier, allowed, LambdaDescription.Serves(lambda.Tier, lambda.Domain), dns);
    }

    /// <summary>
    /// Asks the name servers what the domain points to.
    /// </summary>
    /// <remarks>
    /// Answered as seen from the server, which is the view that counts but
    /// is cached like any other: a record changed a minute ago may still read
    /// as the old one until its time to live runs out.
    /// </remarks>
    private static async ValueTask<DnsResponse> ResolveAsync(string domain)
    {
        using var timeout = new CancellationTokenSource(Patience);

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(domain, timeout.Token);

            return new DnsResponse([.. addresses.Select(Format).Distinct().Order(StringComparer.Ordinal)], null);
        }
        catch (SocketException e) when (e.SocketErrorCode is SocketError.HostNotFound or SocketError.NoData)
        {
            return new DnsResponse([], "The domain does not resolve to anything yet.");
        }
        catch (Exception e) when (e is SocketException or OperationCanceledException)
        {
            return new DnsResponse([], "The name servers could not be asked right now.");
        }
    }

    private static string Format(IPAddress address)
        => address.IsIPv4MappedToIPv6 ? address.MapToIPv4().ToString() : address.ToString();

}
