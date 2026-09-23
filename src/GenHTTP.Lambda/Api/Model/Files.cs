namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// The content of a workspace file, base64 encoded so anything can travel.
/// </summary>
public sealed record FileResponse(string Path, string Content, int Size);

/// <summary>
/// A file to write into the workspace, base64 encoded.
/// </summary>
public sealed record FileRequest(string? Content);
