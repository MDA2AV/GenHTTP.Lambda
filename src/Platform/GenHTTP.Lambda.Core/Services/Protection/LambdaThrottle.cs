using System.Diagnostics;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// How many lambda requests of a platform may run at the same time.
/// </summary>
/// <remarks>
/// A service rather than a field of the concern that enforces it, because a
/// lambda can be reached through more than one route - its subdomain of the
/// hosting domain, a domain of its own, a preview - and each route is a chain of its
/// own. A semaphore per chain would allow twice the configured concurrency.
///
/// A platform that shares its process passes a second gate, the one all of
/// them share (<see cref="ProcessThrottle"/>) - after its own, so requests to
/// one that is busy wait among themselves and not in front of the others.
/// </remarks>
public sealed class LambdaThrottle(LambdaOptions options, ProcessThrottle? process = null) : IDisposable
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
    public Task<bool> EnterAsync() => process == null ? _slots.WaitAsync(Patience) : EnterBothAsync(process);

    private async Task<bool> EnterBothAsync(ProcessThrottle shared)
    {
        var started = Stopwatch.GetTimestamp();

        if (!await _slots.WaitAsync(Patience))
        {
            return false;
        }

        // what is left of the same patience, so nobody waits twice as long
        var left = Patience - Stopwatch.GetElapsedTime(started);

        if (await shared.Slots.WaitAsync(left > TimeSpan.Zero ? left : TimeSpan.Zero))
        {
            return true;
        }

        _slots.Release();

        return false;
    }

    /// <summary>
    /// Gives back a slot taken with <see cref="EnterAsync"/>.
    /// </summary>
    public void Leave()
    {
        process?.Slots.Release();

        _slots.Release();
    }

    public void Dispose() => _slots.Dispose();

    #endregion

}
