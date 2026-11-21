using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// A file on disk: kept by the store of a lambda, or staged to be kept by it.
/// </summary>
/// <remarks>
/// Staged content moves into the store as it is kept, and is read from there
/// from then on, by whoever still holds the file it was staged as.
/// </remarks>
internal sealed class StoredContent(string path, string hash, ContentReads? reads, bool staged, ContentFacts? facts = null) : FileContent
{

    /// <summary>
    /// What was read off staged content as it was staged, so it is not read again to be kept.
    /// </summary>
    public ContentFacts? Facts => facts;

    public string Path { get; private set; } = path;

    public bool Staged { get; private set; } = staged;

    /// <summary>
    /// What counts the reads of the store it is kept by; nothing while it is staged.
    /// </summary>
    public ContentReads? Reads { get; private set; } = reads;

    public override Stream OpenRead() => new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 81920);

    public override byte[] ReadBytes() => File.ReadAllBytes(Path);

    public override void CopyTo(string path) => File.Copy(Path, path, true);

    public override string Hash() => hash;

    /// <summary>
    /// Says that what was staged is kept by the store now, at its place there.
    /// </summary>
    public void Kept(string path, ContentReads reads)
    {
        Path = path;
        Reads = reads;
        Staged = false;
    }

}
