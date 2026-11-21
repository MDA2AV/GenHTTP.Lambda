namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// One column of a table or view.
/// </summary>
/// <param name="Type">The type it was declared with, as written - SQLite does not insist on it</param>
/// <param name="PrimaryKey">Whether it is the primary key, or part of it</param>
/// <param name="Default">The expression it defaults to, as written</param>
public sealed record DatabaseColumn(string Name, string Type, bool NotNull, bool PrimaryKey, string? Default);
