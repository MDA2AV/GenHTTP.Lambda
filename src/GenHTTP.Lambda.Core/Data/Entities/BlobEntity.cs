namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What a piece of content the store of a lambda keeps is, without reading it.
/// </summary>
/// <remarks>
/// The content itself is a file below <c>code/{lambda}/blobs/</c>, named by
/// its <see cref="Hash"/>; which versions and features hold it, their
/// manifests on disk say. The ids git gives it are kept here rather than in
/// its name, so how git names content - SHA-1 now, SHA-256 once its object
/// format is used - is git's concern and decides nothing about the store.
/// A row is what is known about content the store has: one that is lost is
/// made again from the content when it is next kept.
/// </remarks>
public sealed class BlobEntity
{

    public long LambdaId { get; set; }

    /// <summary>
    /// The SHA-256 of the content, in lowercase hex: its name in the store.
    /// </summary>
    public required string Hash { get; set; }

    /// <summary>
    /// How many bytes it holds.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Its length in characters where it is UTF-8 text without a NUL
    /// character, and nothing where it is not text.
    /// </summary>
    public long? Chars { get; set; }

    /// <summary>
    /// The id git gives it as a blob, SHA-1, in lowercase hex.
    /// </summary>
    public required string GitSha1 { get; set; }

    /// <summary>
    /// The id git's SHA-256 object format gives it as a blob, in lowercase hex.
    /// </summary>
    public required string GitSha256 { get; set; }

    public LambdaEntity? Lambda { get; set; }

}
