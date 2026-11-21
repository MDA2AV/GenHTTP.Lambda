namespace GenHTTP.Lambda.Services.UsageReports;

/// <param name="DatabaseBytes">What the lambdas' databases take, all together</param>
/// <param name="WorkspaceBytes">What their workspaces take</param>
/// <param name="LargestBytes">What the largest lambda's database and workspace take together</param>
/// <param name="VersionBytes">What their versions take on disk</param>
/// <param name="FeatureBytes">What their features take on disk, with their copies of the data</param>
public sealed record UsageData(long DatabaseBytes, long WorkspaceBytes, long LargestBytes, long VersionBytes, long FeatureBytes);
