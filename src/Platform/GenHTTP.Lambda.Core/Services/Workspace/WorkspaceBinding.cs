using GenHTTP.Api.Content.IO;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// What the workspace class generated into a lambda calls, once the lambda is
/// loaded: one function per thing its code can do with its files, awaited and
/// plain.
/// </summary>
/// <remarks>
/// Functions rather than an object, because the lambda is compiled without
/// the platform's assemblies and could name none of their types; what crosses
/// is what it can name anyway - strings, bytes, tasks, a tree of GenHTTP's.
/// Each throws what code reading files expects, with what to do about it.
///
/// Both forms, since the lambda keeps both: <c>Workspace.ReadTextAsync</c>
/// and the rest, which is what agents are taught, and the plain calls that
/// code written before makes - which wait where the files integration does
/// not answer at once. A tree is made, not read, and has a plain form only.
/// </remarks>
public sealed record WorkspaceBinding(
    Func<string, bool> Exists,
    Func<string, byte[]> Read,
    Action<string, byte[]> Write,
    Action<string> Delete,
    Func<string[]> List,
    Func<string[]> Folders,
    Action<string> CreateFolder,
    Func<string?, IResourceTree> Tree,
    Func<string, CancellationToken, ValueTask<bool>> ExistsAsync,
    Func<string, CancellationToken, ValueTask<byte[]>> ReadAsync,
    Func<string, byte[], CancellationToken, ValueTask> WriteAsync,
    Func<string, CancellationToken, ValueTask> DeleteAsync,
    Func<CancellationToken, ValueTask<string[]>> ListAsync,
    Func<CancellationToken, ValueTask<string[]>> FoldersAsync,
    Func<string, CancellationToken, ValueTask> CreateFolderAsync
);
