namespace GenHTTP.Lambda.Services.Background;

/// <summary>
/// A task that runs regularly for as long as the server is up.
/// </summary>
public interface IBackgroundJob
{

    /// <summary>
    /// The name of the job, used for logging.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// How often the job should run.
    /// </summary>
    TimeSpan Interval { get; }

    ValueTask ExecuteAsync(CancellationToken cancellation);

}
