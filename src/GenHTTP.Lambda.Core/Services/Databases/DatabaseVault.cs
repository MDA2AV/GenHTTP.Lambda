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
    /// requests a lambda serves at once. The plain call goes through
    /// <see cref="Synchronous"/>, which waits for nothing where the database
    /// answers at once.
    /// </remarks>
    public DatabaseConnector ConnectorFor(long lambdaId, long? featureId)
    {
        var scope = new DataScope(lambdaId, featureId);

        return new DatabaseConnector(() => Synchronous.Run(() => ConnectAsync(scope, CancellationToken.None)),
                                     cancellation => ConnectAsync(scope, cancellation));
    }

    /// <remarks>
    /// The room is asked for on every connection rather than kept in the
    /// snapshot, so a limit the operator lowers holds from the next connection
    /// on - and the room the workspace takes of the allowance both share.
    /// </remarks>
    private async ValueTask<DbConnection> ConnectAsync(DataScope scope, CancellationToken cancellation)
    {
        var snapshot = await CurrentAsync(scope);

        if (!snapshot.Enabled)
        {
            throw new InvalidOperationException(DataKinds.DatabaseOff);
        }

        return await provider.ConnectAsync(scope, await RoomOfAsync(scope.LambdaId, scope.FeatureId, snapshot.Tier, cancellation), cancellation);
    }

    /// <remarks>
    /// What is held is answered with a task that has completed; only a
    /// snapshot that has to be read again is awaited.
    /// </remarks>
    private ValueTask<Snapshot> CurrentAsync(DataScope scope)
    {
        var generation = _generations.GetValueOrDefault(scope.LambdaId);

        if (_snapshots.TryGetValue(scope, out var known) && known.Generation == generation)
        {
            return ValueTask.FromResult(known);
        }

        return ReloadAsync(scope, generation);
    }

    /// <summary>
    /// Reads the snapshot again and keeps it, taken with the generation read
    /// before it - so a switch landing while it loads leaves it old, and the
    /// next connection loads again.
    /// </summary>
    private async ValueTask<Snapshot> ReloadAsync(DataScope scope, long generation)
    {
        var loaded = await LoadAsync(scope.LambdaId, generation);

        _snapshots[scope] = loaded;

        return loaded;
    }

    /// <summary>
    /// Reads whether the lambda has a database and how large it may grow, from
    /// the platform's own records.
    /// </summary>
    /// <remarks>
    /// Without a cancellation token: what is read is kept for every
    /// connection after, not only for the one asking.
    /// </remarks>
    private async ValueTask<Snapshot> LoadAsync(long lambdaId, long generation)
    {
        await using var database = await databases.CreateDbContextAsync();

        var enabled = await database.DataStores.AsNoTracking()
                                    .Where(s => s.LambdaId == lambdaId && s.Kind == DataKinds.DatabaseId)
                                    .Select(s => (bool?)s.Enabled)
                                    .FirstOrDefaultAsync() ?? DataKinds.Database.Default;

        var tier = await database.Lambdas.AsNoTracking()
                                 .Where(l => l.Id == lambdaId)
                                 .Select(l => (LambdaTier?)l.Tier)
                                 .FirstOrDefaultAsync();

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
    public async ValueTask CreateAsync(long lambdaId, CancellationToken cancellation = default)
    {
        await provider.CreateAsync(new DataScope(lambdaId), cancellation);

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

        if ((await LoadAsync(lambdaId, 0)).Enabled)
        {
            // a whole database, a gigabyte in production, which a provider
            // that answers at once copies on the thread it is asked on (see
            // Offload)
            await Offload.Run(async () => await provider.CopyAsync(source, target, cancellation), cancellation);
        }
        else
        {
            await provider.DeleteAsync(target, cancellation);
        }

        Invalidate(lambdaId);
    }

    /// <summary>
    /// Removes a feature's copy of the database.
    /// </summary>
    public async ValueTask RemoveCopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default)
    {
        var scope = new DataScope(lambdaId, featureId);

        _snapshots.TryRemove(scope, out _);

        await provider.DeleteAsync(scope, cancellation);
    }

    /// <summary>
    /// Deletes the database of a lambda, the copies of its features included
    /// - a database switched on again later is a new one - and lets go of
    /// what is remembered of them.
    /// </summary>
    public async ValueTask ClearAsync(long lambdaId, CancellationToken cancellation = default)
    {
        Invalidate(lambdaId);

        foreach (var scope in _snapshots.Keys.Where(s => s.LambdaId == lambdaId).ToList())
        {
            _snapshots.TryRemove(scope, out _);
        }

        await provider.DeleteAllAsync(lambdaId, cancellation);
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
    public async ValueTask<DbConnection?> OpenForReadingAsync(long lambdaId, long? featureId, CancellationToken cancellation = default)
        => (await LoadAsync(lambdaId, 0)).Enabled ? await provider.ReadAsync(new DataScope(lambdaId, featureId), Patience, cancellation) : null;

    /// <summary>
    /// Whether the lambda's database is there.
    /// </summary>
    public ValueTask<bool> ExistsAsync(long lambdaId, CancellationToken cancellation = default) => provider.ExistsAsync(new DataScope(lambdaId), cancellation);

    /// <summary>
    /// The room the database takes.
    /// </summary>
    public ValueTask<long> SizeOfAsync(long lambdaId, long? featureId, CancellationToken cancellation = default)
        => provider.SizeOfAsync(new DataScope(lambdaId, featureId), cancellation);

    /// <summary>
    /// How large the database of a lambda - or a feature's copy - may grow:
    /// the room of its data, less what its workspace takes of it.
    /// </summary>
    /// <remarks>
    /// Never below nothing. A database already larger than this stays as it
    /// is and is read as before; only what would make it grow is refused.
    /// </remarks>
    public async ValueTask<long> RoomOfAsync(long lambdaId, long? featureId, LambdaTier tier, CancellationToken cancellation = default)
        => Math.Max(0, limits.PolicyOf(tier).MaxDataBytes - await workspaces.SizeOfAsync(lambdaId, featureId, cancellation));

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
    public Func<CancellationToken, ValueTask<long>> DataRoomFor(long lambdaId, long? featureId)
    {
        var scope = new DataScope(lambdaId, featureId);

        return async _ => limits.PolicyOf((await CurrentAsync(scope)).Tier).MaxDataBytes;
    }

    /// <summary>
    /// Writes the database of a lambda into a SQLite file of its own, whole -
    /// what an export carries.
    /// </summary>
    /// <remarks>
    /// A whole database, which a provider that answers at once writes on the
    /// thread it is asked on - so asked away from the request's (see
    /// <see cref="Offload"/>).
    /// </remarks>
    public async ValueTask<string?> ExportAsync(long lambdaId, CancellationToken cancellation = default)
        => (await LoadAsync(lambdaId, 0)).Enabled ? await Offload.Run(() => provider.ExportAsync(new DataScope(lambdaId), cancellation), cancellation) : null;

    #endregion

    /// <param name="Tier">The tier of its lambda, which decides how large it may grow</param>
    private sealed record Snapshot(long Generation, bool Enabled, LambdaTier Tier);

}
