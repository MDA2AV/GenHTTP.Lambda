using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Which version to put online. Defaults to the most recent one.
/// </summary>
public sealed record DeployRequest(int? Version);

/// <summary>
/// The result of a deployment attempt.
/// </summary>
public sealed record DeploymentResponse(bool Success, LambdaResponse? Lambda, IReadOnlyList<CompilationDiagnostic> Diagnostics);
