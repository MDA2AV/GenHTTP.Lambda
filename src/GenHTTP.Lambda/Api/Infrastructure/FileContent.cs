using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// A file on the disk, answered as it is.
/// </summary>
/// <remarks>
/// Streamed rather than read, so a file costs a buffer while it is sent,
/// however large it is - which is the point of answering with it instead of
/// its bytes. Always sent as bytes to download: the API shares its origin with
/// the editor, and a page somebody uploaded must not run there.
/// </remarks>
public sealed class FileContent(FileInfo file) : IResponseContent
{

    public ulong? Length => (ulong)file.Length;

    public ContentType? Type => new("application/octet-stream");

    public ReadOnlyMemory<byte>? Encoding => null;

    public ValueTask<ulong?> CalculateChecksumAsync()
        => new((ulong)file.LastWriteTimeUtc.Ticks ^ (ulong)file.Length);

    public async ValueTask WriteAsync(IResponseSink target)
    {
        await using var source = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 81920, true);

        await source.CopyToAsync(target.Stream);
    }

}
