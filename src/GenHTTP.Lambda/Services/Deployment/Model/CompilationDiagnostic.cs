using GenHTTP.Api.Content.Services;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// A single message produced while compiling the code of a lambda. Line and
/// column refer to the code as the user wrote it (one based).
/// </summary>
[Summary("One thing the compiler, or the platform's checks of the code, said about it.")]
public sealed record CompilationDiagnostic(
    [Summary("`Error`, which keeps the code from going online, or `Warning`")]
    string Severity,
    [Summary("What kind of message it is: the C# compiler's (`CS1002`), `LAMBDA0001` for something lambdas may not do, `LAMBDA0002` for a handler the platform cannot serve, or `LAMBDA` for anything else the platform refused")]
    string Id,
    [Summary("What is wrong, and often what to do about it")]
    string Message,
    [Summary("The line in the file, counting from 1; 0 when it is about no line")]
    int Line,
    [Summary("The column in the line, counting from 1; 0 when it is about no line")]
    int Column,
    [Summary("The name of the file it is in, as the lambda names it; absent when it is about none of them")]
    string? File = null)
{

    public static CompilationDiagnostic Error(string message) => new("Error", "LAMBDA", message, 0, 0);

}
