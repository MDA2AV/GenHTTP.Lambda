using System.Collections.Concurrent;

namespace GenHTTP.Lambda.Services.QuietHours;

/// <summary>
/// The requests counted since they were last written down, by the hour of
/// the day they were made in: for the installation as a whole, and for each
/// lambda.
/// </summary>
/// <remarks>
/// Counted on every request, so without a lock: an increment of a slot of a
/// fixed array, which is all a request pays. A lambda's array is made on its
/// first request in this process and kept, so taking the counts never races
/// a request that is counting - it swaps each slot for nothing, and a request
/// that came in between is counted the next time round. What grows is a few
/// hundred bytes per lambda visited since the server came up, not per request.
/// </remarks>
public sealed class HourlyCounts
{
    private readonly long[] _installation = new long[QuietProfile.Hours];

    private readonly ConcurrentDictionary<long, long[]> _lambdas = [];

    #region Get-/Setters

    /// <summary>
    /// When counting began, which is when the platform was built: the hours
    /// of the day wholly after it are counted, requests or none.
    /// </summary>
    public DateTime Since { get; } = DateTime.UtcNow;

    #endregion

    #region Counting

    /// <summary>
    /// Counts a request the server answered.
    /// </summary>
    /// <param name="hour">The hour of the day in UTC it was answered in</param>
    public void CountInstallation(int hour) => Interlocked.Increment(ref _installation[hour]);

    /// <summary>
    /// Counts a request a lambda answered.
    /// </summary>
    /// <param name="hour">The hour of the day in UTC it was answered in</param>
    public void CountLambda(long lambdaId, int hour) => Interlocked.Increment(ref _lambdas.GetOrAdd(lambdaId, static _ => new long[QuietProfile.Hours])[hour]);

    #endregion

    #region Taking

    /// <summary>
    /// What was counted for the installation since it was last taken.
    /// </summary>
    public long[] TakeInstallation() => Take(_installation) ?? new long[QuietProfile.Hours];

    /// <summary>
    /// What was counted for each lambda since it was last taken - only the
    /// lambdas that were asked for something.
    /// </summary>
    public List<(long Lambda, long[] Counts)> TakeLambdas()
    {
        var taken = new List<(long, long[])>();

        foreach (var (lambda, slots) in _lambdas)
        {
            if (Take(slots) is { } counts)
            {
                taken.Add((lambda, counts));
            }
        }

        return taken;
    }

    /// <summary>
    /// Forgets a lambda that was removed, with what it counted since.
    /// </summary>
    public void Forget(long lambdaId) => _lambdas.TryRemove(lambdaId, out _);

    /// <returns>What the slots held, or nothing where they were all empty</returns>
    private static long[]? Take(long[] slots)
    {
        long[]? taken = null;

        for (var hour = 0; hour < slots.Length; hour++)
        {
            if (Volatile.Read(ref slots[hour]) == 0)
            {
                continue;
            }

            taken ??= new long[slots.Length];
            taken[hour] = Interlocked.Exchange(ref slots[hour], 0);
        }

        return taken;
    }

    #endregion

}
