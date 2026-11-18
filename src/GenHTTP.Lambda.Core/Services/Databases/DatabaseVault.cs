using System.Collections.Concurrent;
using System.Data.Common;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// Where a running lambda connects to its own database, and what the other
/// services keep in step with a lambda through.
/// </summary>
/// <remarks>
/// Addressed by the ids of a lambda and a feature rather than by keys, like
/// the secret vault, because it is asked by the platform itself - the
/// compiled lambda, the features, the data service - once whoever called has
/// been checked. What an owner or an agent reads is <see cref="IDatabaseService"/>.
///
/// What is decided here is the platform's: whether a lambda has a database,
/// as its owner left it, and how large it may grow, as its tier allows - read
/// on every connection a lambda opens. What a database is - a file, a schema -
/// and what keeps the SQL sent over a connection inside it, is the database
/// integration's (<see cref="IDatabaseProvider"/>).
///
/// A lambda is handed a function to connect with rather than a connection
/// string, so it never learns where its database is, and the function reads
/// whether the database is on on every call.
/// </remarks>
public sealed class DatabaseVault(IDbContextFactory<LambdaDbContext> databases, IDatabaseProvider provider, ILimitsService limits, IWorkspaceVault workspaces)
    : IDatabaseVault
{

    /// <summary>
    /// How long a statement run by the platform - to show a table, to count
    /// its rows - may take before it is stopped.
    /// </summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(5);

    private readonly ConcurrentDictionary<DataScope, Snapshot> _snapshots = [];

    /// <summary>
    /// How often the database of a lambda was switched or moved to another
    /// tier, so a snapshot loaded while that happened is known to be old.
    /// </summary>
    private readonly ConcurrentDictionary<long, long> _generations = [];

    #region Connecting, from inside a lambda

    /// <summary>
    /// What a compiled lambda is handed to connect to its database with.
    /// </summary>
    /// <remarks>
    /// Each call opens a connection of its own, which the lambda disposes of
    /// once it is done: connections are pooled, so that is what a connection
    /// per request costs, and a connection is not safe to share between the
    /// requests a lambda serves at once.
    /// </remarks>
    public Func<DbConnection> ConnectorFor(long lambdaId, long? featureId)
        => () => Connect(new DataScope(lambdaId, featureId));

    /// <remarks>
    /// The room is asked for on every connection rather than kept in the
    /// snapshot, so a limit the operator lowers holds from the next connection
    /// on - and the room the workspace takes of the allowance both share.
    /// </remarks>
    private DbConnection Connect(DataScope scope)
    {
        var snapshot = Current(scope);

        if (!snapshot.Enabled)
        {
            throw new InvalidOperationException(DataKinds.DatabaseOff);
        }

        return provider.Connect(scope, RoomOf(scope.LambdaId, scope.FeatureId, snapshot.Tier));
    }

    private Snapshot Current(DataScope scope)
    {
        var generation = _generations.GetValueOrDefault(scope.LambdaId);

        if (_snapshots.TryGetValue(scope, out var known) && known.Generation == generation)
        {
            return known;
        }

        // taken with the generation read before it, so a switch landing while
        // it loads leaves it old, and the next connection loads again
        var loaded = Load(scope.LambdaId, generation);

        _snapshots[scope] = loaded;

        return loaded;
    }

    /// <summary>
    /// Reads whether the lambda has a database and how large it may grow,
    /// synchronously: a lambda connects with a plain call, from wherever it
    /// happens to be.
    /// </summary>
    private Snapshot Load(long lambdaId, long generation)
    {
        using var database = databases.CreateDbContext();

        var enabled = database.DataStores.AsNoTracking()
                              .Where(s => s.LambdaId == lambdaId && s.Kind == DataKinds.DatabaseId)
                              .Select(s => (bool?)s.Enabled)
                              .FirstOrDefault() ?? DataKinds.Database.Default;

        var tier = database.Lambdas.AsNoTracking()
                           .Where(l => l.Id == lambdaId)
                           .Select(l => (LambdaTier?)l.Tier)
                           .FirstOrDefault();

        // a lambda that is gone has no database, and the least room of any
        // tier until whatever still holds it lets go
        return new Snapshot(generation, enabled && tier != null, tier ?? LambdaTier.Sandbox);
    }

    #endregion

    #region Keeping them

    /// <summary>
    /// Makes the database of a lambda, empty, where there is none.
    /// </summary>
    /// <remarks>
    /// Made ahead of the first request rather than by it, so what the owner
    /// switched on is there to be looked at before anything used it.
    /// </remarks>
    public void Create(long lambdaId)
    {
        provider.Create(new DataScope(lambdaId));

        Invalidate(lambdaId);
    }

    /// <summary>
    /// Replaces a feature's copy of the database with a copy of the lambda's,
    /// or removes it where the lambda has none.
    /// </summary>
    public async ValueTask CopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default)
    {
        var source = new DataScope(lambdaId);
        var target = new DataScope(lambdaId, featureId);

        _snapshots.TryRemove(target, out _);

        if (Load(lambdaId, 0).Enabled)
        {
            // a whole database, a gigabyte in production (see Offload)
            await Offload.Run(() => provider.Copy(source, target), cancellation);
        }
        else
        {
            provider.Delete(target);
        }

        Invalidate(lambdaId);
    }

    /// <summary>
    /// Removes a feature's copy of the database.
    /// </summary>
    public void RemoveCopy(long lambdaId, long featureId)
    {
        var scope = new DataScope(lambdaId, featureId);

        _snapshots.TryRemove(scope, out _);

        provider.Delete(scope);
    }

    /// <summary>
    /// Deletes the database of a lambda, the copies of its features included
    /// - a database switched on again later is a new one - and lets go of
    /// what is remembered of them.
    /// </summary>
    public void Clear(long lambdaId)
    {
        Invalidate(lambdaId);

        foreach (var scope in _snapshots.Keys.Where(s => s.LambdaId == lambdaId).ToList())
        {
            _snapshots.TryRemove(scope, out _);
        }

        provider.DeleteAll(lambdaId);
    }

    /// <summary>
    /// Forgets what is held in memory for a lambda, so the next connection
    /// reads what is stored - after a switch, or a new tier.
    /// </summary>
    public void Invalidate(long lambdaId) => _generations.AddOrUpdate(lambdaId, 1, (_, was) => was + 1);

    #endregion

    #region Reading, for the platform

    /// <summary>
    /// A connection that only reads, to the database of a lambda or a
    /// feature's copy of it - or nothing where there is none.
    /// </summary>
    public DbConnection? OpenForReading(long lambdaId, long? featureId)
        => Load(lambdaId, 0).Enabled ? provider.Read(new DataScope(lambdaId, featureId), Patience) : null;

    /// <summary>
    /// Whether the lambda's database is there.
    /// </summary>
    public bool Exists(long lambdaId) => provider.Exists(new DataScope(lambdaId));

    /// <summary>
    /// The room the database takes.
    /// </summary>
    public long SizeOf(long lambdaId, long? featureId) => provider.SizeOf(new DataScope(lambdaId, featureId));

    /// <summary>
    /// How large the database of a lambda - or a feature's copy - may grow:
    /// the room of its data, less what its workspace takes of it.
    /// </summary>
    /// <remarks>
    /// Never below nothing. A database already larger than this stays as it
    /// is and is read as before; only what would make it grow is refused.
    /// </remarks>
    public long RoomOf(long lambdaId, long? featureId, LambdaTier tier) => Math.Max(0, limits.PolicyOf(tier).MaxDataBytes - workspaces.SizeOf(lambdaId, featureId));

    /// <summary>
    /// What the room of the data of a lambda is read with, its database and
    /// its workspace together - asked on every write to its workspace.
    /// </summary>
    /// <remarks>
    /// Read through the tier the lambda has now rather than compiled in, so a
    /// lambda moved to another tier, or one whose tier the operator gives more
    /// room, keeps running as it is and writes into the new room from the next
    /// write on. The tier is the one the connections read (see
    /// <see cref="Invalidate"/>), held in memory.
    /// </remarks>
    public Func<long> DataRoomFor(long lambdaId, long? featureId)
        => () => limits.PolicyOf(Current(new DataScope(lambdaId, featureId)).Tier).MaxDataBytes;

    /// <summary>
    /// Writes the database of a lambda into a SQLite file of its own, whole -
    /// what an export carries.
    /// </summary>
    public string? Export(long lambdaId) => Load(lambdaId, 0).Enabled ? provider.Export(new DataScope(lambdaId)) : null;

    #endregion

    /// <param name="Tier">The tier of its lambda, which decides how large it may grow</param>
    private sealed record Snapshot(long Generation, bool Enabled, LambdaTier Tier);

}
