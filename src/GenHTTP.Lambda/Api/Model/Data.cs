using GenHTTP.Api.Content.Services;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// One kind of data of a lambda, as its owner sees it.
/// </summary>
/// <remarks>
/// The figures are the ones every kind can give, so a kind added later is
/// listed the same way.
/// </remarks>
[Summary("One kind of data of a lambda, and how full it is.")]
public sealed record DataStoreResponse(
    [Summary("Which kind: `database`, the SQLite database it keeps its records in; `workspace`, the files it reads and writes; or `secrets`, the API keys and passwords it reads and nobody sees")]
    string Kind,
    [Summary("Whether the lambda has it switched on")]
    bool Enabled,
    [Summary("Whether a lambda has it until its owner decides otherwise: the workspace does, the others do not")]
    bool Default,
    [Summary("When it was last switched on or off; absent while it is as it came")]
    DateTime? Changed,
    [Summary("What it holds: tables, for the database; files, for the workspace; values, for the secrets")]
    int Items,
    [Summary("The room that takes, as the allowance counts it; zero for the secrets, which are counted in values")]
    long UsedBytes,
    [Summary("The room it may take: for the database and the workspace, the data allowance of the tier less what the other takes; zero for the secrets")]
    long QuotaBytes,
    [Summary("How many it may hold, for a kind counted in values rather than bytes - the secrets")]
    int? MaxItems = null
);

[Summary("A secret, as anybody but the lambda gets to see it: never its value.")]
public sealed record SecretResponse(
    [Summary("What the code reads it by: `Secret.Read(\"STRIPE_KEY\")`")]
    string Name,
    [Summary("When it was first stored")]
    DateTime Created,
    [Summary("When its value was last replaced")]
    DateTime Changed,
    [Summary("Whether the code reads it by that name")]
    bool Used
);

[Summary("The secrets of a lambda, or of a feature's copy of them, by name.")]
public sealed record SecretListingResponse(
    [Summary("Whether the lambda has its secrets switched on (`PUT …/data/secrets`)")]
    bool Enabled,
    [Summary("What is stored, by name")]
    List<SecretResponse> Secrets,
    [Summary("Every name the code reads with `Secret.Read(\"…\")` or `Secret.Exists(\"…\")` as a literal - in the version online and the newest one, or in the feature's files")]
    List<string> Used,
    [Summary("The names the code reads that have no value, and that it does not ask about with `Secret.Exists` first: what somebody still has to enter, or the code that reads them fails")]
    List<string> Missing,
    [Summary("The names the code asks about with `Secret.Exists` that have no value: it does without them")]
    List<string> Optional,
    [Summary("How many secrets a lambda may keep")]
    int Limit
);

[Summary("The value to store under a name.")]
public sealed record SecretRequest(
    [Summary("The value, up to 32 KB - sent once, and never shown again")]
    string Value
);

[Summary("The database of a lambda, or a feature's copy of it.")]
public sealed record DatabaseResponse(
    [Summary("Whether the lambda has its database switched on (`PUT …/data/database`)")]
    bool Enabled,
    [Summary("The room it takes on disk")]
    long UsedBytes,
    [Summary("How large it may grow: the data allowance of the tier less what the workspace takes")]
    long QuotaBytes,
    [Summary("Every table and view, tables first")]
    List<DatabaseTableResponse> Tables,
    [Summary("Whether the code connects to it with `Database.GetConnection()` - in the version online and the newest one, or in the feature's files")]
    bool Used
);

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

[Summary("A column of a table or view.")]
public sealed record DatabaseColumnResponse(
    [Summary("Its name")]
    string Name,
    [Summary("The type it was declared with, as written: `INTEGER`, `TEXT`, or anything else SQLite accepts")]
    string Type,
    [Summary("Whether it was declared `NOT NULL`")]
    bool NotNull,
    [Summary("Whether it is the primary key, or part of it")]
    bool PrimaryKey,
    [Summary("The expression it defaults to, as written")]
    string? Default
);

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

[Summary("A text too long to send whole: its beginning, and how long it is.")]
public sealed record TextValue(
    [Summary("Its first 2000 characters")]
    string Text,
    [Summary("How many characters it has")]
    int Length
);

[Summary("A value of bytes, told by its length.")]
public sealed record BlobValue(
    [Summary("How many bytes it holds")]
    long Blob
);
