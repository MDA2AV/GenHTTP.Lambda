using GenHTTP.Api.Content;

using GenHTTP.Lambda.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// Configures the operator's MCP endpoint.
/// </summary>
/// <remarks>
/// On an installation without an admin token there is no operator, so there
/// is no endpoint either: what this builds then answers nothing, and a request
/// is not found - as the REST API's operator routes are not, and never handed
/// on to the panel's pages beside it, which would answer it with a page.
/// </remarks>
public sealed class OperatorMcpBuilder(LambdaPlatform platform) : IHandlerBuilder<OperatorMcpBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    #region Functionality

    public OperatorMcpBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var options = platform.Options;

        if (!options.Administrable)
        {
            return Concerns.Chain(_concerns, new Absent());
        }

        var services = platform.Services;

        // once, rather than per call: the tools keep nothing of a call's
        var lambdas = ActivatorUtilities.CreateInstance<LambdaOperatorTools>(services);
        var installation = ActivatorUtilities.CreateInstance<InstallationOperatorTools>(services);
        var monitoring = ActivatorUtilities.CreateInstance<MonitoringOperatorTools>(services);

        // the installation's name and logo, where its host lets the operator
        // give it some - and no tool for it where it does not
        var branding = options.Branding ? ActivatorUtilities.CreateInstance<BrandingTools>(services) : null;

        // the token is checked in front of the tools, as the REST API checks
        // it: 401 without one, 403 with another
        return Concerns.Chain([.. _concerns, AdminAuthentication.Create(options)],
                              OperatorEndpoint.Create(lambdas, installation, monitoring, branding).Build());
    }

    #endregion

}
