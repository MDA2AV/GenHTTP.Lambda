using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta.Model;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// The credentials a run's calls carry, by which the platform knows which
/// run a call belongs to - what it did, and what it saved, on which options.
/// </summary>
/// <remarks>
/// <para>
/// A credential is made as a run is recorded and handed to the agent with it
/// (<c>AgentAccess</c>); every call of the run carries it in a header. It is
/// random, so an agent of somebody's own cannot claim to be a run of the
/// installation's, and what the platform records comes from the run it
/// issued it for, never from what the agent says.
/// </para>
/// <para>
/// Kept with the run in the platform's records rather than in the memory of
/// the server that issued it, so it outlives a restart and is known to
/// whichever server a call reaches. Only its SHA-256 is kept: what the
/// records hold cannot be presented as one. It is taken while its run waits
/// or works, and for a few minutes after it ended - a call the agent made
/// just before is still the run's. Never logged.
/// </para>
/// <para>
/// The build agent's jobs from before runs were kept were handed tickets of
/// their own (<c>agent_tickets</c>); those are still taken for the day they
/// last, so a job under way while the installation was upgraded marks what
/// it saves, on the model and effort it ran on.
/// </para>
/// </remarks>
public sealed class AgentCredentials(IDbContextFactory<LambdaDbContext> databases)
{

    /// <summary>
    /// How long after its run ended a credential is still taken.
    /// </summary>
    public static readonly TimeSpan Grace = TimeSpan.FromMinutes(5);

    /// <summary>
    /// How long a ticket of the build agent's from before is taken.
    /// </summary>
    public static readonly TimeSpan LegacyLifetime = TimeSpan.FromDays(1);

    #region Functionality

    /// <summary>
    /// A new credential, and what the run it is for keeps of it.
    /// </summary>
    public static (string Credential, string Hash) Issue()
    {
        var credential = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));

        return (credential, HashOf(credential));
    }

    /// <summary>
    /// Who a call carrying the given credential is: the run it was issued
    /// for, on its options - or nothing for one this installation did not
    /// issue, or whose run is over.
    /// </summary>
    /// <remarks>
    /// Anything that cannot be a credential is turned away before the
    /// records are asked, so a call without one costs nothing.
    /// </remarks>
    public async ValueTask<VersionAuthor?> ResolveAsync(string? credential, CancellationToken cancellation = default)
    {
        if (!IsCredential(credential))
        {
            return null;
        }

        var hash = HashOf(credential!);

        var now = DateTime.UtcNow;

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var run = await database.AgentRuns.AsNoTracking()
                                .Where(r => r.Ticket == hash)
                                .Select(r => new { r.Id, r.Options, r.State, r.Ended })
                                .FirstOrDefaultAsync(cancellation);

        if (run != null)
        {
            var taken = run.State != AgentRunState.Ended || (run.Ended is { } ended && now - ended < Grace);

            return taken ? new VersionAuthor(VersionWriter.BuildAgent, StoredOptions.Read(run.Options), run.Id) : null;
        }

        var issued = await database.AgentTickets.AsNoTracking()
                                   .FirstOrDefaultAsync(t => t.Hash == hash && t.Expires > now, cancellation);

        return issued == null ? null : new VersionAuthor(VersionWriter.BuildAgent, AgentModelNames.Options(issued.Model, issued.Effort));
    }

    /// <summary>
    /// Removes the tickets of the build agent's from before that expired, and
    /// says how many.
    /// </summary>
    public async ValueTask<int> ExpireAsync(DateTime now, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentTickets.Where(t => t.Expires <= now).ExecuteDeleteAsync(cancellation);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Whether the value has the shape of a credential: 32 lower case hex digits.
    /// </summary>
    private static bool IsCredential(string? value)
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

    private static string HashOf(string credential) => Convert.ToHexStringLower(SHA256.HashData(Encoding.ASCII.GetBytes(credential)));

    #endregion

}
