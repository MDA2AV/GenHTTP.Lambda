using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// One lambda as the administration panel lists it. No private key: the panel
/// acts on lambdas it does not own, and handing out the editor link of every
/// lambda on the server would make that irreversible for their owners.
/// </summary>
/// <param name="PrivateKey">
/// The editor key. Only ever leaves the process through the panel, which is
/// behind the administration token.
/// </param>
public sealed record LambdaOverview(
    string PublicKey,
    string PrivateKey,
    string Tier,
    TierPolicy Policy,
    DateTime Created,
    DateTime Modified,
    int? ActiveVersion,
    int? LatestVersion,
    int Versions,
    DateTime? DeployedUntil,
    DateTime? KeptUntil,
    string? Domain,
    DateTime? Blocked = null,
    ConnectivityInfo? Connectivity = null,
    LambdaUses? Uses = null,
    DateTime? Visited = null
);
