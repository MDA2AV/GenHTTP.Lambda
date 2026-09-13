using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Asks for a new lambda. The key is optional, a short random one is generated
/// if none is given.
/// </summary>
public sealed record CreateLambdaRequest(string? PublicKey, bool AcceptedTerms, string? Template = null);

/// <summary>
/// The code to be stored as the next version.
/// </summary>
public sealed record CodeRequest(string Code);

/// <summary>
/// Which version to put online. Defaults to the most recent one.
/// </summary>
public sealed record DeployRequest(int? Version);

/// <summary>
/// The key a lambda should move to.
/// </summary>
public sealed record ChangeKeyRequest(string PublicKey);

/// <summary>
/// A lambda as the editor sees it.
/// </summary>
public sealed record LambdaResponse(
    string PublicKey,
    string PrivateKey,
    string Tier,
    DateTime Created,
    DateTime Modified,
    int? ActiveVersion,
    int? LatestVersion,
    string PublicPath,
    string EditorPath
);

/// <summary>
/// One entry of the version history.
/// </summary>
public sealed record VersionResponse(int Version, DateTime Created);

/// <summary>
/// A version including the code it holds.
/// </summary>
public sealed record VersionContentResponse(int Version, DateTime Created, string Code);

/// <summary>
/// The result of a build, whether or not it went online.
/// </summary>
public sealed record CompilationResponse(bool Success, IReadOnlyList<CompilationDiagnostic> Diagnostics);

/// <summary>
/// The result of a deployment attempt.
/// </summary>
public sealed record DeploymentResponse(bool Success, LambdaResponse? Lambda, IReadOnlyList<CompilationDiagnostic> Diagnostics);

/// <summary>
/// Whether a key could be claimed.
/// </summary>
public sealed record AvailabilityResponse(string PublicKey, bool Available, string? Reason);

/// <summary>
/// What is known publicly about a key.
/// </summary>
public sealed record StatusResponse(string PublicKey, bool Exists, bool Deployed);

/// <summary>
/// What the editor needs to know about the platform it talks to.
/// </summary>
public sealed record PlatformResponse(
    string Terms,
    IReadOnlyList<TemplateGroupResponse> Templates,
    int MaxCodeLength,
    int DeploymentLifetimeHours,
    int RetentionDays,
    IReadOnlyList<string> Imports,
    IReadOnlyList<CompletionItem> Completions
);

/// <summary>
/// One example a new lambda can be started from, with the code it would be
/// seeded with so the assistant can show it before anything is created.
/// </summary>
public sealed record TemplateResponse(string Id, string Name, string Description, string Code);

/// <summary>
/// A group of templates, which is the first choice the assistant offers.
/// </summary>
public sealed record TemplateGroupResponse(string Id, string Name, string Description, IReadOnlyList<TemplateResponse> Templates);

/// <summary>
/// A single suggestion for the code editor. <see cref="Insert" /> is set for
/// snippets, where the text to insert differs from the label.
/// </summary>
public sealed record CompletionItem(string Label, string Kind, string Detail, string? Insert = null);

/// <summary>
/// What is running, and on what.
/// </summary>
public sealed record ServerDescription(
    string Engine,
    string Version,
    string Runtime,
    string Platform,
    bool ServerGarbageCollection,
    int Processors,
    DateTime Started,
    long UptimeSeconds
);

/// <summary>
/// What the server has answered since it came up.
/// </summary>
public sealed record TrafficDescription(long Requests, long Failed, long Upgrades, int OpenSockets);

/// <summary>
/// What the platform is holding.
/// </summary>
public sealed record PlatformDescription(int Lambdas, int Deployed, int Versions);

/// <summary>
/// The telemetry page in one document: what is running, what it has done, and
/// the readings taken while it did.
/// </summary>
public sealed record TelemetryResponse(
    ServerDescription Server,
    TrafficDescription Traffic,
    PlatformDescription Platform,
    TelemetrySample Latest,
    int IntervalSeconds,
    IReadOnlyList<TelemetrySample> Samples
);

/// <summary>
/// The content of a workspace file, base64 encoded so anything can travel.
/// </summary>
public sealed record FileResponse(string Path, string Content, int Size);

/// <summary>
/// A file to write into the workspace, base64 encoded.
/// </summary>
public sealed record FileRequest(string? Content);

/// <summary>
/// How an error is reported to the single page application.
/// </summary>
public sealed record ErrorResponse(int Status, string Error, string Message);
