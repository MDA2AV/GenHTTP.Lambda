namespace GenHTTP.Lambda.Server;

/// <summary>
/// The engine the root web server is hosted with, selected via <c>LAMBDA_ENGINE</c>.
/// </summary>
public enum LambdaEngine
{

    /// <summary>
    /// The io_uring based engine. The default, and the fastest option on Linux.
    /// </summary>
    /// <remarks>
    /// Needs a host that allows io_uring: container runtimes commonly block the
    /// syscall in their default seccomp profile.
    /// </remarks>
    Ioxide,

    /// <summary>
    /// The Kestrel based engine, which runs wherever ASP.NET Core runs.
    /// </summary>
    Kestrel,

    /// <summary>
    /// GenHTTP's own engine on .NET sockets, which runs wherever .NET runs.
    /// </summary>
    /// <remarks>
    /// What the community image runs on: it needs nothing a container runtime
    /// withholds. It speaks HTTP/1.1 only, whatever <c>LAMBDA_HTTP_PROTOCOLS</c>
    /// says beyond it.
    /// </remarks>
    Internal

}
