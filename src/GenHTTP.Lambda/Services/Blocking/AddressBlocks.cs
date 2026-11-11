using System.Collections.Frozen;
using System.Net;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Blocking;

/// <summary>
/// The addresses the server refuses: written to the database, and held in
/// memory for the requests that are asked about them.
/// </summary>
/// <remarks>
/// Every request is asked about, a lambda's as well as the platform's, so the
/// question is answered from a frozen table that is swapped whole whenever a
/// block is made or let go of - never from the database, and with nothing to
/// take while no block is held at all.
///
/// What a block refused is counted in memory, since the server came up, so
/// the operator can tell whether one still does anything. The counts survive
/// the table being swapped, not a restart.
/// </remarks>
public sealed class AddressBlocks(IDbContextFactory<LambdaDbContext> databases, LambdaOptions options, ILogger<AddressBlocks> logger) : IAddressBlocks
{

    /// <summary>
    /// How long a reason may be.
    /// </summary>
    public const int MaxReason = 500;

    /// <summary>
    /// What is held, swapped whole.
    /// </summary>
    private volatile Held _held = Held.Empty;

    private readonly Lock _writing = new();

    #region Serving

    public bool Covers(IPAddress? address)
    {
        var held = _held;

        return held.Tallies.Count > 0 && address != null && NetworkAddress.TryKey(address, out var key) && held.Tallies.ContainsKey(key);
    }

    public void Refused(IPAddress address)
    {
        if (NetworkAddress.TryKey(address, out var key) && _held.Tallies.TryGetValue(key, out var tally))
        {
            tally.Count();
        }
    }

    #endregion

    #region Managing

    public IReadOnlyList<AddressBlock> List()
    {
        var held = _held;

        return [.. held.Rows.OrderByDescending(r => r.Created).ThenByDescending(r => r.Id).Select(r => Describe(r, held))];
    }

    public IReadOnlyList<AddressBlock> Block(IReadOnlyList<string> addresses, string? reason, string? publicKey)
    {
        var ranges = new List<IPNetwork>();

        foreach (var address in addresses)
        {
            var range = NetworkAddress.ReadBlockable(address, options.UnblockableNetworks);

            if (!ranges.Contains(range))
            {
                ranges.Add(range);
            }
        }

        var why = Tidy(reason);

        var names = ranges.Select(NetworkAddress.Describe).ToList();

        lock (_writing)
        {
            using var database = databases.CreateDbContext();

            var now = DateTime.UtcNow;

            var existing = database.AddressBlocks.Where(b => names.Contains(b.Address)).ToDictionary(b => b.Address, StringComparer.Ordinal);

            foreach (var name in names)
            {
                if (existing.TryGetValue(name, out var block))
                {
                    // blocked again, for this lambda: its time starts again
                    block.Created = now;
                    block.Reason = why;
                    block.PublicKey = publicKey;
                }
                else
                {
                    database.AddressBlocks.Add(new AddressBlockEntity { Address = name, Reason = why, PublicKey = publicKey, Created = now });
                }
            }

            database.SaveChanges();

            Reload(database);
        }

        var held = _held;

        return [.. held.Rows.Where(r => names.Contains(r.Address)).Select(r => Describe(r, held))];
    }

    public bool Remove(long id)
    {
        lock (_writing)
        {
            using var database = databases.CreateDbContext();

            var removed = database.AddressBlocks.Where(b => b.Id == id).ExecuteDelete() > 0;

            if (removed)
            {
                Reload(database);
            }

            return removed;
        }
    }

    public int Expire(DateTime now)
    {
        var before = now - IAddressBlocks.Kept;

        lock (_writing)
        {
            using var database = databases.CreateDbContext();

            var expired = database.AddressBlocks.Where(b => b.Created < before).ExecuteDelete();

            if (expired > 0)
            {
                Reload(database);

                logger.LogInformation("Expired address blocks count {Count}", expired);
            }

            return expired;
        }
    }

    /// <summary>
    /// Reads the blocks from the database, once before the first request.
    /// </summary>
    public void Reload()
    {
        lock (_writing)
        {
            using var database = databases.CreateDbContext();

            Reload(database);
        }
    }

    #endregion

    #region Helpers

    private void Reload(LambdaDbContext database)
    {
        var rows = database.AddressBlocks.AsNoTracking().ToList();

        var previous = _held;

        var tallies = new Dictionary<UInt128, Tally>();

        var kept = new List<AddressBlockEntity>();

        foreach (var row in rows)
        {
            // written by this class, so always readable - but a row somebody
            // put there by hand is skipped rather than taking the server down
            if (!NetworkAddress.TryRead(row.Address, out var range) || !NetworkAddress.TryKey(range.Value.BaseAddress, out var key))
            {
                logger.LogWarning("Skipped address block {Address}", row.Address);
                continue;
            }

            tallies[key] = previous.Tallies.GetValueOrDefault(key) ?? new Tally();

            kept.Add(row);
        }

        _held = new Held(tallies.ToFrozenDictionary(), kept);
    }

    private static AddressBlock Describe(AddressBlockEntity row, Held held)
    {
        var tally = NetworkAddress.TryRead(row.Address, out var range) && NetworkAddress.TryKey(range.Value.BaseAddress, out var key)
                  ? held.Tallies.GetValueOrDefault(key)
                  : null;

        return new AddressBlock(row.Id, row.Address, row.Reason, row.PublicKey, row.Created, row.Created + IAddressBlocks.Kept,
                                tally?.Refused ?? 0, tally?.Last);
    }

    private static string? Tidy(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return null;
        }

        var trimmed = reason.Trim();

        return trimmed.Length <= MaxReason ? trimmed : trimmed[..MaxReason];
    }

    /// <summary>
    /// The blocks as they are, by the number they are looked up by.
    /// </summary>
    private sealed record Held(FrozenDictionary<UInt128, Tally> Tallies, IReadOnlyList<AddressBlockEntity> Rows)
    {
        public static readonly Held Empty = new(FrozenDictionary<UInt128, Tally>.Empty, []);
    }

    /// <summary>
    /// What one block refused, since the server came up.
    /// </summary>
    private sealed class Tally
    {
        private long _refused;

        private long _last;

        public long Refused => Interlocked.Read(ref _refused);

        public DateTime? Last => Interlocked.Read(ref _last) is var ticks and > 0 ? new DateTime(ticks, DateTimeKind.Utc) : null;

        public void Count()
        {
            Interlocked.Increment(ref _refused);
            Interlocked.Exchange(ref _last, DateTime.UtcNow.Ticks);
        }
    }

    #endregion

}
