using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A column of a table or view.")]
public sealed record DatabaseColumnResponse(
    [Summary("Its name")]
    string Name,
    [Summary("The type it was declared with, as the database names it: `INTEGER`, `TEXT`, or anything else it accepts")]
    string Type,
    [Summary("Whether it was declared `NOT NULL`")]
    bool NotNull,
    [Summary("Whether it is the primary key, or part of it")]
    bool PrimaryKey,
    [Summary("The expression it defaults to, as written")]
    string? Default
);
