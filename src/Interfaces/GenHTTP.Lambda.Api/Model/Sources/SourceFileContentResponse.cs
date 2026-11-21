using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A file of a published version, with its text.")]
public sealed record SourceFileContentResponse(
    [Summary("Its path below the project's folder")]
    string Path,
    [Summary("How large it is, in bytes")]
    long Size,
    [Summary("What it is: `code`, `resource`, `docs`, `tests`, `platform` or `project`")]
    string Kind,
    [Summary("Whether it is text - which is then in `content`, unless it is longer than a megabyte")]
    bool Text,
    [Summary("Its text, while it is text no longer than a megabyte")]
    string? Content,
    [Summary("Where it is fetched as it is")]
    string RawPath
);
