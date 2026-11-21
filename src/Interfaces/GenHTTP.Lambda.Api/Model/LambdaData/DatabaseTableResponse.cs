using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A table or view of a database.")]
public sealed record DatabaseTableResponse(
    [Summary("Its name")]
    string Name,
    [Summary("`table` or `view`")]
    string Kind,
    [Summary("How many rows it holds; absent where counting took too long")]
    long? Rows,
    [Summary("Its columns, in the order they were declared")]
    List<DatabaseColumnResponse> Columns,
    [Summary("Whether it is the history Evolve keeps of the migrations it applied, rather than records of the app")]
    bool Migrations
);
