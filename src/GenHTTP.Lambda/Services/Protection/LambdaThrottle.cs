using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// How many lambda requests may run at the same time, across the whole server.
/// </summary>
/// <remarks>
/// A service rather than a field of the concern that enforces it, because a
/// lambda can be reached through more than one route - its path below
/// <c>/lambda/</c> and a domain of its own - and each route is a chain of its
/// own. A semaphore per chain would allow twice the configured concurrency.
/// </remarks>
public sealed class LambdaThrottle(LambdaOptions options) : IDisposable
{
    private readonly SemaphoreSlim _slots = new(options.MaxConcurrency, options.MaxConcurrency);

    /// <summary>
    /// How long a request may wait for a slot before it is turned away.
    /// </summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(5);

    #region Functionality

    /// <summary>
    /// Waits for a slot. False when none came free in time.
    /// </summary>
    public Task<bool> EnterAsync() => _slots.WaitAsync(Patience);

    /// <summary>
    /// Gives back a slot taken with <see cref="EnterAsync"/>.
    /// </summary>
    public void Leave() => _slots.Release();

    public void Dispose() => _slots.Dispose();

    #endregion

}
