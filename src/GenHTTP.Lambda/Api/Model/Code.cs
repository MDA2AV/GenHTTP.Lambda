using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// The result of a build, whether or not it went online.
/// </summary>
public sealed record CompilationResponse(bool Success, IReadOnlyList<CompilationDiagnostic> Diagnostics);

/// <summary>
/// One name in a snippet and what the compiler resolved it to, in the
/// coordinates of the code the user wrote.
/// </summary>
public sealed record SemanticToken(int Line, int Column, int Length, string Kind);

/// <summary>
/// What every name in a snippet means.
/// </summary>
public sealed record SemanticsResponse(IReadOnlyList<SemanticToken> Tokens);

/// <summary>
/// Asks what may be written at a place in a snippet.
/// </summary>
public sealed record CompletionRequest(string Code, int Line, int Column);

/// <summary>
/// One thing the compiler says could be written there.
/// </summary>
public sealed record ResolvedCompletionResponse(string Label, string Kind, string Detail, string? Documentation);

/// <summary>
/// Everything that could be written there.
/// </summary>
public sealed record CompletionsResponse(IReadOnlyList<ResolvedCompletionResponse> Completions);

/// <summary>
/// Asks where the name under the caret was declared.
/// </summary>
/// <param name="Files">Every file of the lambda, the snippet first</param>
/// <param name="File">The file the caret is in</param>
/// <param name="Line">The line it is on, counting from zero</param>
/// <param name="Column">The column it is at, counting from zero</param>
public sealed record DefinitionRequest(IReadOnlyList<LambdaFile>? Files, string? File, int Line, int Column);

/// <summary>
/// Where to go, or nothing if there is nowhere in the lambda to go.
/// </summary>
public sealed record DefinitionResponse(string? File, int Line, int Column, int Length);
