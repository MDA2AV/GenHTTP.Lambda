namespace GenHTTP.Lambda.Integrations.Databases;

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
