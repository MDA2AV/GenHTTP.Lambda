using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// The tickets the jobs of the build agent are handed, by which what the
/// agent then saves is known to be the build agent's - and on which model and
/// effort.
/// </summary>
/// <remarks>
/// A ticket is issued as a job is handed to the runner, which passes it to
/// the agent's connection to the MCP endpoint as a header; every call of the
/// job carries it. It is random, so an agent of somebody's own cannot claim to
/// be the build agent, and what the platform records comes from what it
/// issued, never from what the runner or the agent says.
///
/// Kept in the platform's records rather than in the memory of the server
/// that issued it, so a ticket outlives a restart and is known to whichever
/// server the job's calls reach. Only its SHA-256 is kept: what the records
/// hold cannot be presented as a ticket. A ticket lasts a day - a job without
/// a clock runs for hours, not days - and the expired ones are removed by
/// <see cref="AgentTicketJob"/>. Never logged.
/// </remarks>
public sealed class AgentTickets(IDbContextFactory<LambdaDbContext> databases)
{

    /// <summary>
    /// How long a ticket is taken.
    /// </summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(1);

    #region Functionality

    /// <summary>
    /// A new ticket for a job on the given model and effort, kept before it
    /// is handed out.
    /// </summary>
    public async ValueTask<string> IssueAsync(AgentChoice choice, CancellationToken cancellation = default)
    {
        var ticket = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));

        await using var database = await databases.CreateDbContextAsync(cancellation);

        database.AgentTickets.Add(new AgentTicketEntity
        {
            Hash = HashOf(ticket),
            Model = choice.Model,
            Effort = choice.Effort,
            Expires = DateTime.UtcNow + Lifetime
        });

        await database.SaveChangesAsync(cancellation);

        return ticket;
    }

    /// <summary>
    /// The model and effort of the job a ticket was issued for, or nothing
    /// for one that was not issued here, or has expired.
    /// </summary>
    /// <remarks>
    /// Anything that cannot be a ticket is turned away before the records are
    /// asked, so a call without one costs nothing.
    /// </remarks>
    public async ValueTask<(AgentModel Model, AgentEffort Effort)?> ResolveAsync(string? ticket, CancellationToken cancellation = default)
    {
        if (!IsTicket(ticket))
        {
            return null;
        }

        var hash = HashOf(ticket!);

        var now = DateTime.UtcNow;

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var issued = await database.AgentTickets.AsNoTracking()
                                   .FirstOrDefaultAsync(t => t.Hash == hash && t.Expires > now, cancellation);

        return issued == null ? null : (issued.Model, issued.Effort);
    }

    /// <summary>
    /// Removes the tickets that expired, and says how many.
    /// </summary>
    public async ValueTask<int> ExpireAsync(DateTime now, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentTickets.Where(t => t.Expires <= now).ExecuteDeleteAsync(cancellation);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Whether the value has the shape of a ticket: 32 lower case hex digits.
    /// </summary>
    private static bool IsTicket(string? value)
    {
        if (value is not { Length: 32 })
        {
            return false;
        }

        foreach (var c in value)
        {
            if (c is not (>= '0' and <= '9' or >= 'a' and <= 'f'))
            {
                return false;
            }
        }

        return true;
    }

    private static string HashOf(string ticket) => Convert.ToHexStringLower(SHA256.HashData(Encoding.ASCII.GetBytes(ticket)));

    #endregion

}
