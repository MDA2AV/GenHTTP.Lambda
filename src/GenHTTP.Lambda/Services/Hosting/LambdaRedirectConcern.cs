using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Sends a request for an address a lambda no longer answers at on to the
/// one it does, for good: its old path below <c>/lambda/</c> on the platform,
/// and its subdomain of the hosting domain while it answers at a domain of
/// its own.
/// </summary>
/// <remarks>
/// In front of everything else the server does, the request log and the
/// telemetry included: the visitor is on their way to the lambda, which
/// counts and logs them when they arrive, and nothing about the platform
/// should run for a request that is only told where to go. That includes the
/// upgrade to HTTPS, so a plain request for an old address is sent to the
/// secure new one in one step rather than two.
///
/// The rest of the path and the query go along, so a link to a page or an API
/// of a lambda goes on working. Moved Permanently for a read, as GenHTTP's own
/// redirects answer, and Permanent Redirect for anything else, which a client
/// follows with the same method and body. Neither is cached for longer than a
/// day: where a lambda answers changes when its owner gives it a domain or
/// takes it away, and a browser that kept the old answer for good would send
/// its visitors to a domain that is no longer the lambda's.
/// </remarks>
public sealed class LambdaRedirectConcern(IHandler content, IDomainRegistry registry, ILambdaAddresses addresses) : IConcern
{
    private const string Prefix = "/lambda/";

    private static ReadOnlySpan<byte> PrefixBytes => "/lambda/"u8;

    #region Get-/Setters

    public IHandler Content => content;

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.ResolveHost(registry) switch
        {
            null => FromPlatform(request),
            HostedLambda hosted => ToDomain(request, hosted),
            _ => null
        };

        return target == null ? content.HandleAsync(request) : new(Redirect(request, target));
    }

    /// <summary>
    /// Where a path below <c>/lambda/</c> went: the lambda named by its first
    /// segment, wherever it answers now - or nothing, for any other path.
    /// </summary>
    private string? FromPlatform(IRequest request)
    {
        // asked of every request to the platform, so on the bytes, before
        // anything is made of them
        if (!request.Header.Path.Bytes.Span.StartsWith(PrefixBytes))
        {
            return null;
        }

        var rest = request.Header.Path.ToString()[Prefix.Length..];

        var cut = rest.IndexOf('/');

        // a key that could never have been one is not a lambda's address, and
        // finds out at the platform that it is not anything else either
        if (!LambdaKeys.TryRead(Uri.UnescapeDataString(cut < 0 ? rest : rest[..cut]), out var key))
        {
            return null;
        }

        var address = registry.DomainOf(key) is { } domain ? $"https://{domain}" : addresses.Of(key).TrimEnd('/');

        return address + (cut < 0 ? "/" : rest[cut..]) + RequestOrigin.Query(request);
    }

    /// <summary>
    /// The domain of its own a lambda answers at instead of its subdomain, if
    /// it has one.
    /// </summary>
    private string? ToDomain(IRequest request, HostedLambda hosted)
    {
        if (!LambdaKeys.TryRead(hosted.Label, out var key) || registry.DomainOf(key) is not { } domain)
        {
            return null;
        }

        return $"https://{domain}{request.Header.Path}{RequestOrigin.Query(request)}";
    }

    private static IResponse Redirect(IRequest request, string target)
        => request.Respond()
                  .Status(request.HasType(RequestMethod.Get, RequestMethod.Head) ? ResponseStatus.MovedPermanently : ResponseStatus.PermanentRedirect)
                  .Header("Location", target)
                  .Header("Cache-Control", "max-age=86400")
                  .Build();

    #endregion

}

public sealed class LambdaRedirectConcernBuilder(IDomainRegistry registry, ILambdaAddresses addresses) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new LambdaRedirectConcern(content, registry, addresses);
}
