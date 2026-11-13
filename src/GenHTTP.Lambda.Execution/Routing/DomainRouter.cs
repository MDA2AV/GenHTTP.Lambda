using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Hosting;

namespace GenHTTP.Lambda.Execution;

/// <summary>
/// The root of the handler tree: a request addressed to a lambda's own domain
/// or to its subdomain of the hosting domain goes to that lambda, one to the
/// hosting domain itself to its page, one to the platform's names to the
/// platform, and one to any other host is told nothing is there.
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
/// never answered with one of the platform's pages - nor is a host that is
/// nobody's yet, which is usually a domain on its way to becoming a lambda's.
/// </remarks>
public sealed class DomainRouter(IDomainRegistry registry, IHandler domains, IHandler hosted, IHandler home, IHandler unknown, IHandler platform)
    : IHandler
{

    public async ValueTask PrepareAsync(IServer server)
    {
        await domains.PrepareAsync(server);
        await hosted.PrepareAsync(server);
        await home.PrepareAsync(server);
        await unknown.PrepareAsync(server);
        await platform.PrepareAsync(server);
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request) => request.ResolveHost(registry) switch
    {
        CustomDomain => domains.HandleAsync(request),
        // sent on to the domain before it gets here; were it not, it is still
        // somebody's site and never the platform
        WwwOfDomain => domains.HandleAsync(request),
        HostedLambda => hosted.HandleAsync(request),
        HostingHome => home.HandleAsync(request),
        UnknownHost => unknown.HandleAsync(request),
        _ => platform.HandleAsync(request)
    };

}
