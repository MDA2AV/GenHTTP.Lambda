namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// Where the resources of one lambda are kept: each once, under the SHA-256
/// of what it holds, for every version and feature of the lambda that ships it.
/// </summary>
/// <remarks>
/// A version names its resources by their hash rather than carrying them, so
/// that a website of a few thousand pages is a small file of code and names,
/// and its pages are files - copied where it goes online, never read into a
/// string, and kept once however many versions ship them unchanged.
/// </remarks>
public interface IResourceStore
{

    /// <summary>
    /// Keeps what a resource holds, and says what it is kept as.
    /// </summary>
    StoredContent Keep(ReadOnlySpan<byte> content);

    /// <summary>
    /// Whether something is kept under the hash.
    /// </summary>
    bool Has(string sha256);

    /// <summary>
    /// Reads what is kept under the hash.
    /// </summary>
    Stream Open(string sha256);

    /// <summary>
    /// Writes what is kept under the hash into a file, replacing it.
    /// </summary>
    void CopyTo(string sha256, string path);

}

/// <summary>
/// A resource as a lambda's resource store keeps it.
/// </summary>
/// <param name="Sha256">The hash of what it holds, in lowercase hex - the name it is kept under</param>
/// <param name="Length">How many bytes it holds</param>
/// <param name="Store">
/// Where it is kept. Absent where the files were read without the store, which
/// can still name, count and compare them, and say so when asked for more.
/// </param>
public sealed record StoredContent(string Sha256, long Length, IResourceStore? Store)
{

    public Stream Open() => Available.Open(Sha256);

    public byte[] ReadAll()
    {
        using var source = Open();

        using var target = new MemoryStream(checked((int)Length));

        source.CopyTo(target);

        return target.ToArray();
    }

    public void CopyTo(string path) => Available.CopyTo(Sha256, path);

    private IResourceStore Available
        => Store ?? throw new InvalidOperationException($"The resource kept as {Sha256} was read without the store that keeps it, which has to be handed to LambdaSource.Parse to read what it holds.");

    /// <summary>
    /// Whether a text is a hash a resource can be kept under: 64 lowercase hex digits.
    /// </summary>
    /// <remarks>
    /// Checked before a hash becomes part of a path, so that what an envelope
    /// says can never name a file outside the store.
    /// </remarks>
    public static bool IsHash(string? text)
        => text is { Length: 64 } && text.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

}
