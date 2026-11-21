using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// An upload as the files integration reads it: refused the moment it grows
/// past the room it may take, and refused at its end where it ended short.
/// </summary>
/// <remarks>
/// Refused by failing to be read, so whatever the integration was writing it
/// into is thrown away by the integration - which writes a file whole or not
/// at all - and nothing of a refused upload is left. A connection the engine
/// dropped looks like a body that ended; kept, it would be a truncated file
/// passing for the whole one.
/// </remarks>
/// <param name="room">How much room the file may take, in blocks as the quota counts it</param>
/// <param name="expected">How many bytes the sender said it would send, if it said</param>
/// <param name="full">What a file past its room is refused with</param>
internal sealed class GuardedUpload(Stream content, long room, long? expected, Func<LambdaException> full) : Stream
{

    #region Get-/Setters

    /// <summary>
    /// How many bytes arrived so far.
    /// </summary>
    public long Received { get; private set; }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => Received;
        set => throw new NotSupportedException();
    }

    #endregion

    #region Functionality

    public override int Read(byte[] buffer, int offset, int count) => Count(content.Read(buffer, offset, count));

    public override int Read(Span<byte> buffer) => Count(content.Read(buffer));

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => Count(await content.ReadAsync(buffer, cancellationToken));

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => Count(await content.ReadAsync(buffer.AsMemory(offset, count), cancellationToken));

    private int Count(int read)
    {
        Received += read;

        // an empty file still takes a block, which is why this is asked at
        // the end as well as on the way
        if (WorkspaceLimits.Footprint(Received) > room)
        {
            throw full();
        }

        if (read == 0 && expected is { } length && Received != length)
        {
            throw LambdaException.Invalid($"The upload ended after {Received:N0} of {length:N0} bytes and was not kept.");
        }

        return read;
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    #endregion

}
