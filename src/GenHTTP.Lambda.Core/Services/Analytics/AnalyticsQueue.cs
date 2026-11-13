using System.Collections.Concurrent;

using GenHTTP.Lambda.Services.Visits;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// The visits waiting to be counted.
/// </summary>
/// <remarks>
/// Filled by every request to a lambda whose owner counts visits, and emptied
/// by the job that writes them down every few minutes - so a request does no
/// more than put its visit on the end, and nothing is written to a database
/// while somebody waits for an answer.
///
/// Without a lock: adding is a compare and swap in the queue and an increment
/// of the count, and taking walks the queue from the front while requests keep
/// adding at the back, so neither waits for the other.
///
/// Bounded, because what fills it comes from outside: a flood of requests
/// between two writings would otherwise be held in memory in full. What does
/// not fit is counted as dropped and not counted otherwise; losing visits is
/// acceptable here, as it is when the server restarts with visits not yet
/// written.
/// </remarks>
public sealed class AnalyticsQueue
{

    /// <summary>
    /// How many visits are held at most.
    /// </summary>
    public const int Most = 250_000;

    private readonly ConcurrentQueue<LambdaVisit> _visits = new();

    private int _count;

    private long _dropped;

    #region Functionality

    /// <summary>
    /// Puts a visit on the end, or drops it when the queue is full.
    /// </summary>
    public void Add(LambdaVisit visit)
    {
        if (Interlocked.Increment(ref _count) > Most)
        {
            Interlocked.Decrement(ref _count);
            Interlocked.Increment(ref _dropped);
            return;
        }

        _visits.Enqueue(visit);
    }

    /// <summary>
    /// Takes every visit that is waiting, and how many were dropped since it
    /// was last asked.
    /// </summary>
    /// <remarks>
    /// Takes what was there when it started: a visit added while it walks the
    /// queue may make it in or wait for the next time, and never both.
    /// </remarks>
    public (List<LambdaVisit> Visits, long Dropped) Take()
    {
        var waiting = Volatile.Read(ref _count);

        var taken = new List<LambdaVisit>(waiting);

        while (taken.Count < waiting && _visits.TryDequeue(out var visit))
        {
            taken.Add(visit);
        }

        Interlocked.Add(ref _count, -taken.Count);

        return (taken, Interlocked.Exchange(ref _dropped, 0));
    }

    #endregion

}
