using System.Net;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Analytics;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Analytics;

/// <summary>
/// What an owner who switched analytics on is shown of their visitors.
/// </summary>
[TestClass]
public sealed class AnalyticsTests
{

    /// <summary>
    /// A page that answers at every path, as a small website does.
    /// </summary>
    private const string Website = """
        return Content.From(Resource.FromString("<!doctype html><title>Hello</title>").Type(new ContentType("text/html; charset=utf-8")));
        """;

    private const string Chrome = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36";

    private const string IPhone = "Mozilla/5.0 (iPhone; CPU iPhone OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.6 Mobile/15E148 Safari/604.1";

    private const string Googlebot = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";

    #region Switching

    [TestMethod]
    public async Task NothingIsCountedUntilTheOwnerSwitchesAnalyticsOn()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await OnlineAsync(fixture, "quiet");

        await VisitAsync(fixture, "http://quiet.localhost/");

        Flush(fixture);

        var before = await ReadAsync(fixture, lambda.PrivateKey);

        Assert.IsFalse(before.Enabled);
        Assert.IsNull(before.Totals, "there is nothing to show while it is off");
        Assert.AreEqual(7, before.Window, "and what a free lambda would show is said");

        await EnableAsync(fixture, lambda.PrivateKey);

        await VisitAsync(fixture, "http://quiet.localhost/");

        Flush(fixture);

        var after = await ReadAsync(fixture, lambda.PrivateKey);

        Assert.IsTrue(after.Enabled);
        Assert.AreEqual(1, after.Totals!.Hits, "the visit before it was switched on is not counted");
    }

    [TestMethod]
    public async Task SwitchingAnalyticsOffDeletesWhatWasCounted()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await OnlineAsync(fixture, "forgetful");

        await EnableAsync(fixture, lambda.PrivateKey);

        await VisitAsync(fixture, "http://forgetful.localhost/");

        Flush(fixture);

        using (var off = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/analytics"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, off.StatusCode);
        }

        // queued while it was on, written after it was switched off
        await VisitAsync(fixture, "http://forgetful.localhost/");

        Flush(fixture);

        await EnableAsync(fixture, lambda.PrivateKey);

        var again = await ReadAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(0, again.Totals!.Hits, "switched on again, it starts from nothing");
        Assert.IsNull(again.Earliest);
    }

    [TestMethod]
    public async Task ADemosVisitorsAreNotCounted()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await OnlineAsync(fixture, "showpiece");

        await fixture.MakeDemoAsync("showpiece");

        using var refused = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/analytics");

        Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode);
    }

    #endregion

    #region Counting

    [TestMethod]
    public async Task AVisitIsCountedByPageReferrerCountryAndDevice()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        Place(fixture, "CH");

        var lambda = await OnlineAsync(fixture, "shop");

        await EnableAsync(fixture, lambda.PrivateKey);

        await VisitAsync(fixture, "http://shop.localhost/", Chrome, referrer: "https://www.google.com/", destination: "document");
        await VisitAsync(fixture, "http://shop.localhost/menu", IPhone, referrer: "https://news.example.org/today?id=4", destination: "document");
        await VisitAsync(fixture, "http://shop.localhost/menu", Googlebot);

        Flush(fixture);

        var shown = await ReadAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(3, shown.Totals!.Hits);
        Assert.AreEqual(3, shown.Totals.Views, "a browser navigating and a crawler fetching HTML both opened a page");

        Assert.AreEqual(2, Views(shown.Pages, "/menu"));
        Assert.AreEqual(1, Views(shown.Pages, "/"));

        Assert.AreEqual(1, Views(shown.Referrers, "google.com"), "the host, without its www");
        Assert.AreEqual(1, Views(shown.Referrers, "news.example.org"), "never the rest of the address");
        Assert.HasCount(2, shown.Referrers!);

        Assert.AreEqual(3, Views(shown.Countries, "CH"));

        Assert.AreEqual(1, Views(shown.Devices, "desktop"));
        Assert.AreEqual(1, Views(shown.Devices, "mobile"));
        Assert.AreEqual(1, Views(shown.Devices, "bot"));

        Assert.AreEqual(1, Views(shown.Browsers, "Chrome"));
        Assert.AreEqual(1, Views(shown.Browsers, "Safari"));
        Assert.AreEqual(1, Views(shown.Systems, "iOS"));
        Assert.IsNull(shown.Browsers!.FirstOrDefault(b => b.Value == "bot"), "a bot is no browser of a person");

        Assert.AreEqual(1, Views(shown.Crawlers, "Google"));

        var today = shown.Daily!.Last();

        Assert.AreEqual(DateOnly.FromDateTime(DateTime.UtcNow), today.Date);
        Assert.AreEqual(3, today.Totals.Views);
    }

    [TestMethod]
    public async Task OnlyOpeningAPageIsAView()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await OnlineAsync(fixture, "loader");

        await EnableAsync(fixture, lambda.PrivateKey);

        await VisitAsync(fixture, "http://loader.localhost/", Chrome, destination: "document");

        // what that page goes on to load, with the page as its referrer
        await VisitAsync(fixture, "http://loader.localhost/app.js", Chrome, referrer: "http://loader.localhost/", destination: "script");
        await VisitAsync(fixture, "http://loader.localhost/api/items", Chrome, referrer: "http://loader.localhost/", destination: "empty");

        Flush(fixture);

        var shown = await ReadAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(3, shown.Totals!.Hits);
        Assert.AreEqual(1, shown.Totals.Views);
        Assert.AreEqual(1, shown.Totals.Visitors, "one visitor, however much their page loaded");

        Assert.AreEqual(1, shown.Pages!.Single(p => p.Value == "/app.js").Hits, "what was loaded is requested, not viewed");
        Assert.AreEqual(0, Views(shown.Pages, "/app.js"));
        Assert.IsEmpty(shown.Referrers!, "the lambda referring to itself is nobody coming from elsewhere");
    }

    [TestMethod]
    public async Task AScannerCannotFillADayWithPaths()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await OnlineAsync(fixture, "scanned");

        await EnableAsync(fixture, lambda.PrivateKey);

        var most = AnalyticsDimensions.MostOf(AnalyticsDimension.Pages)!.Value;

        for (var i = 0; i < most + 10; i++)
        {
            await VisitAsync(fixture, $"http://scanned.localhost/probe-{i}");

            // over more than one writing, as a day is
            if (i == most / 2)
            {
                Flush(fixture);
            }
        }

        Flush(fixture);

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/analytics");

        var store = fixture.Application.Services.GetRequiredService<AnalyticsStore>();

        var id = fixture.Meta.GetId(lambda.PrivateKey)!.Value;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var pages = store.Read(id, today, today, int.MaxValue).Breakdowns[AnalyticsDimension.Pages];

        Assert.HasCount(most + 1, pages, "as many paths as a day has room for, and the rest together");
        Assert.AreEqual(10, pages.Single(p => p.Value == AnalyticsDimensions.Other).Hits);
    }

    [TestMethod]
    public async Task APreviewOfAFeatureIsNotCounted()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await OnlineAsync(fixture, "drafty");

        await EnableAsync(fixture, lambda.PrivateKey);

        using (var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Try", null, null)))
        {
            var feature = await created.GetContentAsync<FeatureResponse>();

            using (await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/preview/start")) { }

            using var preview = await fixture.GetAsync(feature.PreviewPath);

            Assert.AreEqual(HttpStatusCode.OK, preview.StatusCode);
        }

        Flush(fixture);

        Assert.AreEqual(0, (await ReadAsync(fixture, lambda.PrivateKey)).Totals!.Hits, "somebody trying a change is not a visitor");
    }

    #endregion

    #region What is shown and kept

    [TestMethod]
    public async Task AFreeLambdaShowsAWeekAndKeepsTheYearForPremium()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await OnlineAsync(fixture, "growing");

        await EnableAsync(fixture, lambda.PrivateKey);

        await VisitAsync(fixture, "http://growing.localhost/");

        Flush(fixture);

        var analytics = fixture.Application.Services.GetRequiredService<IAnalyticsService>();

        var later = DateTime.UtcNow.AddDays(30);

        var free = analytics.Get(lambda.PrivateKey, 365, later);

        Assert.AreEqual(7, free.Shown, "asked for a year, a free lambda shows a week");
        Assert.AreEqual(0, free.Report!.Totals.Hits, "and the visit is older than that");
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.UtcNow), free.Earliest, "but it is kept, and said to be");
        Assert.IsNull(free.Previous, "nor is the week before compared, which a free lambda does not show");

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        var premium = analytics.Get(lambda.PrivateKey, 60, later);

        Assert.AreEqual(60, premium.Shown);
        Assert.AreEqual(1, premium.Report!.Totals.Hits, "premium shows what was counted before it was premium");
        Assert.IsNotNull(premium.Previous);
    }

    [TestMethod]
    public async Task AYearIsKeptAndNoMore()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await OnlineAsync(fixture, "aging");

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        await EnableAsync(fixture, lambda.PrivateKey);

        await VisitAsync(fixture, "http://aging.localhost/");

        var analytics = fixture.Application.Services.GetRequiredService<IAnalyticsService>();

        analytics.Flush();

        Assert.IsNotNull(analytics.Get(lambda.PrivateKey, now: DateTime.UtcNow.AddDays(AnalyticsStore.Kept - 1)).Earliest, "a year later it is still there");

        analytics.Flush(DateTime.UtcNow.AddDays(AnalyticsStore.Kept + 1));

        Assert.IsNull(analytics.Get(lambda.PrivateKey).Earliest, "a day past the year it is gone");
    }

    [TestMethod]
    public async Task DeletingALambdaDeletesItsAnalytics()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await OnlineAsync(fixture, "leaving");

        await EnableAsync(fixture, lambda.PrivateKey);

        await VisitAsync(fixture, "http://leaving.localhost/");

        Flush(fixture);

        var id = fixture.Meta.GetId(lambda.PrivateKey)!.Value;

        var store = fixture.Application.Services.GetRequiredService<AnalyticsStore>();

        Assert.IsNotNull(store.Earliest(id));

        fixture.Meta.Delete(lambda.PrivateKey);

        Assert.IsNull(store.Earliest(id));
    }

    #endregion

    #region Agents

    [TestMethod]
    public async Task AnAgentReadsWhatTheOwnerIsShown()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await OnlineAsync(fixture, "asked");

        var off = await CallAsync(fixture, new JsonObject { ["privateKey"] = lambda.PrivateKey });

        Assert.IsFalse(off["enabled"]!.GetValue<bool>());

        var on = await CallAsync(fixture, new JsonObject { ["privateKey"] = lambda.PrivateKey, ["enable"] = true });

        Assert.IsTrue(on["analytics"]!["enabled"]!.GetValue<bool>(), "switched on when the user asked for it");

        await VisitAsync(fixture, "http://asked.localhost/", Chrome, destination: "document");

        Flush(fixture);

        var read = await CallAsync(fixture, new JsonObject { ["privateKey"] = lambda.PrivateKey, ["days"] = 3 });

        Assert.AreEqual(3, read["analytics"]!["days"]!.GetValue<int>());
        Assert.AreEqual(1, read["analytics"]!["totals"]!["views"]!.GetValue<long>());
    }

    #endregion

    #region Helpers

    private static async Task<LambdaResponse> OnlineAsync(LambdaFixture fixture, string key)
    {
        var lambda = await fixture.CreateLambdaAsync(key);

        await fixture.DeployAsync(lambda.PrivateKey, Website);

        return lambda;
    }

    private static async Task EnableAsync(LambdaFixture fixture, string privateKey)
    {
        using var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/analytics");

        Assert.AreEqual(HttpStatusCode.OK, on.StatusCode, await on.Content.ReadAsStringAsync());
    }

    private static async Task<AnalyticsResponse> ReadAsync(LambdaFixture fixture, string privateKey)
    {
        using var read = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/analytics");

        Assert.AreEqual(HttpStatusCode.OK, read.StatusCode, await read.Content.ReadAsStringAsync());

        return await read.GetContentAsync<AnalyticsResponse>();
    }

    private static async Task VisitAsync(LambdaFixture fixture, string address, string? agent = null, string? referrer = null, string? destination = null)
    {
        using var request = fixture.Request(address);

        if (agent != null)
        {
            request.Headers.TryAddWithoutValidation("User-Agent", agent);
        }

        if (referrer != null)
        {
            request.Headers.TryAddWithoutValidation("Referer", referrer);
        }

        if (destination != null)
        {
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", destination);
        }

        using var response = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// What the job does every few minutes.
    /// </summary>
    private static void Flush(LambdaFixture fixture) => fixture.Application.Services.GetRequiredService<IAnalyticsService>().Flush();

    /// <summary>
    /// Has every address of the test host - loopback, both families -
    /// allocated to a country.
    /// </summary>
    private static void Place(LambdaFixture fixture, string country)
        => fixture.Application.Services.GetRequiredService<GeoTable>().Load([
            $"test|{country}|ipv4|127.0.0.0|16777216|20000101|allocated",
            $"test|{country}|ipv6|::1|128|20000101|allocated"
        ]);

    private static long Views(IReadOnlyList<AnalyticsValueResponse>? values, string value) => values!.SingleOrDefault(v => v.Value == value)?.Views ?? 0;

    private static async Task<JsonObject> CallAsync(LambdaFixture fixture, JsonObject arguments)
    {
        var call = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = "analytics", ["arguments"] = arguments }
        };

        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Content = new StringContent(call.ToJsonString(), System.Text.Encoding.UTF8, "application/json");
        request.Headers.Add("Accept", "application/json, text/event-stream");

        using var response = await fixture.Host.GetResponseAsync(request);

        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        var result = body["result"]!;

        Assert.IsFalse(result["isError"]?.GetValue<bool>() ?? false, result.ToJsonString());

        return result["structuredContent"]!.AsObject();
    }

    #endregion

}
