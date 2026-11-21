namespace GenHTTP.Lambda.Server;

/// <summary>
/// The databases an installation can be told to keep its data in.
/// </summary>
public enum DatabaseProvider
{

    /// <summary>
    /// SQLite files in the data directory, which every installation has.
    /// </summary>
    Sqlite,

    /// <summary>
    /// One PostgreSQL database, which the community edition offers.
    /// </summary>
    PostgreSql

}
