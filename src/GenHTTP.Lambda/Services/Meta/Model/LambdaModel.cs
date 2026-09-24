using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// Everything the editor needs to know about a lambda.
/// </summary>
public sealed record LambdaInfo(
    string PublicKey,
    string PrivateKey,
    string Tier,
    DateTime Created,
    DateTime Modified,
    int? ActiveVersion,
    int? LatestVersion,
    DateTime? DeployedAt,
    DateTime? DeployedUntil,
    DateTime KeptUntil
);

/// <summary>
/// One page of the lambdas on the installation.
/// </summary>
/// <param name="Lambdas">The page that was asked for</param>
/// <param name="Matched">How many the search found, which is what the pages are counted from</param>
/// <param name="Total">How many there are in total, search or no search</param>
/// <param name="Deployed">How many of the total are online</param>
public sealed record LambdaPage(
    IReadOnlyList<LambdaOverview> Lambdas,
    int Matched,
    int Total,
    int Deployed
);

/// <summary>
/// A stored version of the code of a lambda.
/// </summary>
/// <param name="Specification">What the user wanted and why, where whoever saved it said</param>
/// <param name="Change">What it changed, in a line</param>
/// <param name="Origin">Where it came from: template, api or agent</param>
public sealed record LambdaVersionInfo(int Version, DateTime Created, string? Specification = null, string? Change = null, string? Origin = null);

/// <summary>
/// A stored version, including the code itself.
/// </summary>
public sealed record LambdaVersionContent(int Version, DateTime Created, string Code, string? Specification = null, string? Change = null, string? Origin = null);

/// <summary>
/// Why a version was written, as whoever wrote it tells it.
/// </summary>
/// <remarks>
/// Both parts are optional and neither is checked against the code: they are
/// the account of the author, kept beside what the author did. The origin is
/// not theirs to give - it is filled in by the door the version came through.
/// </remarks>
/// <param name="Specification">What the user wants from the version and why, in their words where possible</param>
/// <param name="Change">What the version changed, in a line</param>
/// <param name="Origin">Which door it came through</param>
public sealed record VersionNote(string? Specification = null, string? Change = null, string? Origin = null)
{

    /// <summary>
    /// How long a specification may be before the rest of it is cut.
    /// </summary>
    public const int MaxSpecification = 4000;

    /// <summary>
    /// How long the line about the change may be.
    /// </summary>
    public const int MaxChange = 500;

}

/// <summary>
/// One stretch of time a version was online.
/// </summary>
/// <param name="Origin">Who put it online: api, agent or admin</param>
/// <param name="Ended">When it went offline, or nothing while it is online</param>
/// <param name="EndedBy">replaced, stopped, expired or admin</param>
public sealed record LambdaActivation(int Version, DateTime Started, string? Origin, DateTime? Ended, string? EndedBy);

/// <summary>
/// A lambda that has been looked up by its public key and is ready to run.
/// </summary>
public sealed record ResolvedLambda(long Id, string PublicKey, string Tier, int ActiveVersion, DateTime DeployedAt);

/// <summary>
/// Everything anybody may know about a public key: whether it could be
/// claimed, and whether something is already answering there.
/// </summary>
/// <param name="PublicKey">The key as it would be stored</param>
/// <param name="Valid">Whether the key is one a lambda could have</param>
/// <param name="Exists">Whether a lambda has it</param>
/// <param name="Deployed">Whether that lambda is online</param>
/// <param name="Reason">Why it cannot be claimed, if it cannot</param>
public sealed record KeyStatus(string PublicKey, bool Valid, bool Exists, bool Deployed, string? Reason)
{

    /// <summary>
    /// Whether a new lambda could be created with this key.
    /// </summary>
    public bool Available => Valid && !Exists;

}

/// <summary>
/// The outcome of a deployment attempt - either the updated lambda or the
/// reasons why its code did not build.
/// </summary>
public sealed record DeploymentResult(bool Success, LambdaInfo? Lambda, IReadOnlyList<CompilationDiagnostic> Diagnostics);

/// <summary>
/// What a maintenance run has cleaned up.
/// </summary>
public sealed record MaintenanceReport(int Undeployed, int Deleted);

/// <summary>
/// How much the platform is holding, for the telemetry page.
/// </summary>
public sealed record LambdaCounts(int Lambdas, int Deployed, int Versions);

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
    DateTime Created,
    DateTime Modified,
    int? ActiveVersion,
    int? LatestVersion,
    int Versions,
    DateTime? DeployedUntil,
    DateTime KeptUntil
);
