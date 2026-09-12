namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// The result of compiling a snippet, with all messages the compiler
/// (or the code guard) produced along the way.
/// </summary>
public sealed record CompilationOutcome(bool Success, IReadOnlyList<CompilationDiagnostic> Diagnostics)
{

    public static CompilationOutcome Succeeded(IReadOnlyList<CompilationDiagnostic>? warnings = null) => new(true, warnings ?? []);

    public static CompilationOutcome Failed(IReadOnlyList<CompilationDiagnostic> diagnostics) => new(false, diagnostics);

    public static CompilationOutcome Failed(string message) => new(false, [CompilationDiagnostic.Error(message)]);

}
