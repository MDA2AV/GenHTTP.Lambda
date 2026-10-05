using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// The content of a response, written away from the reactor and copied to
/// the connection on it.
/// </summary>
/// <remarks>
/// For content whose writing is the work: a git server packs what a clone
/// asks for - reading, hashing and compressing every file - and applies a
/// push - unpacking it and compiling what it holds - while it writes its
/// answer. Written in place, that is seconds in which every other connection
/// of the reactor waits. So the content is written on the pool, in one hop
/// (see <see cref="Offload"/>), into a pipe, and the reactor only copies what
/// comes out of it to the connection - flushed as it comes, so a line a push
/// says along the way reaches whoever pushed as it is said.
///
/// The pipe holds a megabyte at most: a client reading slowly holds up the
/// writing, not the memory of the server.
/// </remarks>
public sealed class OffloadedContent(IResponseContent content) : IResponseContent
{
    private static readonly PipeOptions Options = new(pauseWriterThreshold: 1024 * 1024, resumeWriterThreshold: 512 * 1024);

    public ulong? Length => content.Length;

    public ContentType? Type => content.Type;

    public ReadOnlyMemory<byte>? Encoding => content.Encoding;

    public ValueTask<ulong?> CalculateChecksumAsync() => content.CalculateChecksumAsync();

    public async ValueTask WriteAsync(IResponseSink sink)
    {
        var pipe = new Pipe(Options);

        var writing = Offload.Run(async () =>
        {
            try
            {
                await using var stream = pipe.Writer.AsStream(leaveOpen: true);

                await content.WriteAsync(new Sink(pipe.Writer, stream));

                await pipe.Writer.CompleteAsync();
            }
            catch (Exception e)
            {
                await pipe.Writer.CompleteAsync(e);
            }
        });

        try
        {
            while (true)
            {
                var read = await pipe.Reader.ReadAsync();

                foreach (var segment in read.Buffer)
                {
                    await sink.Stream.WriteAsync(segment);
                }

                await sink.Stream.FlushAsync();

                pipe.Reader.AdvanceTo(read.Buffer.End);

                if (read.IsCompleted)
                {
                    break;
                }
            }
        }
        finally
        {
            // a client that went away leaves nobody to read, which the writing notices
            await pipe.Reader.CompleteAsync();

            await writing;
        }
    }

    private sealed class Sink(IBufferWriter<byte> writer, Stream stream) : IResponseSink
    {
        public IBufferWriter<byte> Writer => writer;

        public Stream Stream => stream;
    }

}
