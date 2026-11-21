using GenHTTP.Lambda.Integrations.Databases;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Integrations.Sqlite;

/// <summary>
/// The database of an installation as SQLite files in its data directory,
/// which is what every installation has unless it chooses another.
/// </summary>
/// <example>
/// <code>
/// LambdaPlatform.Create().Database(SqliteDatabase.Create())
/// </code>
/// </example>
public static class SqliteDatabase
{

    public static SqliteDatabaseBuilder Create() => new();

    /// <summary>
    /// How a lambda is bound to a database SQLite keeps.
    /// </summary>
    /// <remarks>
    /// <see cref="SqliteConnection"/>, because code written before lambdas were
    /// taught a <c>DbConnection</c> names it - and the library with it, for
    /// what only SQLite has, such as the code of an error.
    /// </remarks>
    public static DatabaseBinding Binding { get; } = new(typeof(SqliteConnection), ["Microsoft.Data.Sqlite", "Microsoft.EntityFrameworkCore.Sqlite"], ["Microsoft.Data.Sqlite"]);

}
