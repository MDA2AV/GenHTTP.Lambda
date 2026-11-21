using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// Everything the editor needs to know about a lambda.
/// </summary>
/// <param name="DeployedUntil">When it goes offline unless used, or nothing while it is offline or its tier keeps it online</param>
/// <param name="KeptUntil">When it is removed unless used, or nothing when its tier keeps it</param>
/// <param name="Domain">The domain it is configured to answer at, whether or not its tier lets it</param>
/// <param name="View">How its editor opens for somebody who has not chosen a view of their own</param>
/// <param name="PublicUrl">Its address below the hosting domain, which sends visitors on to its domain while it answers at one</param>
/// <param name="Address">Where to link to it: its domain while that is served, its address below the hosting domain otherwise</param>
/// <param name="Blocked">Since when the operator has blocked it, or nothing</param>
/// <param name="BlockReason">Why, in the operator's words - which its owner reads as well</param>
/// <param name="Connectivity">Whether it may connect to other servers, and where its request for that stands</param>
/// <param name="Policy">What its tier lets it have and how it is kept, as the installation set it</param>
/// <param name="Above">The tier it would be moved to for more, or nothing where there is none</param>
public sealed record LambdaInfo(
    string PublicKey,
    string PrivateKey,
    string Tier,
    TierPolicy Policy,
    TierPolicy? Above,
    DateTime Created,
    DateTime Modified,
    int? ActiveVersion,
    int? LatestVersion,
    DateTime? DeployedAt,
    DateTime? DeployedUntil,
    DateTime? KeptUntil,
    string? Domain,
    string View,
    string PublicUrl,
    string Address,
    DateTime? Blocked = null,
    string? BlockReason = null,
    ConnectivityInfo? Connectivity = null
);
