using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// A lambda that has been looked up by its public key and is ready to run.
/// </summary>
/// <param name="Tier">Read on every request, so a lambda moved to another tier runs under its limits from the next one on</param>
/// <param name="WorkspaceEnabled">Whether its owner left the workspace on, read on every request for the same reason as the tier</param>
/// <param name="Feature">
/// The feature whose preview is being served, or nothing for the lambda itself. A preview runs the feature's files
/// against the feature's copy of the data, and is kept out of the lambda's figures.
/// </param>
/// <param name="AnalyticsSince">
/// Since when its owner has its visits counted, or nothing while analytics are off - read on every request, so
/// switching them on counts from the next one, and switching them off stops at once.
/// </param>
/// <param name="Connectivity">
/// Whether it may connect to other servers, read on every request for the same reason as the tier: it is compiled in,
/// so a decision of the operator builds it again on the next one.
/// </param>
public sealed record ResolvedLambda(long Id, string PublicKey, LambdaTier Tier, int ActiveVersion, DateTime DeployedAt,
                                    bool WorkspaceEnabled = true, ResolvedFeature? Feature = null, DateTime? AnalyticsSince = null,
                                    bool Connectivity = false)
{

    /// <summary>
    /// What it is built with: whether it has a workspace, and whether it may
    /// connect to other servers.
    /// </summary>
    /// <remarks>
    /// Said once, for whatever asks for its handler: a build is served as it
    /// was only to settings that match, so the warm-up at startup and a
    /// request putting these together apart would build it twice.
    /// </remarks>
    public BuildSettings Settings => new(WorkspaceEnabled, Connectivity);

}
