namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Counts what the server does and remembers how the process looked while it
/// did so.
/// </summary>
public interface ITelemetryService
{

    /// <summary>
    /// When the service was created, which is as good as when the server came up.
    /// </summary>
    DateTime Started { get; }

    /// <summary>
    /// Every request the server has answered since it started.
    /// </summary>
    long TotalRequests { get; }

    /// <summary>
    /// Every request that was answered with a server error.
    /// </summary>
    long TotalFailed { get; }

    /// <summary>
    /// Every connection that was upgraded rather than answered.
    /// </summary>
    long TotalUpgrades { get; }

    /// <summary>
    /// How many connections are open right now.
    /// </summary>
    int OpenSockets { get; }

    /// <summary>
    /// Records a finished request.
    /// </summary>
    /// <param name="elapsed">How long the handler took</param>
    /// <param name="status">The status that was answered with</param>
    void Record(TimeSpan elapsed, int status);

    /// <summary>
    /// Records a connection that was upgraded rather than answered.
    /// </summary>
    void Upgraded();

    /// <summary>
    /// Follows a request while it is being handled, so the ones in flight can
    /// be counted.
    /// </summary>
    IDisposable Track();

    /// <summary>
    /// Takes a reading and appends it to the series.
    /// </summary>
    TelemetrySample Sample();

    /// <summary>
    /// The readings taken over the given window, oldest first.
    /// </summary>
    IReadOnlyList<TelemetrySample> Series(TimeSpan window);

}
