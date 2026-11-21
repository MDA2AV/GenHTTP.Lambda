using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A file to write into a workspace.")]
public sealed record FileRequest(
    [Summary("Its bytes, base64 encoded; empty for an empty file")]
    string? Content);
