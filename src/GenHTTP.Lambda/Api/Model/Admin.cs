using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Every lambda on the installation, for the administration panel.
/// </summary>
/// <param name="Matched">How many the search matched, which is what the pages count from</param>
public sealed record AdminListingResponse(
    IReadOnlyList<AdminLambda> Lambdas,
    int Total,
    int Deployed,
    int Matched,
    int Page,
    int Pages
);

/// <summary>
/// One lambda as the panel sees it: what the database knows, and what it has
/// been doing since the server came up.
/// </summary>
/// <remarks>
/// This carries the editor key, so it only ever answers a request that has
/// presented the administration token. The traffic figures are counters held
/// in memory, so they start again with the process and are absent for a lambda
/// nobody has called yet.
/// </remarks>
public sealed record AdminLambda(
    string PublicKey,
    string PrivateKey,
    string Tier,
    DateTime Created,
    DateTime Modified,
    int? ActiveVersion,
    int? LatestVersion,
    int Versions,
    DateTime? DeployedUntil,
    DateTime? KeptUntil,
    long Requests,
    long Failed,
    DateTime? LastSeen,
    string? Domain,
    bool DomainServed
);

/// <summary>
/// Everything about one lambda, for the page the panel shows it on.
/// </summary>
/// <param name="Lambda">The lambda, as its owner sees it - including the editor key</param>
/// <param name="Traffic">What it has been doing since the server came up</param>
/// <param name="Versions">Its stored versions, newest first</param>
/// <param name="Activations">Every stretch of time it was online, newest first</param>
/// <param name="Tiers">The tiers there are, for the panel to offer</param>
public sealed record AdminLambdaDetail(
    LambdaResponse Lambda,
    LambdaTraffic Traffic,
    IReadOnlyList<VersionResponse> Versions,
    IReadOnlyList<ActivationResponse> Activations,
    IReadOnlyList<string> Tiers
);

/// <summary>
/// Moves a lambda to a tier.
/// </summary>
public sealed record TierRequest(string Tier);

/// <summary>
/// Sets the domain of a lambda. Nothing, or an empty one, removes it.
/// </summary>
public sealed record DomainChangeRequest(string? Domain);

/// <summary>
/// What is left of a lambda after the panel acted on it.
/// </summary>
public sealed record LambdaOverviewResponse(string PublicKey, int? ActiveVersion, DateTime? DeployedUntil);
