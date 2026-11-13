using GenHTTP.Api.Content;

using GenHTTP.Lambda.Web;

namespace GenHTTP.Lambda.Community;

/// <summary>
/// The pages of a community installation that are its host's own - the front
/// page, the guide and the showcase - at the root of the host, behind every
/// component that has a path of its own.
/// </summary>
/// <remarks>
/// Says nothing about who runs the installation beyond what its operator
/// configured (LAMBDA_CONTACT, LAMBDA_ABUSE_CONTACT): there are no terms, no
/// legal pages and no description of an organization behind it.
/// </remarks>
public static class LandingPage
{

    public static LandingPageBuilder Create(LambdaPlatform platform) => new(platform);

}

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
