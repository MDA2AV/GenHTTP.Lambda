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
public sealed record WorkspaceListing(
    IReadOnlyList<WorkspaceEntry> Files,
    IReadOnlyList<string> Folders,
    long UsedBytes,
    long QuotaBytes,
    int MaxFiles,
    int MaxFileSize
);

/// <summary>
/// The content of a single file, read for a download.
/// </summary>
public sealed record WorkspaceContent(string Path, byte[] Content);
