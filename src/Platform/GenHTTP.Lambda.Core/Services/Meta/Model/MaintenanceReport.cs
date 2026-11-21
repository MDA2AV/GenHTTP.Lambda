namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// What a maintenance run has cleaned up.
/// </summary>
public sealed record MaintenanceReport(int Undeployed, int Deleted);
