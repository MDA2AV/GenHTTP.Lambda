using System.Collections.Concurrent;
using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// Caps how many lambda requests a single client may send per second, so one
/// visitor cannot occupy the whole server.
/// </summary>
/// <remarks>
/// The window is a second rather than a longer stretch because the budget is
/// spent as fast as it is asked for. The same allowance over a minute lets a
/// client spend all of it at once and then wait out the rest of the minute,
/// which is the burst this exists to flatten.
/// </remarks>
public sealed class RateLimitConcern(IHandler content, LambdaOptions options) : IConcern
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

    public IHandler Content => content;

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var client = request.Client.Address;

        if (client != null && !Allow(client))
        {
            throw new ProviderException(ResponseStatus.TooManyRequests, $"Lambdas accept at most {options.RateLimit} requests per second and client.",
                response => response.Header("Retry-After", "1"));
        }

        return content.HandleAsync(request);
    }

    private bool Allow(IPAddress client)
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

public sealed class RateLimitConcernBuilder(LambdaOptions options) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new RateLimitConcern(content, options);
}
