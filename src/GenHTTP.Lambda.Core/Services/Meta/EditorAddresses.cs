using System.Collections.Concurrent;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Blocking;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Hosting;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Writes down where a lambda was changed from, so the operator who blocks
/// it can block that as well.
/// </summary>
/// <remarks>
/// Noted wherever a lambda is changed - created, saved, put online or taken
/// offline (<see cref="LambdaHistory.Record"/>), and whatever asks for a
/// lambda its owner may change: its drafts, its data, its files, the agent
/// (<see cref="MetaService.RequireEditable"/>) - with the address of the
/// request being served. Through whichever door: the editor, the API, MCP,
/// git. A change the build agent makes comes from the installation's own
/// network and is not noted, so its address never becomes one to block.
///
/// Only where the server keeps addresses at all (LAMBDA_LOG_CLIENT_ADDRESS),
/// once an hour per lambda and address, and for <see cref="Kept"/> after an
/// address was last seen - by then it is likely somebody else's.
/// </remarks>
public sealed class EditorAddresses(IDbContextFactory<LambdaDbContext> databases, LambdaOptions options)
{

    /// <summary>
    /// How long an address is kept after a lambda was last changed from it.
    /// </summary>
    public static readonly TimeSpan Kept = TimeSpan.FromDays(30);

    /// <summary>
    /// How often an address seen again is written down again.
    /// </summary>
    private static readonly TimeSpan Refresh = TimeSpan.FromHours(1);

    /// <summary>
    /// How many lambdas and addresses are remembered as written, before the
    /// memory is let go of and they are written again.
    /// </summary>
    private const int Remembered = 10_000;

    private readonly ConcurrentDictionary<(long Lambda, string Address), DateTime> _written = new();

    #region Functionality

    /// <summary>
    /// Notes the address of the request being served against the lambda.
    /// </summary>
    /// <remarks>
    /// In a context of its own, so a note that cannot be written - the lambda
    /// deleted at that moment - never fails the change it is about.
    /// </remarks>
    public void Note(long lambdaId)
    {
        if (Caller.Ambient is not { Peer: { } peer } caller || NetworkAddress.IsOwn(peer, options.UnblockableNetworks) || ByTheOperator(caller))
        {
            return;
        }

        var address = NetworkAddress.Normalize(peer).ToString();

        var now = DateTime.UtcNow;

        if (!Due((lambdaId, address), now))
        {
            return;
        }

        using var database = databases.CreateDbContext();

        var row = database.EditorAddresses.FirstOrDefault(e => e.LambdaId == lambdaId && e.Address == address);

        if (row == null)
        {
            database.EditorAddresses.Add(new EditorAddressEntity { LambdaId = lambdaId, Address = address, FirstSeen = now, LastSeen = now });
        }
        else
        {
            row.LastSeen = now;
        }

        try
        {
            database.SaveChanges();
        }
        catch (DbUpdateException)
        {
            // the lambda went in the meantime; nothing to note it against
        }
    }

    /// <summary>
    /// Where the lambda was changed from, most recently first.
    /// </summary>
    public IReadOnlyList<EditorAddress> Of(LambdaDbContext database, long lambdaId)
        => database.EditorAddresses.AsNoTracking()
                   .Where(e => e.LambdaId == lambdaId)
                   .OrderByDescending(e => e.LastSeen)
                   .Select(e => new EditorAddress(e.Address, e.FirstSeen, e.LastSeen))
                   .ToList();

    /// <summary>
    /// Forgets the addresses not seen for <see cref="Kept"/>.
    /// </summary>
    public int Prune(LambdaDbContext database, DateTime now)
    {
        var before = now - Kept;

        return database.EditorAddresses.Where(e => e.LastSeen < before).ExecuteDelete();
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Whether the change is the operator's, made in the panel - taking a
    /// lambda offline, deploying it, blocking it.
    /// </summary>
    /// <remarks>
    /// Their address is not where the lambda came from, and offering it to
    /// be blocked with it would be offering to lock them out of the platform.
    /// The panel's API is behind the token, so a request there is theirs.
    /// </remarks>
    private static bool ByTheOperator(CallerInfo caller)
        => caller.Domain == null && caller.Path.StartsWith(PlatformRoutes.ApiBase + "/admin/", StringComparison.Ordinal);

    /// <summary>
    /// Whether this lambda and address are to be written now - which only
    /// one request at a time is told.
    /// </summary>
    private bool Due((long, string) key, DateTime now)
    {
        if (_written.Count > Remembered)
        {
            _written.Clear();
        }

        while (true)
        {
            if (_written.TryGetValue(key, out var last))
            {
                if (now - last < Refresh)
                {
                    return false;
                }

                if (_written.TryUpdate(key, now, last))
                {
                    return true;
                }
            }
            else if (_written.TryAdd(key, now))
            {
                return true;
            }
        }
    }

    #endregion

}
