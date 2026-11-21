using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Web;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Administration;

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
