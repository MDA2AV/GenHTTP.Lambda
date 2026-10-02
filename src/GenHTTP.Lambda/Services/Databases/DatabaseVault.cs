using System.Collections.Concurrent;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Storage;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// Where the databases of the lambdas are kept, and where a running lambda
/// connects to its own.
/// </summary>
/// <remarks>
/// Addressed by the ids of a lambda and a feature rather than by keys, like
/// the secret vault, because it is asked by the platform itself - the
/// compiled lambda, the features, the data service - once whoever called has
/// been checked. What an owner or an agent reads is <see cref="IDatabaseService"/>.
///
/// A database is a SQLite file of its own per lambda, and one per feature
/// that works on a copy. It is not encrypted: SQLite cannot, short of
/// replacing the SQLite of the whole process with a build that can, and a key
/// kept beside the files would protect them from nobody who has the data
/// volume - see CLAUDE.md.
///
/// A lambda is handed a function to connect with rather than a connection
/// string, so it never learns where its database is, and the function reads
/// whether the database is on, as the owner left it, on every call. The
/// connection it gets is watched by <see cref="ConnectionGuard"/>, which is
/// what keeps the SQL sent over it inside that one file.
/// </remarks>
public sealed class DatabaseVault(IDbContextFactory<LambdaDbContext> databases, IStorageService storage, LambdaOptions options,
                                  ILogger<DatabaseVault> logger)
{

    /// <summary>
    /// How long a statement run by the platform - to show a table, to count
    /// its rows - may take before it is stopped.
    /// </summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The page size SQLite writes, which turns the quota into pages.
    /// </summary>
    private const int PageSize = 4096;

    private readonly ConcurrentDictionary<Scope, Snapshot> _snapshots = [];

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
    public Func<SqliteConnection> ConnectorFor(long lambdaId, long? featureId)
        => () => Connect(new Scope(lambdaId, featureId));

    private SqliteConnection Connect(Scope scope)
    {
        var snapshot = Current(scope);

        if (!snapshot.Enabled)
        {
            throw new InvalidOperationException(DataKinds.DatabaseOff);
        }

        var connection = new SqliteConnection(ConnectionString(storage.GetDatabase(scope.LambdaId, scope.FeatureId), false));

        ConnectionGuard.Watch(connection, snapshot.Quota / PageSize);

        try
        {
            connection.Open();
        }
        catch
        {
            connection.Dispose();
            throw;
        }

        return connection;
    }

    private Snapshot Current(Scope scope)
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

        return new Snapshot(generation, enabled && tier != null, options.DatabaseOf(tier ?? LambdaTier.Free));
    }

    #endregion

    #region Keeping them

    /// <summary>
    /// Makes the database of a lambda: an empty file, where there is none.
    /// </summary>
    /// <remarks>
    /// Written ahead of the first request rather than by it, so what the
    /// owner switched on is there to be looked at before anything used it.
    /// Write-ahead logging is chosen here and stays with the file: readers do
    /// not wait for a writer, which is what a lambda serving requests at once
    /// needs.
    /// </remarks>
    public void Create(long lambdaId)
    {
        using (var connection = new SqliteConnection(ConnectionString(storage.GetDatabase(lambdaId), false, pooled: false)))
        {
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = "PRAGMA journal_mode = WAL";
            command.ExecuteNonQuery();
        }

        Invalidate(lambdaId);

        logger.LogInformation("Created database of lambda #{LambdaId}", lambdaId);
    }

    /// <summary>
    /// Replaces a feature's copy of the database with a copy of the lambda's,
    /// or removes it where the lambda has none.
    /// </summary>
    /// <remarks>
    /// Copied with SQLite's backup, which reads the whole database in one
    /// transaction while the lambda goes on writing to it: what the feature
    /// gets is the database as it was at one moment, not half of two.
    /// </remarks>
    public async ValueTask CopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default)
    {
        Remove(lambdaId, featureId);

        var target = storage.GetDatabase(lambdaId, featureId);

        if (Load(lambdaId, 0).Enabled)
        {
            // a whole database, a gigabyte in the premium tier (see Offload)
            await Offload.Run(() => Backup(storage.GetDatabase(lambdaId), target), cancellation);
        }

        Invalidate(lambdaId);
    }

    /// <summary>
    /// Removes a feature's copy of the database.
    /// </summary>
    public void RemoveCopy(long lambdaId, long featureId) => Remove(lambdaId, featureId);

    /// <summary>
    /// Deletes the database of a lambda, the copies of its features included
    /// - a database switched on again later is a new one.
    /// </summary>
    public void Clear(long lambdaId)
    {
        using var database = databases.CreateDbContext();

        var features = database.Features.AsNoTracking()
                               .Where(f => f.LambdaId == lambdaId)
                               .Select(f => f.Id)
                               .ToList();

        Invalidate(lambdaId);

        Remove(lambdaId, null);

        foreach (var feature in features)
        {
            Remove(lambdaId, feature);
        }
    }

    /// <summary>
    /// Lets go of the connections to the databases of a lambda that is being
    /// deleted, before its files are.
    /// </summary>
    public void Forget(long lambdaId)
    {
        foreach (var scope in _snapshots.Keys.Where(s => s.LambdaId == lambdaId).ToList())
        {
            _snapshots.TryRemove(scope, out _);

            Release(storage.GetDatabase(scope.LambdaId, scope.FeatureId));
        }

        Invalidate(lambdaId);
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
    /// <remarks>
    /// What the owner, an agent and the figures read the database through. It
    /// is not pooled: it is opened rarely, and a stopwatch runs on it that a
    /// pooled one would carry over into its next use.
    /// </remarks>
    public SqliteConnection? OpenForReading(long lambdaId, long? featureId)
    {
        var file = storage.GetDatabase(lambdaId, featureId);

        if (!Load(lambdaId, 0).Enabled || !File.Exists(file))
        {
            return null;
        }

        var connection = new SqliteConnection(ConnectionString(file, true, pooled: false));

        try
        {
            connection.Open();

            ConnectionGuard.ApplyReading(connection, Patience);

            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Whether the lambda's database is there.
    /// </summary>
    public bool Exists(long lambdaId) => File.Exists(storage.GetDatabase(lambdaId));

    /// <summary>
    /// The room the database takes on disk, its journal included.
    /// </summary>
    public long SizeOf(long lambdaId, long? featureId)
    {
        var file = storage.GetDatabase(lambdaId, featureId);

        return Length(file) + Length(file + "-wal");
    }

    /// <summary>
    /// How large the database of the lambda may grow, in its tier.
    /// </summary>
    public long QuotaOf(LambdaTier tier) => options.DatabaseOf(tier);

    /// <summary>
    /// Writes the database of a lambda into a file of its own, whole - what
    /// an export carries.
    /// </summary>
    /// <remarks>
    /// Taken with SQLite's backup like a feature's copy, since the lambda may
    /// be writing to it meanwhile, and then out of write-ahead logging, so it
    /// is one file with nothing beside it.
    /// </remarks>
    /// <returns>Where the copy was written, for the caller to delete, or nothing where there is no database</returns>
    public string? Export(long lambdaId)
    {
        var file = storage.GetDatabase(lambdaId);

        if (!Load(lambdaId, 0).Enabled || !File.Exists(file))
        {
            return null;
        }

        var target = Path.Combine(Path.GetTempPath(), $"genhttp-lambda-export-{Guid.NewGuid():N}.db");

        try
        {
            Backup(file, target);

            using var copy = new SqliteConnection(ConnectionString(target, false, pooled: false));

            copy.Open();

            using var command = copy.CreateCommand();

            command.CommandText = "PRAGMA journal_mode = DELETE";
            command.ExecuteNonQuery();

            return target;
        }
        catch
        {
            Delete(target);
            throw;
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// How SQLite is told where a database is.
    /// </summary>
    /// <remarks>
    /// Foreign keys are enforced, as anybody declaring one expects. The
    /// timeout is how long a statement waits for a writer to finish before it
    /// gives up, well within the time a request is given.
    /// </remarks>
    private static string ConnectionString(string file, bool readOnly, bool pooled = true) => new SqliteConnectionStringBuilder
    {
        DataSource = file,
        Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWriteCreate,
        ForeignKeys = true,
        DefaultTimeout = 10,
        Pooling = pooled
    }.ToString();

    /// <summary>
    /// Copies a database with SQLite's backup, consistent while it is written to.
    /// </summary>
    private static void Backup(string file, string target)
    {
        using var source = new SqliteConnection(ConnectionString(file, false, pooled: false));
        using var copy = new SqliteConnection(ConnectionString(target, false, pooled: false));

        source.Open();
        copy.Open();

        source.BackupDatabase(copy);
    }

    /// <summary>
    /// Deletes the database of a lambda or a feature and what SQLite keeps
    /// beside it, letting go of the pooled connections to it first.
    /// </summary>
    /// <remarks>
    /// A pooled connection holds the file open after it is deleted, and one
    /// taken from the pool afterwards would read and write that deleted file
    /// rather than whatever is made in its place. What is remembered about it
    /// goes too, so the next connection starts from what is stored.
    /// </remarks>
    private void Remove(long lambdaId, long? featureId)
    {
        var file = storage.GetDatabase(lambdaId, featureId);

        _snapshots.TryRemove(new Scope(lambdaId, featureId), out _);

        Release(file);

        foreach (var path in new[] { file, $"{file}-wal", $"{file}-shm", $"{file}-journal" })
        {
            Delete(path);
        }
    }

    private static void Release(string file)
    {
        using var connection = new SqliteConnection(ConnectionString(file, false));

        SqliteConnection.ClearPool(connection);
    }

    private void Delete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to delete {Path}", path);
        }
    }

    private static long Length(string path)
    {
        try
        {
            return File.Exists(path) ? new FileInfo(path).Length : 0;
        }
        catch (IOException)
        {
            return 0;
        }
    }

    #endregion

    private readonly record struct Scope(long LambdaId, long? FeatureId);

    /// <param name="Quota">How large it may grow, which its tier decides</param>
    private sealed record Snapshot(long Generation, bool Enabled, long Quota);

}
