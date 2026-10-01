namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// The lambdas whose owners published their source code, and that code as
/// anybody else reads and downloads it.
/// </summary>
/// <remarks>
/// Kept apart from the meta service for the same reason as the showcase:
/// publishing a lambda is not part of building one, and nothing about writing,
/// saving or deploying code reads or changes anything here.
///
/// What is published is the program and what is written about it - every
/// version's code, assets, documentation and tests, as the export packs them,
/// with the license it is published under. Never the data: not the database,
/// not the workspace, not a secret's value. And nothing only the owner may
/// know: no editor key, no traffic, no log, and not what was asked for in the
/// owner's words - a version is listed with the change it made.
/// </remarks>
public interface ISourceService
{

    #region Owner

    /// <summary>
    /// Whether the lambda behind an editor key is published, or nothing when
    /// it never was.
    /// </summary>
    SourceSettings? Get(string privateKey);

    /// <summary>
    /// Publishes the source of a lambda, or changes the license it is
    /// published under.
    /// </summary>
    SourceSettings Publish(string privateKey, SourceDraft draft);

    /// <summary>
    /// Takes the source down. Its stars are kept for when it is published
    /// again; nothing happens when it was not published.
    /// </summary>
    SourceSettings? Withdraw(string privateKey);

    #endregion

    #region Public

    /// <summary>
    /// One page of the published sources.
    /// </summary>
    /// <param name="search">Words the key, the title or what it is about have to contain</param>
    SourceListing List(string? search, SourceOrder order, int skip, int take);

    /// <summary>
    /// The published source at a public key, or nothing where there is none.
    /// </summary>
    SourceProject? GetProject(string publicKey);

    /// <summary>
    /// A version of a published source, packed - or nothing where there is no
    /// such source or version.
    /// </summary>
    ValueTask<SourceArchive?> GetArchiveAsync(string publicKey, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Stars a published source, or takes a star back. Nothing where there is
    /// no such source.
    /// </summary>
    /// <returns>How many stars it has now</returns>
    int? Star(string publicKey, bool starred);

    /// <summary>
    /// How many stars a published source has, and the identity it is filed
    /// under for the guard that counts each visitor's star once. Nothing where
    /// there is no such source.
    /// </summary>
    SourceStars? GetStars(string publicKey);

    /// <summary>
    /// Every published source, for the sitemap.
    /// </summary>
    IReadOnlyList<SourceAddress> ListAddresses();

    #endregion

}
