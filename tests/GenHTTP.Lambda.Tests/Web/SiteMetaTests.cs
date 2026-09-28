using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Infrastructure;
using GenHTTP.Lambda.Web;

namespace GenHTTP.Lambda.Tests.Web;

/// <summary>
/// What crawlers and link previews are told: each public page named in the
/// markup itself, in the language its address names, with its canonical
/// address and the addresses of its translations, robots.txt and the sitemap
/// - and where a visitor is sent who asked for a page in no language.
/// </summary>
[TestClass]
public sealed class SiteMetaTests
{

    private const string Index = """
        <!doctype html>
        <html lang="en" class="dark">
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
          "/": {
            "text": {
              "en": { "title": "Home", "description": "The front page." },
              "de": { "title": "Start", "description": "Die Startseite." }
            }
          },
          "/docs": {
            "image": "/social/docs.png",
            "text": {
              "en": { "title": "How It Works", "description": "Snippets & \"handlers\", hosted." },
              "de": { "title": "So funktioniert es", "description": "Schnipsel & \"Handler\", gehostet.", "image": "/social/de/docs.jpg" }
            }
          }
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
    public async Task APublicPageIsNamedInItsLanguageBeforeItIsSent()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/de/docs", accept: "text/html");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<html lang=\"de\" class=\"dark\">");
        StringAssert.Contains(body, "<title>So funktioniert es - GenHTTP Lambda</title>");
        StringAssert.Contains(body, "<meta name=\"description\" content=\"Schnipsel &amp; &quot;Handler&quot;, gehostet.\" />");
        StringAssert.Contains(body, "<meta property=\"og:title\" content=\"So funktioniert es - GenHTTP Lambda\" />");
        StringAssert.Contains(body, "<meta property=\"og:locale\" content=\"de_DE\" />");
        StringAssert.Contains(body, "<meta property=\"og:locale:alternate\" content=\"en_US\" />");
        StringAssert.Contains(body, "<link rel=\"canonical\" href=\"https://genhttp.dev/de/docs\" />");
        StringAssert.Contains(body, "<meta property=\"og:url\" content=\"https://genhttp.dev/de/docs\" />");

        Assert.DoesNotContain("The front page.", body, "the front page's description was left in place");
        Assert.DoesNotContain("og:locale:alternate\" content=\"de_DE", body, "a page is not an alternate of itself");
        Assert.AreEqual(true, response.Headers.CacheControl?.NoCache, "a named page names the bundle as well, so it must not be kept");
    }

    /// <summary>
    /// Every language of a page names all of them, itself included, and the
    /// address without a language for whoever matches none - the way a search
    /// engine expects a set of translations to be described.
    /// </summary>
    [TestMethod]
    [DataRow("/en/docs")]
    [DataRow("/de/docs")]
    public async Task EveryLanguageOfAPageNamesAllOfThem(string path)
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync(path, accept: "text/html");

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<link rel=\"alternate\" hreflang=\"en\" href=\"https://genhttp.dev/en/docs\" />");
        StringAssert.Contains(body, "<link rel=\"alternate\" hreflang=\"de\" href=\"https://genhttp.dev/de/docs\" />");
        StringAssert.Contains(body, "<link rel=\"alternate\" hreflang=\"x-default\" href=\"https://genhttp.dev/docs\" />");

        // only the languages the page is written in, as the sitemap lists them
        Assert.DoesNotContain("hreflang=\"fr\"", body);
        Assert.DoesNotContain("og:locale:alternate\" content=\"fr_FR", body);
    }

    /// <summary>
    /// Portuguese is written twice, as Brazil and as Portugal write it. Each
    /// is tagged with its region, and the Brazilian pages stand for Portuguese
    /// as a whole, for a reader in a country with no variant of its own.
    /// </summary>
    [TestMethod]
    public async Task AVariantOfALanguageIsTaggedWithItsRegion()
    {
        const string pages = """
            {
              "/docs": {
                "text": {
                  "en": { "title": "How It Works", "description": "Snippets, hosted." },
                  "pt": { "title": "Como funciona", "description": "Trechos, hospedados." },
                  "pt-pt": { "title": "Como funciona", "description": "Excertos, alojados." }
                }
              }
            }
            """;

        await using var fixture = await LambdaFixture.CreateAsync(options =>
        {
            options = Site()(options);
            File.WriteAllText(Path.Combine(options.WebRoot, "pages.json"), pages);
            return options;
        });

        using var response = await fixture.GetAsync("/pt-pt/docs", accept: "text/html");

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<html lang=\"pt-PT\" class=\"dark\">");
        StringAssert.Contains(body, "<meta name=\"description\" content=\"Excertos, alojados.\" />");
        StringAssert.Contains(body, "<meta property=\"og:locale\" content=\"pt_PT\" />");
        StringAssert.Contains(body, "<meta property=\"og:locale:alternate\" content=\"pt_BR\" />");
        StringAssert.Contains(body, "<link rel=\"canonical\" href=\"https://genhttp.dev/pt-pt/docs\" />");
        StringAssert.Contains(body, "<link rel=\"alternate\" hreflang=\"pt-PT\" href=\"https://genhttp.dev/pt-pt/docs\" />");
        StringAssert.Contains(body, "<link rel=\"alternate\" hreflang=\"pt-BR\" href=\"https://genhttp.dev/pt/docs\" />");
        StringAssert.Contains(body, "<link rel=\"alternate\" hreflang=\"pt\" href=\"https://genhttp.dev/pt/docs\" />");

        using var sitemap = await fixture.GetAsync("/sitemap.xml");

        var xml = await sitemap.Content.ReadAsStringAsync();

        StringAssert.Contains(xml, "<loc>https://genhttp.dev/pt-pt/docs</loc>");
        StringAssert.Contains(xml, "hreflang=\"pt-PT\" href=\"https://genhttp.dev/pt-pt/docs\"");
        StringAssert.Contains(xml, "hreflang=\"pt\" href=\"https://genhttp.dev/pt/docs\"");
    }

    [TestMethod]
    public async Task ATrailingSlashIsTheSamePage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/de/docs/", accept: "text/html");

        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "<link rel=\"canonical\" href=\"https://genhttp.dev/de/docs\" />");
    }

    [TestMethod]
    [DataRow("/en")]
    [DataRow("/en/")]
    public async Task TheFrontPageIsCanonicalAtItsLanguage(string path)
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync(path, accept: "text/html");

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<title>Home - GenHTTP Lambda</title>");
        StringAssert.Contains(body, "<link rel=\"canonical\" href=\"https://genhttp.dev/en\" />");
        StringAssert.Contains(body, "<link rel=\"alternate\" hreflang=\"x-default\" href=\"https://genhttp.dev/\" />");
    }

    /// <summary>
    /// A page asked for without a language is sent on to the one the visitor
    /// chose here before, or the best one their browser accepts - and to the
    /// default for a crawler, which asks in none.
    /// </summary>
    [TestMethod]
    [DataRow("de-CH,de;q=0.9,en;q=0.8", null, "/de/docs")]
    [DataRow("zh, fr;q=0, en;q=0.5", null, "/en/docs")]
    [DataRow("ja-JP,ja;q=0.9,en;q=0.8", null, "/ja/docs")]
    [DataRow("in-ID", null, "/id/docs")]
    [DataRow("pt-PT,pt;q=0.9,en;q=0.5", null, "/pt-pt/docs")]
    [DataRow("pt-AO", null, "/pt-pt/docs")]
    [DataRow("pt", null, "/pt/docs")]
    [DataRow("en", "lang=pt-pt", "/pt-pt/docs")]
    [DataRow("it;q=0.4, de;q=0.7", null, "/de/docs")]
    [DataRow("PT-br", null, "/pt/docs")]
    [DataRow("*", null, "/en/docs")]
    [DataRow(null, null, "/en/docs")]
    [DataRow("de", "lang=es", "/es/docs")]
    [DataRow("de", "theme=dark; lang=xx", "/de/docs")]
    public async Task APageWithoutALanguageIsSentOnToThePreferredOne(string? accepted, string? cookie, string expected)
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await RequestAsync(fixture, "/docs", accepted, cookie);

        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        Assert.AreEqual(expected, response.Headers.Location?.OriginalString);

        CollectionAssert.IsSubsetOf(new[] { "Accept-Language", "Cookie" }, response.Headers.Vary.ToList(),
                                    "a cache would hand one visitor's language to the next");
    }

    [TestMethod]
    public async Task TheFrontPageWithoutALanguageIsSentOn()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await RequestAsync(fixture, "/", "fr-FR,fr;q=0.9");

        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        Assert.AreEqual("/fr", response.Headers.Location?.OriginalString);
    }

    [TestMethod]
    public async Task TheQueryGoesAlong()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await RequestAsync(fixture, "/docs/?utm_source=mail&q=a%20b", "de");

        Assert.AreEqual("/de/docs?utm_source=mail&q=a%20b", response.Headers.Location?.OriginalString);
    }

    [TestMethod]
    public async Task ThePageThatMovedIsFoundInTheLanguageAsWell()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await RequestAsync(fixture, "/agentic-coding", "it");

        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        Assert.AreEqual("/it/showcase", response.Headers.Location?.OriginalString);
    }

    /// <summary>
    /// The editor, the pages the examples used to have, a path that does not
    /// exist - in a language or in none - and a language the site is not
    /// written in are left alone: the client marks them as not to be indexed.
    /// </summary>
    [TestMethod]
    [DataRow("/editor/create")]
    [DataRow("/examples/guestbook")]
    [DataRow("/nowhere")]
    [DataRow("/de/nowhere")]
    [DataRow("/sv/docs")]
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

        using var page = await fixture.GetAsync("/en/docs", accept: "text/html");

        var body = await page.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<title>How It Works - GenHTTP Lambda</title>");
        Assert.DoesNotContain("canonical", body);
        Assert.DoesNotContain("hreflang", body);
        Assert.DoesNotContain("og:url", body);

        using var sitemap = await fixture.GetAsync("/sitemap.xml");

        Assert.AreEqual(HttpStatusCode.NotFound, sitemap.StatusCode);

        using var robots = await fixture.GetAsync("/robots.txt");

        Assert.DoesNotContain("Sitemap:", await robots.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task APageIsPreviewedWithItsOwnPicture()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/en/docs", accept: "text/html");

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<meta property=\"og:image\" content=\"https://genhttp.dev/social/docs.png\" />");
        StringAssert.Contains(body, "<meta name=\"twitter:image\" content=\"https://genhttp.dev/social/docs.png\" />");
        StringAssert.Contains(body, "<meta property=\"og:image:alt\" content=\"How It Works - GenHTTP Lambda\" />");
        StringAssert.Contains(body, "<meta property=\"og:image:type\" content=\"image/png\" />");
    }

    [TestMethod]
    public async Task APageIsPreviewedWithThePictureDrawnInItsLanguage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/de/docs", accept: "text/html");

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<meta property=\"og:image\" content=\"https://genhttp.dev/social/de/docs.jpg\" />");
        StringAssert.Contains(body, "<meta name=\"twitter:image\" content=\"https://genhttp.dev/social/de/docs.jpg\" />");
        StringAssert.Contains(body, "<meta property=\"og:image:type\" content=\"image/jpeg\" />");
    }

    /// <summary>
    /// The previews in the other languages are JPEG, which GenHTTP would send
    /// as image/jpg - a type a link preview may not accept.
    /// </summary>
    [TestMethod]
    public async Task APictureInJpegIsSentAsJpeg()
    {
        await using var fixture = await LambdaFixture.CreateAsync(options =>
        {
            options = Site()(options);

            Directory.CreateDirectory(Path.Combine(options.WebRoot, "social", "de"));
            File.WriteAllBytes(Path.Combine(options.WebRoot, "social", "de", "docs.jpg"), [0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3]);

            return options;
        });

        using var response = await fixture.GetAsync("/social/de/docs.jpg");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("image/jpeg", response.Content.Headers.ContentType?.MediaType);
        CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3 }, await response.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    public async Task APageWithoutAPictureIsPreviewedWithTheSites()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/en", accept: "text/html");

        StringAssert.Contains(await response.Content.ReadAsStringAsync(),
                              "<meta property=\"og:image\" content=\"https://genhttp.dev/social/default.png\" />");
    }

    [TestMethod]
    public async Task WithoutAPublicAddressThePictureIsNamedByItsPath()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site(publicUrl: null));

        using var response = await fixture.GetAsync("/en/docs", accept: "text/html");

        StringAssert.Contains(await response.Content.ReadAsStringAsync(),
                              "<meta property=\"og:image\" content=\"/social/docs.png\" />");
    }

    [TestMethod]
    [DataRow("https://genhttp.dev")]
    [DataRow(null)]
    public async Task TheFrontPageDescribesTheSiteUnderItsOwnAddress(string? publicUrl)
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site(publicUrl));

        using var response = await fixture.GetAsync("/de", accept: "text/html");

        var graph = Graph(await response.Content.ReadAsStringAsync());

        var types = graph.Select(node => node.GetProperty("@type").GetString()).ToList();

        CollectionAssert.AreEquivalent(new[] { "Organization", "WebSite", "WebApplication" }, types);

        var website = graph.Single(node => node.GetProperty("@type").GetString() == "WebSite");

        Assert.AreEqual("GenHTTP Lambda", website.GetProperty("name").GetString());
        Assert.AreEqual("https://genhttp.dev/", website.GetProperty("url").GetString());
        Assert.AreEqual(SiteLanguages.All.Count, website.GetProperty("inLanguage").GetArrayLength(), "the site is written in every language it has");

        var application = graph.Single(node => node.GetProperty("@type").GetString() == "WebApplication");

        Assert.AreEqual("Die Startseite.", application.GetProperty("description").GetString());
        Assert.AreEqual("de", application.GetProperty("inLanguage").GetString());
        Assert.AreEqual("https://genhttp.dev/de", application.GetProperty("url").GetString());
    }

    [TestMethod]
    public async Task OnlyTheFrontPageDescribesTheSite()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/en/docs", accept: "text/html");

        Assert.DoesNotContain("application/ld+json", await response.Content.ReadAsStringAsync());
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
    public async Task TheSitemapListsEveryPageInEveryLanguage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/sitemap.xml");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/xml", response.Content.Headers.ContentType?.MediaType);

        var sitemap = XDocument.Parse(await response.Content.ReadAsStringAsync());

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XNamespace xhtml = "http://www.w3.org/1999/xhtml";

        var urls = sitemap.Descendants(ns + "url").ToList();

        var locations = urls.Select(url => url.Element(ns + "loc")!.Value).ToList();

        CollectionAssert.AreEquivalent(new[]
        {
            "https://genhttp.dev/en", "https://genhttp.dev/de",
            "https://genhttp.dev/en/docs", "https://genhttp.dev/de/docs"
        }, locations, "only the pages themselves, not the addresses that send a visitor on");

        var docs = urls.Single(url => url.Element(ns + "loc")!.Value == "https://genhttp.dev/de/docs");

        var alternates = docs.Elements(xhtml + "link").ToDictionary(link => link.Attribute("hreflang")!.Value, link => link.Attribute("href")!.Value);

        Assert.AreEqual("https://genhttp.dev/en/docs", alternates["en"]);
        Assert.AreEqual("https://genhttp.dev/de/docs", alternates["de"]);
        Assert.AreEqual("https://genhttp.dev/docs", alternates["x-default"]);
    }

    private static async Task<HttpResponseMessage> RequestAsync(LambdaFixture fixture, string path, string? accepted, string? cookie = null)
    {
        using var request = fixture.Host.GetRequest(path);

        request.Headers.TryAddWithoutValidation("Accept", "text/html");

        if (accepted != null)
        {
            request.Headers.TryAddWithoutValidation("Accept-Language", accepted);
        }

        if (cookie != null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        return await fixture.Host.GetResponseAsync(request);
    }

    private static List<JsonElement> Graph(string body)
    {
        var script = Regex.Match(body, "<script type=\"application/ld\\+json\">(.*?)</script>", RegexOptions.Singleline);

        Assert.IsTrue(script.Success, "the front page carries no structured data");

        return JsonDocument.Parse(script.Groups[1].Value).RootElement.GetProperty("@graph").EnumerateArray().ToList();
    }

}
