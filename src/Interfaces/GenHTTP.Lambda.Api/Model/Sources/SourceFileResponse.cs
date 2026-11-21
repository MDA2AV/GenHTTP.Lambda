using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A file of a published version.")]
public sealed record SourceFileResponse(
    [Summary("Its path below the project's folder")]
    string Path,
    [Summary("How large it is, in bytes")]
    long Size,
    [Summary("What it is: `code`, `resource`, `docs`, `tests`, `platform` (what stands in for the platform) or `project` (what makes it a project)")]
    string Kind
);
