using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Features;

/// <summary>
/// How putting the preview of a feature online went.
/// </summary>
/// <param name="Feature">The feature afterwards</param>
/// <param name="Diagnostics">What the compiler said on the way</param>
public sealed record FeatureDeployment(bool Success, FeatureInfo Feature, IReadOnlyList<CompilationDiagnostic> Diagnostics);
