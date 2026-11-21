using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// The content of a single file, read for a download.
/// </summary>
public sealed record WorkspaceContent(string Path, byte[] Content);
