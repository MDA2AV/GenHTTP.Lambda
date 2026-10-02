namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// One file in the workspace of a lambda.
/// </summary>
public sealed record WorkspaceEntry(string Path, long Size, DateTime Modified);

/// <summary>
/// The workspace as the editor shows it: what is in it, and how much of it is
/// left.
/// </summary>
/// <remarks>
/// Folders are listed separately rather than being inferred from the paths of
/// the files, because a folder with nothing in it is a real thing here - it
/// can be made before there is anything to put in it - and inferring them
/// would make it vanish the moment it was created.
/// </remarks>
/// <param name="UsedBytes">The room the files and folders take, counted in blocks as the quota is</param>
/// <param name="QuotaBytes">The room the tier of the lambda gives it</param>
/// <param name="Enabled">Whether the owner left the workspace switched on; off, it holds nothing and takes nothing</param>
public sealed record WorkspaceListing(
    IReadOnlyList<WorkspaceEntry> Files,
    IReadOnlyList<string> Folders,
    long UsedBytes,
    long QuotaBytes,
    bool Enabled = true
);

/// <summary>
/// The content of a single file, read for a download.
/// </summary>
public sealed record WorkspaceContent(string Path, byte[] Content);

/// <summary>
/// The content of a single file as base64, the way it travels in a JSON
/// document.
/// </summary>
/// <param name="Length">How many bytes the file holds</param>
/// <param name="Content">Its bytes as base64, or nothing for a file larger than travels that way - which has to be sent as it is</param>
public sealed record WorkspaceEncoded(string Path, long Length, string? Content);
