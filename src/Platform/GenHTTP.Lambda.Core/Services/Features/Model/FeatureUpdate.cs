namespace GenHTTP.Lambda.Services.Features;

/// <summary>
/// What to change about a feature. What is left out stays as it is.
/// </summary>
/// <param name="Base">
/// The version it is now based on - said once the changes of that version are in its files, and what allows it to be
/// merged once that is the newest
/// </param>
public sealed record FeatureUpdate(string? Name = null, string? Specification = null, string? Change = null, int? Base = null);
