using GenHTTP.Api.Infrastructure;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Blocking;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Telemetry;

using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Practices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Server;

/// <summary>
/// Puts what every request to the platform passes around a server host.
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
    /// reads from the inside out: the last one sees a request first.
    /// </remarks>
    public static IServerHost Platform(this IServerHost host, LambdaPlatform platform)
    {
        var services = platform.Services;

        return host
               /*
                * Without the engine's line per request: CallerConcern writes
                * a better one, with who was asking. The engine's was only
                * thrown away again, after a frame around every request and
                * the formatting it took to throw it away.
                */
               .Logging(services.GetRequiredService<ILoggerFactory>(), logRequests: false)
               .Development(platform.Options.Development)
               .Add(services.GetRequiredService<ServerRegistry>().Capture())
               .Add(Dependent.Concern<TelemetryConcern>())
               /*
                * The upgrade to the secure endpoint covers a lambda's own
                * domain as well: it redirects to the host that was asked for,
                * so http://shop.example.com goes to https://shop.example.com.
                * The certificate for such a domain is put beside the others
                * by the operator (see CertificateLoader).
                */
               .Defaults()
               /*
                * Outside everything but the scope and the redirect below,
                * which sends a request elsewhere without serving it, because a concern
                * added later wraps the ones added before it.
                *
                * It has to be outside the defaults specifically. Those carry
                * the upgrade from plain HTTP to HTTPS, which answers the
                * request itself and never calls through - so from anywhere
                * inside them, every visitor who typed the bare domain, every
                * scanner knocking on port 80 and the container's own health
                * check are simply not there. They were being logged by the
                * engine and then dropped from the book as duplicates of a
                * line that was never written.
                *
                * Outermost also means the mark is in place before anything
                * below can log under it, and that what is timed is the whole
                * answer rather than the part after the throttle.
                */
               .Add(Dependent.Concern<CallerConcern>())
               /*
                * The scope of the request, which every concern taken from
                * the container is resolved from - this one and the telemetry
                * above, and those in front of the lambdas. A concern outside
                * it would find no scope to be resolved from.
                */
               .AddDependencyInjection(services)
               /*
                * Outside even the scope: an old address of a lambda, and its
                * subdomain while it has a domain of its own, are answered
                * with where it is now before anything about the platform
                * runs - no log line, no telemetry, no upgrade to HTTPS, which
                * the address it is sent to has already. Built by hand, since
                * there is no scope out here to take it from.
                */
               .Add(new LambdaRedirectConcernBuilder(services.GetRequiredService<IDomainRegistry>(),
                                                     services.GetRequiredService<ILambdaAddresses>()))
               /*
                * Outermost of all: a blocked address is refused before it is
                * redirected, logged or counted - apart from the panel, so an
                * operator who blocked themselves can lift it (see
                * AddressBlockConcern).
                */
               .Add(new AddressBlockConcernBuilder(services.GetRequiredService<IAddressBlocks>(),
                                                   services.GetRequiredService<IDomainRegistry>()));
    }

}
