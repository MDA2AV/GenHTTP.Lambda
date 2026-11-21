using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// The outcome of a deployment attempt - either the updated lambda or the
/// reasons why its code did not build.
/// </summary>
public sealed record DeploymentResult(bool Success, LambdaInfo? Lambda, IReadOnlyList<CompilationDiagnostic> Diagnostics);
