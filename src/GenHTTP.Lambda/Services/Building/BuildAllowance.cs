using System.Collections.Concurrent;
using System.Net;

using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// Counts the jobs each address asks the agent for in a day.
/// </summary>
/// <remarks>
/// A day rather than an hour, because what is being defended is somebody's
/// subscription rather than server load, and because a handful of builds
/// is more than enough to decide whether you like this. Builds and changes
/// count against the same allowance, since both spend the same subscription.
///
/// An address that is not known - a caller the server cannot name - is not
/// counted. The count is kept in memory, so a restart forgives everybody.
/// </remarks>
public sealed class BuildAllowance(ILimitsService limits)
{

    /// <summary>
    /// How many addresses are remembered before those that asked for nothing
    /// today are forgotten.
    /// </summary>
    private const int Remembered = 20_000;

    private readonly ConcurrentDictionary<IPAddress, Tally> _asked = [];

    /// <summary>How many jobs one address is allowed in a day, as the operator set it.</summary>
    public int PerDay => limits.Get().BuildsPerDay;

    /// <summary>
    /// Counts one job against an address, and says whether it was allowed.
    /// </summary>
    public bool Spend(IPAddress? caller)
    {
        if (caller == null)
        {
            return true;
        }

        var today = DateTime.UtcNow.Date;

        if (_asked.Count > Remembered)
        {
            Forget(today);
        }

        var tally = _asked.GetOrAdd(caller, _ => new Tally(today));

        lock (tally.Sync)
        {
            if (tally.Day != today)
            {
                tally.Day = today;
                tally.Count = 0;
            }

            return ++tally.Count <= PerDay;
        }
    }

    /// <summary>
    /// Gives back what <see cref="Spend"/> took, for a job the agent never
    /// took on: a full queue or an agent that is down is not something the
    /// caller should pay for.
    /// </summary>
    public void Refund(IPAddress? caller)
    {
        if (caller == null || !_asked.TryGetValue(caller, out var tally))
        {
            return;
        }

        lock (tally.Sync)
        {
            if (tally.Day == DateTime.UtcNow.Date && tally.Count > 0)
            {
                tally.Count--;
            }
        }
    }

    /// <summary>How many jobs an address has left today.</summary>
    public int Left(IPAddress? caller)
    {
        if (caller == null || !_asked.TryGetValue(caller, out var tally))
        {
            return PerDay;
        }

        lock (tally.Sync)
        {
            return tally.Day == DateTime.UtcNow.Date ? Math.Max(0, PerDay - tally.Count) : PerDay;
        }
    }

    private void Forget(DateTime today)
    {
        foreach (var (address, tally) in _asked)
        {
            if (tally.Day != today)
            {
                _asked.TryRemove(address, out _);
            }
        }
    }

    private sealed class Tally(DateTime day)
    {
        public readonly Lock Sync = new();

        public DateTime Day = day;

        public int Count;
    }

}
