namespace GenHTTP.Lambda.Integrations.Databases;

/// <summary>
/// A column of a table, as it was declared.
/// </summary>
/// <param name="Type">The type it was declared with, as the database names it</param>
/// <param name="Default">Its default, as SQL, if it has one</param>
public sealed record DatabaseColumnInfo(string Name, string Type, bool NotNull, bool PrimaryKey, string? Default);
