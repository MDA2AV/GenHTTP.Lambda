namespace GenHTTP.Lambda.Integrations.PostgreSql;

/// <summary>
/// The database of an installation in PostgreSQL: the platform's records and
/// analytics, and a schema for every lambda.
/// </summary>
/// <example>
/// <code>
/// LambdaPlatform.Create().Database(PostgreSqlDatabase.Create("Host=db;Database=lambda;Username=lambda;Password=..."))
/// </code>
/// </example>
public static class PostgreSqlDatabase
{

    /// <param name="connection">
    /// How to reach the database, as Npgsql reads it. The user it names needs
    /// to create schemas in the database and roles on the server (CREATEROLE).
    /// </param>
    public static PostgreSqlDatabaseBuilder Create(string connection) => new(connection);

}
