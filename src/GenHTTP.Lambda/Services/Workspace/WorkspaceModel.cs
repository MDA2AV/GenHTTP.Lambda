namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// One file in the workspace of a lambda.
/// </summary>
public sealed record WorkspaceEntry(string Path, long Size, DateTime Modified);

/// <summary>
/// The workspace as the editor shows it: what is in it, and how much of it is
/// left.
/// </summary>
public sealed record WorkspaceListing(
    IReadOnlyList<WorkspaceEntry> Files,
    long UsedBytes,
    long QuotaBytes,
    int MaxFiles,
    int MaxFileSize
);

/// <summary>
/// The content of a single file, read for a download.
/// </summary>
public sealed record WorkspaceContent(string Path, byte[] Content);
