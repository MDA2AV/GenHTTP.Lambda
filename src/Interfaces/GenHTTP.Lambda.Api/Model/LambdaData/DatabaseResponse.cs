using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

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
    [Summary("Whether the code connects to it with `Database.GetConnectionAsync()` - in the version online and the newest one, or in the feature's files")]
    bool Used
);
