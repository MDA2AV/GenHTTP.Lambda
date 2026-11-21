namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// The content of a single file as base64, the way it travels in a JSON
/// document.
/// </summary>
/// <param name="Length">How many bytes the file holds</param>
/// <param name="Content">Its bytes as base64, or nothing for a file larger than travels that way - which has to be sent as it is</param>
public sealed record WorkspaceEncoded(string Path, long Length, string? Content);
