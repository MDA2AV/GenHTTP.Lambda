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
    Func<DbConnection> ConnectorFor(long lambdaId, long? featureId);

    /// <summary>
    /// Makes the database of a lambda, empty, where there is none.
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
    /// Forgets what is held in memory for a lambda, so the next connection
    /// reads what is stored.
    /// </summary>
    void Invalidate(long lambdaId);

    /// <summary>
    /// A connection that only reads, to the database of a lambda or a
    /// feature's copy of it - or nothing where there is none.
    /// </summary>
    DbConnection? OpenForReading(long lambdaId, long? featureId);

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
    /// What a compiled lambda reads the room of its data with - its database
    /// and its workspace together, as its tier allows it now.
    /// </summary>
    Func<long> DataRoomFor(long lambdaId, long? featureId);

    /// <summary>
    /// Writes the database of a lambda into a SQLite file of its own, whole,
    /// for an exported project.
    /// </summary>
    /// <returns>Where the copy was written, for the caller to delete, or nothing where there is no database or it cannot be</returns>
    string? Export(long lambdaId);

}
