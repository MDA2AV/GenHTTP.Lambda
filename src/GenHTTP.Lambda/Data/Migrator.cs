using System.Reflection;

using GenHTTP.Lambda.Configuration;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Data;

/// <summary>
/// Brings the SQLite file up to date using the SQL scripts embedded
/// in <c>Data/Migrations</c> (applied by Evolve).
/// </summary>
public static class Migrator
{
    private const string MigrationNamespace = "GenHTTP.Lambda.Data.Migrations";

    public static void Migrate(LambdaOptions options, ILogger logger)
    {
        Directory.CreateDirectory(options.DataDirectory);

        using var connection = new SqliteConnection(options.ConnectionString);

        connection.Open();

        // write ahead logging survives in the database file and keeps
        // readers from blocking the occasional editor write
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA journal_mode = WAL; PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();
        }

        var evolve = new EvolveDb.Evolve(connection, message => logger.LogDebug("{Message}", message))
        {
            Locations = [],
            EmbeddedResourceAssemblies = [Assembly.GetExecutingAssembly()],
            EmbeddedResourceFilters = [MigrationNamespace],
            IsEraseDisabled = true,
            MetadataTableName = "schema_versions"
        };

        evolve.Migrate();

        logger.LogInformation("Database at '{Database}' is up to date", options.DatabaseFile);
    }

}
