using System.Net;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// How the frontend is served: the bundle's own files, and what a browser is
/// allowed to remember about them.
/// </summary>
[TestClass]
public sealed class WebAssetTests
{

    private const string Bundle = "index-BwtdxquV.js";

    /// <summary>
    /// A web root with a built bundle in it, the way a deployment looks.
    /// </summary>
    private static LambdaOptions WithBundle(LambdaOptions options)
    {
        var assets = Path.Combine(options.WebRoot, "assets");

        Directory.CreateDirectory(assets);

        File.WriteAllText(Path.Combine(assets, Bundle), "export const answer = 42;\n");

        return options;
    }

    /// <summary>
    /// The failure this guards against: a browser holding a stale index page
    /// asks for the bundle it was built with, that file is long gone, and server
    /// side routing hands it the index page with a 200 and a text/html type. The
    /// browser then parses markup as script, throws on the first angle bracket
    /// and never starts the application - with no failed request to show for it.
    /// </summary>
    [TestMethod]
    public async Task AMissingBundleIsNotFoundRatherThanTheIndexPage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithBundle);

        using var response = await fixture.GetAsync("/assets/index-STALEHASH.js");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.IsFalse(body.Contains("<!doctype html", StringComparison.OrdinalIgnoreCase),
                       "a missing bundle was answered with the index page");
    }

    [TestMethod]
    public async Task TheBundleIsServedAndMayBeKeptForever()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithBundle);

        using var response = await fixture.GetAsync($"/assets/{Bundle}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("export const answer = 42;\n", await response.Content.ReadAsStringAsync());

        var cacheControl = response.Headers.CacheControl?.ToString() ?? "";

        StringAssert.Contains(cacheControl, "immutable");
        StringAssert.Contains(cacheControl, "max-age=31536000");
    }

    /// <summary>
    /// The index page may be reused, but only after asking - otherwise it
    /// outlives the bundle it names.
    /// </summary>
    [TestMethod]
    public async Task TheIndexPageIsRevalidated()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithBundle);

        using var response = await fixture.GetAsync("/");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(true, response.Headers.CacheControl?.NoCache);
    }

    /// <summary>
    /// Routes the client side router owns still resolve to the application, so
    /// narrowing the fallback did not break deep links into the editor.
    /// </summary>
    [TestMethod]
    public async Task AClientRouteStillReachesTheApplication()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithBundle);

        var lambda = await fixture.CreateLambdaAsync("deep-link");

        using var response = await fixture.GetAsync(lambda.EditorPath, accept: "text/html");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<div id=\"app\">");
    }

    /// <summary>
    /// A web root without a built frontend still starts, and the asset route is
    /// simply not there.
    /// </summary>
    [TestMethod]
    public async Task AWebRootWithoutABundleStillServes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

}
