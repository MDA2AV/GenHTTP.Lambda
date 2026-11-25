using GenHTTP.Api.Infrastructure;

using GenHTTP.Lambda.Infrastructure;

using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Practices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Server;

/// <summary>
/// Puts what every request to the platform passes around a server host that
/// serves nothing but the platform.
/// </summary>
public static class PlatformHosting
{

    /// <summary>
    /// Applies the platform to the given host: its logging, and the concerns
    /// every request passes before it reaches the handler, in the order they
    /// have to be in.
    /// </summary>
    /// <remarks>
    /// Concerns added later wrap the ones added before them, so the list below
    /// reads from the inside out: the last one sees a request first. What is
    /// the platform's own is the same wherever it is served (see
    /// <see cref="PlatformConcerns"/>); the rest is the server's.
    /// </remarks>
    public static IServerHost Platform(this IServerHost host, LambdaPlatform platform)
    {
        var services = platform.Services;

        /*
         * Without the engine's line per request: CallerConcern writes
         * a better one, with who was asking. The engine's was only
         * thrown away again, after a frame around every request and
         * the formatting it took to throw it away.
         */
        host.Logging(services.GetRequiredService<ILoggerFactory>(), logRequests: false)
            .Development(platform.Options.Development);

        /*
         * The scope of a request, for what a host of somebody's own adds to
         * the site with GenHTTP's dependency injection - so inside everything
         * else. The platform and its components take what they need from
         * their container as they are built, and look for no scope.
         */
        host.AddDependencyInjection(services);

        foreach (var concern in PlatformConcerns.Inner(platform))
        {
            host.Add(concern);
        }

        /*
         * The upgrade to the secure endpoint covers a lambda's own
         * domain as well: it redirects to the host that was asked for,
         * so http://shop.example.com goes to https://shop.example.com.
         * The certificate for such a domain is put beside the others
         * by the operator (see CertificateLoader).
         */
        host.Defaults();

        foreach (var concern in PlatformConcerns.Outer(platform))
        {
            host.Add(concern);
        }

        return host;
    }

}
