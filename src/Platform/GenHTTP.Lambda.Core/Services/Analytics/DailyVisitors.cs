using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// Tells a visitor seen before today from a new one, without remembering who
/// either was.
/// </summary>
/// <remarks>
/// A visitor is their address and their browser, hashed with a salt made at
/// random for the day and the lambda - so the same person is the same hash all
/// day on one lambda, and a different one on another lambda or tomorrow. Only
/// the hashes of today and yesterday are held, in memory, and the salts with
/// them: nothing here is written anywhere, and once a day is over nothing can
/// tell whether a hash was anybody in particular.
///
/// So what is counted is visitors per day. A server that restarts forgets who
/// it saw, and counts somebody who comes back the same day once more; an
/// installation that keeps no addresses counts no visitors at all.
///
/// Used by the job that writes visits down, one at a time, never by a request.
/// </remarks>
public sealed class DailyVisitors
{

    /// <summary>
    /// How many visitors of a lambda are told apart in a day. Past that every
    /// one counts as new, rather than every hash being held.
    /// </summary>
    public const int Most = 200_000;

    private readonly Dictionary<(long Lambda, DateOnly Day), Seen> _days = [];

    #region Functionality

    /// <summary>
    /// Whether the visitor is new to the lambda on that day.
    /// </summary>
    /// <returns>Whether they are, or nothing where there is no address to tell them by</returns>
    public bool? IsNew(long lambda, DateOnly day, string? client, string? agent)
    {
        if (client == null)
        {
            return null;
        }

        if (!_days.TryGetValue((lambda, day), out var seen))
        {
            _days[(lambda, day)] = seen = new Seen(RandomNumberGenerator.GetBytes(32));
        }

        var hash = Hash(seen.Salt, client, agent);

        if (seen.Hashes.Count >= Most)
        {
            return !seen.Hashes.Contains(hash);
        }

        return seen.Hashes.Add(hash);
    }

    /// <summary>
    /// Forgets every day before yesterday, with its salts.
    /// </summary>
    /// <remarks>
    /// Yesterday is kept, because a visit made just before midnight may be
    /// written down just after it.
    /// </remarks>
    public void Prune(DateOnly today)
    {
        foreach (var key in _days.Keys.Where(k => k.Day < today.AddDays(-1)).ToList())
        {
            _days.Remove(key);
        }
    }

    private static ulong Hash(byte[] salt, string client, string? agent)
    {
        // only the address that was claimed: the proxy it came through says
        // nothing about who it was
        var cut = client.IndexOf(' ');

        var address = cut < 0 ? client : client[..cut];

        Span<byte> digest = stackalloc byte[32];

        HMACSHA256.HashData(salt, Encoding.UTF8.GetBytes(address + "\n" + agent), digest);

        return BinaryPrimitives.ReadUInt64LittleEndian(digest);
    }

    private sealed record Seen(byte[] Salt)
    {
        public HashSet<ulong> Hashes { get; } = [];
    }

    #endregion

}
