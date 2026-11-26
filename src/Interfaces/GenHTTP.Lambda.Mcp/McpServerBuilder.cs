using GenHTTP.Api.Content;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Agents;

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

        // a call of a run of the agent is the run's: found before the module reads the body
        return Concerns.Chain([.. _concerns, ServiceConcern.Of<AgentCallerConcern>(platform.Services)], McpEndpoint.Create(platform.Options, tools).Build());
    }

    #endregion

}
