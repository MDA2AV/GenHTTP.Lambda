using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Databases;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The database of a lambda - a SQLite file its code reaches with
/// <c>Database.GetConnection()</c> - as its owner reads it: the tables, and
/// what is in them.
/// </summary>
/// <remarks>
/// Reading only: what is in the database is written by the lambda. It is
/// data, so every version reads the same, deploying and rolling back leave it
/// alone, and a feature works on a copy of it. It is off until it is switched
/// on (<c>PUT /lambdas/{privateKey}/data/database</c>), which makes it, empty;
/// switching it off deletes it.
/// </remarks>
public sealed class DatabaseResource(IDatabaseService databases)
{

    #region The lambda's

    /// <summary>
    /// Whether the lambda has a database, how full it is, and every table and
    /// view in it with its columns and how many rows it holds.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/database")]
    public async ValueTask<DatabaseResponse> Get(string privateKey)
        => Describe(await databases.GetAsync(privateKey));

    /// <summary>
    /// A page of the rows of one table or view, newest first unless a column
    /// to sort by is named.
    /// </summary>
    /// <param name="table">The name of the table, encoded</param>
    /// <param name="offset">How many rows to skip; none, left out</param>
    /// <param name="limit">How many rows to answer with, up to 200; 50, left out</param>
    /// <param name="order">A column to sort by; left out, the order the rows were written in</param>
    /// <param name="descending">Whether that order is reversed, which it is unless this says otherwise</param>
    [ResourceMethod("lambdas/:privateKey/database/tables/:table")]
    public async ValueTask<DatabaseRowsResponse> Rows(string privateKey, string table, int? offset, int? limit, string? order, bool? descending)
        => Describe(await databases.ReadAsync(privateKey, table, offset ?? 0, limit ?? 50, order, descending ?? true));

    #endregion

    #region A feature's copy

    /// <summary>
    /// A feature's copy of the database, which its preview reads and writes.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/features/:feature/database")]
    public async ValueTask<DatabaseResponse> GetOfFeature(string privateKey, string feature)
        => Describe(await databases.GetAsync(privateKey, feature));

    /// <summary>
    /// A page of the rows of one table of a feature's copy.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/features/:feature/database/tables/:table")]
    public async ValueTask<DatabaseRowsResponse> RowsOfFeature(string privateKey, string feature, string table, int? offset, int? limit, string? order,
                                                               bool? descending)
        => Describe(await databases.ReadAsync(privateKey, table, offset ?? 0, limit ?? 50, order, descending ?? true, feature));

    #endregion

    internal static DatabaseResponse Describe(DatabaseOverview overview)
        => new(overview.Enabled, overview.UsedBytes, overview.QuotaBytes, [.. overview.Tables.Select(Describe)], overview.Used);

    private static DatabaseTableResponse Describe(DatabaseTable table)
        => new(table.Name, table.Kind, table.Rows, [.. table.Columns.Select(Describe)], table.Migrations);

    private static DatabaseColumnResponse Describe(DatabaseColumn column)
        => new(column.Name, column.Type, column.NotNull, column.PrimaryKey, column.Default);

    internal static DatabaseRowsResponse Describe(DatabaseRows rows)
        => new(rows.Table, [.. rows.Columns.Select(Describe)], [.. rows.Rows.Select(r => r.Select(Describe).ToList())], rows.Total, rows.Offset, rows.Limit,
               rows.Order, rows.Descending);

    private static object? Describe(object? value) => value switch
    {
        TruncatedText text => new TextValue(text.Text, text.Length),
        BinaryValue binary => new BlobValue(binary.Blob),
        _ => value
    };

}
