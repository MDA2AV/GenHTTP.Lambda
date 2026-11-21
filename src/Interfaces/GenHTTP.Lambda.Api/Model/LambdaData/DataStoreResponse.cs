using GenHTTP.Api.Content.Documentation;

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
    [Summary("Which kind: `database`, the database it keeps its records in; `workspace`, the files it reads and writes; or `secrets`, the API keys and passwords it reads and nobody sees")]
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
