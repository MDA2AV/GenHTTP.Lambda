using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Integrations.PostgreSql;

using Npgsql;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// A PostgreSQL database of its own for one test, on the server
/// <c>LAMBDA_TEST_POSTGRES</c> names - and a test that needs one is
/// inconclusive where it names none.
/// </summary>
/// <remarks>
/// The server is outside the process, as the build agent is: CI starts one
/// beside the tests, and a developer points the variable at one of their own
/// (<c>Host=localhost;Username=postgres;Password=postgres</c>). Every test
/// gets a database and a prefix for its roles of its own, since roles belong
/// to the server, and both are dropped once it is done.
/// </remarks>
internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly NpgsqlConnectionStringBuilder _server;

    private TestDatabase(NpgsqlConnectionStringBuilder server, string name, string rolePrefix)
    {
        _server = server;

        Name = name;
        RolePrefix = rolePrefix;
    }

    #region Get-/Setters

    public string Name { get; }

    public string RolePrefix { get; }

    /// <summary>
    /// How to reach the database, as the installation is told.
    /// </summary>
    public string Connection => new NpgsqlConnectionStringBuilder(_server.ConnectionString) { Database = Name }.ConnectionString;

    /// <summary>
    /// The integration a fixture keeps its data in.
    /// </summary>
    public IIntegrationBuilder<IDatabaseProvider> Integration => PostgreSqlDatabase.Create(Connection).RolePrefix(RolePrefix).PoolSize(4);

    #endregion

    #region Functionality

    public static async Task<TestDatabase> CreateAsync()
    {
        var configured = Environment.GetEnvironmentVariable("LAMBDA_TEST_POSTGRES");

        if (string.IsNullOrWhiteSpace(configured))
        {
            Assert.Inconclusive("Set LAMBDA_TEST_POSTGRES to a PostgreSQL server to run this test, such as Host=localhost;Username=postgres;Password=postgres");
        }

        var server = new NpgsqlConnectionStringBuilder(configured) { Database = "postgres", Pooling = false };

        var id = Guid.NewGuid().ToString("N")[..12];

        var database = new TestDatabase(server, $"lambda_test_{id}", $"t{id}");

        await using var connection = new NpgsqlConnection(server.ConnectionString);

        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{database.Name}\"", connection);

        await command.ExecuteNonQueryAsync();

        return database;
    }

    /// <summary>
    /// Runs a statement in the database as the user the installation
    /// connects as, and answers with the first value it gives.
    /// </summary>
    public async Task<object?> QueryAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(new NpgsqlConnectionStringBuilder(Connection) { Pooling = false }.ConnectionString);

        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        return await command.ExecuteScalarAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_server.ConnectionString);

            await connection.OpenAsync();

            await using (var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{Name}\" WITH (FORCE)", connection))
            {
                await drop.ExecuteNonQueryAsync();
            }

            var roles = new List<string>();

            await using (var listing = new NpgsqlCommand("SELECT rolname FROM pg_roles WHERE starts_with(rolname, @prefix)", connection))
            {
                listing.Parameters.AddWithValue("prefix", RolePrefix + "_");

                await using var reader = await listing.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    roles.Add(reader.GetString(0));
                }
            }

            foreach (var role in roles)
            {
                await using var drop = new NpgsqlCommand($"DROP ROLE IF EXISTS \"{role}\"", connection);

                await drop.ExecuteNonQueryAsync();
            }
        }
        catch (NpgsqlException)
        {
            // a server that went away takes its databases with it
        }
    }

    #endregion

}
