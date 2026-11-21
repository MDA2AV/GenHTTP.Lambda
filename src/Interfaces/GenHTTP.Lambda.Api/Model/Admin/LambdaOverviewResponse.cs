using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Services.Census;
using GenHTTP.Lambda.Services.Integrations;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Optimization;
using GenHTTP.Lambda.Services.QuietHours;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What is left of a lambda after the operator acted on it.")]
public sealed record LambdaOverviewResponse(
    [Summary("Its public key")]
    string PublicKey,
    [Summary("The version online; absent while none is")]
    int? ActiveVersion,
    [Summary("When it is taken offline unless it is used before then")]
    DateTime? DeployedUntil
);
