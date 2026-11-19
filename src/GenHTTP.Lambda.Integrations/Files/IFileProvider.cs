using GenHTTP.Api.Content.IO;

namespace GenHTTP.Lambda.Integrations.Files;

/// <summary>
/// Where files are kept: the workspace of every lambda and the copy a feature
/// works on, and the pictures the platform keeps for itself - its logo, the
/// pictures of the showcase.
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
/// a provider without them keeps a marker for each. Synchronous but for an
/// upload, which arrives as it is sent: a lambda writes a file with a plain
/// call, wherever it happens to be.
/// </para>
/// <para>
/// The platform's own files are in named stores (<see cref="FileStores"/>),
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
    byte[]? Read(string store, string name);

    /// <summary>
    /// Writes a file into one of the platform's stores, replacing it where
    /// it is there - whole, or not at all.
    /// </summary>
    void Write(string store, string name, ReadOnlySpan<byte> content);

    /// <summary>
    /// Removes a file from one of the platform's stores, where it is there.
    /// </summary>
    void Delete(string store, string name);

    #endregion

    #region Reading

    /// <summary>
    /// Every file and folder in a workspace, none where it has none.
    /// </summary>
    IReadOnlyList<FileEntry> List(DataScope scope);

    /// <summary>
    /// The file or folder at a path, or nothing.
    /// </summary>
    FileEntry? Find(DataScope scope, string path);

    /// <summary>
    /// The content of a file, or nothing where there is no file at the path.
    /// </summary>
    byte[]? Read(DataScope scope, string path);

    /// <summary>
    /// A file as it is served, without reading it first - or nothing where
    /// there is no file at the path.
    /// </summary>
    IResource? Resource(DataScope scope, string path);

    /// <summary>
    /// A folder of the workspace, or all of it, as it is served.
    /// </summary>
    /// <param name="folder">The folder, which the caller checked is there, or nothing for the whole workspace</param>
    IResourceTree Tree(DataScope scope, string? folder);

    #endregion

    #region Writing

    /// <summary>
    /// Writes a file, replacing it where it is there, with the folders it goes
    /// into - whole, or not at all.
    /// </summary>
    void Write(DataScope scope, string path, ReadOnlySpan<byte> content);

    /// <summary>
    /// Writes a file as it arrives, replacing it where it is there, with the
    /// folders it goes into - whole, or not at all: a content that fails while
    /// it is read leaves what was there, and no folder it would have gone into.
    /// </summary>
    ValueTask WriteAsync(DataScope scope, string path, Stream content, CancellationToken cancellation = default);

    /// <summary>
    /// Makes a folder, with the folders it is in.
    /// </summary>
    void CreateFolder(DataScope scope, string path);

    /// <summary>
    /// Removes a file, or a folder with everything in it - nothing where
    /// there is neither.
    /// </summary>
    void Delete(DataScope scope, string path);

    /// <summary>
    /// Removes everything in a workspace.
    /// </summary>
    void Clear(DataScope scope);

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
    void Copy(DataScope source, DataScope target, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a workspace altogether, such as a feature's copy.
    /// </summary>
    void Delete(DataScope scope);

    /// <summary>
    /// Removes the workspace of a lambda and every copy of its features.
    /// </summary>
    void DeleteAll(long lambdaId);

    #endregion

}

/// <summary>
/// The stores the platform keeps its own files in.
/// </summary>
public static class FileStores
{

    /// <summary>
    /// What the installation presents itself with: its logo.
    /// </summary>
    public const string Branding = "branding";

    /// <summary>
    /// The pictures of the showcase, one per lambda that is listed there.
    /// </summary>
    public const string Showcase = "showcase";

}

/// <summary>
/// A file or folder of a workspace.
/// </summary>
/// <param name="Path">Relative to the workspace, its folders separated by slashes</param>
/// <param name="Folder">Whether it is a folder</param>
/// <param name="Length">How many bytes a file holds, none for a folder</param>
/// <param name="Modified">When a file was last written, in UTC</param>
public sealed record FileEntry(string Path, bool Folder, long Length, DateTime Modified);
