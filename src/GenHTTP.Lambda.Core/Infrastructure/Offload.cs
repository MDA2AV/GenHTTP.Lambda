namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// Runs a long piece of work away from the thread that serves the request.
/// </summary>
/// <remarks>
/// On the ioxide engine a request runs on a reactor: one thread per core that
/// owns that core's connections, and resumes each request where its I/O
/// completed. Whatever a request does on it, every other connection of that
/// core waits for - which is nothing for a lookup and seconds for compiling a
/// lambda. So work that takes long goes to the thread pool in one hop and the
/// request resumes on its reactor afterwards (ioxide posts the continuation
/// back), instead of being done in place or in many small asynchronous steps,
/// each of which would hop out and back again.
///
/// What it is for: compiling and binding code with Roslyn, packing and
/// unpacking, reading or writing a file that may be large. What it is not
/// for: a query against SQLite or a small file, which take less than the hop.
/// On Kestrel it is a hop to the pool and nothing more.
///
/// The only way to the pool: <c>Task.Run</c> is refused when the server is
/// compiled (BannedSymbols.txt), so every hop is marked as one here, and
/// this is where they could be given a scheduler of their own - a bounded
/// one for compiling, say - without looking for them. Work that is started in
/// the background rather than awaited by a request is not a hop and says so
/// where it starts.
/// </remarks>
public static class Offload
{

#pragma warning disable RS0030 // this is what everything else goes through

    public static Task<T> Run<T>(Func<T> work, CancellationToken cancellation = default) => Task.Run(work, cancellation);

    public static Task<T> Run<T>(Func<Task<T>> work, CancellationToken cancellation = default) => Task.Run(work, cancellation);

    public static Task<T> Run<T>(Func<ValueTask<T>> work, CancellationToken cancellation = default)
        => Task.Run(async () => await work(), cancellation);

    public static Task Run(Func<Task> work, CancellationToken cancellation = default) => Task.Run(work, cancellation);

    public static Task Run(Action work, CancellationToken cancellation = default) => Task.Run(work, cancellation);

#pragma warning restore RS0030

}
