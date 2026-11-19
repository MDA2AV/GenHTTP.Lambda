namespace GenHTTP.Lambda.Server;

/// <summary>
/// Which database the environment asks the installation to keep its data in,
/// and how to reach it - as the environment says it, for a host to choose an
/// integration by.
/// </summary>
/// <remarks>
/// Settings only: what is built of them is the host's, which offers the
/// databases it has an integration for (see <see cref="LambdaSettings.FromEnvironment"/>).
/// </remarks>
/// <param name="Provider">The database: SQLite unless the environment names another (<c>LAMBDA_DATABASE</c>)</param>
/// <param name="Connection">How to reach a database server, as its client library reads it (<c>LAMBDA_POSTGRES_CONNECTION</c>)</param>
/// <param name="RolePrefix">What the roles of the lambdas are called before their id, where the database has roles (<c>LAMBDA_POSTGRES_ROLE_PREFIX</c>)</param>
/// <param name="PoolSize">How many connections one lambda keeps open at most, where it connects to a server (<c>LAMBDA_POSTGRES_POOL_SIZE</c>)</param>
public sealed record DatabaseSettings(DatabaseProvider Provider = DatabaseProvider.Sqlite, string? Connection = null, string? RolePrefix = null, int? PoolSize = null)
{

    /// <summary>
    /// The database <c>LAMBDA_DATABASE</c> names, whatever case it is written
    /// in - or nothing where it names none the platform knows.
    /// </summary>
    public static DatabaseProvider? Parse(string value) => value.Trim().ToLowerInvariant() switch
    {
        "sqlite" => DatabaseProvider.Sqlite,
        "postgresql" => DatabaseProvider.PostgreSql,
        _ => null
    };

    /// <summary>
    /// What <c>LAMBDA_DATABASE</c> is set to for a database.
    /// </summary>
    public static string NameOf(DatabaseProvider provider) => provider switch
    {
        DatabaseProvider.Sqlite => "sqlite",
        DatabaseProvider.PostgreSql => "postgresql",
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "There is no such database.")
    };

}

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
