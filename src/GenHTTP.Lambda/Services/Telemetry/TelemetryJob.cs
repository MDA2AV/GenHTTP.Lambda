using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Background;
using GenHTTP.Lambda.Services.Diagnostics;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Takes a reading on an interval, which is the only way a leak shows itself:
/// one number says nothing, a hundred of them describe a slope.
/// </summary>
public sealed class TelemetryJob(ITelemetryService telemetry, RunLog runs, LambdaOptions options) : IBackgroundJob
{

    public string Name => "Telemetry";

    public TimeSpan Interval => options.TelemetryInterval;

    public ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        var sample = telemetry.Sample();

        // the same reading, left on disk where the next run can find it: a
        // process that disappears takes its series with it, and what it was
        // holding at the last beat is most of why it disappeared
        runs.Beat(sample.WorkingSetBytes, sample.ManagedBytes, sample.Requests, sample.OpenSockets);

        return ValueTask.CompletedTask;
    }

}
