using GenHTTP.Api.Content;

using Microsoft.Extensions.DependencyInjection;

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

/// <summary>
/// Configures the MCP endpoint.
/// </summary>
/// <remarks>
/// The browsers it may be driven from are the platform's own names (see
/// <c>LambdaOptions.McpOrigins</c>), since they are the names the platform
/// answers at as well.
/// </remarks>
public sealed class McpServerBuilder(LambdaPlatform platform) : IHandlerBuilder<McpServerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    #region Functionality

    public McpServerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        // once, rather than per call: the tools keep nothing of a call's
        var tools = ActivatorUtilities.CreateInstance<McpTools>(platform.Services);

        // what only the operator changes about the installation, where the
        // host lets them and there is a token to let them in with - nobody
        // else is shown a tool they could not use
        var options = platform.Options;

        var installation = options.Branding && options.Administrable ? ActivatorUtilities.CreateInstance<InstallationTools>(platform.Services) : null;

        return Concerns.Chain(_concerns, McpEndpoint.Create(options, tools, installation).Build());
    }

    #endregion

}
