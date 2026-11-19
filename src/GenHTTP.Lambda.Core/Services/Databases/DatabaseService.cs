using System.Collections.Concurrent;
using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// What the owner and the agents read of a lambda's database.
/// </summary>
/// <remarks>
/// What a database holds is asked of the database integration, which knows
/// where its catalog is; what is shown of it - a page of rows, a text cut
/// short, a number a browser reads exactly - is decided here, the same for
/// every database.
/// </remarks>
public sealed partial class DatabaseService(IDbContextFactory<LambdaDbContext> databases, IMetaService meta, IFeatureService features,
                                            IStorageService storage, DatabaseVault vault, IDatabaseProvider provider) : IDatabaseService
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
        var (lambdaId, featureId) = await ResolveAsync(privateKey, feature, cancellation);

        var (enabled, tier) = await StateAsync(lambdaId, cancellation);

        var used = await UsedAsync(lambdaId, featureId, cancellation);

        var room = await vault.RoomOfAsync(lambdaId, featureId, tier, cancellation);

        if (!enabled)
        {
            return new DatabaseOverview(false, 0, room, [], used);
        }

        // statements of the lambda's own - a view is SQL it wrote - which a
        // database that answers at once runs on the thread they are sent on
        var tables = await Offload.Run(() => TablesOfAsync(lambdaId, featureId, cancellation), cancellation);

        return new DatabaseOverview(true, await vault.SizeOfAsync(lambdaId, featureId, cancellation), room, tables, used);
    }

    public async ValueTask<DatabaseRows> ReadAsync(string privateKey, string table, int offset = 0, int limit = 50, string? order = null, bool descending = true,
                                                   string? feature = null, CancellationToken cancellation = default)
    {
        var (lambdaId, featureId) = await ResolveAsync(privateKey, feature, cancellation);

        var (enabled, _) = await StateAsync(lambdaId, cancellation);

        if (!enabled)
        {
            throw LambdaException.Conflict(DataKinds.DatabaseOff);
        }

        var name = table ?? string.Empty;

        return await Offload.Run(() => RowsOfAsync(lambdaId, featureId, name, Math.Max(0, offset), Math.Clamp(limit, 1, MaxLimit), order, descending, cancellation),
                                 cancellation);
    }

    /// <summary>
    /// Whether code calls <c>Database.GetConnectionAsync()</c>, or
    /// <c>Database.GetConnection()</c> as code written before does.
    /// </summary>
    /// <remarks>
    /// Read off the text, as the names of the secrets are: it is there to
    /// warn somebody about to switch the database off that the code would
    /// fail, and to tell an agent that switched it on nothing reads it.
    /// </remarks>
    public static bool Uses(string code) => UsePattern().IsMatch(code);

    [GeneratedRegex(@"\bDatabase\s*\.\s*GetConnection(?:Async)?\s*\(")]
    private static partial Regex UsePattern();

    #endregion

    #region Reading

    private async ValueTask<List<DatabaseTable>> TablesOfAsync(long lambdaId, long? featureId, CancellationToken cancellation)
    {
        await using var connection = await vault.OpenForReadingAsync(lambdaId, featureId, cancellation);

        return connection == null ? [] : await DescribeAsync(provider, connection, cancellation);
    }

    private async ValueTask<DatabaseRows> RowsOfAsync(long lambdaId, long? featureId, string table, int offset, int limit, string? order, bool descending,
                                                      CancellationToken cancellation)
    {
        await using var connection = await vault.OpenForReadingAsync(lambdaId, featureId, cancellation)
                                  ?? throw LambdaException.NotFound($"There is no table called '{table}': the database is empty.");

        return await PageAsync(provider, connection, table, offset, limit, order, descending, cancellation);
    }

    public ValueTask<string?> ExportAsync(long lambdaId, CancellationToken cancellation = default) => vault.ExportAsync(lambdaId, cancellation);

    /// <summary>
    /// The tables and views, with their columns and how many rows each holds.
    /// </summary>
    /// <remarks>
    /// What the database keeps for itself is left out by the integration:
    /// none of it is anything the app wrote.
    /// </remarks>
    private static async ValueTask<List<DatabaseTable>> DescribeAsync(IDatabaseProvider provider, DbConnection connection, CancellationToken cancellation)
    {
        var tables = new List<DatabaseTable>();

        foreach (var found in await provider.TablesAsync(connection, cancellation))
        {
            var columns = await ColumnsAsync(provider, connection, found.Name, cancellation);

            tables.Add(new DatabaseTable(found.Name, found.View ? "view" : "table", await CountAsync(provider, connection, found.Name, cancellation), columns,
                                         IsChangelog(found.Name, columns)));
        }

        return tables;
    }

    private static async ValueTask<DatabaseRows> PageAsync(IDatabaseProvider provider, DbConnection connection, string table, int offset, int limit, string? order,
                                                          bool descending, CancellationToken cancellation)
    {
        var columns = await ColumnsAsync(provider, connection, table, cancellation);

        if (columns.Count == 0)
        {
            throw LambdaException.NotFound($"There is no table called '{table}'.");
        }

        // a column to sort by is one the table has, or none at all - never
        // text taken from the request and put into SQL
        var sorted = order == null ? null : columns.FirstOrDefault(c => c.Name == order)?.Name
                  ?? throw LambdaException.Invalid($"The table '{table}' has no column called '{order}'.");

        var direction = descending ? "DESC" : "ASC";

        // the order rows were written in, where no column is named and the
        // database keeps one - else whatever order it hands them over in
        var natural = sorted == null ? await provider.NaturalOrderAsync(connection, table, cancellation) : null;

        var by = sorted != null ? $"ORDER BY {provider.Quote(sorted)} {direction}" : natural != null ? $"ORDER BY {natural} {direction}" : string.Empty;

        var rows = new List<IReadOnlyList<object?>>();

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT * FROM {provider.Quote(table)} {by} LIMIT @limit OFFSET @offset";

            Add(command, "@limit", limit);
            Add(command, "@offset", offset);

            await using var reader = await command.ExecuteReaderAsync(cancellation);

            while (await reader.ReadAsync(cancellation))
            {
                var row = new object?[reader.FieldCount];

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = Value(reader, i);
                }

                rows.Add(row);
            }
        }

        return new DatabaseRows(table, columns, rows, await CountAsync(provider, connection, table, cancellation) ?? rows.Count + offset, offset, limit, sorted, descending);
    }

    private static async ValueTask<List<DatabaseColumn>> ColumnsAsync(IDatabaseProvider provider, DbConnection connection, string table, CancellationToken cancellation)
        => [.. (await provider.ColumnsAsync(connection, table, cancellation)).Select(c => new DatabaseColumn(c.Name, c.Type, c.NotNull, c.PrimaryKey, c.Default))];

    /// <summary>
    /// How many rows a table holds, or nothing where counting took longer
    /// than reading a database is allowed to.
    /// </summary>
    private static async ValueTask<long?> CountAsync(IDatabaseProvider provider, DbConnection connection, string table, CancellationToken cancellation)
    {
        try
        {
            await using var command = connection.CreateCommand();

            command.CommandText = $"SELECT count(*) FROM {provider.Quote(table)}";

            return Convert.ToInt64(await command.ExecuteScalarAsync(cancellation), CultureInfo.InvariantCulture);
        }
        catch (DbException)
        {
            return null;
        }
    }

    /// <summary>
    /// A value as it can travel as JSON: numbers as numbers where a browser
    /// reads them exactly, text cut short where it is long, bytes by their
    /// length - and anything else a database may hold as the text it reads as.
    /// </summary>
    private static object? Value(DbDataReader reader, int column)
    {
        if (reader.IsDBNull(column))
        {
            return null;
        }

        var type = reader.GetFieldType(column);

        if (type == typeof(long) || type == typeof(int) || type == typeof(short) || type == typeof(byte))
        {
            var value = Convert.ToInt64(reader.GetValue(column), CultureInfo.InvariantCulture);

            // beyond 2^53 a JavaScript number is no longer the number
            return Math.Abs(value) <= (1L << 53) ? value : value.ToString(CultureInfo.InvariantCulture);
        }

        if (type == typeof(double) || type == typeof(float))
        {
            var value = Convert.ToDouble(reader.GetValue(column), CultureInfo.InvariantCulture);

            return double.IsFinite(value) ? value : value.ToString(CultureInfo.InvariantCulture);
        }

        if (type == typeof(byte[]))
        {
            return new BinaryValue(reader.GetBytes(column, 0, null, 0, 0));
        }

        if (type == typeof(bool))
        {
            return reader.GetBoolean(column);
        }

        var text = type == typeof(string) ? reader.GetString(column) : Text(reader.GetValue(column));

        return text.Length > MaxText ? new TruncatedText(text[..MaxText], text.Length) : text;
    }

    /// <summary>
    /// What a value of a type JSON has no number for reads as: a decimal
    /// exactly, a time in ISO 8601, the rest as it prints.
    /// </summary>
    private static string Text(object value) => value switch
    {
        DateTime time => time.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset time => time.ToString("O", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static void Add(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();

        parameter.ParameterName = name;
        parameter.Value = value;

        command.Parameters.Add(parameter);
    }

    /// <summary>
    /// Whether a table is the one Evolve keeps the migrations it applied in.
    /// </summary>
    private static bool IsChangelog(string name, List<DatabaseColumn> columns)
        => name.Equals("changelog", StringComparison.OrdinalIgnoreCase)
        && columns.Any(c => c.Name == "checksum") && columns.Any(c => c.Name == "installed_on");

    #endregion

    #region Helpers

    private async ValueTask<(long LambdaId, long? FeatureId)> ResolveAsync(string privateKey, string? feature, CancellationToken cancellation)
    {
        if (!string.IsNullOrWhiteSpace(feature))
        {
            var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, false, cancellation);

            return (lambdaId, featureId);
        }

        return (await meta.GetIdAsync(privateKey, cancellation) ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted)."), null);
    }

    private async ValueTask<(bool Enabled, LambdaTier Tier)> StateAsync(long lambdaId, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var tier = await database.Lambdas.AsNoTracking().Where(l => l.Id == lambdaId).Select(l => l.Tier).FirstOrDefaultAsync(cancellation);

        return (await DataSwitches.IsEnabledAsync(database, lambdaId, DataKinds.Database, cancellation), tier);
    }

    /// <summary>
    /// Whether the code connects to the database: the feature's own, or the
    /// lambda's - what is online and what was saved last.
    /// </summary>
    private async ValueTask<bool> UsedAsync(long lambdaId, long? featureId, CancellationToken cancellation)
    {
        if (featureId is { } feature)
        {
            return UsedIn(storage.ReadFeature(lambdaId, feature));
        }

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var active = await database.Lambdas.AsNoTracking().Where(l => l.Id == lambdaId).Select(l => l.ActiveVersion).FirstOrDefaultAsync(cancellation);

        var newest = await database.Deployments.AsNoTracking().Where(d => d.LambdaId == lambdaId).MaxAsync(d => (int?)d.Version, cancellation);

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

    private static bool UsedIn(string? source) => source != null && LambdaSource.Parse(source).Any(f => f.IsCompiled && Uses(f.Code));

    #endregion

}
