using GenHTTP.Api.Content;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Git;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;
using GenHTTP.Lambda.Web;

using GenHTTP.Modules.Files;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Sources;

/// <summary>
/// Configures the pages of the published sources.
/// </summary>
public sealed class SourcePagesBuilder(LambdaPlatform platform) : IHandlerBuilder<SourcePagesBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private Web.Bundle _bundle = Web.Bundle.Beside("sources");

    #region Functionality

    /// <summary>
    /// Where the bundle of the pages was built to, where it is not beside the host.
    /// </summary>
    public SourcePagesBuilder Bundle(string directory)
    {
        _bundle = Web.Bundle.In(directory);
        return this;
    }

    public SourcePagesBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var services = platform.Services;

        var repositories = ActivatorUtilities.CreateInstance<GitRoutesBuilder>(services, GitArea.Published).Build();

        IHandler files = Assets.From(_bundle.Files()).Add(CacheControl.NoCache()).Build();

        if (_bundle.HasAssets)
        {
            files = GenHTTP.Modules.Layouting.Layout.Create().Add("assets", _bundle.Assets()).Add(files).Build();
        }

        var handler = new SourcePagesHandler(_bundle, new SiteMeta(_bundle, platform.Options.PublicUrl), services.GetRequiredService<ISourceService>(),
                                             services.GetRequiredService<ILambdaAddresses>(), services.GetRequiredService<IBrandingService>(), repositories, files);

        return Concerns.Chain(_concerns, handler);
    }

    #endregion

}
