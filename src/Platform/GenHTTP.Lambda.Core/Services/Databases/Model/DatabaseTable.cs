namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// One table or view of a database.
/// </summary>
/// <param name="Name">What it is called</param>
/// <param name="Kind"><c>table</c> or <c>view</c></param>
/// <param name="Rows">How many rows it holds, or nothing where counting them took too long</param>
/// <param name="Columns">Its columns, in the order they were declared</param>
/// <param name="Migrations">Whether it is the history Evolve keeps of the migrations it applied, rather than records of the app</param>
public sealed record DatabaseTable(string Name, string Kind, long? Rows, IReadOnlyList<DatabaseColumn> Columns, bool Migrations);
