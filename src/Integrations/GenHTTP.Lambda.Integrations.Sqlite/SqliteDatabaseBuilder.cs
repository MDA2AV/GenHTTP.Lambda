using GenHTTP.Lambda.Integrations.Databases;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Integrations.Sqlite;

/// <summary>
/// Configures the SQLite database of an installation.
/// </summary>
/// <remarks>
/// Nothing to configure: where the files are follows from the data directory.
/// </remarks>
public sealed class SqliteDatabaseBuilder : IIntegrationBuilder<IDatabaseProvider>
{

    public IDatabaseProvider Build(IntegrationContext context)
        => new SqliteDatabaseProvider(context.DataDirectory, context.Loggers.CreateLogger<SqliteDatabaseProvider>());

}
