using System.Net;
using System.Xml.Linq;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Sources;

/// <summary>
/// The page of a published source as a search engine and a link preview find
/// it: named after the lambda in the language its address names, described as
/// source code, and listed in the sitemap - and not there at all while the
/// source is not published.
/// </summary>
[TestClass]
public sealed class SourcePageTests
{

    private const string Index = """
        <!doctype html>
        <html lang="en" class="dark">
        <head>
        <meta name="description" content="The front page." />
        <title>Front - GenHTTP Lambda</title>
        </head>
        <body><div id="root"></div></body>
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
          "/source": {
            "text": {
              "en": { "title": "Open Source", "description": "The source of apps built here." },
              "de": { "title": "Open Source", "description": "Der Quellcode der Apps von hier." }
            }
          },
          "/source/:key": {
            "text": {
              "en": { "title": "{name} - Source code", "description": "The source code of {name}." },
              "de": { "title": "{name} - Quellcode", "description": "Der Quellcode von {name}." }
            }
          }
        }
        """;

    private static Func<LambdaOptions, LambdaOptions> Site(string? publicUrl = "https://genhttp.dev") => options =>
    {
        File.WriteAllText(Path.Combine(options.WebRoot, "index.html"), Index);
        File.WriteAllText(Path.Combine(options.WebRoot, "pages.json"), Pages);

        return options with { PublicUrl = publicUrl };
    };

    [TestMethod]
    public async Task ThePageOfASourceIsNamedAfterTheLambdaInItsLanguage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        var lambda = await PublishAsync(fixture, "quiz", "# Quiz\n\nScores for the Tuesday pub quiz.\n");

        using var response = await fixture.GetAsync("/de/source/quiz", accept: "text/html");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<html lang=\"de\" class=\"dark\">");
        StringAssert.Contains(body, "<title>quiz - Quellcode - GenHTTP Lambda</title>");
        StringAssert.Contains(body, "<meta name=\"description\" content=\"Scores for the Tuesday pub quiz.\" />", "what the lambda says it is");
        StringAssert.Contains(body, "<link rel=\"canonical\" href=\"https://genhttp.dev/de/source/quiz\" />");
        StringAssert.Contains(body, "<link rel=\"alternate\" hreflang=\"en\" href=\"https://genhttp.dev/en/source/quiz\" />");
        StringAssert.Contains(body, "\"@type\":\"SoftwareSourceCode\"");
        StringAssert.Contains(body, "https://spdx.org/licenses/MIT.html");

        Assert.DoesNotContain(lambda.PrivateKey, body);
    }

    [TestMethod]
    public async Task APageBelowASourceIsItsPageToo()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        await PublishAsync(fixture, "quiz", null);

        using var response = await fixture.GetAsync("/en/source/quiz/files/Project.cs", accept: "text/html");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<title>quiz - Source code - GenHTTP Lambda</title>");
        StringAssert.Contains(body, "The source code of quiz.", "described in the words of the page where the lambda says nothing");
        StringAssert.Contains(body, "<link rel=\"canonical\" href=\"https://genhttp.dev/en/source/quiz/files/Project.cs\" />");
    }

    [TestMethod]
    public async Task ASourceThatIsNotPublishedIsNotFound()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        await fixture.CreateLambdaAsync("hidden");

        using var response = await fixture.GetAsync("/en/source/hidden", accept: "text/html");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "the same as a key nobody has");

        using var nobody = await fixture.GetAsync("/en/source/nobody-has-this", accept: "text/html");

        Assert.AreEqual(HttpStatusCode.NotFound, nobody.StatusCode);
    }

    [TestMethod]
    public async Task ASourceAskedForWithoutALanguageIsSentOnToOne()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        await PublishAsync(fixture, "quiz", null);

        using var request = fixture.Host.GetRequest("/source/quiz/docs?version=2");

        request.Headers.TryAddWithoutValidation("Accept", "text/html");
        request.Headers.TryAddWithoutValidation("Accept-Language", "de-AT, en;q=0.5");

        using var response = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        Assert.AreEqual("/de/source/quiz/docs?version=2", response.Headers.Location?.OriginalString);

        using var listing = await fixture.GetAsync("/source", accept: "text/html");

        Assert.AreEqual(HttpStatusCode.Found, listing.StatusCode, "the listing is a page of the build like any other");
    }

    [TestMethod]
    public async Task TheSitemapListsEveryPublishedSourceInEveryLanguage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        await PublishAsync(fixture, "quiz", null);

        await fixture.CreateLambdaAsync("hidden");

        using var response = await fixture.GetAsync("/sitemap.xml");

        var sitemap = XDocument.Parse(await response.Content.ReadAsStringAsync());

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var locations = sitemap.Descendants(ns + "loc").Select(l => l.Value).ToList();

        CollectionAssert.IsSubsetOf(new[]
        {
            "https://genhttp.dev/en/source", "https://genhttp.dev/de/source",
            "https://genhttp.dev/en/source/quiz", "https://genhttp.dev/de/source/quiz"
        }, locations);

        Assert.IsFalse(locations.Any(l => l.Contains(":key", StringComparison.Ordinal)), "the template names pages, it is none");
        Assert.IsFalse(locations.Any(l => l.Contains("hidden", StringComparison.Ordinal)), "only what is published");

        var quiz = sitemap.Descendants(ns + "url").Single(u => u.Element(ns + "loc")!.Value == "https://genhttp.dev/en/source/quiz");

        Assert.IsNotNull(quiz.Element(ns + "lastmod"), "when its newest version was saved");
    }

    [TestMethod]
    public async Task ACrawlerMayFetchWhatThePageOfASourceIsDrawnFrom()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/robots.txt");

        var rules = (await response.Content.ReadAsStringAsync()).Split('\n');

        CollectionAssert.Contains(rules, "Allow: /api/v1/sources/", "the page is not prerendered: what it shows comes from here");
        CollectionAssert.Contains(rules, "Allow: /api/v1/showcases/", "and its picture from here");
        CollectionAssert.Contains(rules, "Disallow: /api/v1/sources/*/zip", "a download is no page");
        CollectionAssert.Contains(rules, "Disallow: /api/", "the rest of the API stays out");
    }

    private static async Task<LambdaResponse> PublishAsync(LambdaFixture fixture, string key, string? product)
    {
        var lambda = await fixture.CreateLambdaAsync(key);

        if (product != null)
        {
            using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                      new VersionRequest([new LambdaFile(LambdaSource.EntryName, "return Inline.Create();"),
                                                                          new LambdaFile(LambdaSource.ProductDoc, product)]));

            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);
        }

        using var published = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/source", new SourceRequest(null, null));

        Assert.AreEqual(HttpStatusCode.OK, published.StatusCode);

        return lambda;
    }

}
