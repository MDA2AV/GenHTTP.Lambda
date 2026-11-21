namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// One page of the rows of a table.
/// </summary>
/// <param name="Rows">Each row as its values, in the order of the columns</param>
/// <param name="Total">How many rows the table holds</param>
/// <param name="Order">The column the rows are sorted by, or nothing for the order they were written in</param>
/// <param name="Descending">Whether that order is reversed - newest first, where no column is named</param>
public sealed record DatabaseRows(string Table, IReadOnlyList<DatabaseColumn> Columns, IReadOnlyList<IReadOnlyList<object?>> Rows, long Total,
                                  int Offset, int Limit, string? Order, bool Descending);
