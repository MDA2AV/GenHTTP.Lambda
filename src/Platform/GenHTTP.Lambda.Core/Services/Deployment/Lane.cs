namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// Something that is done one at a time in the whole process.
/// </summary>
internal sealed class Lane : IDisposable
{
    private readonly SemaphoreSlim _turns = new(1, 1);

    #region Functionality

    /// <summary>
    /// Waits for a turn.
    /// </summary>
    /// <param name="limit">
    /// How long the others wait for it at most, where code of a lambda runs in
    /// it: the next one has its turn then, while this one goes on
    /// </param>
    /// <returns>The turn, held until it is disposed of</returns>
    public async ValueTask<IDisposable> EnterAsync(TimeSpan? limit, CancellationToken cancellation)
    {
        await _turns.WaitAsync(cancellation);

        return new Turn(_turns, limit);
    }

    public void Dispose() => _turns.Dispose();

    private sealed class Turn : IDisposable
    {
        private readonly SemaphoreSlim _turns;

        private readonly Timer? _lapse;

        private int _over;

        public Turn(SemaphoreSlim turns, TimeSpan? limit)
        {
            _turns = turns;

            if (limit is { } most)
            {
                _lapse = new Timer(_ => End(), null, most, Timeout.InfiniteTimeSpan);
            }
        }

        public void Dispose()
        {
            _lapse?.Dispose();

            End();
        }

        private void End()
        {
            if (Interlocked.Exchange(ref _over, 1) != 0)
            {
                return;
            }

            try
            {
                _turns.Release();
            }
            catch (ObjectDisposedException)
            {
                // the process is done with it, and nobody waits any more
            }
        }
    }

    #endregion

}
