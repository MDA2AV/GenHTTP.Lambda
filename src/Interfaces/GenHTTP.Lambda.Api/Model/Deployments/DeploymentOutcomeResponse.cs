using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What came of an attempt to put a version online.")]
public sealed record DeploymentOutcomeResponse(
    [Summary("Whether it compiled and is online now. Where it is not, whatever was online before still is.")]
    bool Success,
    [Summary("The lambda afterwards; absent where nothing about it could be read")]
    LambdaResponse? Lambda,
    [Summary("What the compiler said: the errors that kept it offline, or the warnings of a version that went online")]
    IReadOnlyList<CompilationDiagnostic> Diagnostics
);
