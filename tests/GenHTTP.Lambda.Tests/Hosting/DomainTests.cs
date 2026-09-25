using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// A premium lambda answering at a domain of its own, and everything that
/// has to stay true of it there.
/// </summary>
[TestClass]
public sealed class DomainTests
{

    private const string Domain = "shop.example.com";

    /// <summary>
    /// Answers at its root and at a path the platform has a route of its own
    /// for, so a request that reached the platform instead is told apart.
    /// </summary>
    private const string Code = """
        return Inline.Create()
                     .Get(() => "home")
                     .Get("start", () => "mine");
        """;

    #region Reaching it

    [TestMethod]
    public async Task ThePremiumLambdaAnswersAtItsDomain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "shop");

        using var home = await fixture.GetAsync("/", host: Domain);

        Assert.AreEqual(HttpStatusCode.OK, home.StatusCode);
        Assert.AreEqual("home", await home.GetContentAsync());
    }

    [TestMethod]
    public async Task EveryPathOfTheDomainBelongsToTheLambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "shop");

        // the platform answers /start itself; at the domain it is the lambda's
        using var start = await fixture.GetAsync("/start", host: Domain);

        Assert.AreEqual("mine", await start.GetContentAsync());

        using var api = await fixture.GetAsync("/api/v1/system", host: Domain);

        Assert.AreEqual(HttpStatusCode.NotFound, api.StatusCode, "the platform's API is not reachable through somebody's domain");
    }

    [TestMethod]
    public async Task ThePortAndCaseOfTheHostDoNotMatter()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "shop");

        using var response = await fixture.GetAsync("/", host: "Shop.Example.COM:8080");

        Assert.AreEqual("home", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task ThePathOnThePlatformKeepsWorking()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "shop");

        using var response = await fixture.GetAsync("/lambda/shop/start");

        Assert.AreEqual("mine", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task AnyOtherHostIsThePlatform()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "shop");

        using var response = await fixture.GetAsync("/api/v1/system", host: "somewhere-else.example.com");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task AnOfflineLambdaSaysSoRatherThanShowingThePlatform()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await ServeAsync(fixture, "shop");

        using (await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/stop")) { }

        using var response = await fixture.GetAsync("/", accept: "text/html", host: Domain);

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var page = await response.GetContentAsync();

        Assert.DoesNotContain(LambdaFixture.SpaMarkup, page, "the platform's pages would load their assets from the domain and fall apart");
        Assert.Contains("not online", page);
    }

    #endregion

    #region The tier

    [TestMethod]
    public async Task AFreeLambdaCannotHaveADomain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("free");

        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/domain", new DomainChangeRequest(Domain));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task LeavingTheTierStopsServingTheDomainButKeepsIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await ServeAsync(fixture, "shop");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Free);

        using (var response = await fixture.GetAsync("/start", host: Domain))
        {
            Assert.AreNotEqual("mine", await response.GetContentAsync(), "the domain is the platform's again");
        }

        var described = await DescribeAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(Domain, described.Domain, "still configured");
        Assert.IsFalse(described.Served);
        Assert.IsFalse(described.Allowed);

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Premium);

        using (var response = await fixture.GetAsync("/start", host: Domain))
        {
            Assert.AreEqual("mine", await response.GetContentAsync(), "and served again once it is back");
        }
    }

    [TestMethod]
    public async Task APremiumLambdaIsNotSwept()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await ServeAsync(fixture, "shop");

        var report = await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow + fixture.Options.Retention + TimeSpan.FromDays(1));

        Assert.AreEqual(0, report.Undeployed);
        Assert.AreEqual(0, report.Deleted);

        var described = await fixture.Meta.GetAsync(lambda.PrivateKey);

        Assert.IsNotNull(described!.ActiveVersion);
        Assert.IsNull(described.DeployedUntil, "nothing is going to take it down");
        Assert.IsNull(described.KeptUntil);
    }

    #endregion

    #region Configuring it

    [TestMethod]
    public async Task TheDomainIsStoredTheWayItIsMatched()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("tidy");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Premium);

        var described = await SetAsync(fixture, lambda.PrivateKey, "https://Shop.Example.com./some/page");

        Assert.AreEqual(Domain, described.Domain);
        Assert.IsTrue(described.Served);
    }

    [TestMethod]
    public async Task TwoLambdasCannotShareADomain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "first");

        var second = await fixture.CreateLambdaAsync("second");

        await fixture.ChangeTierAsync(second.PrivateKey, LambdaTier.Premium);

        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{second.PrivateKey}/domain", new DomainChangeRequest(Domain));

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }

    [TestMethod]
    public async Task ThePlatformsOwnDomainCannotBeClaimed()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { PublicUrl = "https://genhttp.dev" });

        var lambda = await fixture.CreateLambdaAsync("greedy");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Premium);

        foreach (var claimed in new[] { "genhttp.dev", "www.genhttp.dev", "localhost" })
        {
            using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/domain", new DomainChangeRequest(claimed));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, claimed);
        }
    }

    [TestMethod]
    public async Task RemovingTheDomainStopsServingIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await ServeAsync(fixture, "shop");

        using (await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/domain")) { }

        using var response = await fixture.GetAsync("/start", host: Domain);

        Assert.AreNotEqual("mine", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task DeletingTheLambdaFreesItsDomain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await ServeAsync(fixture, "shop");

        using (await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}")) { }

        Assert.IsFalse(fixture.Application.Services.GetRequiredService<DomainRegistry>().TryFind(Domain, out _));

        await ServeAsync(fixture, "successor");
    }

    [TestMethod]
    public async Task MovingTheKeyKeepsTheDomain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await ServeAsync(fixture, "shop");

        using (await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}", new UpdateLambdaRequest("store"))) { }

        using var response = await fixture.GetAsync("/start", host: Domain);

        Assert.AreEqual("mine", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TheOwnerIsToldWhatTheDomainResolvesTo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("local");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Premium);

        // the one name that resolves the same everywhere, without a network
        await using var database = fixture.Application.Services.GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<GenHTTP.Lambda.Data.LambdaDbContext>>()
                                          .CreateDbContext();

        var row = database.Lambdas.Single(l => l.PublicKey == "local");
        row.Domain = "localhost";
        await database.SaveChangesAsync();

        var described = await DescribeAsync(fixture, lambda.PrivateKey);

        Assert.IsNotNull(described.Dns);
        Assert.IsTrue(described.Dns.Addresses.Count > 0 || described.Dns.Problem != null);
    }

    #endregion

    #region The layers around it

    [TestMethod]
    public async Task TheBudgetOfAClientIsSharedBetweenTheRoutes()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { RateLimit = 2 });

        await ServeAsync(fixture, "shop");

        using (var first = await fixture.GetAsync("/lambda/shop/"))
        {
            Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
        }

        using (var second = await fixture.GetAsync("/", host: Domain))
        {
            Assert.AreEqual(HttpStatusCode.OK, second.StatusCode);
        }

        using var third = await fixture.GetAsync("/", host: Domain);

        Assert.AreEqual(HttpStatusCode.TooManyRequests, third.StatusCode, "one allowance, whichever door it is spent at");
    }

    [TestMethod]
    public async Task TheRequestLineNamesTheDomain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "shop");

        using (await fixture.GetAsync("/start", host: Domain)) { }

        var line = RequestLines(fixture).Last();

        Assert.AreEqual("shop", line.Lambda);
        Assert.AreEqual(Domain, line.Domain);
        Assert.StartsWith($"GET {Domain}/start", line.Text);
    }

    [TestMethod]
    public async Task APathOfTheDomainIsNeverTakenForOneOfThePlatforms()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "shop");

        // the platform keeps its own polling of the log out of the log; a
        // lambda that happens to have the same path is not polling anything
        using (await fixture.GetAsync("/api/v1/logs", host: Domain)) { }

        Assert.IsTrue(RequestLines(fixture).Any(l => l.Text.StartsWith($"GET {Domain}/api/v1/logs", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task ARequestToThePlatformNamesNoDomain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await ServeAsync(fixture, "shop");

        using (await fixture.GetAsync("/lambda/shop/start")) { }

        var line = RequestLines(fixture).Last();

        Assert.IsNull(line.Domain);
        Assert.StartsWith("GET /lambda/shop/start", line.Text);
    }

    [TestMethod]
    public async Task TheTrafficSaysWhichWayVisitorsCame()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await ServeAsync(fixture, "shop");

        using (await fixture.GetAsync("/", host: Domain)) { }
        using (await fixture.GetAsync("/start", host: Domain)) { }
        using (await fixture.GetAsync("/lambda/shop/")) { }

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/traffic");

        var traffic = await response.GetContentAsync<LambdaTraffic>();

        Assert.AreEqual(2, traffic.Entrances.Single(e => e.Domain == Domain).Requests);
        Assert.AreEqual(1, traffic.Entrances.Single(e => e.Domain == null).Requests);

        Assert.IsTrue(traffic.Paths.Any(p => p.Path == "/start"), "paths are the lambda's own, whichever way it was reached");
    }

    #endregion

    #region Helpers

    /// <summary>
    /// A premium lambda, deployed and answering at <see cref="Domain"/>.
    /// </summary>
    private static async Task<LambdaResponse> ServeAsync(LambdaFixture fixture, string key)
    {
        var lambda = await fixture.CreateLambdaAsync(key);

        await fixture.DeployAsync(lambda.PrivateKey, Code);

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Premium);

        var described = await SetAsync(fixture, lambda.PrivateKey, Domain);

        Assert.IsTrue(described.Served);

        return lambda;
    }

    private static async Task<DomainResponse> SetAsync(LambdaFixture fixture, string privateKey, string domain)
    {
        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/domain", new DomainChangeRequest(domain));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<DomainResponse>();
    }

    private static async Task<DomainResponse> DescribeAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/domain");

        return await response.GetContentAsync<DomainResponse>();
    }

    private static List<LogLine> RequestLines(LambdaFixture fixture)
        => [.. fixture.Book.Read(0, null, Microsoft.Extensions.Logging.LogLevel.Trace, 10_000).Lines.Where(l => l.Source == "Requests")];

    #endregion

}
