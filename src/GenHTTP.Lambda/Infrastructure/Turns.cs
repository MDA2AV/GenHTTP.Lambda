namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// Takes turns over what is filed under an id, one id at a time.
/// </summary>
/// <remarks>
/// Striped rather than one lock per id, so it does not grow with what it
/// guards; two ids sharing a stripe merely wait for each other.
///
/// Plain locks, held only while a change is read and written - never across
/// an await. A lock is held by a thread, and a reactor that awaited under one
/// would hand it to whatever request it resumed next. What takes seconds -
/// compiling, copying - is done first, and the turn is taken to write down
/// what it came to.
/// </remarks>
public sealed class Turns
{
    private readonly Lock[] _stripes = [.. Enumerable.Range(0, 64).Select(_ => new Lock())];

    /// <summary>
    /// Waits for the turn of an id.
    /// </summary>
    /// <returns>The turn, held until it is disposed of</returns>
    public IDisposable Take(long id) => new Turn(_stripes[(int)((ulong)id % (ulong)_stripes.Length)]);

    /// <summary>
    /// A stripe taken, given back once.
    /// </summary>
    private sealed class Turn : IDisposable
    {
        private readonly Lock _stripe;

        private int _released;

        public Turn(Lock stripe)
        {
            _stripe = stripe;
            _stripe.Enter();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                _stripe.Exit();
            }
        }
    }

}
