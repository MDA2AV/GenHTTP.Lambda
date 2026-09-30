using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// A lambda packed into a project, sent from the file it was packed into and
/// deleted once it is.
/// </summary>
/// <remarks>
/// Packed into a file rather than into memory, because a project carries the
/// database of the lambda along, and a database may be as large as the tier
/// allows. Streamed from there, so it costs a buffer while it is sent.
/// </remarks>
public sealed class ExportContent(string archive) : IResponseContent
{

    public ulong? Length => (ulong)new FileInfo(archive).Length;

    public ContentType? Type => new("application/zip");

    public ReadOnlyMemory<byte>? Encoding => null;

    // packed anew each time, with the data as it is then
    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong?)null);

    public async ValueTask WriteAsync(IResponseSink target)
    {
        await using var source = new FileStream(archive, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 81920,
                                                FileOptions.Asynchronous | FileOptions.DeleteOnClose);

        await source.CopyToAsync(target.Stream);
    }

}
