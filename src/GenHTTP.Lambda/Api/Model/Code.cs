using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// The code something should be worked out from, and where in it to look.
/// </summary>
/// <remarks>
/// One shape for every question about code, whether or not it needs a
/// position: the analyses that read a single file take the one named by
/// <paramref name="File" />, those that read the lambda take them all.
/// </remarks>
/// <param name="Files">The files of the lambda, <c>lambda.cs</c> first</param>
/// <param name="File">The file the question is about, the first one if omitted</param>
/// <param name="Line">The line the caret is on, counting from zero</param>
/// <param name="Column">The column it is at, counting from zero</param>
public sealed record CodeRequest(IReadOnlyList<LambdaFile>? Files, string? File = null, int Line = 0, int Column = 0);

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
/// One thing the compiler says could be written there.
/// </summary>
public sealed record ResolvedCompletionResponse(string Label, string Kind, string Detail, string? Documentation);

/// <summary>
/// Everything that could be written there.
/// </summary>
public sealed record CompletionsResponse(IReadOnlyList<ResolvedCompletionResponse> Completions);

/// <summary>
/// Where to go, or nothing if there is nowhere in the lambda to go.
/// </summary>
public sealed record DefinitionResponse(string? File, int Line, int Column, int Length);
