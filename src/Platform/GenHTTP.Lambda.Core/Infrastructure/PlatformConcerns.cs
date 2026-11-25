using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Blocking;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.QuietHours;
using GenHTTP.Lambda.Services.Telemetry;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// What every request to a platform passes before it reaches the handler,
/// whichever server it arrives at: its own (<c>PlatformHosting</c>), or one
/// that serves more than the platform (<c>PlatformHandler</c>).
/// </summary>
/// <remarks>
/// Concerns added later wrap the ones added before them, so both lists read
/// from the inside out: the last one sees a request first. They are two lists
/// because a server of its own puts GenHTTP's defaults between them.
/// </remarks>
public static class PlatformConcerns
{

    /// <summary>
    /// Nearest to the handler, inside whatever a server does to every answer.
    /// </summary>
    public static IReadOnlyList<IConcernBuilder> Inner(LambdaPlatform platform)
    {
        var services = platform.Services;

        return
        [
            services.GetRequiredService<ServerRegistry>().Capture(),
            ServiceConcern.Of<TelemetryConcern>(services),
            /*
             * Beside the telemetry, so it counts what that counts: every
             * request answered, by the hour of the day, and the lambda's
             * from the visit made further in - for the quiet hours.
             */
            ServiceConcern.Of<QuietHoursConcern>(services)
        ];
    }

    /// <summary>
    /// Outside whatever a server does to every answer.
    /// </summary>
    public static IReadOnlyList<IConcernBuilder> Outer(LambdaPlatform platform)
    {
        var services = platform.Services;

        return
        [
            /*
             * Outside everything but the redirect below, which sends a
             * request elsewhere without serving it, because a concern added
             * later wraps the ones added before it.
             *
             * It has to be outside the defaults of a server specifically.
             * Those carry the upgrade from plain HTTP to HTTPS, which answers
             * the request itself and never calls through - so from anywhere
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
            ServiceConcern.Of<CallerConcern>(services),
            /*
             * Outside even that: an old address of a lambda, and its
             * subdomain while it has a domain of its own, are answered
             * with where it is now before anything about the platform
             * runs - no log line, no telemetry, no upgrade to HTTPS, which
             * the address it is sent to has already.
             */
            new LambdaRedirectConcernBuilder(services.GetRequiredService<IDomainRegistry>(), services.GetRequiredService<ILambdaAddresses>()),
            /*
             * Outermost of all: a blocked address is refused before it is
             * redirected, logged or counted - apart from the panel, so an
             * operator who blocked themselves can lift it (see
             * AddressBlockConcern).
             */
            new AddressBlockConcernBuilder(services.GetRequiredService<IAddressBlocks>(), services.GetRequiredService<IDomainRegistry>())
        ];
    }

}
