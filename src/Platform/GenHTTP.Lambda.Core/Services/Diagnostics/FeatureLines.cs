namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Which of a lambda's lines to read: its own, or one feature's preview's.
/// </summary>
/// <param name="Id">The feature, or nothing for the lambda itself</param>
public readonly record struct FeatureLines(long? Id)
{

    /// <summary>What the lambda itself said, without its previews.</summary>
    public static FeatureLines None => new(null);

    /// <summary>What the preview of one feature said.</summary>
    public static FeatureLines Of(long featureId) => new(featureId);

}
