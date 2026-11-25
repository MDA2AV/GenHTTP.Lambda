namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// The files whoever runs the installation shares across it, as they manage
/// them.
/// </summary>
/// <remarks>
/// A place for a few small files that are nobody's lambda's: fonts and
/// pictures for the installation's own pages, a style guide for the agents
/// that build on it, a style sheet for their apps. Public, each at an address
/// of its own.
/// </remarks>
public interface ISharedFileService
{

    /// <summary>
    /// Every shared file with what of the installation uses it, by name.
    /// </summary>
    ValueTask<IReadOnlyList<SharedFileUsage>> ListAsync(CancellationToken cancellation = default);

    /// <summary>
    /// Shares a file, or replaces the one of that name: whatever uses it has
    /// the new one from then on.
    /// </summary>
    /// <returns>The file as it is kept, and whether it replaced another</returns>
    ValueTask<(SharedFileUsage File, bool Replaced)> WriteAsync(string name, byte[] content, CancellationToken cancellation = default);

    /// <summary>
    /// Takes a file away. Refused while the branding or the guide uses it.
    /// </summary>
    /// <returns>The file that was there</returns>
    ValueTask<SharedFile> DeleteAsync(string name, CancellationToken cancellation = default);

}
