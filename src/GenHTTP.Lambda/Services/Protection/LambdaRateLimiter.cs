using System.Collections.Concurrent;
using System.Net;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// Keeps count of how many lambda requests each client sent within the last
/// second, so one visitor cannot occupy the whole server.
/// </summary>
/// <remarks>
/// The window is a second rather than a longer stretch because the budget is
/// spent as fast as it is asked for. The same allowance over a minute lets a
/// client spend all of it at once and then wait out the rest of the minute,
/// which is the burst this exists to flatten.
///
/// One budget per client across every route a lambda can be reached through.
/// Held here rather than in the concern that enforces it: a lambda answering
/// at a domain of its own as well as at its path is served by two chains, and
/// buckets per chain would give a client its allowance twice.
/// </remarks>
public sealed class LambdaRateLimiter(LambdaOptions options)
{
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(1);

    /// <summary>
    /// How long a client has to be quiet before its bucket is forgotten.
    /// </summary>
    /// <remarks>
    /// Much longer than the window on purpose. Sweeping a dictionary every
    /// second to reclaim a handful of small objects costs more than holding
    /// them, and nothing about the cap depends on how soon they go.
    /// </remarks>
    private static readonly TimeSpan Idle = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<IPAddress, Bucket> _clients = [];

    private long _cleaned = Environment.TickCount64;

    #region Get-/Setters

    /// <summary>
    /// How many requests a client may send per second.
    /// </summary>
    public int Limit => options.RateLimit;

    #endregion

    #region Functionality

    /// <summary>
    /// Counts a request of the client, and says whether it is within budget.
    /// </summary>
    public bool Allow(IPAddress client)
    {
        var now = DateTime.UtcNow;

        Cleanup(now);

        var bucket = _clients.GetOrAdd(client, _ => new Bucket(now));

        lock (bucket)
        {
            if (now - bucket.Started > Window)
            {
                bucket.Started = now;
                bucket.Count = 0;
            }

            return ++bucket.Count <= options.RateLimit;
        }
    }

    /// <summary>
    /// Drops the clients that have not been seen for a while, at most once a minute.
    /// </summary>
    private void Cleanup(DateTime now)
    {
        var last = Interlocked.Read(ref _cleaned);

        if (Environment.TickCount64 - last < (long)Idle.TotalMilliseconds)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _cleaned, Environment.TickCount64, last) != last)
        {
            return;
        }

        foreach (var (address, bucket) in _clients)
        {
            if (now - bucket.Started > Idle)
            {
                _clients.TryRemove(address, out _);
            }
        }
    }

    private sealed class Bucket(DateTime started)
    {
        public DateTime Started { get; set; } = started;

        public int Count { get; set; }
    }

    #endregion

}
