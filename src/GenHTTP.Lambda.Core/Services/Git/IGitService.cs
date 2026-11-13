using GenHTTP.Modules.Git;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// Every lambda as a git repository: cloned, fetched and pushed to by whoever
/// holds its editor key, and cloned by anybody once its source is published.
/// </summary>
/// <remarks>
/// The repository is the project the lambda is exported as. Its versions are
/// the commits of main, tagged <c>v1</c>, <c>v2</c> and so on, and each of its
/// features is a branch. A commit pushed to main becomes a version, a branch
/// pushed a feature. The data is never in it - nor what only the owner may
/// know - so the commits the owner reads are those a published source shows.
/// </remarks>
public interface IGitService
{

    /// <summary>
    /// The repository of the lambda behind an editor key, to read and push to,
    /// or nothing when no lambda has the key.
    /// </summary>
    /// <param name="origin">The address it was reached at, for the addresses a push answers with</param>
    ValueTask<IGitRepository?> OpenAsync(string privateKey, string origin, CancellationToken cancellation = default);

    /// <summary>
    /// The published source at a public key, to read, or nothing where none
    /// is published.
    /// </summary>
    /// <param name="origin">The address it was reached at</param>
    ValueTask<IGitRepository?> ReadAsync(string publicKey, string origin, CancellationToken cancellation = default);

}
