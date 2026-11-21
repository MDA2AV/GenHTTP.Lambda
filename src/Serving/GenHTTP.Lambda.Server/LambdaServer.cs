namespace GenHTTP.Lambda.Server;

/// <summary>
/// Runs a platform on an engine until the process is told to stop.
/// </summary>
/// <example>
/// <code>
/// return await LambdaServer.Create(platform)
///                          .Handler(Runner.Create(platform).Site(site))
///                          .RunAsync();
/// </code>
/// </example>
public static class LambdaServer
{

    /// <summary>
    /// A server for the given platform.
    /// </summary>
    public static LambdaServerBuilder Create(LambdaPlatform platform) => new(platform);

}
