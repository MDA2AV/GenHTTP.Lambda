using System.Collections.Concurrent;

namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Counts what each lambda does, so an owner can tell a lambda nobody visits
/// from one that is busy, and a slow one from a broken one.
/// </summary>
/// <remarks>
/// Held in memory and keyed by the identity of the lambda rather than by its
/// public key, because a key can be changed and the history should follow the
/// lambda rather than the name. The key is carried along for display and
/// updated whenever a request arrives under a new one.
/// </remarks>
public sealed class LambdaTelemetry
{
    private readonly ConcurrentDictionary<long, Counters> _lambdas = [];

    #region Functionality

    /// <summary>
    /// Records a request a lambda answered.
    /// </summary>
    /// <param name="id">The lambda that answered</param>
    /// <param name="publicKey">The key it was reached under</param>
    /// <param name="elapsed">How long its handler took</param>
    /// <param name="status">The status it answered with</param>
    /// <param name="bytes">What the response carried, where that is known</param>
    public void Record(long id, string publicKey, TimeSpan elapsed, int status, long bytes)
    {
        var counters = _lambdas.GetOrAdd(id, _ => new Counters());

        var now = DateTime.UtcNow;

        lock (counters)
        {
            counters.PublicKey = publicKey;
            counters.FirstSeen ??= now;
            counters.LastSeen = now;
            counters.Bytes += bytes;

            if (status == 101)
            {
                counters.Upgrades++;

                // the handler returned at the handshake; what follows is the
                // connection, and timing it as a request would say nothing
                return;
            }

            counters.Requests++;
            counters.TotalMillis += elapsed.TotalMilliseconds;

            if (elapsed.TotalMilliseconds > counters.SlowestMillis)
            {
                counters.SlowestMillis = elapsed.TotalMilliseconds;
            }

            if (status >= 500)
            {
                counters.Failed++;
            }
        }
    }

    /// <summary>
    /// Forgets a lambda, called when one is deleted.
    /// </summary>
    public void Evict(long id) => _lambdas.TryRemove(id, out _);

    /// <summary>
    /// What every lambda seen since the server came up has been doing, busiest first.
    /// </summary>
    public IReadOnlyList<LambdaActivity> Describe()
    {
        var described = new List<LambdaActivity>(_lambdas.Count);

        foreach (var (_, counters) in _lambdas)
        {
            lock (counters)
            {
                described.Add(new LambdaActivity(
                    counters.PublicKey,
                    counters.Requests,
                    counters.Failed,
                    counters.Upgrades,
                    counters.Requests > 0 ? Math.Round(counters.TotalMillis / counters.Requests, 2) : 0,
                    Math.Round(counters.SlowestMillis, 2),
                    counters.Bytes,
                    counters.FirstSeen,
                    counters.LastSeen
                ));
            }
        }

        described.Sort((a, b) => (b.Requests + b.Upgrades).CompareTo(a.Requests + a.Upgrades));

        return described;
    }

    private sealed class Counters
    {
        internal string PublicKey = string.Empty;
        internal long Requests;
        internal long Failed;
        internal long Upgrades;
        internal double TotalMillis;
        internal double SlowestMillis;
        internal long Bytes;
        internal DateTime? FirstSeen;
        internal DateTime LastSeen;
    }

    #endregion

}
