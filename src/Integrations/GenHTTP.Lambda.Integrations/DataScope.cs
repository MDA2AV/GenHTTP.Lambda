namespace GenHTTP.Lambda.Integrations;

/// <summary>
/// Whose data is meant: a lambda's own, or the copy one of its features works on.
/// </summary>
/// <remarks>
/// By the ids the platform gives them, never by their keys - an integration is
/// asked once whoever called has been checked, and has no business with who
/// may. A feature's copy belongs to its lambda, so everything of a lambda is
/// found by its id alone.
/// </remarks>
/// <param name="LambdaId">The lambda the data belongs to</param>
/// <param name="FeatureId">The feature whose copy is meant, or nothing for the lambda's own</param>
public readonly record struct DataScope(long LambdaId, long? FeatureId = null)
{

    /// <summary>
    /// The lambda's own data, rather than a feature's copy of it.
    /// </summary>
    public DataScope Own => new(LambdaId);

    public override string ToString() => FeatureId is { } feature ? $"lambda #{LambdaId}, feature #{feature}" : $"lambda #{LambdaId}";

}

/// <summary>
/// What a part of the guide an integration writes may refer to.
/// </summary>
/// <param name="Entry">The file a lambda's top-level code is in, lambda.cs</param>
public sealed record GuideContext(string Entry);
