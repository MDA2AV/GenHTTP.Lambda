namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// The database of a lambda, or a feature's copy of it, as its owner sees it.
/// </summary>
/// <param name="Enabled">Whether the lambda has its database switched on</param>
/// <param name="UsedBytes">The room it takes on disk, its journal included</param>
/// <param name="QuotaBytes">How large it may grow, which the tier of the lambda decides</param>
/// <param name="Tables">What it holds, tables first and then views, by name</param>
/// <param name="Used">Whether the code calls <c>Database.GetConnection()</c> - the version online and the newest one, or the feature's files</param>
public sealed record DatabaseOverview(bool Enabled, long UsedBytes, long QuotaBytes, IReadOnlyList<DatabaseTable> Tables, bool Used);

/// <summary>
/// One table or view of a database.
/// </summary>
/// <param name="Name">What it is called</param>
/// <param name="Kind"><c>table</c> or <c>view</c></param>
/// <param name="Rows">How many rows it holds, or nothing where counting them took too long</param>
/// <param name="Columns">Its columns, in the order they were declared</param>
/// <param name="Migrations">Whether it is the history Evolve keeps of the migrations it applied, rather than records of the app</param>
public sealed record DatabaseTable(string Name, string Kind, long? Rows, IReadOnlyList<DatabaseColumn> Columns, bool Migrations);

/// <summary>
/// One column of a table or view.
/// </summary>
/// <param name="Type">The type it was declared with, as written - SQLite does not insist on it</param>
/// <param name="PrimaryKey">Whether it is the primary key, or part of it</param>
/// <param name="Default">The expression it defaults to, as written</param>
public sealed record DatabaseColumn(string Name, string Type, bool NotNull, bool PrimaryKey, string? Default);

/// <summary>
/// One page of the rows of a table.
/// </summary>
/// <param name="Rows">Each row as its values, in the order of the columns</param>
/// <param name="Total">How many rows the table holds</param>
/// <param name="Order">The column the rows are sorted by, or nothing for the order they were written in</param>
/// <param name="Descending">Whether that order is reversed - newest first, where no column is named</param>
public sealed record DatabaseRows(string Table, IReadOnlyList<DatabaseColumn> Columns, IReadOnlyList<IReadOnlyList<object?>> Rows, long Total,
                                  int Offset, int Limit, string? Order, bool Descending);

/// <summary>
/// A value too long to send whole: its beginning, and how long it is.
/// </summary>
public sealed record TruncatedText(string Text, int Length);

/// <summary>
/// A value of bytes rather than text, told by its length.
/// </summary>
public sealed record BinaryValue(long Blob);
