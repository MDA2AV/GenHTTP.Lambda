using GenHTTP.Api.Content.Documentation;

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
