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
///
/// Beside the totals each lambda keeps two short rings of buckets - the last
/// hour by the minute and the last day by the quarter hour - so its owner can
/// see a trend and not just a count. They are allocated on the first request
/// a lambda answers and are a few kilobytes each, fixed: what grows with the
/// number of lambdas is how many have been called, not how often.
/// </remarks>
public sealed class LambdaTelemetry
{
    private readonly ConcurrentDictionary<long, Counters> _lambdas = [];

    /// <summary>
    /// How many distinct paths are counted per lambda before the rest are
    /// gathered under one row. Paths come from visitors, and a scanner trying
    /// ten thousand of them should not cost ten thousand rows.
    /// </summary>
    private const int MostPaths = 64;

    private const string OtherPaths = "(other)";

    #region Get-/Setters

    /// <summary>
    /// When counting started, which is when the server came up.
    /// </summary>
    public DateTime Started { get; } = DateTime.UtcNow;

    #endregion

    #region Functionality

    /// <summary>
    /// Records a request a lambda answered.
    /// </summary>
    /// <param name="id">The lambda that answered</param>
    /// <param name="publicKey">The key it was reached under</param>
    /// <param name="elapsed">How long its handler took</param>
    /// <param name="status">The status it answered with</param>
    /// <param name="bytes">What the response carried, where that is known</param>
    /// <param name="path">What was asked for, relative to the lambda</param>
    public void Record(long id, string publicKey, TimeSpan elapsed, int status, long bytes, string? path = null)
    {
        var counters = _lambdas.GetOrAdd(id, _ => new Counters());

        var now = DateTime.UtcNow;

        lock (counters)
        {
            counters.PublicKey = publicKey;
            counters.FirstSeen ??= now;
            counters.LastSeen = now;
            counters.Bytes += bytes;

            counters.Minutes.Add(now, elapsed, status, bytes);
            counters.Quarters.Add(now, elapsed, status, bytes);

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

            switch (status)
            {
                case >= 500:
                    counters.Failed++;
                    counters.ServerErrors++;
                    break;
                case >= 400:
                    counters.ClientErrors++;
                    break;
                case >= 300:
                    counters.Redirects++;
                    break;
                default:
                    counters.Successes++;
                    break;
            }

            if (path != null)
            {
                var key = counters.Paths.Count < MostPaths || counters.Paths.ContainsKey(path) ? path : OtherPaths;

                if (!counters.Paths.TryGetValue(key, out var tally))
                {
                    counters.Paths[key] = tally = new PathTally();
                }

                tally.Requests++;
                tally.Millis += elapsed.TotalMilliseconds;

                if (status >= 500)
                {
                    tally.Failed++;
                }
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
                described.Add(Totals(counters));
            }
        }

        described.Sort((a, b) => (b.Requests + b.Upgrades).CompareTo(a.Requests + a.Upgrades));

        return described;
    }

    /// <summary>
    /// What one lambda has been doing, with the rings read out.
    /// </summary>
    /// <remarks>
    /// A lambda nobody has called yet is described as well, with empty rings,
    /// so a page drawing it has the same shape to draw either way.
    /// </remarks>
    public LambdaTraffic Describe(long id)
    {
        var now = DateTime.UtcNow;

        if (!_lambdas.TryGetValue(id, out var counters))
        {
            return new LambdaTraffic(null, Ring.Empty(now, MinuteRing), Ring.Empty(now, QuarterRing),
                                     new StatusClasses(0, 0, 0, 0), [], Started);
        }

        lock (counters)
        {
            var paths = counters.Paths
                                .Select(p => new PathTraffic(p.Key, p.Value.Requests, p.Value.Failed,
                                                             p.Value.Requests > 0 ? Math.Round(p.Value.Millis / p.Value.Requests, 2) : 0))
                                .OrderByDescending(p => p.Requests)
                                .ToList();

            return new LambdaTraffic(
                Totals(counters),
                counters.Minutes.Read(now),
                counters.Quarters.Read(now),
                new StatusClasses(counters.Successes, counters.Redirects, counters.ClientErrors, counters.ServerErrors),
                paths,
                Started
            );
        }
    }

    private static LambdaActivity Totals(Counters counters) => new(
        counters.PublicKey,
        counters.Requests,
        counters.Failed,
        counters.Upgrades,
        counters.Requests > 0 ? Math.Round(counters.TotalMillis / counters.Requests, 2) : 0,
        Math.Round(counters.SlowestMillis, 2),
        counters.Bytes,
        counters.FirstSeen,
        counters.LastSeen
    );

    private static readonly (int Length, TimeSpan Width) MinuteRing = (60, TimeSpan.FromMinutes(1));

    private static readonly (int Length, TimeSpan Width) QuarterRing = (96, TimeSpan.FromMinutes(15));

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

        internal long Successes;
        internal long Redirects;
        internal long ClientErrors;
        internal long ServerErrors;

        internal readonly Ring Minutes = new(MinuteRing.Length, MinuteRing.Width);
        internal readonly Ring Quarters = new(QuarterRing.Length, QuarterRing.Width);

        internal readonly Dictionary<string, PathTally> Paths = new(StringComparer.Ordinal);
    }

    private sealed class PathTally
    {
        internal long Requests;
        internal long Failed;
        internal double Millis;
    }

    /// <summary>
    /// A fixed number of intervals, each overwritten when the clock comes
    /// round to it again.
    /// </summary>
    /// <remarks>
    /// A bucket knows which interval it holds, so one left over from the last
    /// time round reads as empty rather than as last hour's traffic.
    /// </remarks>
    private sealed class Ring(int length, TimeSpan width)
    {
        private readonly Bucket[] _buckets = new Bucket[length];

        private readonly long _width = width.Ticks;

        internal void Add(DateTime at, TimeSpan elapsed, int status, long bytes)
        {
            var stamp = at.Ticks / _width;

            ref var bucket = ref _buckets[stamp % length];

            if (bucket.Stamp != stamp)
            {
                bucket = new Bucket { Stamp = stamp };
            }

            bucket.Bytes += bytes;

            if (status == 101)
            {
                bucket.Upgrades++;
                return;
            }

            bucket.Requests++;
            bucket.Millis += elapsed.TotalMilliseconds;

            if (status >= 500)
            {
                bucket.Failed++;
            }
            else if (status >= 400)
            {
                bucket.Rejected++;
            }
        }

        internal IReadOnlyList<TrafficPoint> Read(DateTime now)
        {
            var current = now.Ticks / _width;

            var points = new List<TrafficPoint>(length);

            for (var stamp = current - length + 1; stamp <= current; stamp++)
            {
                var bucket = _buckets[stamp % length];

                var at = new DateTime(stamp * _width, DateTimeKind.Utc);

                points.Add(bucket.Stamp == stamp
                    ? new TrafficPoint(at, bucket.Requests, bucket.Failed, bucket.Rejected, bucket.Upgrades,
                                       bucket.Requests > 0 ? Math.Round(bucket.Millis / bucket.Requests, 2) : 0, bucket.Bytes)
                    : new TrafficPoint(at, 0, 0, 0, 0, 0, 0));
            }

            return points;
        }

        internal static IReadOnlyList<TrafficPoint> Empty(DateTime now, (int Length, TimeSpan Width) shape)
            => new Ring(shape.Length, shape.Width).Read(now);

        private struct Bucket
        {
            internal long Stamp;
            internal int Requests;
            internal int Failed;
            internal int Rejected;
            internal int Upgrades;
            internal double Millis;
            internal long Bytes;
        }
    }

    #endregion

}
