namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// A file of a workspace as it is sent: its name, and its content to stream.
/// </summary>
/// <param name="Name">The name of the file, without the folders it is in</param>
public sealed record WorkspaceFile(string Name, GenHTTP.Api.Content.IO.IResource Content);
