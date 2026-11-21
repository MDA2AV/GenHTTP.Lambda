using System.Data.Common;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Integrations.Databases;

/// <summary>
/// The relational database of an installation: what the platform keeps its
/// own records in, and what every lambda that keeps records has one of.
/// </summary>
/// <remarks>
/// <para>
/// The platform's own records are in named stores (<see cref="DatabaseStore"/>)
/// whose schema the platform migrates itself, in the <see cref="Dialect"/> the
/// provider says it speaks. A lambda's database, and the copy a feature works
/// on, is a database of its own however the provider makes one: a file, a
/// schema, a database on a server. Whatever it is, a lambda reaches only its
/// own.
/// </para>
/// <para>
/// Whether a lambda has a database, how large it may grow and who may read
/// it is decided by the platform before it asks; what is asked here is only
/// ever done. Whatever reaches the database is awaited (see
/// <see cref="IIntegration"/>): a provider for a server awaits the network, a
/// provider for files on the disk answers at once. A whole database - a
/// copy, an export - is asked for away from the thread a request is served
/// on, so a provider that answers at once may take its time there.
/// </para>
/// </remarks>
public interface IDatabaseProvider : IIntegration
{

    #region The platform's own

    /// <summary>
    /// The SQL the provider speaks, which decides the migrations the
    /// platform's own stores are made with.
    /// </summary>
    /// <remarks>
    /// A provider of a dialect the platform has no migrations for is refused
    /// as the platform starts.
    /// </remarks>
    DatabaseDialect Dialect { get; }

    /// <summary>
    /// Configures a context of Entity Framework for one of the platform's
    /// stores, as it is pooled for the life of the platform.
    /// </summary>
    void ConfigureStore(DbContextOptionsBuilder options, DatabaseStore store);

    /// <summary>
    /// Opens a connection to one of the platform's stores - made where it is
    /// not there yet - for its migrations and for plain commands.
    /// </summary>
    /// <remarks>
    /// The caller disposes of it. Commands on it are written in the
    /// <see cref="Dialect"/>, with parameters named <c>@name</c>.
    /// </remarks>
    ValueTask<DbConnection> OpenAsync(DatabaseStore store, CancellationToken cancellation = default);

    #endregion

    #region A lambda's

    /// <summary>
    /// How the code of a lambda is compiled against its database.
    /// </summary>
    DatabaseBinding Binding { get; }

    /// <summary>
    /// Configures a context of Entity Framework a lambda makes on the
    /// connection it was handed - what <c>options.UseDatabase(connection)</c>
    /// does in its code - with the context owning the connection.
    /// </summary>
    void ConfigureContext(DbContextOptionsBuilder options, DbConnection connection);

    /// <summary>
    /// Makes the database of a lambda, empty, where there is none.
    /// </summary>
    ValueTask CreateAsync(DataScope scope, CancellationToken cancellation = default);

    /// <summary>
    /// Whether the database is there.
    /// </summary>
    ValueTask<bool> ExistsAsync(DataScope scope, CancellationToken cancellation = default);

    /// <summary>
    /// Opens a connection a running lambda reads and writes its database
    /// with, and that reaches nothing else.
    /// </summary>
    /// <remarks>
    /// Opened for every request that asks, so the provider pools them. The
    /// lambda disposes of it.
    /// </remarks>
    /// <param name="room">
    /// How large the database may grow, in bytes. A write past it is refused,
    /// or - where the database cannot refuse one - a connection opened while
    /// it is past it only reads.
    /// </param>
    ValueTask<DbConnection> ConnectAsync(DataScope scope, long room, CancellationToken cancellation = default);

    /// <summary>
    /// Opens a connection that only reads, for the owner and the agents to
    /// look into a database - or nothing where there is none.
    /// </summary>
    /// <param name="patience">How long a statement may run before it is stopped</param>
    ValueTask<DbConnection?> ReadAsync(DataScope scope, TimeSpan patience, CancellationToken cancellation = default);

    /// <summary>
    /// The room the database takes, in bytes, which counts against the data
    /// of its lambda.
    /// </summary>
    ValueTask<long> SizeOfAsync(DataScope scope, CancellationToken cancellation = default);

    /// <summary>
    /// Replaces the database of one scope with a copy of another's, as it
    /// was at one moment while it may be written to - or removes it where
    /// the other has none.
    /// </summary>
    /// <remarks>
    /// A whole database, so the platform asks for it away from the thread
    /// a request is served on.
    /// </remarks>
    ValueTask CopyAsync(DataScope source, DataScope target, CancellationToken cancellation = default);

    /// <summary>
    /// Deletes a database, letting go of the connections to it first. A
    /// database made later in its place is a new one.
    /// </summary>
    ValueTask DeleteAsync(DataScope scope, CancellationToken cancellation = default);

    /// <summary>
    /// Deletes the database of a lambda and every copy of its features.
    /// </summary>
    ValueTask DeleteAllAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// Writes what a lambda's database holds into a SQLite file of its own,
    /// which is what an exported project keeps its records in - or nothing
    /// where it has none, or the provider cannot.
    /// </summary>
    /// <remarks>
    /// A whole database, so the platform asks for it away from the thread
    /// a request is served on.
    /// </remarks>
    /// <returns>Where the file was written, for the caller to delete</returns>
    ValueTask<string?> ExportAsync(DataScope scope, CancellationToken cancellation = default);

    #endregion

    #region Looking into one

    /// <summary>
    /// The tables and views a database holds that its lambda made - not
    /// what the database keeps for itself - views last.
    /// </summary>
    ValueTask<IReadOnlyList<DatabaseTableInfo>> TablesAsync(DbConnection connection, CancellationToken cancellation = default);

    /// <summary>
    /// The columns of a table or view, in their order - none where there is
    /// no table of that name.
    /// </summary>
    ValueTask<IReadOnlyList<DatabaseColumnInfo>> ColumnsAsync(DbConnection connection, string table, CancellationToken cancellation = default);

    /// <summary>
    /// What a table's rows are ordered by where nobody chose a column -
    /// the order they were written in, where the database keeps one - or
    /// nothing.
    /// </summary>
    ValueTask<string?> NaturalOrderAsync(DbConnection connection, string table, CancellationToken cancellation = default);

    /// <summary>
    /// A name as the dialect quotes it in a statement.
    /// </summary>
    string Quote(string name);

    #endregion

    #region Keeping it in shape

    /// <summary>
    /// Keeps one of the platform's stores in shape, as its database is kept
    /// in shape: the statistics its planner reads brought up to date, and
    /// room it no longer uses given back where that is cheap to do.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Asked for by the platform once it decided the store is due - at a quiet
    /// hour, on a background job, never while a request waits for it - so a
    /// provider may take what the work takes, and may hold up writers to the
    /// store for a moment. What is in the store is never changed, only how it
    /// is kept.
    /// </para>
    /// <para>
    /// What is worth doing, and whether a step is too costly for a store as it
    /// is now, is the provider's to judge: it knows its database. Whether a
    /// store is optimized at all, and when, is not.
    /// </para>
    /// </remarks>
    ValueTask<DatabaseOptimization> OptimizeAsync(DatabaseStore store, CancellationToken cancellation = default);

    /// <summary>
    /// Keeps the database of a lambda, or a feature's copy of it, in shape -
    /// as <see cref="OptimizeAsync(DatabaseStore, CancellationToken)"/> does a
    /// store - or does nothing where there is none.
    /// </summary>
    /// <remarks>
    /// A database that is not there is not made by it: one switched off while
    /// the platform decided to optimize it stays gone.
    /// </remarks>
    ValueTask<DatabaseOptimization> OptimizeAsync(DataScope scope, CancellationToken cancellation = default);

    #endregion

}

/// <summary>
/// What optimizing a database did.
/// </summary>
/// <param name="Before">The room it took before, in bytes, as the provider measures the room of a database</param>
/// <param name="After">The room it takes now</param>
/// <param name="Steps">
/// What was done, as the operator would recognize it in the database's own
/// terms - the statements run, such as <c>PRAGMA optimize</c> or
/// <c>VACUUM (ANALYZE)</c> - in the order they ran; nothing where there was no
/// database
/// </param>
/// <param name="Skipped">
/// What was considered and left out, and why - such as a rebuild a database is
/// too large for - so the operator is not left wondering
/// </param>
public sealed record DatabaseOptimization(long Before, long After, IReadOnlyList<string> Steps, IReadOnlyList<string> Skipped)
{

    /// <summary>
    /// The answer for a database that is not there.
    /// </summary>
    public static readonly DatabaseOptimization None = new(0, 0, [], []);

}

/// <summary>
/// The SQL a database speaks, which the platform has migrations for.
/// </summary>
public enum DatabaseDialect
{

    /// <summary>
    /// SQLite's.
    /// </summary>
    Sqlite,

    /// <summary>
    /// PostgreSQL's.
    /// </summary>
    PostgreSql

}

/// <summary>
/// The stores the platform keeps its own records in.
/// </summary>
/// <remarks>
/// What a store is called where it is kept - a file, a schema - is the
/// provider's to decide, and stays what it was once data is kept under it.
/// </remarks>
public enum DatabaseStore
{

    /// <summary>
    /// The lambdas, their versions, features, settings and secrets.
    /// </summary>
    Platform,

    /// <summary>
    /// What was counted of the visits to lambdas, kept apart so writing a
    /// batch of counts never holds up the platform's own records.
    /// </summary>
    Analytics

}

/// <summary>
/// A table or a view of a lambda's database.
/// </summary>
public sealed record DatabaseTableInfo(string Name, bool View);

/// <summary>
/// A column of a table, as it was declared.
/// </summary>
/// <param name="Type">The type it was declared with, as the database names it</param>
/// <param name="Default">Its default, as SQL, if it has one</param>
public sealed record DatabaseColumnInfo(string Name, string Type, bool NotNull, bool PrimaryKey, string? Default);
