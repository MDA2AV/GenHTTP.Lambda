using GenHTTP.Api.Content.IO;

namespace GenHTTP.Lambda.Integrations.Files;

/// <summary>
/// Where files are kept: the workspace of every lambda and the copy a feature
/// works on, and what the platform keeps for itself - its logo, the pictures
/// of the showcase, the files its operator shares across the installation.
/// </summary>
/// <remarks>
/// <para>
/// A lambda writes and reads its files through the platform, which checks the
/// name, the room and whether the owner left the workspace on, and then asks
/// here - the same for what the lambda writes and what its owner or an agent
/// puts there. So a provider keeps files and nothing else: no quota, no
/// switch, no names it refuses.
/// </para>
/// <para>
/// A path is relative to the workspace, its folders separated by slashes, and
/// already checked: it stays inside, and is not empty. Folders are things of
/// their own - a lambda may make an empty one and rely on it being there - so
/// a provider without them keeps a marker for each. Whatever reaches where
/// the files are is awaited (see <see cref="IIntegration"/>): object storage
/// awaits the network, a directory on the disk answers at once. A whole
/// workspace - a copy - is asked for away from the thread a request is
/// served on.
/// </para>
/// <para>
/// The platform's own files are in named stores (<see cref="FileStore"/>),
/// as its records are in the stores of the database: a picture is a file,
/// never a row. Their names are the platform's, never anybody's input.
/// </para>
/// </remarks>
public interface IFileProvider : IIntegration
{

    #region The platform's own

    /// <summary>
    /// A file of one of the platform's stores, or nothing where there is none.
    /// </summary>
    ValueTask<byte[]?> ReadAsync(FileStore store, string name, CancellationToken cancellation = default);

    /// <summary>
    /// Writes a file into one of the platform's stores, replacing it where
    /// it is there - whole, or not at all.
    /// </summary>
    ValueTask WriteAsync(FileStore store, string name, ReadOnlyMemory<byte> content, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a file from one of the platform's stores, where it is there.
    /// </summary>
    ValueTask DeleteAsync(FileStore store, string name, CancellationToken cancellation = default);

    #endregion

    #region Reading

    /// <summary>
    /// Every file and folder in a workspace, none where it has none.
    /// </summary>
    ValueTask<IReadOnlyList<FileEntry>> ListAsync(DataScope scope, CancellationToken cancellation = default);

    /// <summary>
    /// The file or folder at a path, or nothing.
    /// </summary>
    ValueTask<FileEntry?> FindAsync(DataScope scope, string path, CancellationToken cancellation = default);

    /// <summary>
    /// The content of a file, or nothing where there is no file at the path.
    /// </summary>
    ValueTask<byte[]?> ReadAsync(DataScope scope, string path, CancellationToken cancellation = default);

    /// <summary>
    /// A file as it is served, without reading it first - or nothing where
    /// there is no file at the path.
    /// </summary>
    ValueTask<IResource?> ResourceAsync(DataScope scope, string path, CancellationToken cancellation = default);

    /// <summary>
    /// A folder of the workspace, or all of it, as it is served.
    /// </summary>
    /// <remarks>
    /// Made without reaching the files: a tree is read as it is served, which
    /// GenHTTP awaits.
    /// </remarks>
    /// <param name="folder">The folder, which the caller checked is there, or nothing for the whole workspace</param>
    IResourceTree Tree(DataScope scope, string? folder);

    #endregion

    #region Writing

    /// <summary>
    /// Writes a file, replacing it where it is there, with the folders it goes
    /// into - whole, or not at all.
    /// </summary>
    ValueTask WriteAsync(DataScope scope, string path, ReadOnlyMemory<byte> content, CancellationToken cancellation = default);

    /// <summary>
    /// Writes a file as it arrives, replacing it where it is there, with the
    /// folders it goes into - whole, or not at all: a content that fails while
    /// it is read leaves what was there, and no folder it would have gone into.
    /// </summary>
    ValueTask WriteAsync(DataScope scope, string path, Stream content, CancellationToken cancellation = default);

    /// <summary>
    /// Makes a folder, with the folders it is in.
    /// </summary>
    ValueTask CreateFolderAsync(DataScope scope, string path, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a file, or a folder with everything in it - nothing where
    /// there is neither.
    /// </summary>
    ValueTask DeleteAsync(DataScope scope, string path, CancellationToken cancellation = default);

    /// <summary>
    /// Removes everything in a workspace.
    /// </summary>
    ValueTask ClearAsync(DataScope scope, CancellationToken cancellation = default);

    #endregion

    #region Keeping them

    /// <summary>
    /// Replaces the workspace of one scope with a copy of another's,
    /// empty folders included - or with an empty one where the other has none.
    /// </summary>
    /// <remarks>
    /// The copy it replaces stays until the new one is complete. A whole
    /// workspace, so the platform asks for it away from the thread a request
    /// is served on.
    /// </remarks>
    ValueTask CopyAsync(DataScope source, DataScope target, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a workspace altogether, such as a feature's copy.
    /// </summary>
    ValueTask DeleteAsync(DataScope scope, CancellationToken cancellation = default);

    /// <summary>
    /// Removes the workspace of a lambda and every copy of its features.
    /// </summary>
    ValueTask DeleteAllAsync(long lambdaId, CancellationToken cancellation = default);

    #endregion

}
