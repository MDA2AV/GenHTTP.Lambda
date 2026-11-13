using GenHTTP.Api.Content;

using GenHTTP.Lambda.Web;

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
/// Without one, the panel says that it is switched off.
/// </remarks>
/// <example>
/// <code>
/// Layout.Create().Add("admin", AdminPanel.Create());
/// </code>
/// </example>
public static class AdminPanel
{

    /// <summary>
    /// The panel.
    /// </summary>
    public static AdminPanelBuilder Create() => new();

}

/// <summary>
/// Configures the panel.
/// </summary>
public sealed class AdminPanelBuilder : IHandlerBuilder<AdminPanelBuilder>
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
                                                      .Header("X-Robots-Tag", "noindex, nofollow")
                                                      .Build());

    #endregion

}
