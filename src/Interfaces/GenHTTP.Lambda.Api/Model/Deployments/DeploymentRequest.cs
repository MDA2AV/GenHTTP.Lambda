using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Which version to put online.")]
public sealed record DeploymentRequest(
    [Summary("The number of the version; left out, the newest")]
    int? Version
);
