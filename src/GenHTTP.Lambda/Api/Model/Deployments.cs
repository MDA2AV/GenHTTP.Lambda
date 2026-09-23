using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Which version to put online. Defaults to the most recent one.
/// </summary>
public sealed record DeploymentRequest(int? Version);

/// <summary>
/// What is online, and for how long.
/// </summary>
/// <param name="Deployed">Whether anything is being served</param>
/// <param name="Version">The version that is served</param>
/// <param name="DeployedAt">When it went online</param>
/// <param name="DeployedUntil">When it will be taken offline again</param>
public sealed record DeploymentResponse(bool Deployed, int? Version, DateTime? DeployedAt, DateTime? DeployedUntil);

/// <summary>
/// The result of an attempt to put a version online.
/// </summary>
/// <param name="Lambda">The lambda afterwards, where the attempt succeeded</param>
/// <param name="Diagnostics">What the compiler said on the way</param>
public sealed record DeploymentOutcomeResponse(bool Success, LambdaResponse? Lambda, IReadOnlyList<CompilationDiagnostic> Diagnostics);
