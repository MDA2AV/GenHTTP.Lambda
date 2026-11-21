using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

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
