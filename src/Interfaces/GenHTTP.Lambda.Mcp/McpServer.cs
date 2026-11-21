namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// The MCP endpoint of the platform, at <c>/mcp</c>: the tools an agent
/// creates, changes and publishes lambdas with.
/// </summary>
/// <example>
/// <code>
/// Layout.Create().Add("mcp", McpServer.Create(platform));
/// </code>
/// </example>
public static class McpServer
{

    /// <summary>
    /// The endpoint over the given platform.
    /// </summary>
    public static McpServerBuilder Create(LambdaPlatform platform) => new(platform);

}
