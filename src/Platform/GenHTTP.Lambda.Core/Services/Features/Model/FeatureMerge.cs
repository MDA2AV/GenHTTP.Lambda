using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Features;

/// <summary>
/// How merging a feature went.
/// </summary>
/// <param name="Name">The name of the feature, to say which one it was once it is gone</param>
/// <param name="Merged">Whether its files became a version, and the feature is gone</param>
/// <param name="Version">The version it became</param>
/// <param name="Diagnostics">Why it was not merged, where its code does not compile</param>
/// <param name="Deployment">The outcome of putting that version online, if that was asked for</param>
public sealed record FeatureMerge(string Name, bool Merged, LambdaVersionInfo? Version, IReadOnlyList<CompilationDiagnostic> Diagnostics,
                                  DeploymentResult? Deployment);
