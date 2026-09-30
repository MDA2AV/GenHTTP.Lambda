namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// One kind of data of a lambda, as its owner sees it.
/// </summary>
/// <remarks>
/// Data belongs to the lambda rather than to a version: every version reads
/// and writes the same, deploying or rolling back leaves it alone, and it goes
/// only with the lambda - or when its owner switches the kind off, which
/// deletes what it held. The figures are the ones every kind can give, so a
/// kind added later is listed the same way.
/// </remarks>
/// <param name="Kind">
/// Which kind of data: <c>database</c>, the SQLite database the lambda keeps its
/// records in, <c>workspace</c>, the files it reads and writes, or <c>secrets</c>,
/// the API keys and passwords it reads and nobody sees
/// </param>
/// <param name="Enabled">Whether the lambda has it</param>
/// <param name="Default">Whether a lambda has it until its owner decides otherwise</param>
/// <param name="Changed">When the owner last switched it; absent while it is as it came</param>
/// <param name="Items">What it holds: tables, for the database; files, for the workspace; values, for the secrets</param>
/// <param name="UsedBytes">The room that takes, as the quota counts it; zero for a kind counted in things</param>
/// <param name="QuotaBytes">The room the tier of the lambda gives it; zero for a kind counted in things</param>
/// <param name="MaxItems">How many things it may hold, for a kind counted in things such as the secrets</param>
public sealed record DataStoreResponse(string Kind, bool Enabled, bool Default, DateTime? Changed, int Items, long UsedBytes, long QuotaBytes,
                                       int? MaxItems = null);

/// <summary>
/// One secret, as anybody but the lambda gets to see it: never its value.
/// </summary>
/// <param name="Name">What the code reads it by: <c>Secret.Read("STRIPE_KEY")</c></param>
/// <param name="Created">When it was first stored</param>
/// <param name="Changed">When its value was last replaced</param>
/// <param name="Used">Whether the code reads it by that name</param>
public sealed record SecretResponse(string Name, DateTime Created, DateTime Changed, bool Used);

/// <summary>
/// The secrets of a lambda, or of a feature's copy of them.
/// </summary>
/// <param name="Enabled">Whether the lambda has secrets switched on (<c>PUT …/data/secrets</c>)</param>
/// <param name="Secrets">What is stored, by name</param>
/// <param name="Used">
/// Every name the code reads with <c>Secret.Read("…")</c> or <c>Secret.Exists("…")</c>
/// as a literal - the version online and the newest one, or the feature's files
/// </param>
/// <param name="Missing">
/// The names the code reads that have no value yet, and does not ask about
/// with <c>Secret.Exists</c> first: what somebody still has to set, or it fails
/// </param>
/// <param name="Optional">The names the code asks about with <c>Secret.Exists</c> that have no value: it does without them</param>
/// <param name="Limit">How many secrets there may be</param>
public sealed record SecretListingResponse(bool Enabled, List<SecretResponse> Secrets, List<string> Used, List<string> Missing, List<string> Optional,
                                           int Limit);

/// <summary>
/// The value to store under a name.
/// </summary>
/// <param name="Value">The value, up to 32 KB - sent once, and never shown again</param>
public sealed record SecretRequest(string Value);

/// <summary>
/// The database of a lambda, or a feature's copy of it.
/// </summary>
/// <param name="Enabled">Whether the lambda has its database switched on (<c>PUT …/data/database</c>)</param>
/// <param name="UsedBytes">The room it takes on disk</param>
/// <param name="QuotaBytes">How large it may grow in the tier of the lambda</param>
/// <param name="Tables">Every table and view, tables first</param>
/// <param name="Used">
/// Whether the code connects to it with <c>Database.GetConnection()</c> - the
/// version online and the newest one, or the feature's files
/// </param>
public sealed record DatabaseResponse(bool Enabled, long UsedBytes, long QuotaBytes, List<DatabaseTableResponse> Tables, bool Used);

/// <summary>
/// One table or view of a database.
/// </summary>
/// <param name="Kind"><c>table</c> or <c>view</c></param>
/// <param name="Rows">How many rows it holds; absent where counting took too long</param>
/// <param name="Migrations">Whether it is the history Evolve keeps of the migrations it applied</param>
public sealed record DatabaseTableResponse(string Name, string Kind, long? Rows, List<DatabaseColumnResponse> Columns, bool Migrations);

/// <summary>
/// One column of a table or view.
/// </summary>
/// <param name="Type">The type it was declared with, as written</param>
/// <param name="PrimaryKey">Whether it is the primary key, or part of it</param>
/// <param name="Default">The expression it defaults to, as written</param>
public sealed record DatabaseColumnResponse(string Name, string Type, bool NotNull, bool PrimaryKey, string? Default);

/// <summary>
/// A page of the rows of a table.
/// </summary>
/// <param name="Rows">
/// Each row as its values, in the order of the columns: numbers, text, null -
/// an integer beyond what JavaScript holds exactly as text, text of more than
/// 2000 characters as <c>{ text, length }</c>, and bytes as <c>{ blob }</c>, their length
/// </param>
/// <param name="Total">How many rows the table holds</param>
/// <param name="Order">The column they are sorted by; absent for the order they were written in</param>
public sealed record DatabaseRowsResponse(string Table, List<DatabaseColumnResponse> Columns, List<List<object?>> Rows, long Total, int Offset, int Limit,
                                          string? Order, bool Descending);

/// <summary>
/// A text too long to send whole: its beginning, and how long it is.
/// </summary>
public sealed record TextValue(string Text, int Length);

/// <summary>
/// A value of bytes, told by its length.
/// </summary>
public sealed record BlobValue(long Blob);
