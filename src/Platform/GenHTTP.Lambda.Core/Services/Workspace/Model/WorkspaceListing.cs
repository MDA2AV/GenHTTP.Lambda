using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Workspace;

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
