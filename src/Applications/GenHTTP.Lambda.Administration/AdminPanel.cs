using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Web;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Administration;

/// <summary>
/// The operator's panel, at <c>/admin</c>: the lambdas of the installation,
/// its limits and settings, what it is doing and what it logged, and the
/// addresses it blocks.
/// </summary>
/// <remarks>
/// Pages only - a bundle of their own (Frontend/), in English - which call the
/// REST API's operator routes with the admin token the operator enters. The
/// token is the platform's (LambdaOptions.AdminToken): the API answers to it,
/// and the panel stays reachable from an address the operator blocked.
/// Without one, the panel says that it is switched off. Of the platform it
/// takes nothing but how the installation presents itself, which its page
/// names it after before a script of it runs, as every other page does.
/// </remarks>
/// <example>
/// <code>
/// Layout.Create().Add("admin", AdminPanel.Create(platform));
/// </code>
/// </example>
public static class AdminPanel
{

    /// <summary>
    /// The panel of the given platform.
    /// </summary>
    public static AdminPanelBuilder Create(LambdaPlatform platform) => new(platform);

}

/// <summary>
/// Configures the panel.
/// </summary>
public sealed class AdminPanelBuilder(LambdaPlatform platform) : IHandlerBuilder<AdminPanelBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private Web.Bundle _bundle = Web.Bundle.Beside("admin");

    #region Functionality

    /// <summary>
    /// Where the panel's bundle was built to, where it is not beside the host.
    /// </summary>
    public AdminPanelBuilder Bundle(string directory)
    {
        _bundle = Web.Bundle.In(directory);
        return this;
    }

    public AdminPanelBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
        => Concerns.Chain(_concerns, ClientApplication.From(_bundle)
                                                      .Branding(platform.Services.GetRequiredService<IBrandingService>())
                                                      .Header("X-Robots-Tag", "noindex, nofollow")
                                                      .Build());

    #endregion

}
