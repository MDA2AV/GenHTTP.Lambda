using GenHTTP.Api.Content;

using GenHTTP.Lambda.Web;

namespace GenHTTP.Lambda.Cloud;

/// <summary>
/// The pages of genhttp.dev that are this host's own - the front page, which
/// shows what every app gets, the workspace that builds them, the guide for
/// whoever builds with their own agent, self-hosting, the showcase and the
/// legal pages - at the root of the host, behind every component that has a
/// path of its own.
/// </summary>
public static class Website
{

    public static WebsiteBuilder Create(LambdaPlatform platform) => new(platform);

}
