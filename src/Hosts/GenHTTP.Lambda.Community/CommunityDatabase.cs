using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Integrations.PostgreSql;
using GenHTTP.Lambda.Integrations.Sqlite;
using GenHTTP.Lambda.Server;

namespace GenHTTP.Lambda.Community;

/// <summary>
/// The databases the community edition can keep its data in, and which one
/// its operator chose.
/// </summary>
/// <remarks>
/// SQLite files in the data directory unless the environment names
/// PostgreSQL (<c>LAMBDA_DATABASE=postgresql</c>). genhttp.dev offers SQLite
/// alone, so only this host knows of PostgreSQL.
/// </remarks>
public static class CommunityDatabase
{

    /// <summary>
    /// What <c>LAMBDA_DATABASE</c> may choose from here.
    /// </summary>
    public static IReadOnlyCollection<DatabaseProvider> Offered { get; } = [DatabaseProvider.Sqlite, DatabaseProvider.PostgreSql];

    /// <summary>
    /// The integration of the database the settings name.
    /// </summary>
    public static IIntegrationBuilder<IDatabaseProvider> Of(DatabaseSettings settings)
    {
        if (settings.Provider != DatabaseProvider.PostgreSql)
        {
            return SqliteDatabase.Create();
        }

        var database = PostgreSqlDatabase.Create(settings.Connection!);

        if (settings.RolePrefix is { } prefix)
        {
            database.RolePrefix(prefix);
        }

        if (settings.PoolSize is { } size)
        {
            database.PoolSize(size);
        }

        return database;
    }

}
