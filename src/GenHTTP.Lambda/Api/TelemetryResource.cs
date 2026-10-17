using System.Runtime;
using System.Runtime.InteropServices;

using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// What the process looks like from the inside, so a long run can be watched
/// rather than guessed at.
/// </summary>
/// <remarks>
/// For the operator only, behind the token (see <see cref="AdminAuthentication"/>):
/// the owner of a lambda sees its own figures in the editor, and nobody sees
/// everybody's.
/// </remarks>
[Tag(ApiDocumentation.Telemetry)]
[Summary("What the server process looks like from the inside, so a long run can be watched rather than guessed at.")]
[Remarks("""
         Readings of memory, the collector, the threads and the connections, taken on an interval and kept in memory, and what
         happened on the platform per day. Only the public key identifies a lambda here.
         """)]
[RequestHeader(AdminAuthentication.Header, AdminAuthentication.HeaderDescription, Required = true)]
[Response(ResponseStatus.Unauthorized, AdminAuthentication.MissingDescription)]
[Response(ResponseStatus.Forbidden, AdminAuthentication.WrongDescription)]
public sealed class TelemetryResource(ITelemetryService telemetry, IMetaService meta, LambdaOptions options)
{

    [ResourceMethod]
    [Summary("Reads the telemetry of the server")]
    [return: Summary("What is running, what it did, and the readings taken while it did")]
    public TelemetryResponse Get(IRequest request, [Summary("How far back the readings reach, in minutes: 1 to 1440, as far as they are kept")] int minutes = 60,
                                 [Summary("How far back the events reach, in days: 1 to 365")] int days = 30)
    {
        var window = TimeSpan.FromMinutes(Math.Clamp(minutes, 1, 60 * 24));

        // a different window from the readings on purpose: memory is watched
        // over an hour, and whether anybody is using the platform is a
        // question about weeks
        var history = meta.GetEvents(days);

        var series = telemetry.Series(window);

        var latest = series.Count > 0 ? series[^1] : telemetry.Sample();

        var counts = meta.Count();

        var server = request.Server;

        return new TelemetryResponse(
            new ServerDescription(
                server.ServerEngine.ToString().ToLowerInvariant(),
                server.Version,
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
            series,
            history
        );
    }

    [ResourceMethod("lambdas")]
    [Summary("Reads what each lambda has been doing")]
    [Remarks("Only the public key identifies a lambda here - the editor key, the code and the visitors are not part of it. A lambda nobody called is not listed.")]
    [return: Summary("Every lambda that was called, and the totals")]
    public ActivityResponse GetLambdas()
    {
        var activity = telemetry.DescribeLambdas();

        return new ActivityResponse(
            activity,
            activity.Sum(a => a.Requests),
            activity.Sum(a => a.Upgrades)
        );
    }

}
