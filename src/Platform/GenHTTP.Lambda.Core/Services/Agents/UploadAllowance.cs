using System.Collections.Concurrent;
using System.Net;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Counts what each address uploads for messages to the agent in an hour.
/// </summary>
/// <remarks>
/// Anybody may upload, before they send anything, and the room for files that
/// wait to be sent is the installation's - so one address could fill it for
/// everybody. An hour, as long as an unsent file is kept; kept in memory like
/// the build allowance, so a restart forgives everybody, and each server of an
/// installation counts what it took.
/// </remarks>
public sealed class UploadAllowance
{

    /// <summary>What one address may upload in an hour.</summary>
    public const long PerHour = 100 * 1024 * 1024;

    /// <summary>How many addresses are remembered before the quiet ones are forgotten.</summary>
    private const int Remembered = 20_000;

    private readonly ConcurrentDictionary<IPAddress, Tally> _sent = [];

    /// <summary>
    /// Counts an upload against an address, and says whether it was allowed.
    /// An address the server cannot name is not counted.
    /// </summary>
    public bool Spend(IPAddress? caller, long bytes)
    {
        if (caller == null)
        {
            return true;
        }

        var now = DateTime.UtcNow;

        if (_sent.Count > Remembered)
        {
            foreach (var quiet in _sent.Where(t => now - t.Value.Since > AgentAttachments.Unclaimed).Select(t => t.Key).ToList())
            {
                _sent.TryRemove(quiet, out _);
            }
        }

        var tally = _sent.GetOrAdd(caller, _ => new Tally(now));

        lock (tally)
        {
            if (now - tally.Since > AgentAttachments.Unclaimed)
            {
                tally.Since = now;
                tally.Bytes = 0;
            }

            if (tally.Bytes + bytes > PerHour)
            {
                return false;
            }

            tally.Bytes += bytes;

            return true;
        }
    }

    private sealed class Tally(DateTime since)
    {

        public DateTime Since { get; set; } = since;

        public long Bytes { get; set; }

    }

}
