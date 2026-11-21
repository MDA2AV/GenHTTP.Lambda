using System.Data.Common;

using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// The databases of the lambdas as the other services keep them in step with
/// a lambda: what a compiled lambda connects through, the copy a feature
/// works on, and what goes when a lambda or its database does.
/// </summary>
/// <remarks>
/// Apart from <see cref="IDatabaseService"/>, which is what the API and MCP
/// read tables and rows with. This one has databases made, copied and deleted
/// and hands out connections, so the doors into the platform use the other
/// one. What a database physically is, is the database integration's.
/// </remarks>
public interface IDatabaseVault
{

    /// <summary>
    /// What a compiled lambda is handed to connect to its database with.
    /// </summary>
    DatabaseConnector ConnectorFor(long lambdaId, long? featureId);

    /// <summary>
    /// Makes the database of a lambda, empty, where there is none.
    /// </summary>
    ValueTask CreateAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// Replaces a feature's copy of the database with a copy of the lambda's,
    /// or removes it where the lambda has none.
    /// </summary>
    ValueTask CopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a feature's copy of the database.
    /// </summary>
    ValueTask RemoveCopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Deletes the database of a lambda, the copies of its features included.
    /// </summary>
    ValueTask ClearAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// Forgets what is held in memory for a lambda, so the next connection
    /// reads what is stored.
    /// </summary>
    void Invalidate(long lambdaId);

    /// <summary>
    /// A connection that only reads, to the database of a lambda or a
    /// feature's copy of it - or nothing where there is none.
    /// </summary>
    ValueTask<DbConnection?> OpenForReadingAsync(long lambdaId, long? featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Whether the lambda's database is there.
    /// </summary>
    ValueTask<bool> ExistsAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// The room the database takes, which counts against the data of its lambda.
    /// </summary>
    ValueTask<long> SizeOfAsync(long lambdaId, long? featureId, CancellationToken cancellation = default);

    /// <summary>
    /// How large the database of a lambda - or a feature's copy - may grow:
    /// the room of its data in its tier, less what its workspace takes of it.
    /// </summary>
    ValueTask<long> RoomOfAsync(long lambdaId, long? featureId, LambdaTier tier, CancellationToken cancellation = default);

    /// <summary>
    /// What a compiled lambda reads the room of its data with - its database
    /// and its workspace together, as its tier allows it now.
    /// </summary>
    Func<CancellationToken, ValueTask<long>> DataRoomFor(long lambdaId, long? featureId);

    /// <summary>
    /// Writes the database of a lambda into a SQLite file of its own, whole,
    /// for an exported project.
    /// </summary>
    /// <returns>Where the copy was written, for the caller to delete, or nothing where there is no database or it cannot be</returns>
    ValueTask<string?> ExportAsync(long lambdaId, CancellationToken cancellation = default);

}

/// <summary>
/// What the database class generated into a lambda calls, once the lambda is
/// loaded: one function that opens a connection to its database.
/// </summary>
/// <remarks>
/// Both forms, since the lambda keeps both: <c>Database.GetConnectionAsync()</c>,
/// which is what agents are taught, and <c>Database.GetConnection()</c>, which
/// code written before calls - and which waits where the database does not
/// answer at once.
/// </remarks>
/// <param name="Connect">The plain call: <c>Database.GetConnection()</c></param>
/// <param name="ConnectAsync">The awaited one: <c>Database.GetConnectionAsync()</c></param>
public sealed record DatabaseConnector(Func<DbConnection> Connect, Func<CancellationToken, ValueTask<DbConnection>> ConnectAsync);
