using GenHTTP.Api.Content;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Mcp;

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

        return Concerns.Chain(_concerns, McpEndpoint.Create(platform.Options, tools).Build());
    }

    #endregion

}
