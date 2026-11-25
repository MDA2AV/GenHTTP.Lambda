using GenHTTP.Api.Content;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Hosting;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// Configures the operator's MCP endpoint, which the host puts in front of
/// the panel: <c>AdminPanel.Create(platform).Add(OperatorMcp.Create(platform))</c>.
/// </summary>
/// <remarks>
/// A concern rather than a handler of its own beside the panel: a layout at
/// <c>/admin</c> would answer <c>/admin</c> with a redirect to <c>/admin/</c>
/// before the panel is asked, where the panel answers it itself. The concern
/// takes the one segment it serves and hands everything else to the panel
/// untouched; neither serves the other's content.
///
/// On an installation without an admin token there is no operator, so there
/// is no endpoint either: <c>/admin/mcp</c> is then not found - as the REST
/// API's operator routes are not - rather than handed on to the panel, which
/// would answer it with a page.
/// </remarks>
public sealed class OperatorMcpBuilder(LambdaPlatform platform) : IConcernBuilder
{

    #region Functionality

    public IConcern Build(IHandler content)
    {
        var options = platform.Options;

        if (!options.Administrable)
        {
            return new OperatorRoute(content, null);
        }

        var services = platform.Services;

        // once, rather than per call: the tools keep nothing of a call's
        var lambdas = ActivatorUtilities.CreateInstance<LambdaOperatorTools>(services);
        var installation = ActivatorUtilities.CreateInstance<InstallationOperatorTools>(services);
        var monitoring = ActivatorUtilities.CreateInstance<MonitoringOperatorTools>(services);

        var files = ActivatorUtilities.CreateInstance<SharedFileTools>(services);

        // the installation's name, logo, colours and fonts, where its host
        // lets the operator give it some - and no tool for it where it does not
        var branding = options.Branding ? ActivatorUtilities.CreateInstance<BrandingTools>(services) : null;

        // the token is checked in front of the tools, as the REST API checks
        // it: 401 without one, 403 with another
        var endpoint = Concerns.Chain([AdminAuthentication.Create(options)], OperatorEndpoint.Create(lambdas, installation, monitoring, files, branding).Build());

        return new OperatorRoute(content, endpoint);
    }

    #endregion

}
