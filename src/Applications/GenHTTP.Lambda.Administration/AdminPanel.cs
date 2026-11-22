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
