using System.Text.Json.Serialization;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// The content of a workspace file, base64 encoded so anything can travel.
/// </summary>
public sealed record FileResponse(string Path, [property: JsonConverter(typeof(LongStringConverter))] string Content, int Size);

/// <summary>
/// A file to write into the workspace, base64 encoded.
/// </summary>
public sealed record FileRequest(string? Content);
