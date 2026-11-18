using System.Text.RegularExpressions;

using GenHTTP.Lambda.Integrations.Databases;

using Microsoft.Extensions.Logging;

using Npgsql;

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

/// <summary>
/// Configures the PostgreSQL database of an installation.
/// </summary>
public sealed partial class PostgreSqlDatabaseBuilder : IIntegrationBuilder<IDatabaseProvider>
{
    private readonly string _connection;

    private string _rolePrefix = "lambda";

    private int _poolSize = 10;

    internal PostgreSqlDatabaseBuilder(string connection)
    {
        _connection = connection;
    }

    #region Functionality

    /// <summary>
    /// What the role every lambda reaches its database as is called before its
    /// id: <c>lambda</c> by default, so <c>lambda_42</c>.
    /// </summary>
    /// <remarks>
    /// A role belongs to the server rather than to one database, so two
    /// installations on one server need prefixes of their own.
    /// </remarks>
    public PostgreSqlDatabaseBuilder RolePrefix(string prefix)
    {
        if (!Identifier().IsMatch(prefix))
        {
            throw new ArgumentException($"'{prefix}' is no prefix a role may have: lower case letters, digits and underscores, starting with a letter, up to 40 of them.",
                                        nameof(prefix));
        }

        _rolePrefix = prefix;
        return this;
    }

    /// <summary>
    /// How many connections the database of one lambda - or a feature's copy -
    /// keeps open at most: 10 by default.
    /// </summary>
    /// <remarks>
    /// Every lambda has a pool of its own, since every lambda connects as a
    /// role of its own, and the server allows as many connections as its
    /// <c>max_connections</c> says - which has to cover the lambdas busy at once.
    /// Connections a lambda has not used for a minute are closed.
    /// </remarks>
    public PostgreSqlDatabaseBuilder PoolSize(int connections)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(connections, 1);

        _poolSize = connections;
        return this;
    }

    public IDatabaseProvider Build(IntegrationContext context)
    {
        var connection = new NpgsqlConnectionStringBuilder(_connection);

        if (string.IsNullOrEmpty(connection.Database))
        {
            throw new InvalidOperationException("The connection to PostgreSQL names no database (Database=...): the platform keeps its schemas in the one it names.");
        }

        return new PostgreSqlDatabaseProvider(connection, _rolePrefix, _poolSize, context.Loggers.CreateLogger<PostgreSqlDatabaseProvider>());
    }

    [GeneratedRegex("^[a-z][a-z0-9_]{0,39}$")]
    private static partial Regex Identifier();

    #endregion

}
