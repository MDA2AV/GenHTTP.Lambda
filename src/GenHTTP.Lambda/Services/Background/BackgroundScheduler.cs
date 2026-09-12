using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Background;

/// <summary>
/// Runs the background jobs of the application. GenHTTP has no scheduler of its
/// own and this needs no more than a loop per job.
/// </summary>
public sealed class BackgroundScheduler(IEnumerable<IBackgroundJob> jobs, ILogger<BackgroundScheduler> logger) : IAsyncDisposable
{
    private readonly CancellationTokenSource _shutdown = new();

    private readonly List<Task> _running = [];

    private bool _disposed;

    #region Functionality

    /// <summary>
    /// Starts every registered job. Each one runs once immediately and then on
    /// its own interval.
    /// </summary>
    public void Start()
    {
        foreach (var job in jobs)
        {
            _running.Add(Task.Run(() => RunAsync(job, _shutdown.Token)));
        }

        if (_running.Count > 0)
        {
            logger.LogInformation("Started {Count} background job(s)", _running.Count);
        }
    }

    private async Task RunAsync(IBackgroundJob job, CancellationToken cancellation)
    {
        using var timer = new PeriodicTimer(job.Interval);

        do
        {
            try
            {
                await job.ExecuteAsync(cancellation);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Background job '{Job}' failed", job.Name);
            }
        }
        while (await SafeWaitAsync(timer, cancellation));
    }

    private static async ValueTask<bool> SafeWaitAsync(PeriodicTimer timer, CancellationToken cancellation)
    {
        try
        {
            return await timer.WaitForNextTickAsync(cancellation);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// Stops every running job. The scheduler is disposed by the application and
    /// again by the container it is registered in, so this has to be repeatable.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        await _shutdown.CancelAsync();

        try
        {
            await Task.WhenAll(_running);
        }
        catch (Exception)
        {
            // jobs that did not stop cleanly must not hold up the shutdown
        }

        _shutdown.Dispose();
    }

    #endregion

}
