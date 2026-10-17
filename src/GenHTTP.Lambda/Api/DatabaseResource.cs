using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Databases;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// Reading only: what is in the database is written by the lambda, and a
/// change to the records is a change to the app.
/// </remarks>
[Tag(ApiDocumentation.Database)]
[Summary("The database of a lambda, read as its owner reads it: its tables, and the records in them.")]
[Remarks("""
         Every lambda may have a SQLite database, which its code reaches with `Database.GetConnection()`. Its schema is made
         by migrations the lambda ships in `resources/migrations/`, applied as it starts; its records are written by the
         lambda. This API only reads it - there is no way to change a record from outside the lambda.

         It is data: every version reads the same database, deploying and rolling back leave it alone, and a feature works on
         a copy of it, read below `…/features/{feature}/database`. It is off until it is switched on
         (`PUT /lambdas/{privateKey}/data/database`), which makes it empty. Switching it off deletes it.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class DatabaseResource(IDatabaseService databases)
{

    #region The lambda's

    [ResourceMethod("lambdas/:privateKey/database")]
    [Summary("Lists the tables of the database")]
    [Remarks("""
             And whether the lambda has a database at all, how much room it takes, and whether its code connects to it. Tables
             SQLite keeps for itself are left out; the one Evolve keeps of the migrations it applied is marked (`migrations`).
             While the database is switched off, the answer says so and lists nothing.
             """)]
    [return: Summary("The database and its tables")]
    public async ValueTask<DatabaseResponse> Get([Summary(ApiDocumentation.PrivateKey)] string privateKey)
        => Describe(await databases.GetAsync(privateKey));

    [ResourceMethod("lambdas/:privateKey/database/tables/:table")]
    [Summary("Reads rows of a table")]
    [Remarks("""
             The newest rows first, unless a column to sort by is named. Each row is a list of values in the order of the
             columns. A text longer than 2000 characters is cut and sent as `{ "text": …, "length": … }`, bytes as
             `{ "blob": length }`, and an integer beyond what JavaScript holds exactly as text.
             """)]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or its database has no such table.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.BadRequest, "The table has no column of the name to sort by.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The database is switched off.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("A page of rows")]
    public async ValueTask<DatabaseRowsResponse> Rows([Summary(ApiDocumentation.PrivateKey)] string privateKey,
                                                      [Summary("The name of the table or view, encoded as one segment")] string table,
                                                      [Summary("A column to sort by; left out, the order the rows were written in")] string? order,
                                                      [Summary("How many rows to skip")] int offset = 0,
                                                      [Summary("How many rows to answer with, 1 to 200")] int limit = 50,
                                                      [Summary("Whether the order is reversed - newest, or highest, first - which it is unless this is `false`")] bool descending = true)
        => Describe(await databases.ReadAsync(privateKey, table, offset, limit, order, descending));

    #endregion

    #region A feature's copy

    [ResourceMethod("lambdas/:privateKey/features/:feature/database")]
    [Summary("Lists the tables of a feature's database")]
    [Remarks("The copy its preview reads and writes, taken when the feature was started or its data last refreshed. Its migrations may have changed its schema.")]
    [Response(ResponseStatus.NotFound, ApiDocumentation.NoFeature, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The feature's copy of the database and its tables")]
    public async ValueTask<DatabaseResponse> GetOfFeature([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature)
        => Describe(await databases.GetAsync(privateKey, feature));

    [ResourceMethod("lambdas/:privateKey/features/:feature/database/tables/:table")]
    [Summary("Reads rows of a feature's database")]
    [Remarks("As for the lambda's own database.")]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, it has no such feature, or the copy has no such table.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.BadRequest, "The table has no column of the name to sort by.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The lambda's database is switched off.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("A page of rows")]
    public async ValueTask<DatabaseRowsResponse> RowsOfFeature([Summary(ApiDocumentation.PrivateKey)] string privateKey,
                                                               [Summary(ApiDocumentation.FeatureKey)] string feature,
                                                               [Summary("The name of the table or view, encoded as one segment")] string table,
                                                               [Summary("A column to sort by; left out, the order the rows were written in")] string? order,
                                                               [Summary("How many rows to skip")] int offset = 0,
                                                               [Summary("How many rows to answer with, 1 to 200")] int limit = 50,
                                                               [Summary("Whether the order is reversed, which it is unless this is `false`")] bool descending = true)
        => Describe(await databases.ReadAsync(privateKey, table, offset, limit, order, descending, feature));

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
