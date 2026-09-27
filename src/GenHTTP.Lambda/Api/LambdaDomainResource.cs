using System.Net;
using System.Net.Sockets;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The domain a lambda answers at besides its path, for its owner.
/// </summary>
/// <remarks>
/// Behind the editor key like everything else about a lambda. Whether a lambda
/// may have one at all is decided by its tier, which only an administrator
/// sets - so the owner can read here why the domain is not served, but can
/// only change the domain itself.
/// </remarks>
public sealed class LambdaDomainResource(IMetaService meta)
{

    /// <summary>
    /// How long the server waits for the name servers before it says it
    /// could not find out.
    /// </summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(3);

    /// <summary>
    /// The domain of the lambda, whether it is served, and what it resolves to.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/domain")]
    public async ValueTask<DomainResponse> Get(string privateKey)
        => await DescribeAsync(await meta.RequireAsync(privateKey));

    /// <summary>
    /// Sets the domain the lambda answers at.
    /// </summary>
    /// <remarks>
    /// Only in the premium tier. Takes effect at once: the next request
    /// addressed to the domain reaches the lambda, and one addressed to the
    /// previous domain no longer does.
    /// </remarks>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/domain")]
    public async ValueTask<DomainResponse> Put(string privateKey, DomainChangeRequest request)
        => await DescribeAsync(await meta.ChangeDomainAsync(privateKey, request.Domain));

    /// <summary>
    /// Stops the lambda answering at its domain. Its path stays as it is.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/domain")]
    public async ValueTask<DomainResponse> Delete(string privateKey)
        => await DescribeAsync(await meta.ChangeDomainAsync(privateKey, null));

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
