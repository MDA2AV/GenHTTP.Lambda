using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// Every lambda answers at a subdomain of the hosting domain named after its
/// key, and its old address below /lambda/ on the platform sends visitors
/// there.
/// </summary>
[TestClass]
public sealed class HostingTests
{

    private const string Code = """
        return Inline.Create()
                     .Get(() => "home")
                     .Get("start", () => "mine")
                     .Post("items", () => "posted");
        """;

    #region Its address

    [TestMethod]
    public async Task ALambdaAnswersAtTheSubdomainNamedAfterItsKey()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "quiz");

        using var home = await fixture.GetAsync("http://quiz.localhost/");

        Assert.AreEqual(HttpStatusCode.OK, home.StatusCode);
        Assert.AreEqual("home", await home.GetContentAsync());

        using var start = await fixture.GetAsync("/start", host: "Quiz.LocalHost:8080");

        Assert.AreEqual("mine", await start.GetContentAsync(), "whatever the case and the port");
    }

    [TestMethod]
    public async Task EveryPathOfTheSubdomainBelongsToTheLambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "quiz");

        using var api = await fixture.GetAsync("http://quiz.localhost/api/v1/system");

        Assert.AreEqual(HttpStatusCode.NotFound, api.StatusCode, "the platform's API is not reachable at a lambda's address");

        using var editor = await fixture.GetAsync("http://quiz.localhost/editor/create", accept: "text/html");

        Assert.DoesNotContain(LambdaFixture.SpaMarkup, await editor.GetContentAsync(), "nor are its pages, which would run beside the lambda's scripts");
    }

    [TestMethod]
    public async Task TheAddressIsWhatTheApiHandsOut()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { HostingUrl = "https://genhttp.run" });

        var lambda = await ServeAsync(fixture, "quiz");

        Assert.AreEqual("https://quiz.genhttp.run/", lambda.PublicUrl);
        Assert.AreEqual("https://quiz.genhttp.run/", lambda.Address);

        using var served = await fixture.GetAsync("/", host: "quiz.genhttp.run");

        Assert.AreEqual("home", await served.GetContentAsync());

        using var platform = await fixture.GetAsync("/api/v1/system");

        var described = await platform.GetContentAsync<PlatformResponse>();

        Assert.AreEqual("https://{key}.genhttp.run/", described.LambdaUrl, "what the pages that ask for a key show around it");
        Assert.IsTrue(described.Starters.Where(s => s.Demo != null).All(s => s.Demo!.StartsWith("https://demo-", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task AnAgentIsGivenTheAddress()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { HostingUrl = "https://genhttp.run" });

        using var response = await fixture.SendAsync(HttpMethod.Post, "/mcp", new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "tools/call",
            @params = new { name = "create_lambda", arguments = new { acceptTerms = true, publicKey = "agents" } }
        }, accept: "application/json, text/event-stream");

        var text = await response.Content.ReadAsStringAsync();

        Assert.Contains("https://agents.genhttp.run/", text);
        Assert.DoesNotContain("/lambda/agents/", text);
    }

    [TestMethod]
    public async Task NothingOnlineAtAnAddressIsSaidPlainly()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { PublicUrl = "https://genhttp.dev" });

        var lambda = await fixture.CreateLambdaAsync("offline");

        foreach (var address in (string[]) ["http://nobody.localhost/", "http://offline.localhost/", "http://www.localhost/", "http://a.b.localhost/"])
        {
            using var page = await fixture.GetAsync(address, accept: "text/html");

            Assert.AreEqual(HttpStatusCode.NotFound, page.StatusCode, address);

            var markup = await page.GetContentAsync();

            Assert.Contains("Nothing is running here", markup, address);
            Assert.Contains("href=\"https://genhttp.dev/\"", markup, "and it leads to where an app is made");
            Assert.DoesNotContain(LambdaFixture.SpaMarkup, markup, "never a page of the platform, whose scripts would run there");

            using var client = await fixture.GetAsync(address, accept: "application/json");

            Assert.AreEqual(HttpStatusCode.NotFound, client.StatusCode);
            Assert.AreEqual("application/json", client.Content.Headers.ContentType?.MediaType);
        }

        Assert.IsNull(lambda.ActiveVersion);
    }

    #endregion

    #region Its old address

    [TestMethod]
    public async Task TheOldAddressSendsVisitorsToTheNewOne()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { HostingUrl = "https://genhttp.run" });

        await ServeAsync(fixture, "quiz");

        using var page = await fixture.GetAsync("/lambda/quiz/start?team=a%26b&round=2");

        Assert.AreEqual(HttpStatusCode.MovedPermanently, page.StatusCode);
        Assert.AreEqual("https://quiz.genhttp.run/start?team=a%26b&round=2", page.Headers.Location?.ToString(), "with the path and the query it was asked for");
        Assert.AreEqual(TimeSpan.FromDays(1), page.Headers.CacheControl?.MaxAge, "not for good: where it answers changes with a domain");

        using var root = await fixture.GetAsync("/lambda/Quiz");

        Assert.AreEqual("https://quiz.genhttp.run/", root.Headers.Location?.ToString(), "a key is read as it is stored");

        using var posted = await fixture.SendAsync(HttpMethod.Post, "/lambda/quiz/items");

        Assert.AreEqual(HttpStatusCode.PermanentRedirect, posted.StatusCode, "followed with the same method and body");
        Assert.AreEqual("https://quiz.genhttp.run/items", posted.Headers.Location?.ToString());

        using var put = await fixture.SendAsync(HttpMethod.Put, "/lambda/quiz/items");

        Assert.AreEqual(HttpStatusCode.PermanentRedirect, put.StatusCode);

        using var head = await fixture.SendAsync(HttpMethod.Head, "/lambda/quiz/");

        Assert.AreEqual(HttpStatusCode.MovedPermanently, head.StatusCode, "a read, like a GET");
    }

    [TestMethod]
    public async Task TheOldAddressPassesTheQueryOnAsItWasSent()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { HostingUrl = "https://genhttp.run" });

        // a callback whose sender signed the query as it sent it
        using var moved = await fixture.GetAsync("/lambda/quiz/callback?state=a+b%2Fc&flag&code=x%26y");

        Assert.AreEqual("https://quiz.genhttp.run/callback?state=a+b%2Fc&flag&code=x%26y", moved.Headers.Location?.OriginalString);
    }

    [TestMethod]
    public async Task NothingAboutThePlatformRunsForTheOldAddress()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "quiz");

        var telemetry = fixture.Application.Services.GetRequiredService<ITelemetryService>();

        var requests = telemetry.TotalRequests;

        var lines = RequestLines(fixture).Count;

        using (var moved = await fixture.GetAsync("/lambda/quiz/start"))
        {
            Assert.AreEqual(HttpStatusCode.MovedPermanently, moved.StatusCode);
        }

        Assert.AreEqual(requests, telemetry.TotalRequests, "not counted");
        Assert.AreEqual(lines, RequestLines(fixture).Count, "not logged");

        using var traffic = await fixture.GetAsync("http://quiz.localhost/start");

        Assert.AreEqual("mine", await traffic.GetContentAsync());
        Assert.IsGreaterThan(requests, telemetry.TotalRequests, "where it is sent to, it is");
    }

    [TestMethod]
    public async Task AKeyNobodyHasIsSentOnAsWell()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var moved = await fixture.GetAsync("/lambda/later/");

        Assert.AreEqual("http://later.localhost:8080/", moved.Headers.Location?.ToString(), "the address moved, whether or not something is there");

        using var invalid = await fixture.GetAsync("/lambda/not_a_key!/", accept: "text/html");

        Assert.IsNull(invalid.Headers.Location, "what could never have been a key is sent nowhere - it is the platform's to say it is not found");
    }

    [TestMethod]
    public async Task ALambdaWhoseKeyIsReservedSinceKeepsAnswering()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await ServeAsync(fixture, "mailroom");

        // the key it had before the name was kept for something else
        await using (var database = fixture.Application.Services.GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<GenHTTP.Lambda.Data.LambdaDbContext>>()
                                           .CreateDbContext())
        {
            database.Lambdas.Single(l => l.PublicKey == "mailroom").PublicKey = "mail";
            await database.SaveChangesAsync();
        }

        using var served = await fixture.GetAsync("http://mail.localhost/start");

        Assert.AreEqual("mine", await served.GetContentAsync(), "a rule for claiming a key takes no lambda offline");

        using var moved = await fixture.GetAsync("/lambda/mail/start");

        Assert.AreEqual("http://mail.localhost:8080/start", moved.Headers.Location?.ToString(), "nor its old address");

        using var claimed = await fixture.SendAsync(HttpMethod.Post, "/api/v1/lambdas", new CreateLambdaRequest("mta-sts", true));

        Assert.AreEqual(HttpStatusCode.BadRequest, claimed.StatusCode, "while nobody claims such a name any more");

        Assert.IsNotNull(lambda.PrivateKey);
    }

    [TestMethod]
    public async Task APathOfALambdaThatStartsWithLambdaIsItsOwn()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("nested");

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(\"lambda/other\", () => \"still mine\");");

        using var response = await fixture.GetAsync("http://nested.localhost/lambda/other");

        Assert.AreEqual("still mine", await response.GetContentAsync(), "the old address is the platform's, never a lambda's");
    }

    #endregion

    #region The hosting domain itself

    [TestMethod]
    public async Task TheHostingDomainHasAPageOfItsOwn()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { HostingUrl = "https://genhttp.run", PublicUrl = "https://genhttp.dev" });

        using var page = await fixture.GetAsync("/", accept: "text/html", host: "genhttp.run");

        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode);

        var markup = await page.GetContentAsync();

        Assert.Contains("Apps made with GenHTTP Lambda", markup);
        Assert.Contains("<a href=\"https://genhttp.dev/\">genhttp.dev</a>", markup, "it leads to the site");
        Assert.Contains("href=\"https://genhttp.dev/en/imprint\"", markup, "and to who runs it");
        Assert.Contains("href=\"https://genhttp.dev/en/privacy\"", markup, "and to what is recorded of a visit");
        Assert.DoesNotContain(LambdaFixture.SpaMarkup, markup, "and is none of the site's pages");

        using var www = await fixture.GetAsync("/", accept: "text/html", host: "www.genhttp.run");

        Assert.AreEqual(HttpStatusCode.OK, www.StatusCode, "where somebody typed it the way sites used to be found");
        Assert.Contains("Apps made with GenHTTP Lambda", await www.GetContentAsync());

        using var other = await fixture.GetAsync("/anything", accept: "text/html", host: "genhttp.run");

        Assert.AreEqual(HttpStatusCode.NotFound, other.StatusCode);
        Assert.Contains("genhttp.dev", await other.GetContentAsync(), "but says the same, to whoever was looking for an app");
    }

    [TestMethod]
    public async Task ThePageOfTheHostingDomainIsInTheLanguageOfTheBrowser()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { HostingUrl = "https://genhttp.run", PublicUrl = "https://genhttp.dev" });

        await File.WriteAllTextAsync(Path.Combine(fixture.Options.WebRoot, "pages.json"), JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["hosting:/"] = new
            {
                text = new Dictionary<string, object>
                {
                    ["en"] = new { title = "Apps", description = "Made at {site}." },
                    ["de"] = new { title = "Apps hier", description = "Gemacht auf {site}." }
                }
            }
        }));

        using var request = fixture.Request("https://genhttp.run/");

        request.Headers.Add("Accept-Language", "de-AT,de;q=0.9,en;q=0.5");

        using var page = await fixture.Host.GetResponseAsync(request);

        var markup = await page.GetContentAsync();

        Assert.Contains("<html lang=\"de\">", markup);
        Assert.Contains("Gemacht auf <a href=\"https://genhttp.dev/\">genhttp.dev</a>.", markup);
        Assert.AreEqual("de", page.Content.Headers.ContentLanguage.Single());
        Assert.IsTrue(page.Headers.Vary.Contains("Accept-Language"));

        using var english = await fixture.GetAsync("https://genhttp.run/");

        Assert.Contains("Made at", await english.GetContentAsync(), "and in English where the browser asks for none of the languages");
    }

    [TestMethod]
    public async Task OnLocalhostThePlatformIsTheHostingDomain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var platform = await fixture.GetAsync("/api/v1/system", host: "localhost");

        Assert.AreEqual(HttpStatusCode.OK, platform.StatusCode, "localhost is the platform's, its subdomains the lambdas'");
    }

    [TestMethod]
    public async Task ASubdomainOfTheHostingDomainIsNobodysDomainOfTheirOwn()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { HostingUrl = "https://genhttp.run" });

        var lambda = await fixture.CreateLambdaAsync("greedy");

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        foreach (var claimed in new[] { "genhttp.run", "quiz.genhttp.run" })
        {
            using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/domain", new DomainChangeRequest(claimed));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, claimed);
        }
    }

    [TestMethod]
    public async Task AHostingAddressNoLambdaCanAnswerBelowIsRefusedOnStartup()
    {
        foreach (var configured in new[] { "not an address", "ftp://genhttp.run", "https://127.0.0.1", "https://genhttp.run/apps", "https://run" })
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await using var _ = await LambdaFixture.CreateAsync(o => o with { HostingUrl = configured });
            }, configured);
        }
    }

    #endregion

    #region What it is logged as

    [TestMethod]
    public async Task TheRequestLineNamesTheAddress()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "quiz");

        using (await fixture.GetAsync("http://quiz.localhost/api/v1/logs")) { }

        var line = RequestLines(fixture).Last();

        Assert.AreEqual("quiz", line.Lambda);
        Assert.AreEqual("quiz.localhost", line.Domain);
        Assert.StartsWith("GET quiz.localhost/api/v1/logs", line.Text, "a path of a lambda is never taken for one of the platform's");
    }

    [TestMethod]
    public async Task TheTrafficCountsItUnderItsAddress()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await ServeAsync(fixture, "quiz");

        using (await fixture.GetAsync("http://quiz.localhost/start")) { }

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/traffic");

        var traffic = await response.GetContentAsync<LambdaTraffic>();

        Assert.AreEqual(1, traffic.Entrances.Single(e => e.Domain == "quiz.localhost").Requests);
        Assert.IsTrue(traffic.Paths.Any(p => p.Path == "/start"));
    }

    #endregion

    #region Helpers

    private static async Task<LambdaResponse> ServeAsync(LambdaFixture fixture, string key)
    {
        var lambda = await fixture.CreateLambdaAsync(key);

        var deployment = await fixture.DeployAsync(lambda.PrivateKey, Code);

        return deployment.Lambda!;
    }

    private static List<LogLine> RequestLines(LambdaFixture fixture)
        => [.. fixture.Book.Read(0, null, Microsoft.Extensions.Logging.LogLevel.Trace, 10_000).Lines.Where(l => l.Source == "Requests")];

    #endregion

}
