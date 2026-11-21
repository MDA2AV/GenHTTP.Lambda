namespace GenHTTP.Lambda.Server;

/// <summary>
/// The engine the root web server is hosted with, selected via <c>LAMBDA_ENGINE</c>.
/// </summary>
public enum LambdaEngine
{

    /// <summary>
    /// The io_uring based engine, the fastest option on Linux.
    /// </summary>
    /// <remarks>
    /// Needs a host that allows io_uring: container runtimes commonly block the
    /// syscall in their default seccomp profile, and it runs on Linux only. So
    /// it is chosen where it is known to work - genhttp.dev names it - and
    /// never fallen back to.
    /// </remarks>
    Ioxide,

    /// <summary>
    /// The Kestrel based engine, which runs wherever ASP.NET Core runs.
    /// </summary>
    Kestrel,

    /// <summary>
    /// GenHTTP's own engine on .NET sockets, which runs wherever .NET runs.
    /// The default.
    /// </summary>
    /// <remarks>
    /// The default because it asks the least of where it runs: nothing a
    /// container runtime withholds, no ASP.NET Core, any operating system. It
    /// speaks HTTP/1.1 only, whatever <c>LAMBDA_HTTP_PROTOCOLS</c> says beyond it.
    /// </remarks>
    Internal

}
