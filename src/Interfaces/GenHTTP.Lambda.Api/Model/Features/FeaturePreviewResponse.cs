using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What came of putting the preview of a feature online.")]
public sealed record FeaturePreviewResponse(
    [Summary("Whether it compiled and the preview is online with it")]
    bool Success,
    [Summary("The feature afterwards")]
    FeatureResponse Feature,
    [Summary("What the compiler said: the errors that kept it offline, or the warnings of a preview that went online")]
    IReadOnlyList<CompilationDiagnostic> Diagnostics
);
