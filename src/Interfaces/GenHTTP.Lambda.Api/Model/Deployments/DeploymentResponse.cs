using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

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
