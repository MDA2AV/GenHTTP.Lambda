using System.Text.Json.Serialization;

using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A file to write into a workspace.")]
public sealed record FileRequest(
    [Summary("Its bytes, base64 encoded; empty for an empty file")]
    string? Content);
