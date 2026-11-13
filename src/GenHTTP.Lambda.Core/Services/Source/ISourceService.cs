using System.Net;

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
/// version's code, resources, documentation and tests, as the export packs them,
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
    /// A ticket to star the source at a public key with, as the page that
    /// reads it is handed.
    /// </summary>
    string IssueTicket(string publicKey);

    /// <summary>
    /// Stars a published source for a visitor, or takes their star back.
    /// Nothing where there is no such source.
    /// </summary>
    /// <remarks>
    /// Refused where the ticket is too fresh or too old, and where the visitor
    /// changed a lot of stars lately. Each visitor stars a source once, so
    /// starring again changes nothing (see <see cref="StarGuard"/>).
    /// </remarks>
    /// <param name="ticket">The ticket the page was handed when it read the source</param>
    /// <param name="client">Who is starring</param>
    StarOutcome? Star(string publicKey, string? ticket, IPAddress? client, bool starred);

    /// <summary>
    /// Every published source, for the sitemap.
    /// </summary>
    IReadOnlyList<SourceAddress> ListAddresses();

    #endregion

}
