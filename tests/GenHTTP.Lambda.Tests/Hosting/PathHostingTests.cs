using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// An installation can have its lambdas answer below /lambda/ on its own
/// host instead of at subdomains - the way the community edition runs on a
/// single name, such as http://localhost:8080.
/// </summary>
[TestClass]
public sealed class PathHostingTests
{

    private const string Code = """
        return Inline.Create()
                     .Get(() => "home")
                     .Get("start", () => "mine")
                     .Post("items", () => "posted");
        """;

    private const string Domain = "shop.example.com";

    #region Its address

    [TestMethod]
    public async Task ALambdaAnswersBelowLambdaOnThePlatformsHost()
    {
        await using var fixture = await CreateAsync();

        var lambda = await ServeAsync(fixture, "quiz");

        Assert.AreEqual("http://localhost:8080/lambda/quiz/", lambda.PublicUrl);
        Assert.AreEqual("http://localhost:8080/lambda/quiz/", lambda.Address);

        using var home = await fixture.GetAsync("/lambda/quiz/");

        Assert.AreEqual(HttpStatusCode.OK, home.StatusCode);
        Assert.AreEqual("home", await home.GetContentAsync());
    }

    [TestMethod]
    public async Task ALambdaRoutesOnWhatComesAfterItsKey()
    {
        await using var fixture = await CreateAsync();

        await ServeAsync(fixture, "quiz");

        using var start = await fixture.GetAsync("/lambda/quiz/start");

        Assert.AreEqual("mine", await start.GetContentAsync(), "the lambda answers at the root of its address, as at a host of its own");

        using var posted = await fixture.SendAsync(HttpMethod.Post, "/lambda/quiz/items", new { });

        Assert.AreEqual("posted", await posted.GetContentAsync());
    }

    [TestMethod]
    public async Task ItsAddressWithoutTheSlashIsSentToTheOneWithIt()
    {
        await using var fixture = await CreateAsync();

        await ServeAsync(fixture, "quiz");

        using var response = await fixture.GetAsync("/lambda/quiz?ref=link");

        Assert.AreEqual(HttpStatusCode.MovedPermanently, response.StatusCode, "or every relative link of its page would leave it");
        Assert.AreEqual("/lambda/quiz/?ref=link", response.Headers.Location?.ToString());
    }

    [TestMethod]
    public async Task ThePlatformAnswersBesideItsLambdas()
    {
        await using var fixture = await CreateAsync();

        await ServeAsync(fixture, "quiz");

        using var system = await fixture.GetAsync("/api/v1/system");

        var described = await system.GetContentAsync<PlatformResponse>();

        Assert.AreEqual("http://localhost:8080/lambda/{key}/", described.LambdaUrl, "what the pages that ask for a key show around it");
    }

    [TestMethod]
    public async Task ASubdomainIsNoLambdaWhereTheyAnswerByPath()
    {
        await using var fixture = await CreateAsync();

        await ServeAsync(fixture, "quiz");

        using var response = await fixture.GetAsync("http://quiz.localhost/api/v1/system");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "the host is the platform's, whatever is in front of it");
    }

    [TestMethod]
    public async Task AKeyNothingIsOnlineAtIsSaidPlainly()
    {
        await using var fixture = await CreateAsync();

        using var page = await fixture.GetAsync("/lambda/nobody/", accept: "text/html");

        Assert.AreEqual(HttpStatusCode.NotFound, page.StatusCode);

        Assert.DoesNotContain(LambdaFixture.SpaMarkup, await page.GetContentAsync(), "never a page of the platform at a lambda's address");

        using var client = await fixture.GetAsync("/lambda/nobody/", accept: "application/json");

        Assert.AreEqual(HttpStatusCode.NotFound, client.StatusCode);
        Assert.AreEqual("application/json", client.Content.Headers.ContentType?.MediaType);
    }

    [TestMethod]
    public async Task ThePublicAddressIsWhatTheAddressesAreMadeFrom()
    {
        await using var fixture = await CreateAsync(o => o with { PublicUrl = "https://lambda.example.com" });

        var lambda = await ServeAsync(fixture, "quiz");

        Assert.AreEqual("https://lambda.example.com/lambda/quiz/", lambda.PublicUrl);
    }

    #endregion

    #region A domain of its own

    [TestMethod]
    public async Task ALambdaWithADomainAnswersThere()
    {
        await using var fixture = await CreateAsync();

        await ServeWithDomainAsync(fixture, "shop");

        using var home = await fixture.GetAsync("/start", host: Domain);

        Assert.AreEqual("mine", await home.GetContentAsync());
    }

    [TestMethod]
    public async Task ItsPathSendsVisitorsOnToItsDomain()
    {
        await using var fixture = await CreateAsync();

        await ServeWithDomainAsync(fixture, "shop");

        using var response = await fixture.GetAsync("/lambda/shop/start?ref=old");

        Assert.AreEqual(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.AreEqual($"https://{Domain}/start?ref=old", response.Headers.Location?.ToString());
    }

    #endregion

    #region Helpers

    private static Task<LambdaFixture> CreateAsync(Func<LambdaOptions, LambdaOptions>? configure = null)
        => LambdaFixture.CreateAsync(o => (configure?.Invoke(o) ?? o) with { Addressing = LambdaAddressing.Paths });

    private static async Task<LambdaResponse> ServeAsync(LambdaFixture fixture, string key)
    {
        var lambda = await fixture.CreateLambdaAsync(key);

        var deployment = await fixture.DeployAsync(lambda.PrivateKey, Code);

        return deployment.Lambda!;
    }

    private static async Task ServeWithDomainAsync(LambdaFixture fixture, string key)
    {
        var lambda = await ServeAsync(fixture, key);

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/domain", new DomainChangeRequest(Domain));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    #endregion

}
