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
/// <param name="Provider">The database: <see cref="Sqlite"/> unless the environment names another (<c>LAMBDA_DATABASE</c>)</param>
/// <param name="Connection">How to reach a database server, as its client library reads it (<c>LAMBDA_POSTGRES_CONNECTION</c>)</param>
/// <param name="RolePrefix">What the roles of the lambdas are called before their id, where the database has roles (<c>LAMBDA_POSTGRES_ROLE_PREFIX</c>)</param>
/// <param name="PoolSize">How many connections one lambda keeps open at most, where it connects to a server (<c>LAMBDA_POSTGRES_POOL_SIZE</c>)</param>
public sealed record DatabaseSettings(string Provider = DatabaseSettings.Sqlite, string? Connection = null, string? RolePrefix = null, int? PoolSize = null)
{

    /// <summary>
    /// SQLite files in the data directory, which every installation has.
    /// </summary>
    public const string Sqlite = "sqlite";

    /// <summary>
    /// One PostgreSQL database, which the community edition offers.
    /// </summary>
    public const string PostgreSql = "postgresql";

}
