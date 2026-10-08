using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Web;

/// <summary>
/// The content of a public page, put into the index page before it is sent so
/// a crawler reads it without running a script.
/// </summary>
[TestClass]
public sealed class SitePrerenderTests
{

    private const string Index = """
        <!doctype html>
        <html>
        <head>
        <title>Front - GenHTTP Lambda</title>
        </head>
        <body><div id="root"></div></body>
        </html>
        """;

    private const string Pages = """
        {
          "/": { "text": { "en": { "title": "Home", "description": "The front page." }, "de": { "title": "Start", "description": "Die Startseite." } } },
          "/terms": { "text": { "en": { "title": "Terms", "description": "The rules." }, "de": { "title": "Regeln", "description": "Die Regeln." } } },
          "/showcase": { "text": { "en": { "title": "Showcase", "description": "What people built." }, "de": { "title": "Showcase", "description": "Was gebaut wurde." } } }
        }
        """;

    /// <summary>
    /// The entry the build renders on its own, and finds again in the showcase.
    /// </summary>
    private const string Entry = """<div class="rise" style="animation-delay:0ms"><a href="__LAMBDA_ENTRY_ADDRESS__" aria-label="__LAMBDA_ENTRY_TITLE__, opens __LAMBDA_ENTRY_ADDRESS__"><img src="__LAMBDA_ENTRY_IMAGE__"/><h3>__LAMBDA_ENTRY_TITLE__</h3><p>__LAMBDA_ENTRY_DESCRIPTION__</p><span>__LAMBDA_ENTRY_ADDRESS__</span></a></div>""";

    /// <summary>
    /// The same entry in German, which says in German that it opens a new tab.
    /// </summary>
    private static readonly string GermanEntry = Entry.Replace(", opens ", ", öffnet ");

    private static readonly string Prerendered = JsonSerializer.Serialize(new
    {
        pages = new Dictionary<string, string>
        {
            ["/en"] = "<main><h1>Describe an app.</h1><code>claude mcp add genhttp __LAMBDA_ORIGIN__/mcp</code></main>",
            ["/de"] = "<main><h1>Beschreibe eine App.</h1></main>",
            ["/en/terms"] = "<main>Online for about __LAMBDA_LIFETIME_HOURS__ hours, kept for __LAMBDA_RETENTION_DAYS__ days, offline after __LAMBDA_OFFLINE_DAYS__ days at __LAMBDA_HOST__.</main>",
            ["/de/terms"] = "<main>Etwa __LAMBDA_LIFETIME_HOURS__ Stunden online.</main>",
            ["/en/showcase"] = "<main>Loading the showcase</main>",
            ["/de/showcase"] = "<main>Die Showcase lädt</main>"
        },
        showcase = new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new()
            {
                ["empty"] = "<main>Nothing on show yet</main>",
                ["one"] = $"<main><p>1 lambda</p><div class=\"grid\">{Entry}</div></main>",
                ["complete"] = $"<main><p>__LAMBDA_SHOWCASE_TOTAL__ lambdas</p><div class=\"grid\">{Entry}</div></main>",
                ["partial"] = $"<main><p>__LAMBDA_SHOWCASE_TOTAL__ lambdas</p><div class=\"grid\">{Entry}</div><button>Show more</button></main>"
            },
            ["de"] = new()
            {
                ["empty"] = "<main>Noch nichts zu sehen</main>",
                ["one"] = $"<main><p>1 Lambda</p><div class=\"grid\">{GermanEntry}</div></main>",
                ["complete"] = $"<main><p>__LAMBDA_SHOWCASE_TOTAL__ Lambdas</p><div class=\"grid\">{GermanEntry}</div></main>",
                ["partial"] = $"<main><p>__LAMBDA_SHOWCASE_TOTAL__ Lambdas</p><div class=\"grid\">{GermanEntry}</div><button>Mehr</button></main>"
            }
        },
        entry = new Dictionary<string, string>
        {
            ["en"] = Entry,
            ["de"] = GermanEntry
        }
    });

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];

    private static Func<LambdaOptions, LambdaOptions> Site(string? publicUrl = "https://genhttp.dev", bool prerendered = true) => options =>
    {
        File.WriteAllText(Path.Combine(options.WebRoot, "index.html"), Index);
        File.WriteAllText(Path.Combine(options.WebRoot, "pages.json"), Pages);

        if (prerendered)
        {
            File.WriteAllText(Path.Combine(options.WebRoot, "prerender.json"), Prerendered);
        }

        return options with
        {
            PublicUrl = publicUrl,
            DeploymentLifetime = TimeSpan.FromHours(36),
            Retention = TimeSpan.FromDays(45)
        };
    };

    [TestMethod]
    public async Task APublicPageArrivesWithItsContent()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        var body = await ReadAsync(fixture, "/en");

        StringAssert.Contains(body, "<div id=\"root\"><main><h1>Describe an app.</h1><code>claude mcp add genhttp https://genhttp.dev/mcp</code></main></div>");
    }

    [TestMethod]
    public async Task APageArrivesWithTheContentOfItsLanguage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        StringAssert.Contains(await ReadAsync(fixture, "/de"), "<div id=\"root\"><main><h1>Beschreibe eine App.</h1></main></div>");
        StringAssert.Contains(await ReadAsync(fixture, "/de/terms"), "<main>Etwa 36 Stunden online.</main>");
    }

    [TestMethod]
    public async Task WhatTheBuildCouldNotKnowIsFilledIn()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        var body = await ReadAsync(fixture, "/en/terms/");

        // 36 hours are a day and a half, rounded up the way the browser does
        StringAssert.Contains(body, "Online for about 36 hours, kept for 45 days, offline after 2 days at genhttp.dev.");

        Assert.DoesNotContain("__LAMBDA_", body);

        var facts = Facts(body);

        Assert.AreEqual("https://genhttp.dev", facts.GetProperty("origin").GetString());
        Assert.AreEqual("genhttp.dev", facts.GetProperty("host").GetString());
        Assert.AreEqual(36, facts.GetProperty("lifetimeHours").GetInt32());
        Assert.AreEqual(2, facts.GetProperty("offlineDays").GetInt32());
        Assert.AreEqual(45, facts.GetProperty("retentionDays").GetInt32());
    }

    [TestMethod]
    public async Task WithoutAPublicAddressThePageIsAnsweredWhereItWasAskedFor()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site(publicUrl: null));

        var body = await ReadAsync(fixture, "/en", host: "lambda.example:8080");

        StringAssert.Contains(body, "claude mcp add genhttp http://lambda.example:8080/mcp");
        Assert.AreEqual("lambda.example:8080", Facts(body).GetProperty("host").GetString());
    }

    [TestMethod]
    public async Task AHostThatIsNotOneCannotWriteIntoThePage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site(publicUrl: null));

        // a proxy passes on what it was told, and the address is read from there
        using var request = fixture.Host.GetRequest("/en");

        request.Headers.TryAddWithoutValidation("X-Forwarded-Host", "evil\"><script>alert(1)</script>");

        using var response = await fixture.Host.GetResponseAsync(request);

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "claude mcp add genhttp");
        Assert.DoesNotContain("<script>alert(1)</script>", body);
    }

    [TestMethod]
    public async Task AnEmptyShowcaseSaysSo()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        var body = await ReadAsync(fixture, "/en/showcase");

        StringAssert.Contains(body, "<main>Nothing on show yet</main>");
        Assert.AreEqual(0, Facts(body).GetProperty("showcase").GetProperty("total").GetInt32());
    }

    [TestMethod]
    public async Task TheShowcaseArrivesWithWhatIsOnIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        await ShowAsync(fixture, "quiz", "Pub quiz", "Scores for the Tuesday quiz.");
        await ShowAsync(fixture, "poll", "Lunch poll", "Where we eat on Friday.");

        var body = await ReadAsync(fixture, "/en/showcase");

        StringAssert.Contains(body, "<p>2 lambdas</p>");

        StringAssert.Contains(body, "href=\"http://quiz.localhost:8080/\"");
        StringAssert.Contains(body, "<h3>Pub quiz</h3><p>Scores for the Tuesday quiz.</p><span>quiz.localhost:8080</span>");
        StringAssert.Contains(body, "<h3>Lunch poll</h3>");

        // staggered the way the page does it, one after the other
        StringAssert.Contains(body, "animation-delay:0ms");
        StringAssert.Contains(body, "animation-delay:40ms");

        Assert.DoesNotContain("__LAMBDA_", body);
        Assert.DoesNotContain("Show more", body, "everything fit on the first page");

        var entries = Facts(body).GetProperty("showcase").GetProperty("entries");

        Assert.AreEqual(2, entries.GetArrayLength(), "the browser takes over what it was sent rather than asking again");
    }

    [TestMethod]
    public async Task OneLambdaIsNotLambdas()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        await ShowAsync(fixture, "quiz", "Pub quiz", "Scores.");

        var body = await ReadAsync(fixture, "/en/showcase");

        StringAssert.Contains(body, "<p>1 lambda</p>");
        StringAssert.Contains(body, "<h3>Pub quiz</h3>");
    }

    [TestMethod]
    public async Task TheShowcaseIsFilledInInItsLanguage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        await ShowAsync(fixture, "quiz", "Pub quiz", "Scores.");
        await ShowAsync(fixture, "poll", "Lunch poll", "Where we eat on Friday.");

        var body = await ReadAsync(fixture, "/de/showcase");

        StringAssert.Contains(body, "<p>2 Lambdas</p>");
        StringAssert.Contains(body, "aria-label=\"Pub quiz, öffnet quiz.localhost:8080\"");
        Assert.DoesNotContain("__LAMBDA_", body);
    }

    /// <summary>
    /// What an owner writes is theirs, and shown as text: it cannot add markup,
    /// end the script carrying the facts, or pass for one of the placeholders.
    /// </summary>
    [TestMethod]
    public async Task AnEntryIsShownAsItWasWritten()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        await ShowAsync(fixture, "quiz", "</script><script>alert(1)</script>", "Visit __LAMBDA_ORIGIN__ & win");

        var body = await ReadAsync(fixture, "/en/showcase");

        Assert.DoesNotContain("<script>alert(1)", body);
        StringAssert.Contains(body, "<h3>&lt;/script&gt;&lt;script&gt;alert(1)&lt;/script&gt;</h3>");
        StringAssert.Contains(body, "<p>Visit __LAMBDA_ORIGIN__ &amp; win</p>");

        var entry = Facts(body).GetProperty("showcase").GetProperty("entries")[0];

        Assert.AreEqual("</script><script>alert(1)</script>", entry.GetProperty("title").GetString());
    }

    [TestMethod]
    public async Task APageThatIsNotPublicIsLeftEmpty()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        using var response = await fixture.GetAsync("/editor/create", accept: "text/html");

        Assert.AreEqual(Index, await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task WithoutARenderedBuildThePagesAreStillNamed()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site(prerendered: false));

        var body = await ReadAsync(fixture, "/en/terms");

        StringAssert.Contains(body, "<title>Terms - GenHTTP Lambda</title>");
        StringAssert.Contains(body, "<div id=\"root\"></div>");
        Assert.DoesNotContain("site-facts", body);
    }

    private static async Task ShowAsync(LambdaFixture fixture, string publicKey, string title, string description)
    {
        var lambda = await fixture.CreateLambdaAsync(publicKey);

        await fixture.DeployAsync(lambda.PrivateKey);

        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/showcase",
                                                     new ShowcaseRequest(title, description, Convert.ToBase64String(Png)));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private static async Task<string> ReadAsync(LambdaFixture fixture, string path, string? host = null)
    {
        using var response = await fixture.GetAsync(path, accept: "text/html", host: host);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.Content.ReadAsStringAsync();
    }

    private static JsonElement Facts(string body)
    {
        var script = Regex.Match(body, "<script id=\"site-facts\" type=\"application/json\">(.*?)</script>", RegexOptions.Singleline);

        Assert.IsTrue(script.Success, "the page carries no facts");

        return JsonDocument.Parse(script.Groups[1].Value).RootElement;
    }

}
