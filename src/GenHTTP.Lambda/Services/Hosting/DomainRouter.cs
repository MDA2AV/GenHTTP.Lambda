using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// The root of the handler tree: a request addressed to a lambda's own domain
/// or to its subdomain of the hosting domain goes to that lambda, one to the
/// hosting domain itself to its page, everything else to the platform.
/// </summary>
/// <remarks>
/// The virtual hosting module of the framework does the same with a table
/// fixed when the server is built. Domains are configured while the server
/// runs, and a subdomain is named after a key that may be claimed at any
/// moment, so the table is the <see cref="DomainRegistry"/> instead, asked per
/// request.
///
/// Everything the server applies to every request - the telemetry, the log
/// line naming the caller, compression, the upgrade from plain HTTP to HTTPS
/// on the same host - sits outside this and covers every branch alike. The
/// platform's routes live in the platform branch only, so a lambda's host is
/// never answered with one of the platform's pages.
/// </remarks>
public sealed class DomainRouter(DomainRegistry registry, IHandler domains, IHandler hosted, IHandler home, IHandler platform) : IHandler
{

    public async ValueTask PrepareAsync(IServer server)
    {
        await domains.PrepareAsync(server);
        await hosted.PrepareAsync(server);
        await home.PrepareAsync(server);
        await platform.PrepareAsync(server);
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request) => request.ResolveHost(registry) switch
    {
        CustomDomain => domains.HandleAsync(request),
        HostedLambda => hosted.HandleAsync(request),
        HostingHome => home.HandleAsync(request),
        _ => platform.HandleAsync(request)
    };

}
