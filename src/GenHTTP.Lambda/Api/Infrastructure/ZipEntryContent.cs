using System.IO.Compression;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// One file of a zip on the disk, sent as it is.
/// </summary>
/// <remarks>
/// Read out of the zip while it is sent rather than beforehand, so an asset of
/// a hundred megabytes costs a buffer rather than a hundred megabytes. The zip
/// is opened again for the sending, which may be after the answer was decided
/// on and the zip it was decided with let go of.
/// </remarks>
/// <param name="archive">Where the zip is</param>
/// <param name="entry">The file's full name inside it</param>
/// <param name="length">How long it is, unpacked</param>
public sealed class ZipEntryContent(string archive, string entry, long length, string type) : IResponseContent
{

    public ulong? Length => (ulong)length;

    public ContentType? Type => new(type);

    public ReadOnlyMemory<byte>? Encoding => null;

    /// <summary>
    /// The zip is named after everything it was packed from, and so is what
    /// is in it: its name and the entry's are the file, hashed the same way on
    /// every run so a browser's copy stays good across a restart.
    /// </summary>
    public ValueTask<ulong?> CalculateChecksumAsync()
    {
        var hash = 14695981039346656037UL;

        foreach (var character in $"{Path.GetFileName(archive)}\n{entry}\n{length}")
        {
            hash = (hash ^ character) * 1099511628211UL;
        }

        return new(hash);
    }

    public async ValueTask WriteAsync(IResponseSink target)
    {
        using var zip = ZipFile.OpenRead(archive);

        var found = zip.GetEntry(entry) ?? throw new FileNotFoundException($"'{entry}' is no longer in the source it was found in.");

        await using var source = found.Open();

        await source.CopyToAsync(target.Stream);
    }

}
