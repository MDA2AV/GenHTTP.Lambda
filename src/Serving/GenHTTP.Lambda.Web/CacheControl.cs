using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// The two policies this application needs.
/// </summary>
public static class CacheControl
{

    /// <summary>
    /// Keep the copy, but ask before using it. The entity tag the server already
    /// sends turns that question into a 304, so revalidating costs nothing.
    /// </summary>
    public static CacheControlBuilder NoCache() => new("no-cache");

    /// <summary>
    /// Never ask again. Only safe for names that carry their own content hash,
    /// which is exactly what the bundler emits under assets.
    /// </summary>
    public static CacheControlBuilder Immutable() => new("public, max-age=31536000, immutable");

}
