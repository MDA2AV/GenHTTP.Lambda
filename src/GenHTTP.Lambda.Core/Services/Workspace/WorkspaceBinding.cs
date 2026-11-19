using GenHTTP.Api.Content.IO;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// What the workspace class generated into a lambda calls, once the lambda is
/// loaded: one function per thing its code can do with its files.
/// </summary>
/// <remarks>
/// Functions rather than an object, because the lambda is compiled without
/// the platform's assemblies and could name none of their types; what crosses
/// is what it can name anyway - strings, bytes, a tree of GenHTTP's. Each
/// throws what code reading files expects, with what to do about it.
/// </remarks>
public sealed record WorkspaceBinding(
    Func<string, bool> Exists,
    Func<string, byte[]> Read,
    Action<string, byte[]> Write,
    Action<string> Delete,
    Func<string[]> List,
    Func<string[]> Folders,
    Action<string> CreateFolder,
    Func<string?, IResourceTree> Tree
);
