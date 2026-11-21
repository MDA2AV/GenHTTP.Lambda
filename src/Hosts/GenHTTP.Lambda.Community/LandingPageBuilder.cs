using GenHTTP.Api.Content;

using GenHTTP.Lambda.Web;

namespace GenHTTP.Lambda.Community;

public sealed class LandingPageBuilder(LambdaPlatform platform) : IHandlerBuilder<LandingPageBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private Web.Bundle _bundle = Web.Bundle.Beside("community");

    /// <summary>
    /// Where the site's bundle was built to, where it is not beside the host.
    /// </summary>
    public LandingPageBuilder Bundle(string directory)
    {
        _bundle = Web.Bundle.In(directory);
        return this;
    }

    public LandingPageBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build() => Concerns.Chain(_concerns, SitePages.Create(platform, _bundle).Build());

}
