namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// A single message produced while compiling the code of a lambda. Line and
/// column refer to the code as the user wrote it (one based).
/// </summary>
public sealed record CompilationDiagnostic(string Severity, string Id, string Message, int Line, int Column)
{

    public static CompilationDiagnostic Error(string message) => new("Error", "LAMBDA", message, 0, 0);

}
