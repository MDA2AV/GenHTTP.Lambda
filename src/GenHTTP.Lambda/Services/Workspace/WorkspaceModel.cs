using GenHTTP.Api.Content.Services;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// One file in the workspace of a lambda.
/// </summary>
[Summary("One file of a workspace.")]
public sealed record WorkspaceEntry(
    [Summary("Its path within the workspace, with forward slashes: `logs/today.txt`")]
    string Path,
    [Summary("How many bytes it holds")]
    long Size,
    [Summary("When it was last written")]
    DateTime Modified);

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
[Summary("What a workspace holds, and how much room it has left.")]
public sealed record WorkspaceListing(
    [Summary("Every file, in every folder")]
    IReadOnlyList<WorkspaceEntry> Files,
    [Summary("Every folder, by its path - listed apart, since a folder may be empty")]
    IReadOnlyList<string> Folders,
    [Summary("The room the files and folders take, counted in blocks of 4 KB as the allowance is")]
    long UsedBytes,
    [Summary("The room the workspace may take: the data allowance of the lambda's tier, less what its database takes")]
    long QuotaBytes,
    [Summary("Whether the workspace is switched on; while it is off, it holds nothing and nothing can be written")]
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
