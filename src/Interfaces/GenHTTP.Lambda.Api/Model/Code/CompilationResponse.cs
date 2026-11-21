using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Whether code compiles, and what the compiler said about it.")]
public sealed record CompilationResponse(
    [Summary("Whether it compiles, and would go online if it were deployed")]
    bool Success,
    [Summary("The errors that keep it from compiling, or the warnings of code that does")]
    IReadOnlyList<CompilationDiagnostic> Diagnostics
);
