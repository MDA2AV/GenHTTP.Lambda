using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Features;

/// <summary>
/// A feature with the files it holds.
/// </summary>
/// <param name="Files">Every file, whose content is read by whoever needs it</param>
public sealed record FeatureContent(FeatureInfo Feature, IReadOnlyList<VersionFile> Files);
