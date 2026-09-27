using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// Bytes already in hand, answered as they are.
/// </summary>
/// <remarks>
/// Written out because the resource helpers all read from somewhere - a file,
/// an assembly, a string - and this is none of those: a zip built in memory
/// a moment ago, or a picture read from the database. Writing either to a
/// temporary file so that something could read it back would be a worse way
/// to say the same thing.
/// </remarks>
public sealed class BinaryContent(byte[] content, string type) : IResponseContent
{

    public ulong? Length => (ulong)content.Length;

    public ContentType? Type => new(type);

    public ReadOnlyMemory<byte>? Encoding => null;

    /// <summary>
    /// Something stable to hang a 304 on: the same code packed twice is the
    /// same zip, so a browser that already has it does not fetch it again.
    /// </summary>
    public ValueTask<ulong?> CalculateChecksumAsync()
    {
        // FNV-1a over the bytes, which is plenty to tell one packing from
        // another and needs nothing brought in to do it
        var hash = 14695981039346656037UL;

        foreach (var value in content)
        {
            hash = (hash ^ value) * 1099511628211UL;
        }

        return new ValueTask<ulong?>(hash);
    }

    public async ValueTask WriteAsync(IResponseSink target)
        => await target.Stream.WriteAsync(content);

}
