using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Which version to put online.")]
public sealed record DeploymentRequest(
    [Summary("The number of the version; left out, the newest")]
    int? Version
);

[Summary("What is online, and for how long.")]
public sealed record DeploymentResponse(
    [Summary("Whether a version is online")]
    bool Deployed,
    [Summary("The version that is online; absent while none is")]
    int? Version,
    [Summary("When it went online")]
    DateTime? DeployedAt,
    [Summary("When it is taken offline unless it is visited or edited before then; absent for a tier that keeps it online")]
    DateTime? DeployedUntil
);

[Summary("What came of an attempt to put a version online.")]
public sealed record DeploymentOutcomeResponse(
    [Summary("Whether it compiled and is online now. Where it is not, whatever was online before still is.")]
    bool Success,
    [Summary("The lambda afterwards; absent where nothing about it could be read")]
    LambdaResponse? Lambda,
    [Summary("What the compiler said: the errors that kept it offline, or the warnings of a version that went online")]
    IReadOnlyList<CompilationDiagnostic> Diagnostics
);

[Summary("One stretch of time a version was online.")]
public sealed record ActivationResponse(
    [Summary("The version that was online")]
    int Version,
    [Summary("When it went online")]
    DateTime Started,
    [Summary("Who put it online: `api`, `agent`, `git`, `admin` (the operator) or `system`")]
    string? Origin,
    [Summary("When it went offline; absent while it is still online")]
    DateTime? Ended,
    [Summary("Why it went offline: `replaced` by another deployment, `stopped` by its owner, `expired` because nobody used it, or taken down by the operator (`admin`)")]
    string? EndedBy,
    [Summary("How long it was online, or has been so far, in seconds")]
    long Seconds
);
