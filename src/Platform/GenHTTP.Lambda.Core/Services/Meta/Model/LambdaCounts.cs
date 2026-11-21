namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// How much the platform is holding, for the telemetry page.
/// </summary>
public sealed record LambdaCounts(int Lambdas, int Deployed, int Versions);
