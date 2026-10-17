using System.Text.Json.Serialization;

using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A file of a workspace, base64 encoded so anything can travel in JSON.")]
public sealed record FileResponse(
    [Summary("Its path within the workspace, with forward slashes")]
    string Path,
    [Summary("Its bytes, base64 encoded")]
    [property: JsonConverter(typeof(LongStringConverter))] string Content,
    [Summary("How many bytes it holds")]
    int Size);

[Summary("A file to write into a workspace.")]
public sealed record FileRequest(
    [Summary("Its bytes, base64 encoded; empty for an empty file")]
    string? Content);
