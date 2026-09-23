namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Every lambda on the installation, for the administration panel.
/// </summary>
public sealed record AdminListingResponse(
    IReadOnlyList<AdminLambda> Lambdas,
    int Total,
    int Deployed,
    /// <summary>How many the search matched, which is what the pages count from.</summary>
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
    DateTime KeptUntil,
    long Requests,
    long Failed,
    DateTime? LastSeen
);

/// <summary>
/// What is left of a lambda after the panel acted on it.
/// </summary>
public sealed record LambdaOverviewResponse(string PublicKey, int? ActiveVersion, DateTime? DeployedUntil);
