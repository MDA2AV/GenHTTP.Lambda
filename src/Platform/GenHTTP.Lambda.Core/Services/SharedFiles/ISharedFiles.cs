using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// The files whoever runs the installation shares across it, as whatever
/// uses them reads them: the pages their fonts, agents the texts the guide
/// names, anybody what an app links to.
/// </summary>
/// <remarks>
/// Apart from changing them (<see cref="ISharedFileService"/>), which has to
/// know who uses a file: the branding and the guide read from here, and are
/// asked there.
/// </remarks>
public interface ISharedFiles
{

    /// <summary>
    /// Every shared file, by name.
    /// </summary>
    ValueTask<IReadOnlyList<SharedFile>> ListAsync(CancellationToken cancellation = default);

    /// <summary>
    /// The file of the given name, or nothing.
    /// </summary>
    ValueTask<SharedFile?> FindAsync(string name, CancellationToken cancellation = default);

    /// <summary>
    /// What a file holds - nothing where it was replaced or removed meanwhile.
    /// </summary>
    ValueTask<byte[]?> ReadAsync(SharedFile file, CancellationToken cancellation = default);

    /// <summary>
    /// A text, for an agent to read.
    /// </summary>
    /// <exception cref="LambdaException">There is no such file, it is no text, or larger than a tool's answer</exception>
    ValueTask<string> ReadTextAsync(string name, CancellationToken cancellation = default);

}
