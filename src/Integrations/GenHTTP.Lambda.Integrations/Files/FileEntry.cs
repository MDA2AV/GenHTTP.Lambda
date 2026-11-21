namespace GenHTTP.Lambda.Integrations.Files;

/// <summary>
/// A file or folder of a workspace.
/// </summary>
/// <param name="Path">Relative to the workspace, its folders separated by slashes</param>
/// <param name="Folder">Whether it is a folder</param>
/// <param name="Length">How many bytes a file holds, none for a folder</param>
/// <param name="Modified">When a file was last written, in UTC</param>
public sealed record FileEntry(string Path, bool Folder, long Length, DateTime Modified);
