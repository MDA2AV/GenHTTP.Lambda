using System.Collections.Concurrent;
using System.Data.Common;

using GenHTTP.Lambda.Integrations.Databases;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Integrations.Sqlite;

/// <summary>
/// Keeps every database as a SQLite file in the data directory.
/// </summary>
/// <remarks>
/// <para>
/// The platform's records are in <c>lambda.db</c>, its analytics in
/// <c>analytics.db</c>; a lambda's database is <c>databases/{id}/database.db</c>,
/// and the copy a feature works on <c>features/{id}/{feature}/database.db</c>,
/// beside the rest of what the feature keeps. Each with SQLite's journal
/// written beside it.
/// </para>
/// <para>
/// A file is not encrypted: SQLite cannot, short of replacing the SQLite of
/// the whole process with a build that can, and a key kept beside the files
/// would protect them from nobody who has the data volume. A connection handed
/// to a lambda is watched by <see cref="ConnectionGuard"/>, which is what keeps
/// the SQL sent over it inside that one file, and its size to the room it is
/// given.
/// </para>
/// <para>
/// Synchronous inside, whatever the contract says: SQLite reads and writes a
/// file of this machine, and answers on the thread that asks - which on the
/// Ioxide engine is the reactor of the request, and stays it. Every task it
/// returns has completed already, and its connections answer the async
/// methods of ADO.NET and Entity Framework synchronously as well.
/// </para>
/// </remarks>
internal sealed class SqliteDatabaseProvider(string dataDirectory, ILogger<SqliteDatabaseProvider> logger) : IDatabaseProvider
{

    /// <summary>
    /// The page size SQLite writes, which turns the room into pages.
    /// </summary>
    private const int PageSize = 4096;

    /// <summary>
    /// The stores whose journal was switched to write-ahead logging by this process.
    /// </summary>
    private readonly ConcurrentDictionary<DatabaseStore, bool> _prepared = [];

    #region The platform's own

    public DatabaseDialect Dialect => DatabaseDialect.Sqlite;

    public void ConfigureStore(DbContextOptionsBuilder options, DatabaseStore store) => options.UseSqlite(StoreConnection(store));

    /// <remarks>
    /// Write-ahead logging is chosen the first time a store is opened and stays
    /// with the file: readers do not wait for the occasional writer. Foreign
    /// keys are on, which is what the migrations expect.
    /// </remarks>
    public ValueTask<DbConnection> OpenAsync(DatabaseStore store, CancellationToken cancellation = default) => ValueTask.FromResult(Open(store));

    private DbConnection Open(DatabaseStore store)
    {
        Directory.CreateDirectory(dataDirectory);

        var connection = new SqliteConnection(StoreConnection(store));

        try
        {
            connection.Open();

            if (_prepared.TryAdd(store, true))
            {
                Execute(connection, "PRAGMA journal_mode = WAL;");
            }

            Execute(connection, "PRAGMA foreign_keys = ON;");

            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    /// <summary>
    /// The file of a store: the platform's kept the name it had before there
    /// were others.
    /// </summary>
    /// <remarks>
    /// Spelled out rather than made of the name of the store: the files are
    /// where the data of every installation is, whatever the store is called
    /// in the code.
    /// </remarks>
    private string StoreFile(DatabaseStore store) => Path.Combine(dataDirectory, store switch
    {
        DatabaseStore.Platform => "lambda.db",
        DatabaseStore.Analytics => "analytics.db",
        _ => throw new ArgumentOutOfRangeException(nameof(store), store, "The platform keeps no such store.")
    });

    private string StoreConnection(DatabaseStore store) => $"Data Source={StoreFile(store)};Default Timeout=30;Pooling=True";

    #endregion

    #region A lambda's

    public DatabaseBinding Binding => SqliteDatabase.Binding;

    public void ConfigureContext(DbContextOptionsBuilder options, DbConnection connection) => options.UseSqlite(connection, contextOwnsConnection: true);

    /// <summary>
    /// Makes an empty file, with write-ahead logging chosen for it.
    /// </summary>
    /// <remarks>
    /// Readers do not wait for a writer, which is what a lambda serving
    /// requests at once needs; the choice stays with the file.
    /// </remarks>
    public ValueTask CreateAsync(DataScope scope, CancellationToken cancellation = default)
    {
        using (var connection = new SqliteConnection(ConnectionString(FileOf(scope), false, pooled: false)))
        {
            connection.Open();

            Execute(connection, "PRAGMA journal_mode = WAL");
        }

        logger.LogInformation("Created database of {Scope}", scope);

        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> ExistsAsync(DataScope scope, CancellationToken cancellation = default) => ValueTask.FromResult(File.Exists(FileOf(scope)));

    /// <remarks>
    /// Pooled, so a connection per request costs nothing, and watched from
    /// every time it opens: a lambda that closes and opens it again takes one
    /// from the pool that has nothing applied yet. The room is applied as it
    /// is asked for, so a limit the operator lowers holds from the next
    /// connection on.
    /// </remarks>
    public ValueTask<DbConnection> ConnectAsync(DataScope scope, long room, CancellationToken cancellation = default)
    {
        var connection = new SqliteConnection(ConnectionString(FileOf(scope), false));

        ConnectionGuard.Watch(connection, Math.Max(0, room) / PageSize);

        try
        {
            connection.Open();
        }
        catch
        {
            connection.Dispose();
            throw;
        }

        return ValueTask.FromResult<DbConnection>(connection);
    }

    /// <remarks>
    /// Not pooled: it is opened rarely, and a stopwatch runs on it that a
    /// pooled one would carry over into its next use.
    /// </remarks>
    public ValueTask<DbConnection?> ReadAsync(DataScope scope, TimeSpan patience, CancellationToken cancellation = default)
    {
        var file = FileOf(scope);

        if (!File.Exists(file))
        {
            return ValueTask.FromResult<DbConnection?>(null);
        }

        var connection = new SqliteConnection(ConnectionString(file, true, pooled: false));

        try
        {
            connection.Open();

            ConnectionGuard.ApplyReading(connection, patience);

            return ValueTask.FromResult<DbConnection?>(connection);
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    /// <summary>
    /// The room the file takes on the disk, its journal included.
    /// </summary>
    public ValueTask<long> SizeOfAsync(DataScope scope, CancellationToken cancellation = default)
    {
        var file = FileOf(scope);

        return ValueTask.FromResult(Length(file) + Length(file + "-wal"));
    }

    /// <remarks>
    /// Copied with SQLite's backup, which reads the whole database in one
    /// transaction while the lambda goes on writing to it: what the copy holds
    /// is the database as it was at one moment, not half of two. On the
    /// thread it is asked on, which the platform chose to be away from a
    /// request's.
    /// </remarks>
    public ValueTask CopyAsync(DataScope source, DataScope target, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();

        Remove(FileOf(target));

        var file = FileOf(source);

        if (File.Exists(file))
        {
            Backup(file, FileOf(target));
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(DataScope scope, CancellationToken cancellation = default)
    {
        Remove(FileOf(scope));

        return ValueTask.CompletedTask;
    }

    /// <remarks>
    /// The copies of its features are found where they are kept, by the id of
    /// the lambda alone.
    /// </remarks>
    public ValueTask DeleteAllAsync(long lambdaId, CancellationToken cancellation = default)
    {
        Remove(FileOf(new DataScope(lambdaId)));

        var features = Path.Combine(dataDirectory, "features", lambdaId.ToString());

        if (Directory.Exists(features))
        {
            foreach (var feature in Directory.EnumerateDirectories(features))
            {
                Remove(Path.Combine(feature, DatabaseName));
            }
        }

        RemoveDirectory(Path.Combine(dataDirectory, "databases", lambdaId.ToString()));

        return ValueTask.CompletedTask;
    }

    /// <remarks>
    /// Taken with SQLite's backup like a feature's copy, since the lambda may
    /// be writing to it meanwhile, and then out of write-ahead logging, so it
    /// is one file with nothing beside it.
    /// </remarks>
    public ValueTask<string?> ExportAsync(DataScope scope, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();

        var file = FileOf(scope);

        if (!File.Exists(file))
        {
            return ValueTask.FromResult<string?>(null);
        }

        var target = Path.Combine(Path.GetTempPath(), $"genhttp-lambda-export-{Guid.NewGuid():N}.db");

        try
        {
            Backup(file, target);

            using var copy = new SqliteConnection(ConnectionString(target, false, pooled: false));

            copy.Open();

            Execute(copy, "PRAGMA journal_mode = DELETE");

            return ValueTask.FromResult<string?>(target);
        }
        catch
        {
            Delete(target);
            throw;
        }
    }

    #endregion

    #region Looking into one

    /// <remarks>
    /// What SQLite keeps for itself - its sequences, its statistics, the
    /// tables behind a full text index - is left out: none of it is anything
    /// the app wrote.
    /// </remarks>
    public ValueTask<IReadOnlyList<DatabaseTableInfo>> TablesAsync(DbConnection connection, CancellationToken cancellation = default)
    {
        var found = new List<DatabaseTableInfo>();

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT name, type FROM pragma_table_list WHERE schema = 'main' AND type IN ('table', 'view', 'virtual') AND name NOT LIKE 'sqlite\\_%' ESCAPE '\\' ORDER BY type = 'view', name";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            found.Add(new DatabaseTableInfo(reader.GetString(0), reader.GetString(1) == "view"));
        }

        return ValueTask.FromResult<IReadOnlyList<DatabaseTableInfo>>(found);
    }

    public ValueTask<IReadOnlyList<DatabaseColumnInfo>> ColumnsAsync(DbConnection connection, string table, CancellationToken cancellation = default)
    {
        var columns = new List<DatabaseColumnInfo>();

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT name, type, \"notnull\", pk, dflt_value FROM pragma_table_info(@table)";

        Parameter(command, "@table", table);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            columns.Add(new DatabaseColumnInfo(reader.GetString(0), reader.IsDBNull(1) ? string.Empty : reader.GetString(1), reader.GetInt64(2) != 0,
                                               reader.GetInt64(3) > 0, reader.IsDBNull(4) ? null : reader.GetString(4)));
        }

        return ValueTask.FromResult<IReadOnlyList<DatabaseColumnInfo>>(columns);
    }

    /// <remarks>
    /// What a rowid table holds is numbered in the order it was written; a
    /// view or a table without one is left in whatever order SQLite hands it
    /// over.
    /// </remarks>
    public ValueTask<string?> NaturalOrderAsync(DbConnection connection, string table, CancellationToken cancellation = default)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT type = 'table' AND wr = 0 FROM pragma_table_list WHERE schema = 'main' AND name = @table";

        Parameter(command, "@table", table);

        return ValueTask.FromResult(command.ExecuteScalar() is long yes && yes != 0 ? "rowid" : null);
    }

    public string Quote(string name) => $"\"{name.Replace("\"", "\"\"")}\"";

    #endregion

    #region Describing it

    public IntegrationInfo Describe() => new(IntegrationKind.Database, "SQLite",
                                             [
                                                 new IntegrationDetail("Platform", StoreFile(DatabaseStore.Platform)),
                                                 new IntegrationDetail("Analytics", StoreFile(DatabaseStore.Analytics)),
                                                 new IntegrationDetail("Lambdas", Path.Combine(dataDirectory, "databases", "{lambda}", DatabaseName)),
                                                 new IntegrationDetail("Drafts", Path.Combine(dataDirectory, "features", "{lambda}", "{draft}", DatabaseName))
                                             ]);

    public string Guide(GuideContext context)
        => "- Here: SQLite. ATTACH and VACUUM INTO are refused; past the data's room (see limits) writes fail with \"database or disk is full\".";

    #endregion

    #region Helpers

    private const string DatabaseName = "database.db";

    /// <summary>
    /// Where the database of a lambda, or a feature's copy of it, is kept -
    /// its folder made, the file only once the database is switched on.
    /// </summary>
    private string FileOf(DataScope scope)
    {
        var directory = scope.FeatureId is { } feature
            ? Path.Combine(dataDirectory, "features", scope.LambdaId.ToString(), feature.ToString())
            : Path.Combine(dataDirectory, "databases", scope.LambdaId.ToString());

        Directory.CreateDirectory(directory);

        return Path.Combine(directory, DatabaseName);
    }

    /// <summary>
    /// How SQLite is told where a database of a lambda is.
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
    /// Deletes a database and what SQLite keeps beside it, letting go of the
    /// pooled connections to it first.
    /// </summary>
    /// <remarks>
    /// A pooled connection holds the file open after it is deleted, and one
    /// taken from the pool afterwards would read and write that deleted file
    /// rather than whatever is made in its place.
    /// </remarks>
    private void Remove(string file)
    {
        using (var connection = new SqliteConnection(ConnectionString(file, false)))
        {
            SqliteConnection.ClearPool(connection);
        }

        foreach (var path in new[] { file, $"{file}-wal", $"{file}-shm", $"{file}-journal" })
        {
            Delete(path);
        }
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

    private static void RemoveDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
        catch (IOException)
        {
            // content that is still in use cannot be removed on every
            // platform - the next start cleans it up
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

    private static void Execute(DbConnection connection, string sql)
    {
        using var command = connection.CreateCommand();

        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static void Parameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();

        parameter.ParameterName = name;
        parameter.Value = value;

        command.Parameters.Add(parameter);
    }

    #endregion

}
