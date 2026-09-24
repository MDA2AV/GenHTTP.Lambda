using System.Net;
using System.Xml.Linq;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Web;

/// <summary>
/// What crawlers and link previews are told: each public page named in the
/// markup itself, a canonical address, robots.txt and the sitemap.
/// </summary>
[TestClass]
public sealed class SiteMetaTests
{

    private const string Index = """
        <!doctype html>
        <html>
        <head>
        <meta name="description" content="The front page." />
        <meta property="og:title" content="Front" />
        <title>Front - GenHTTP Lambda</title>
        </head>
        <body><div id="app"></div></body>
        </html>
        """;

    private const string Pages = """
        {
          "/": { "title": "Home", "description": "The front page." },
          "/docs": { "title": "How It Works", "description": "Snippets & \"handlers\", hosted." }
        }
        """;

    /// <summary>
    /// A web root the way the frontend build leaves it, with or without a
    /// public address configured.
    /// </summary>
    private static Func<LambdaOptions, LambdaOptions> Site(string? publicUrl = "https://genhttp.dev") => options =>
    {
        File.WriteAllText(Path.Combine(options.WebRoot, "index.html"), Index);
        File.WriteAllText(Path.Combine(options.WebRoot, "pages.json"), Pages);

        return options with { PublicUrl = publicUrl };
    };

    [TestMethod]
    public async Task APublicPageIsNamedBeforeItIsSent()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/docs", accept: "text/html");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<title>How It Works - GenHTTP Lambda</title>");
        StringAssert.Contains(body, "<meta name=\"description\" content=\"Snippets &amp; &quot;handlers&quot;, hosted.\" />");
        StringAssert.Contains(body, "<meta property=\"og:title\" content=\"How It Works - GenHTTP Lambda\" />");
        StringAssert.Contains(body, "<link rel=\"canonical\" href=\"https://genhttp.dev/docs\" />");
        StringAssert.Contains(body, "<meta property=\"og:url\" content=\"https://genhttp.dev/docs\" />");

        Assert.DoesNotContain("The front page.", body, "the front page's description was left in place");
        Assert.AreEqual(true, response.Headers.CacheControl?.NoCache, "a named page names the bundle as well, so it must not be kept");
    }

    [TestMethod]
    public async Task ATrailingSlashIsTheSamePage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/docs/", accept: "text/html");

        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "href=\"https://genhttp.dev/docs\"");
    }

    [TestMethod]
    public async Task TheFrontPageIsCanonicalAtTheRoot()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/", accept: "text/html");

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<title>Home - GenHTTP Lambda</title>");
        StringAssert.Contains(body, "href=\"https://genhttp.dev/\"");
    }

    [TestMethod]
    public async Task AnExampleIsNamedFromTheCatalogue()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/examples/guestbook", accept: "text/html");

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<title>Guestbook Example - GenHTTP Lambda</title>");
        StringAssert.Contains(body, "href=\"https://genhttp.dev/examples/guestbook\"");
    }

    /// <summary>
    /// The editor, an unknown example and a path that does not exist are left
    /// alone - the client marks them as not to be indexed.
    /// </summary>
    [TestMethod]
    [DataRow("/editor/create")]
    [DataRow("/examples/no-such-thing")]
    [DataRow("/nowhere")]
    public async Task AnythingElseIsTheIndexPageUntouched(string path)
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync(path, accept: "text/html");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(Index, await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task WithoutAPublicAddressNothingClaimsToBeCanonical()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site(publicUrl: null));

        using var page = await fixture.GetAsync("/docs", accept: "text/html");

        var body = await page.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<title>How It Works - GenHTTP Lambda</title>");
        Assert.DoesNotContain("canonical", body);
        Assert.DoesNotContain("og:url", body);

        using var sitemap = await fixture.GetAsync("/sitemap.xml");

        Assert.AreEqual(HttpStatusCode.NotFound, sitemap.StatusCode);

        using var robots = await fixture.GetAsync("/robots.txt");

        Assert.DoesNotContain("Sitemap:", await robots.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task RobotsKeepOutOfThePrivatePartsAndPointAtTheSitemap()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/robots.txt");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("text/plain", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "Disallow: /api/");
        StringAssert.Contains(body, "Disallow: /editor/");
        StringAssert.Contains(body, "Sitemap: https://genhttp.dev/sitemap.xml");
    }

    [TestMethod]
    public async Task TheSitemapListsThePagesAndTheExamples()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/sitemap.xml");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/xml", response.Content.Headers.ContentType?.MediaType);

        var sitemap = XDocument.Parse(await response.Content.ReadAsStringAsync());

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var locations = sitemap.Descendants(ns + "loc").Select(l => l.Value).ToList();

        CollectionAssert.Contains(locations, "https://genhttp.dev/");
        CollectionAssert.Contains(locations, "https://genhttp.dev/docs");
        CollectionAssert.Contains(locations, "https://genhttp.dev/examples/guestbook");
    }

}
