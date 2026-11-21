using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// The tickets the jobs of the build agent are handed, by which what the
/// agent then saves is known to be the build agent's - and on which model and
/// effort.
/// </summary>
/// <remarks>
/// A ticket is issued as a job is handed to the runner, which passes it to
/// the agent's connection to the MCP endpoint as a header; every call of the
/// job carries it. It is random and kept nowhere but in memory, so an agent of
/// somebody's own cannot claim to be the build agent, and what the platform
/// records comes from what it issued, never from what the runner or the agent
/// says. A ticket lasts a day - a job without a clock runs for hours, not days
/// - and a restart forgets them all: what a job saves after one is recorded as
/// any agent's. At most <see cref="Most"/> are kept; past that the oldest go.
/// Never logged.
/// </remarks>
public sealed class AgentTickets
{
    private readonly ConcurrentDictionary<string, Issued> _issued = new(StringComparer.Ordinal);

    /// <summary>How long a ticket is honoured.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(1);

    /// <summary>How many tickets are kept at most.</summary>
    public const int Most = 10_000;

    private sealed record Issued(AgentChoice Choice, DateTime Expires);

    #region Functionality

    /// <summary>
    /// A ticket for a job on the given model and effort.
    /// </summary>
    public string Issue(AgentChoice choice)
    {
        var now = DateTime.UtcNow;

        if (_issued.Count >= Most)
        {
            Prune(now);
        }

        var ticket = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));

        _issued[ticket] = new Issued(choice, now + Lifetime);

        return ticket;
    }

    /// <summary>
    /// The job a ticket was issued for, or nothing where it is not one this
    /// platform issued, or no longer honoured.
    /// </summary>
    public AgentChoice? Resolve(string? ticket)
    {
        if (ticket is not { Length: 32 } || !_issued.TryGetValue(ticket, out var issued))
        {
            return null;
        }

        return issued.Expires > DateTime.UtcNow ? issued.Choice : null;
    }

    /// <summary>
    /// Forgets the tickets no longer honoured, and the oldest where that
    /// leaves too many.
    /// </summary>
    private void Prune(DateTime now)
    {
        foreach (var (ticket, issued) in _issued)
        {
            if (issued.Expires <= now)
            {
                _issued.TryRemove(ticket, out _);
            }
        }

        foreach (var (ticket, _) in _issued.OrderBy(t => t.Value.Expires).Take(Math.Max(0, _issued.Count - Most + 1)))
        {
            _issued.TryRemove(ticket, out _);
        }
    }

    #endregion

}
