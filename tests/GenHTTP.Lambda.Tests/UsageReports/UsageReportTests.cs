using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.UsageReports;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.UsageReports;

/// <summary>
/// The daily usage report a community installation sends to genhttp.dev:
/// how it is set up and used, never what its lambdas are or hold.
/// </summary>
[TestClass]
public sealed class UsageReportTests
{
    private const string Snippet = "return Inline.Create().Get(() => \"Hello from a lambda\");";

    [TestMethod]
    public async Task AReportSaysHowTheInstallationIsUsedAndNothingOfWhatItsLambdasAre()
    {
        await using var collector = FakeCollector.Start();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => o with { UsageReportUrl = collector.Url, PublicUrl = "https://lambda.example.com" });

        var online = await fixture.CreateLambdaAsync();
        var keeping = await fixture.CreateLambdaAsync();
        var demo = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(online.PrivateKey, Snippet);

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{keeping.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        using (var counting = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{keeping.PrivateKey}/analytics"))
        {
            Assert.IsTrue(counting.IsSuccessStatusCode);
        }

        fixture.ChangeTier(keeping.PrivateKey, LambdaTier.Premium);

        // every installation has the demos, so they say nothing about this one
        await fixture.MakeDemoAsync(demo.PublicKey);

        var status = await Reports(fixture).SendAsync();

        Assert.IsNotNull(status.Sent, status.Failure);
        Assert.IsNull(status.Failure);

        var received = collector.Reports.Single();

        Assert.AreEqual("POST", received.Method);
        Assert.AreEqual("application/json", received.ContentType);
        StringAssert.StartsWith(received.UserAgent, "GenHTTP-Lambda/");

        using var report = JsonDocument.Parse(received.Body);

        var root = report.RootElement;

        Assert.AreEqual(1, root.GetProperty("schema").GetInt32());
        Assert.MatchesRegex("^[0-9a-f]{32}$", root.GetProperty("installation").GetString()!);
        Assert.AreEqual("https://lambda.example.com", root.GetProperty("url").GetString());
        Assert.AreEqual(UsageSurvey.PlatformVersion, root.GetProperty("version").GetString());

        var system = root.GetProperty("system");

        Assert.AreEqual(Environment.ProcessorCount, system.GetProperty("processors").GetInt32());
        Assert.IsGreaterThan(0, system.GetProperty("memoryBytes").GetInt64());

        Assert.IsTrue(root.GetProperty("server").TryGetProperty("genhttp", out _), "named as the collector reads it");

        Assert.AreEqual("Paths", root.GetProperty("configuration").GetProperty("addressing").GetString());

        var lambdas = root.GetProperty("lambdas");

        Assert.AreEqual(2, lambdas.GetProperty("total").GetInt32(), "the demo is not counted");
        Assert.AreEqual(1, lambdas.GetProperty("free").GetInt32());
        Assert.AreEqual(1, lambdas.GetProperty("premium").GetInt32());
        Assert.AreEqual(1, lambdas.GetProperty("online").GetInt32());
        Assert.AreEqual(2, lambdas.GetProperty("created30Days").GetInt32());

        var switches = root.GetProperty("switches");

        Assert.AreEqual(1, switches.GetProperty("database").GetInt32());
        Assert.AreEqual(1, switches.GetProperty("analytics").GetInt32());
        Assert.AreEqual(0, switches.GetProperty("openSource").GetInt32());

        Assert.IsGreaterThan(0, root.GetProperty("data").GetProperty("databaseBytes").GetInt64(), "the switched on database has its file");

        Assert.IsGreaterThan(0, root.GetProperty("activity").GetProperty("versions").GetProperty("api").GetInt32(), "the version was saved through the API");

        // nothing that names a lambda: no key, and so no address either
        foreach (var lambda in (LambdaResponse[])[online, keeping, demo])
        {
            Assert.DoesNotContain(lambda.PublicKey, received.Body);
            Assert.DoesNotContain(lambda.PrivateKey, received.Body);
        }

        Assert.DoesNotContain("Hello from a lambda", received.Body);
    }

    [TestMethod]
    public async Task AnInstallationKeepsItsIdentifierFromOneReportToTheNext()
    {
        await using var collector = FakeCollector.Start();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => o with { UsageReportUrl = collector.Url });

        await Reports(fixture).SendAsync();
        await Reports(fixture).SendAsync();

        var installations = collector.Reports.Select(r => JsonDocument.Parse(r.Body).RootElement.GetProperty("installation").GetString())
                                     .ToList();

        Assert.HasCount(2, installations);
        Assert.AreEqual(installations[0], installations[1]);
    }

    [TestMethod]
    public async Task WithoutAnAddressNothingIsGatheredNorSent()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync();

        await fixture.CreateLambdaAsync();

        var status = await Reports(fixture).SendAsync();

        Assert.IsFalse(status.Enabled);
        Assert.IsNull(status.Attempted);
        Assert.IsNull(status.Report);

        // not even the identifier is made
        var databases = fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        await using var database = await databases.CreateDbContextAsync();

        Assert.IsFalse(await database.Settings.AnyAsync(s => s.Key == "installation"));
    }

    [TestMethod]
    public async Task ACollectorThatCannotBeReachedIsWarnedAboutOnceAndTriedAgainNextTime()
    {
        // a port nobody listens on, which refuses the connection
        var nowhere = $"http://127.0.0.1:{FakeCollector.FreePort()}/api/reports";

        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => o with { UsageReportUrl = nowhere });

        var first = await Reports(fixture).SendAsync();
        var second = await Reports(fixture).SendAsync();

        Assert.IsNotNull(first.Failure);
        Assert.IsNull(second.Sent);
        Assert.IsNotNull(second.Report, "what would have been sent is kept for the panel");

        var warnings = fixture.Book.Read(0, null, LogLevel.Warning, 5000).Lines
                              .Where(l => l.Text.StartsWith("Failed to send usage report", StringComparison.Ordinal))
                              .ToList();

        Assert.HasCount(1, warnings, "a firewall that keeps it in is not complained about every day");
    }

    [TestMethod]
    public async Task ACollectorThatNeverAnswersHoldsUpNothing()
    {
        // takes the connection and never answers, as a firewall that drops
        // packets would leave it hanging
        var silent = new TcpListener(IPAddress.Loopback, 0);
        silent.Start();

        try
        {
            var url = $"http://127.0.0.1:{((IPEndPoint)silent.LocalEndpoint).Port}/api/reports";

            await using var fixture = await LambdaFixture.CreateCommunityAsync(o => o with
            {
                UsageReportUrl = url,
                // the jobs that fetch from the internet stay quiet in a test
                Geo = false,
                GeoPlaces = false,
                CrawlerLists = false
            });

            var watch = Stopwatch.StartNew();

            fixture.Platform.StartBackgroundJobs();

            using var response = await fixture.GetAsync("/api/v1/system");

            watch.Stop();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsLessThan(UsageReportSender.Patience, watch.Elapsed, "the server answers while the report waits");

            var status = Reports(fixture).Status;

            Assert.IsNull(status.Sent);
            Assert.IsNull(status.Failure, "still waiting");
        }
        finally
        {
            silent.Stop();
        }
    }

    [TestMethod]
    public async Task TheOperatorReadsTheLastReportAsItWasSent()
    {
        await using var collector = FakeCollector.Start();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => LambdaFixture.WithPanel(o) with { UsageReportUrl = collector.Url });

        using (var before = await fixture.GetAsOperatorAsync("/api/v1/admin/usage-report"))
        {
            var nothingYet = await before.GetContentAsync<UsageReportResponse>();

            Assert.IsTrue(nothingYet.Enabled);
            Assert.AreEqual(collector.Url, nothingYet.Endpoint);
            Assert.IsNull(nothingYet.Report);
        }

        await Reports(fixture).SendAsync();

        using var after = await fixture.GetAsOperatorAsync("/api/v1/admin/usage-report");

        var sent = await after.GetContentAsync<UsageReportResponse>();

        Assert.IsNotNull(sent.Sent);
        Assert.AreEqual(collector.Reports.Single().Body, sent.Report);
    }

    [TestMethod]
    public async Task TheReportIsTheOperatorsToRead()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        using var response = await fixture.GetAsync("/api/v1/admin/usage-report");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #region Helpers

    private static IUsageReportService Reports(LambdaFixture fixture) => fixture.Platform.Services.GetRequiredService<IUsageReportService>();

    #endregion

}
