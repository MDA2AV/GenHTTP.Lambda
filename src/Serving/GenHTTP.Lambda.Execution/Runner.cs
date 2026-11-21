namespace GenHTTP.Lambda.Execution;

/// <summary>
/// Serves the lambdas of the platform, and hands every other request to the
/// site it is given.
/// </summary>
/// <remarks>
/// It goes around the site rather than into it: a lambda at a host of its own
/// - its own domain, its subdomain of the hosting domain - owns every path
/// there, so <c>shop.example.com/api</c> is the shop's and never the
/// platform's API. On the platform's own host it takes the previews of
/// features, below <c>/features/</c>, and the lambdas below <c>/lambda/</c>
/// where they answer by path, and leaves the rest to the site.
/// </remarks>
/// <example>
/// <code>
/// Host.Create().Handler(Runner.Create(platform).Site(site));
/// </code>
/// </example>
public static class Runner
{

    /// <summary>
    /// The lambdas of the given platform.
    /// </summary>
    public static RunnerBuilder Create(LambdaPlatform platform) => new(platform);

}
