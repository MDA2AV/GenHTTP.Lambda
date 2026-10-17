using GenHTTP.Api.Content.Services;

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
[Summary("The files a question about code is asked of, and where in them to look.")]
public sealed record CodeRequest(
    [Summary("The files of the lambda, `lambda.cs` first, as they would be saved")]
    IReadOnlyList<LambdaFile>? Files,
    [Summary("The file a question about one file is about; left out, the first")]
    string? File = null,
    [Summary("The line of the position, counting from zero")]
    int Line = 0,
    [Summary("The column of the position, counting from zero")]
    int Column = 0
);

[Summary("Whether code compiles, and what the compiler said about it.")]
public sealed record CompilationResponse(
    [Summary("Whether it compiles, and would go online if it were deployed")]
    bool Success,
    [Summary("The errors that keep it from compiling, or the warnings of code that does")]
    IReadOnlyList<CompilationDiagnostic> Diagnostics
);

[Summary("A name in a file, and what the compiler resolved it to.")]
public sealed record SemanticToken(
    [Summary("The line it is on, counting from zero")]
    int Line,
    [Summary("The column it starts at, counting from zero")]
    int Column,
    [Summary("How many characters it spans")]
    int Length,
    [Summary("What it names: `type`, `interface`, `struct`, `enum`, `enumMember`, `method`, `property`, `parameter`, `local` or `namespace`")]
    string Kind
);

[Summary("What every name in a file means.")]
public sealed record SemanticsResponse(
    [Summary("The names, in the order they appear")]
    IReadOnlyList<SemanticToken> Tokens
);

[Summary("Something that could be written at a position.")]
public sealed record ResolvedCompletionResponse(
    [Summary("What to write")]
    string Label,
    [Summary("What it is: `class`, `interface`, `struct`, `enum`, `method`, `property`, `field`, `parameter`, `variable` or `text`")]
    string Kind,
    [Summary("Its signature, as C# writes it")]
    string Detail,
    [Summary("What its documentation says about it, where it has any")]
    string? Documentation
);

[Summary("Everything that could be written at a position.")]
public sealed record CompletionsResponse(
    [Summary("The suggestions")]
    IReadOnlyList<ResolvedCompletionResponse> Completions
);

[Summary("Where a name was declared, or nothing where that is in none of the files.")]
public sealed record DefinitionResponse(
    [Summary("The file it was declared in; absent where it was not declared in any file of the request")]
    string? File,
    [Summary("The line of the declaration, counting from zero")]
    int Line,
    [Summary("The column the declared name starts at, counting from zero")]
    int Column,
    [Summary("How many characters the declared name spans")]
    int Length
);
