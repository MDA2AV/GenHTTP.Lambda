namespace GenHTTP.Lambda.Integrations.PostgreSql;

/// <summary>
/// What the schemas and roles of the lambdas are called, and how a name is
/// written into a statement.
/// </summary>
/// <remarks>
/// Statements that make schemas and roles cannot take their names as
/// parameters, so they are written in - quoted, always, and only names the
/// platform made from ids.
/// </remarks>
internal static class PostgreSqlNames
{

    /// <summary>
    /// What follows a lambda's schema in the name of a feature's copy.
    /// </summary>
    internal const string FeatureMark = "_feature_";

    /// <summary>
    /// The schema of a lambda's database, or of a feature's copy of it.
    /// </summary>
    public static string Schema(DataScope scope)
        => scope.FeatureId is { } feature ? $"lambda_{scope.LambdaId}{FeatureMark}{feature}" : $"lambda_{scope.LambdaId}";

    /// <summary>
    /// The role a lambda reaches its schemas as.
    /// </summary>
    public static string Role(string prefix, long lambdaId) => $"{prefix}_{lambdaId}";

    /// <summary>
    /// A name, quoted as an identifier.
    /// </summary>
    public static string Quote(string name) => $"\"{name.Replace("\"", "\"\"")}\"";

    /// <summary>
    /// A text, quoted as a literal.
    /// </summary>
    public static string Literal(string text) => $"'{text.Replace("'", "''")}'";

}
