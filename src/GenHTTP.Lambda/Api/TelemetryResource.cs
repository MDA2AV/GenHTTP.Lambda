using System.Runtime;
using System.Runtime.InteropServices;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// What the process looks like from the inside, so a long run can be watched
/// rather than guessed at.
/// </summary>
/// <remarks>
/// Everything here is an aggregate. No key, no code and no client address
/// leaves through this resource, which is what makes it safe to serve without
/// asking who is looking.
/// </remarks>
public sealed class TelemetryResource(TelemetryService telemetry, LambdaTelemetry lambdas, ServerRegistry registry, IMetaService meta, LambdaOptions options)
{

    /// <summary>
    /// The current state of the server and the readings of the last hour.
    /// </summary>
    /// <param name="minutes">How far back the series should reach</param>
    [ResourceMethod]
    public async ValueTask<TelemetryResponse> Get(int? minutes, IRequest request)
    {
        AdminGate.RequireForFigures(request, options);

        var window = TimeSpan.FromMinutes(Math.Clamp(minutes ?? 60, 1, 60 * 24));

        var series = telemetry.Series(window);

        var latest = series.Count > 0 ? series[^1] : telemetry.Sample();

        var counts = await meta.CountAsync();

        var server = registry.Instance;

        return new TelemetryResponse(
            new ServerDescription(
                server?.ServerEngine.ToString().ToLowerInvariant() ?? options.Engine.ToString().ToLowerInvariant(),
                server?.Version ?? "unknown",
                RuntimeInformation.FrameworkDescription,
                RuntimeInformation.RuntimeIdentifier,
                GCSettings.IsServerGC,
                Environment.ProcessorCount,
                telemetry.Started,
                (long)(DateTime.UtcNow - telemetry.Started).TotalSeconds
            ),
            new TrafficDescription(
                telemetry.TotalRequests,
                telemetry.TotalFailed,
                telemetry.TotalUpgrades,
                telemetry.OpenSockets
            ),
            new PlatformDescription(counts.Lambdas, counts.Deployed, counts.Versions),
            latest,
            (int)options.TelemetryInterval.TotalSeconds,
            series
        );
    }

    /// <summary>
    /// What each lambda has been doing since the server came up.
    /// </summary>
    /// <remarks>
    /// Public while the platform is small; set LAMBDA_PUBLIC_ACTIVITY=false and
    /// it stops being served without anything stopping being counted. Only the
    /// public key identifies a lambda here - the editor key, the code and the
    /// visitors are not part of it.
    /// </remarks>
    [ResourceMethod("lambdas")]
    public ActivityResponse GetLambdas(IRequest request)
    {
        AdminGate.RequireForFigures(request, options);

        if (!options.PublicActivity)
        {
            throw LambdaException.NotFound("The activity of the lambdas is not public on this installation.");
        }

        var activity = lambdas.Describe();

        return new ActivityResponse(
            activity,
            activity.Sum(a => a.Requests),
            activity.Sum(a => a.Upgrades)
        );
    }

}
