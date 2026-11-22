using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

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
