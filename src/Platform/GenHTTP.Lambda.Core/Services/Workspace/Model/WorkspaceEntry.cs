using GenHTTP.Api.Content.Documentation;

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
