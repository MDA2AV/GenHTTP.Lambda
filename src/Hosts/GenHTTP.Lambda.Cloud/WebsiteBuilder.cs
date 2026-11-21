using GenHTTP.Api.Content;

using GenHTTP.Lambda.Web;

namespace GenHTTP.Lambda.Cloud;

public sealed class WebsiteBuilder(LambdaPlatform platform) : IHandlerBuilder<WebsiteBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private Web.Bundle _bundle = Web.Bundle.Beside("cloud");

    /// <summary>
    /// Where the site's bundle was built to, where it is not beside the host.
    /// </summary>
    public WebsiteBuilder Bundle(string directory)
    {
        _bundle = Web.Bundle.In(directory);
        return this;
    }

    public WebsiteBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
        => Concerns.Chain(_concerns, SitePages.Create(platform, _bundle)
                                              // genhttp.dev says who runs it, and what it is, on its front page
                                              .Head(page => page.Path == "/" ? CloudStructuredData.Render("GenHTTP Lambda", page.Description, page.Language) : null)
                                              .Catalog()
                                              // the features were a page of their own until they became the front page
                                              .Moved("/platform", "/")
                                              .Build());

}
