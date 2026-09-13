using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Background;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Takes a reading on an interval, which is the only way a leak shows itself:
/// one number says nothing, a hundred of them describe a slope.
/// </summary>
public sealed class TelemetryJob(ITelemetryService telemetry, LambdaOptions options) : IBackgroundJob
{

    public string Name => "Telemetry";

    public TimeSpan Interval => options.TelemetryInterval;

    public ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        telemetry.Sample();

        return ValueTask.CompletedTask;
    }

}
