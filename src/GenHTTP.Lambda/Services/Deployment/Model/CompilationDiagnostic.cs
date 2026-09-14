namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// A single message produced while compiling the code of a lambda. Line and
/// column refer to the code as the user wrote it (one based).
/// </summary>
/// <param name="File">Which file it is in, absent when it is about none of them</param>
public sealed record CompilationDiagnostic(string Severity, string Id, string Message, int Line, int Column, string? File = null)
{

    public static CompilationDiagnostic Error(string message) => new("Error", "LAMBDA", message, 0, 0);

}
