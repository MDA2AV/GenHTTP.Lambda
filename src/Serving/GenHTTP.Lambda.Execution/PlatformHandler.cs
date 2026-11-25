namespace GenHTTP.Lambda.Execution;

/// <summary>
/// The platform as a handler, for a server that serves more than the platform:
/// a GenHTTP application of its own with an installation in it, or several.
/// </summary>
/// <remarks>
/// What <c>LambdaServer</c> is to a platform with a server for itself. A
/// request passes the same concerns in the same order - blocked addresses, the
/// addresses lambdas had, the log, the figures - but for what a server does to
/// every answer, which here is somebody else's to decide: GenHTTP's defaults
/// (compression, caching, the upgrade to HTTPS), the engine, the ports and the
/// certificates.
///
/// It is handed every request addressed to the platform, whole: the platform's
/// pages are at the root of a host and link to one another from there, so
/// installations are told apart by their hosts, not by a path in front of them.
/// Prepared like any handler before it is asked - which a server does to what
/// is there as it starts, and whoever adds a platform to one that runs does
/// themselves, followed by <see cref="LambdaPlatform.Start"/>.
/// </remarks>
/// <example>
/// <code>
/// var handler = PlatformHandler.Create(platform)
///                              .Handler(Runner.Create(platform).Site(site))
///                              .Build();
///
/// await handler.PrepareAsync(server);
///
/// platform.Start();
/// </code>
/// </example>
public static class PlatformHandler
{

    /// <summary>
    /// The given platform.
    /// </summary>
    public static PlatformHandlerBuilder Create(LambdaPlatform platform) => new(platform);

}
