namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Counts what the server does and remembers how the process looked while it
/// did so.
/// </summary>
public interface ITelemetryService
{

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
