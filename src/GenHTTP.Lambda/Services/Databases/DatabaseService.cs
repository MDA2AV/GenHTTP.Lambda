using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Databases;

public sealed partial class DatabaseService(IDbContextFactory<LambdaDbContext> databases, IMetaService meta, IFeatureService features,
                                            IStorageService storage, DatabaseVault vault) : IDatabaseService
{

    /// <summary>
    /// The most rows one page holds.
    /// </summary>
    public const int MaxLimit = 200;

    /// <summary>
    /// How much of a text a page carries before it is cut short.
    /// </summary>
    /// <remarks>
    /// A column holding the JSON of a document would otherwise make a page of
    /// fifty rows weigh megabytes, to be read one line at a time. What is cut
    /// says how long it was.
    /// </remarks>
    public const int MaxText = 2000;

    private readonly ConcurrentDictionary<(long LambdaId, int Version), bool> _uses = [];

    #region Functionality

    public async ValueTask<DatabaseOverview> GetAsync(string privateKey, string? feature = null, CancellationToken cancellation = default)
    {
        var (lambdaId, featureId) = Resolve(privateKey, feature);

        var (enabled, tier) = State(lambdaId);

        var used = Used(lambdaId, featureId);

        if (!enabled)
        {
            return new DatabaseOverview(false, 0, vault.QuotaOf(tier), [], used);
        }

        var tables = await Offload.Run(() =>
        {
            using var connection = vault.OpenForReading(lambdaId, featureId);

            return connection == null ? [] : Describe(connection);
        }, cancellation);

        return new DatabaseOverview(true, vault.SizeOf(lambdaId, featureId), vault.QuotaOf(tier), tables, used);
    }

    public async ValueTask<DatabaseRows> ReadAsync(string privateKey, string table, int offset = 0, int limit = 50, string? order = null, bool descending = true,
                                                   string? feature = null, CancellationToken cancellation = default)
    {
        var (lambdaId, featureId) = Resolve(privateKey, feature);

        var (enabled, _) = State(lambdaId);

        if (!enabled)
        {
            throw LambdaException.Conflict(DataKinds.DatabaseOff);
        }

        var name = table ?? string.Empty;

        return await Offload.Run(() =>
        {
            using var connection = vault.OpenForReading(lambdaId, featureId)
                                ?? throw LambdaException.NotFound($"There is no table called '{name}': the database is empty.");

            return Read(connection, name, Math.Max(0, offset), Math.Clamp(limit, 1, MaxLimit), order, descending);
        }, cancellation);
    }

    /// <summary>
    /// Whether code calls <c>Database.GetConnection()</c>.
    /// </summary>
    /// <remarks>
    /// Read off the text, as the names of the secrets are: it is there to
    /// warn somebody about to switch the database off that the code would
    /// fail, and to tell an agent that switched it on nothing reads it.
    /// </remarks>
    public static bool Uses(string code) => UsePattern().IsMatch(code);

    [GeneratedRegex(@"\bDatabase\s*\.\s*GetConnection\s*\(")]
    private static partial Regex UsePattern();

    #endregion

    #region Reading

    public string? Export(long lambdaId) => vault.Export(lambdaId);

    /// <summary>
    /// The tables and views, with their columns and how many rows each holds.
    /// </summary>
    /// <remarks>
    /// What SQLite keeps for itself - its sequences, its statistics, the
    /// tables behind a full text index - is left out: none of it is anything
    /// the app wrote.
    /// </remarks>
    private static List<DatabaseTable> Describe(SqliteConnection connection)
    {
        var found = new List<(string Name, string Kind)>();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT name, type FROM pragma_table_list WHERE schema = 'main' AND type IN ('table', 'view', 'virtual') AND name NOT LIKE 'sqlite\\_%' ESCAPE '\\' ORDER BY type = 'view', name";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                found.Add((reader.GetString(0), reader.GetString(1) == "view" ? "view" : "table"));
            }
        }

        var tables = new List<DatabaseTable>();

        foreach (var (name, kind) in found)
        {
            var columns = Columns(connection, name);

            tables.Add(new DatabaseTable(name, kind, Count(connection, name), columns, IsChangelog(name, columns)));
        }

        return tables;
    }

    private static DatabaseRows Read(SqliteConnection connection, string table, int offset, int limit, string? order, bool descending)
    {
        var columns = Columns(connection, table);

        if (columns.Count == 0)
        {
            throw LambdaException.NotFound($"There is no table called '{table}'.");
        }

        // a column to sort by is one the table has, or none at all - never
        // text taken from the request and put into SQL
        var sorted = order == null ? null : columns.FirstOrDefault(c => c.Name == order)?.Name
                  ?? throw LambdaException.Invalid($"The table '{table}' has no column called '{order}'.");

        var direction = descending ? "DESC" : "ASC";

        // written in, where no column is named: what a rowid table holds is
        // numbered in that order, and a view or a table without one is left
        // in whatever order SQLite hands it over
        var by = sorted != null ? $"ORDER BY {Quote(sorted)} {direction}" : HasRowId(connection, table) ? $"ORDER BY rowid {direction}" : string.Empty;

        var rows = new List<IReadOnlyList<object?>>();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT * FROM {Quote(table)} {by} LIMIT $limit OFFSET $offset";
            command.Parameters.AddWithValue("$limit", limit);
            command.Parameters.AddWithValue("$offset", offset);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var row = new object?[reader.FieldCount];

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = Value(reader, i);
                }

                rows.Add(row);
            }
        }

        return new DatabaseRows(table, columns, rows, Count(connection, table) ?? rows.Count + offset, offset, limit, sorted, descending);
    }

    private static List<DatabaseColumn> Columns(SqliteConnection connection, string table)
    {
        var columns = new List<DatabaseColumn>();

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT name, type, \"notnull\", pk, dflt_value FROM pragma_table_info($table)";
        command.Parameters.AddWithValue("$table", table);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            columns.Add(new DatabaseColumn(reader.GetString(0), reader.IsDBNull(1) ? string.Empty : reader.GetString(1), reader.GetInt64(2) != 0,
                                           reader.GetInt64(3) > 0, reader.IsDBNull(4) ? null : reader.GetString(4)));
        }

        return columns;
    }

    /// <summary>
    /// How many rows a table holds, or nothing where counting took longer
    /// than reading a database is allowed to.
    /// </summary>
    private static long? Count(SqliteConnection connection, string table)
    {
        try
        {
            using var command = connection.CreateCommand();

            command.CommandText = $"SELECT count(*) FROM {Quote(table)}";

            return (long)command.ExecuteScalar()!;
        }
        catch (SqliteException)
        {
            return null;
        }
    }

    private static bool HasRowId(SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT type = 'table' AND wr = 0 FROM pragma_table_list WHERE schema = 'main' AND name = $table";
        command.Parameters.AddWithValue("$table", table);

        return command.ExecuteScalar() is long yes && yes != 0;
    }

    /// <summary>
    /// A value as it can travel as JSON: numbers as numbers where a browser
    /// reads them exactly, text cut short where it is long, bytes by their
    /// length.
    /// </summary>
    private static object? Value(SqliteDataReader reader, int column)
    {
        if (reader.IsDBNull(column))
        {
            return null;
        }

        switch (reader.GetFieldType(column))
        {
            case var type when type == typeof(long):
                {
                    var value = reader.GetInt64(column);

                    // beyond 2^53 a JavaScript number is no longer the number
                    return Math.Abs(value) <= (1L << 53) ? value : value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            case var type when type == typeof(double):
                {
                    var value = reader.GetDouble(column);

                    return double.IsFinite(value) ? value : value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            case var type when type == typeof(byte[]):
                return new BinaryValue(reader.GetBytes(column, 0, null, 0, 0));
            default:
                {
                    var text = reader.GetString(column);

                    return text.Length > MaxText ? new TruncatedText(text[..MaxText], text.Length) : text;
                }
        }
    }

    /// <summary>
    /// Whether a table is the one Evolve keeps the migrations it applied in.
    /// </summary>
    private static bool IsChangelog(string name, List<DatabaseColumn> columns)
        => name.Equals("changelog", StringComparison.OrdinalIgnoreCase)
        && columns.Any(c => c.Name == "checksum") && columns.Any(c => c.Name == "installed_on");

    private static string Quote(string name) => $"\"{name.Replace("\"", "\"\"")}\"";

    #endregion

    #region Helpers

    private (long LambdaId, long? FeatureId) Resolve(string privateKey, string? feature)
    {
        if (!string.IsNullOrWhiteSpace(feature))
        {
            var (lambdaId, featureId) = features.Require(privateKey, feature, false);

            return (lambdaId, featureId);
        }

        return (meta.GetId(privateKey) ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted)."), null);
    }

    private (bool Enabled, LambdaTier Tier) State(long lambdaId)
    {
        using var database = databases.CreateDbContext();

        var tier = database.Lambdas.AsNoTracking().Where(l => l.Id == lambdaId).Select(l => l.Tier).FirstOrDefault();

        return (DataSwitches.IsEnabled(database, lambdaId, DataKinds.Database), tier);
    }

    /// <summary>
    /// Whether the code connects to the database: the feature's own, or the
    /// lambda's - what is online and what was saved last.
    /// </summary>
    private bool Used(long lambdaId, long? featureId)
    {
        if (featureId is { } feature)
        {
            return UsedIn(storage.ReadFeature(lambdaId, feature));
        }

        using var database = databases.CreateDbContext();

        var active = database.Lambdas.AsNoTracking().Where(l => l.Id == lambdaId).Select(l => l.ActiveVersion).FirstOrDefault();

        var newest = database.Deployments.AsNoTracking().Where(d => d.LambdaId == lambdaId).Max(d => (int?)d.Version);

        foreach (var version in new[] { active, newest }.OfType<int>().Distinct())
        {
            if (Uses(lambdaId, version))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Whether one version connects, remembered - a version never changes.
    /// </summary>
    private bool Uses(long lambdaId, int version)
    {
        if (_uses.TryGetValue((lambdaId, version), out var known))
        {
            return known;
        }

        var found = UsedIn(storage.Read(lambdaId, version));

        if (_uses.Count > 4096)
        {
            _uses.Clear();
        }

        return _uses[(lambdaId, version)] = found;
    }

    private static bool UsedIn(string? source) => source != null && LambdaSource.Parse(source).Any(f => f.IsCode && Uses(f.Code));

    #endregion

}
