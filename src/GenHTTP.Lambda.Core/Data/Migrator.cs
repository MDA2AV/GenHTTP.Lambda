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
    private static readonly Dictionary<(string Store, string Dialect), string> Locations = new()
    {
        [(DatabaseStores.Platform, "sqlite")] = "GenHTTP.Lambda.Data.Migrations",
        [(DatabaseStores.Platform, "postgresql")] = "GenHTTP.Lambda.Data.PostgreSql",
        [(DatabaseStores.Analytics, "sqlite")] = "GenHTTP.Lambda.Services.Analytics.Migrations",
        [(DatabaseStores.Analytics, "postgresql")] = "GenHTTP.Lambda.Services.Analytics.PostgreSql"
    };

    /// <summary>
    /// Migrates the platform's own store.
    /// </summary>
    public static void Migrate(IDatabaseProvider database, ILogger logger) => Migrate(database, DatabaseStores.Platform, logger);

    /// <summary>
    /// Migrates one store of the platform.
    /// </summary>
    public static void Migrate(IDatabaseProvider database, string store, ILogger logger)
    {
        if (!Locations.TryGetValue((store, database.Dialect), out var location))
        {
            throw new InvalidOperationException($"The platform has no schema for its {store} store in the dialect '{database.Dialect}' its database speaks. "
                                              + $"The dialects it has one in: {string.Join(", ", Locations.Keys.Where(k => k.Store == store).Select(k => k.Dialect))}.");
        }

        using var connection = database.Open(store);

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
