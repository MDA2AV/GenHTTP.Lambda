using GenHTTP.Lambda.Data.Entities;

using Microsoft.Data.Sqlite;

namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// The databases of the lambdas as the other services keep them in step with
/// a lambda: what a compiled lambda connects through, the copy a feature
/// works on, and what goes when a lambda or its database does.
/// </summary>
/// <remarks>
/// Apart from <see cref="IDatabaseService"/>, which is what the API and MCP
/// read tables and rows with. This one makes, copies and deletes files and
/// hands out connections, so the doors into the platform use the other one.
/// </remarks>
public interface IDatabaseVault
{

    /// <summary>
    /// What a compiled lambda is handed to connect to its database with.
    /// </summary>
    Func<SqliteConnection> ConnectorFor(long lambdaId, long? featureId);

    /// <summary>
    /// Makes the database of a lambda: an empty file, where there is none.
    /// </summary>
    void Create(long lambdaId);

    /// <summary>
    /// Replaces a feature's copy of the database with a copy of the lambda's,
    /// or removes it where the lambda has none.
    /// </summary>
    ValueTask CopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a feature's copy of the database.
    /// </summary>
    void RemoveCopy(long lambdaId, long featureId);

    /// <summary>
    /// Deletes the database of a lambda, the copies of its features included.
    /// </summary>
    void Clear(long lambdaId);

    /// <summary>
    /// Lets go of the connections to the databases of a lambda that is being
    /// deleted, before its files are.
    /// </summary>
    void Forget(long lambdaId);

    /// <summary>
    /// Forgets what is held in memory for a lambda, so the next connection
    /// reads what is stored.
    /// </summary>
    void Invalidate(long lambdaId);

    /// <summary>
    /// A connection that only reads, to the database of a lambda or a
    /// feature's copy of it - or nothing where there is none.
    /// </summary>
    SqliteConnection? OpenForReading(long lambdaId, long? featureId);

    /// <summary>
    /// Whether the lambda's database is there.
    /// </summary>
    bool Exists(long lambdaId);

    /// <summary>
    /// The room the database takes on disk, its journal included.
    /// </summary>
    long SizeOf(long lambdaId, long? featureId);

    /// <summary>
    /// How large the database of a lambda - or a feature's copy - may grow:
    /// the room of its data in its tier, less what its workspace takes of it.
    /// </summary>
    long RoomOf(long lambdaId, long? featureId, LambdaTier tier);

    /// <summary>
    /// Writes the database of a lambda into a file of its own, whole.
    /// </summary>
    /// <returns>Where the copy was written, for the caller to delete, or nothing where there is no database</returns>
    string? Export(long lambdaId);

}
