namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// The private directory of a lambda, as seen from outside it.
/// </summary>
/// <remarks>
/// The same directory the generated workspace class writes to from within a
/// lambda, and bound by the same rules - so a file put here by hand behaves
/// exactly like one the lambda wrote itself.
///
/// Every call can name a feature instead, and then works on that feature's
/// copy of the workspace - the one its preview reads and writes - under the
/// same rules and the same quota.
/// </remarks>
public interface IWorkspaceService
{

    /// <summary>
    /// Lists the files of a workspace with what is left of its quota.
    /// </summary>
    WorkspaceListing List(long lambdaId, long? featureId = null);

    /// <summary>
    /// Makes a folder, so that files can be put into it afterwards.
    /// </summary>
    void CreateFolder(long lambdaId, string path, long? featureId = null);

    /// <summary>
    /// Reads a single file, or null if there is none by that name.
    /// </summary>
    ValueTask<WorkspaceContent?> ReadAsync(long lambdaId, string path, long? featureId = null, CancellationToken cancellation = default);

    /// <summary>
    /// Reads a single file as base64, or null if there is none by that name.
    /// A file larger than travels that way comes back without its content.
    /// </summary>
    ValueTask<WorkspaceEncoded?> ReadEncodedAsync(long lambdaId, string path, long? featureId = null, CancellationToken cancellation = default);

    /// <summary>
    /// Finds a single file to stream as it is, or null if there is none by
    /// that name.
    /// </summary>
    FileInfo? Find(long lambdaId, string path, long? featureId = null);

    /// <summary>
    /// Writes a file, replacing it if it exists.
    /// </summary>
    /// <param name="expected">How many bytes the sender said it would send. An upload that ends short of it broke off, and is not kept</param>
    ValueTask<WorkspaceEntry> WriteAsync(long lambdaId, string path, Stream content, long? expected = null, long? featureId = null,
                                         CancellationToken cancellation = default);

    /// <summary>
    /// Writes a file given as base64, replacing it if it exists.
    /// </summary>
    ValueTask<WorkspaceEntry> WriteEncodedAsync(long lambdaId, string path, string? content, long? featureId = null,
                                                CancellationToken cancellation = default);

    /// <summary>
    /// Removes a file, if it is there.
    /// </summary>
    void Delete(long lambdaId, string path, long? featureId = null);

    /// <summary>
    /// Removes everything in the workspace, for when it is switched off.
    /// </summary>
    void Clear(long lambdaId, long? featureId = null);

}
