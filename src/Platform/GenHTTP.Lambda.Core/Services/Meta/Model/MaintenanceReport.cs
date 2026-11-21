using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// What a maintenance run has cleaned up.
/// </summary>
public sealed record MaintenanceReport(int Undeployed, int Deleted);
