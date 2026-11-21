namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// The MCP endpoint of whoever runs the installation, at <c>/admin/mcp</c>:
/// what the administration panel shows and does, as tools an agent of the
/// operator's calls with the admin token.
/// </summary>
/// <remarks>
/// A door of its own beside <c>/mcp</c> rather than tools on it: every agent
/// is shown the public endpoint's tools, and none of them should be shown what
/// it could only use with a token it should never have been given. Here the
/// token is a header the client sends with every call, checked in front of the
/// tools as the REST API checks it, so it is never an argument that lands in
/// the agent's transcript.
///
/// Put in front of the panel by the host, taking the one path it serves and
/// leaving every other to the panel (see <see cref="OperatorMcpBuilder"/>).
/// </remarks>
/// <example>
/// <code>
/// Layout.Create().Add("admin", AdminPanel.Create(platform).Add(OperatorMcp.Create(platform)));
/// </code>
/// </example>
public static class OperatorMcp
{

    /// <summary>
    /// The operator's endpoint over the given platform.
    /// </summary>
    public static OperatorMcpBuilder Create(LambdaPlatform platform) => new(platform);

}
