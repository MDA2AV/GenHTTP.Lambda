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

/// <summary>
/// What the operator can switch on or off.
/// </summary>
/// <param name="EnterprisePage">Whether the header links to the enterprise page</param>
/// <param name="BuildBox">Whether /build offers its text box, where there is an agent</param>
/// <param name="ChangeBox">Whether the Change section of the editor offers its text box, where there is an agent</param>
public sealed record SettingsModel(bool EnterprisePage, bool BuildBox = true, bool ChangeBox = true);

/// <summary>
/// What a lambda may have and do, as the operator set it.
/// </summary>
/// <param name="Free">What a free lambda, or a demo, may have</param>
/// <param name="Premium">What a premium lambda may have; never less than a free one</param>
/// <param name="OfflineAfterHours">How long a free lambda may go without visits or edits before it is taken offline</param>
/// <param name="RemovedAfterHours">How long a free lambda may go without visits or edits before it is removed</param>
/// <param name="ShowcaseImageBytes">How large the picture of a showcase entry may be</param>
/// <param name="RequestsPerSecond">How many requests one client may send to the lambdas in a second</param>
/// <param name="BuildsPerDay">How many builds and changes one address may ask the build agent for in a day</param>
public sealed record LimitsModel(
    TierLimitsModel Free,
    TierLimitsModel Premium,
    int OfflineAfterHours,
    int RemovedAfterHours,
    int ShowcaseImageBytes,
    int RequestsPerSecond,
    int BuildsPerDay
);

/// <summary>
/// What a lambda of one tier may have.
/// </summary>
/// <param name="BuildBytes">How large a version may be: its code and its resources together</param>
/// <param name="DataBytes">The room its data may take: its database and its workspace together</param>
/// <param name="Versions">How many of its versions are kept</param>
/// <param name="Features">How many features it may have open at once</param>
public sealed record TierLimitsModel(long BuildBytes, long DataBytes, int Versions, int Features);
