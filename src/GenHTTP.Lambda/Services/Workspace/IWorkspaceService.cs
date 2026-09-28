namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// The private directory of a lambda, as seen from outside it.
/// </summary>
/// <remarks>
/// The same directory the generated workspace class writes to from within a
/// lambda, and bound by the same rules - so a file put here by hand behaves
/// exactly like one the lambda wrote itself.
/// </remarks>
public interface IWorkspaceService
{

    /// <summary>
    /// Lists the files of a workspace with what is left of its quota.
    /// </summary>
    ValueTask<WorkspaceListing> ListAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// Makes a folder, so that files can be put into it afterwards.
    /// </summary>
    ValueTask CreateFolderAsync(long lambdaId, string path, CancellationToken cancellation = default);

    /// <summary>
    /// Reads a single file, or null if there is none by that name.
    /// </summary>
    ValueTask<WorkspaceContent?> ReadAsync(long lambdaId, string path, CancellationToken cancellation = default);

    /// <summary>
    /// Finds a single file to stream as it is, or null if there is none by
    /// that name.
    /// </summary>
    ValueTask<FileInfo?> FindAsync(long lambdaId, string path, CancellationToken cancellation = default);

    /// <summary>
    /// Writes a file, replacing it if it exists.
    /// </summary>
    /// <param name="expected">How many bytes the sender said it would send. An upload that ends short of it broke off, and is not kept</param>
    ValueTask<WorkspaceEntry> WriteAsync(long lambdaId, string path, Stream content, long? expected = null, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a file, if it is there.
    /// </summary>
    ValueTask DeleteAsync(long lambdaId, string path, CancellationToken cancellation = default);

    /// <summary>
    /// Removes everything in the workspace, for when it is switched off.
    /// </summary>
    ValueTask ClearAsync(long lambdaId, CancellationToken cancellation = default);

}
