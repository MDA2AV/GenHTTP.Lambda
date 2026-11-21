namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// Takes turns over what is filed under an id, one id at a time.
/// </summary>
/// <remarks>
/// Striped rather than one semaphore per id, so it does not grow with what it
/// guards; two ids sharing a stripe merely wait for each other.
///
/// Semaphores, waited for asynchronously, because what a turn encloses
/// awaits the platform's records: a lock is held by a thread, and a reactor
/// that awaited under one would hand it to whatever request it resumed next.
/// A turn nobody holds is taken at once, without leaving the thread. Held
/// only while a change is read and written - what takes seconds, compiling or
/// copying, is done first, and the turn is taken to write down what it came
/// to.
///
/// Not re-entrant, unlike a lock: whatever holds a turn never asks for
/// another of the same instance, which might be its own stripe. Turns of
/// different instances nest in one order only - a feature's before its
/// lambda's.
/// </remarks>
public sealed class Turns
{
    private readonly SemaphoreSlim[] _stripes = [.. Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1))];

    /// <summary>
    /// Waits for the turn of an id.
    /// </summary>
    /// <remarks>
    /// Waited for without a cancellation token: a turn is held for a moment,
    /// and what waits for one is often what has to be written down once
    /// begun - what a deployment came to, a merge.
    /// </remarks>
    /// <returns>The turn, held until it is disposed of</returns>
    public async ValueTask<IDisposable> TakeAsync(long id)
    {
        var stripe = _stripes[(int)((ulong)id % (ulong)_stripes.Length)];

        await stripe.WaitAsync();

        return new Turn(stripe);
    }

    /// <summary>
    /// A stripe taken, given back once.
    /// </summary>
    private sealed class Turn(SemaphoreSlim stripe) : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                stripe.Release();
            }
        }
    }

}
