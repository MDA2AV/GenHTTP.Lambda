using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A page of the rows of a table or view.")]
public sealed record DatabaseRowsResponse(
    [Summary("The name of the table")]
    string Table,
    [Summary("Its columns, in the order of the values of each row")]
    List<DatabaseColumnResponse> Columns,
    [Summary("Each row as its values, in the order of the columns: a number, a text or null - an integer beyond what JavaScript holds exactly as text, a text of more than 2000 characters as `{ text, length }`, and bytes as `{ blob }`, their length")]
    List<List<object?>> Rows,
    [Summary("How many rows the table holds")]
    long Total,
    [Summary("How many rows were skipped")]
    int Offset,
    [Summary("How many rows were asked for at most")]
    int Limit,
    [Summary("The column the rows are sorted by; absent for the order they were written in")]
    string? Order,
    [Summary("Whether the order is reversed: newest, or highest, first")]
    bool Descending
);
