namespace GenHTTP.Lambda.Community;

/// <summary>
/// The pages of a community installation that are its host's own - the front
/// page and the showcase - at the root of the host, behind every component
/// that has a path of its own.
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
