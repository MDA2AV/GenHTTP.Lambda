using System.Reflection;

using GenHTTP.Lambda.Integrations.Databases;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Data;

/// <summary>
/// Brings the stores of the platform up to date, from the SQL scripts
/// embedded for the dialect the database provider speaks (applied by Evolve).
/// </summary>
/// <remarks>
/// The schema is the platform's, the SQL it is written in the database's - so
/// it is kept once per dialect, and every change to it is a migration in each:
/// <c>Data/Migrations</c> for SQLite, <c>Data/PostgreSql</c> for PostgreSQL,
/// and the analytics' beside them. Each set is numbered on its own. The SQLite
/// scripts keep the names they were recorded by in every database made before
/// there was another set (see the project file).
/// </remarks>
public static class Migrator
{

    /// <summary>
    /// Where the migrations of each store are embedded, by dialect.
    /// </summary>
    private static readonly Dictionary<(DatabaseStore Store, DatabaseDialect Dialect), string> Locations = new()
    {
        [(DatabaseStore.Platform, DatabaseDialect.Sqlite)] = "GenHTTP.Lambda.Data.Migrations",
        [(DatabaseStore.Platform, DatabaseDialect.PostgreSql)] = "GenHTTP.Lambda.Data.PostgreSql",
        [(DatabaseStore.Analytics, DatabaseDialect.Sqlite)] = "GenHTTP.Lambda.Services.Analytics.Migrations",
        [(DatabaseStore.Analytics, DatabaseDialect.PostgreSql)] = "GenHTTP.Lambda.Services.Analytics.PostgreSql"
    };

    /// <summary>
    /// Migrates every store of the platform: its own records and its analytics.
    /// </summary>
    /// <remarks>
    /// Evolve applies the scripts with plain calls, on a connection the
    /// integration opened - as the platform starts, before anything is served.
    /// </remarks>
    public static async ValueTask MigrateAsync(IDatabaseProvider database, ILogger logger, CancellationToken cancellation = default)
    {
        foreach (var store in Enum.GetValues<DatabaseStore>())
        {
            await MigrateAsync(database, store, logger, cancellation);
        }
    }

    /// <summary>
    /// Migrates one store of the platform.
    /// </summary>
    public static async ValueTask MigrateAsync(IDatabaseProvider database, DatabaseStore store, ILogger logger, CancellationToken cancellation = default)
    {
        if (!Locations.TryGetValue((store, database.Dialect), out var location))
        {
            throw new InvalidOperationException($"The platform has no schema for its {store} store in the dialect {database.Dialect} its database speaks. "
                                              + $"The dialects it has one in: {string.Join(", ", Locations.Keys.Where(k => k.Store == store).Select(k => k.Dialect))}.");
        }

        await using var connection = await database.OpenAsync(store, cancellation);

        var evolve = new EvolveDb.Evolve(connection, message => logger.LogDebug("{Message}", message))
        {
            Locations = [],
            EmbeddedResourceAssemblies = [Assembly.GetExecutingAssembly()],
            EmbeddedResourceFilters = [location],
            IsEraseDisabled = true,
            MetadataTableName = "schema_versions"
        };

        logger.LogInformation("Migrating store {Store}", store);

        evolve.Migrate();

        logger.LogInformation("Migrated store {Store}", store);
    }

}
