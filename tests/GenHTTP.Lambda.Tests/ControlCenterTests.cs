using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// What the control center reads about one lambda: why each version exists,
/// what was online when, how it is being used and what it has said.
/// </summary>
[TestClass]
public sealed class ControlCenterTests
{

    private const string Token = "a-token-nobody-would-guess";

    #region Version notes

    [TestMethod]
    public async Task AVersionKeepsWhyItWasWritten()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
            new VersionRequest([new LambdaFile(LambdaSource.EntryName, "return Content.From(Resource.FromString(\"hi\"));")],
                               "  Make it say hi  ", "Says hi instead of the example"));

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);

        var created = await saved.GetContentAsync<VersionResponse>();

        Assert.AreEqual("Make it say hi", created.Prompt, "kept trimmed");
        Assert.AreEqual("Says hi instead of the example", created.Change);
        Assert.AreEqual("api", created.Origin);

        using var listed = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions");

        var versions = await listed.GetContentAsync<List<VersionResponse>>();

        Assert.AreEqual("Says hi instead of the example", versions[0].Change);

        // the one the lambda was created with says where it came from
        Assert.AreEqual("template", versions[1].Origin);
        Assert.IsNotNull(versions[1].Change);

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/{created.Version}");

        var content = await read.GetContentAsync<VersionContentResponse>();

        Assert.AreEqual("Make it say hi", content.Prompt);
        Assert.AreEqual("Says hi instead of the example", content.Change);
    }

    [TestMethod]
    public async Task NotesAreOptional()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                  LambdaFixture.Version("return Content.From(Resource.FromString(\"hi\"));"));

        var created = await saved.GetContentAsync<VersionResponse>();

        Assert.IsNull(created.Prompt);
        Assert.IsNull(created.Change);
    }

    [TestMethod]
    public async Task ALongNoteIsCutRatherThanRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
            new VersionRequest([new LambdaFile(LambdaSource.EntryName, "return Content.From(Resource.FromString(\"hi\"));")],
                               new string('p', 10_000), new string('c', 2_000)));

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, "the code matters more than the note about it");

        var created = await saved.GetContentAsync<VersionResponse>();

        Assert.IsLessThanOrEqualTo(VersionNote.MaxPrompt, created.Prompt!.Length);
        Assert.IsLessThanOrEqualTo(VersionNote.MaxChange, created.Change!.Length);
    }

    #endregion

    #region Deployment history

    [TestMethod]
    public async Task EveryDeploymentIsRemembered()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);
        await fixture.DeployAsync(lambda.PrivateKey, "return Content.From(Resource.FromString(\"second\"));");

        var history = await HistoryAsync(fixture, lambda.PrivateKey);

        Assert.HasCount(2, history);

        Assert.AreEqual(2, history[0].Version, "newest first");
        Assert.IsNull(history[0].Ended, "what is online has not ended");
        Assert.AreEqual("api", history[0].Origin);

        Assert.AreEqual(1, history[1].Version);
        Assert.IsNotNull(history[1].Ended);
        Assert.AreEqual("replaced", history[1].EndedBy);
    }

    [TestMethod]
    public async Task TakingItOfflineEndsTheDeployment()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        using var stopped = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/stop");

        Assert.AreEqual(HttpStatusCode.OK, stopped.StatusCode);

        var history = await HistoryAsync(fixture, lambda.PrivateKey);

        Assert.HasCount(1, history);
        Assert.AreEqual("stopped", history[0].EndedBy);
    }

    [TestMethod]
    public async Task TheOperatorTakingItOfflineSaysSo()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { AdminToken = Token });

        var lambda = await fixture.CreateLambdaAsync("watched");

        await fixture.DeployAsync(lambda.PrivateKey);

        using var request = fixture.Host.GetRequest("/api/v1/admin/lambdas/watched/deployment/stop", HttpMethod.Post);

        request.Headers.Add("X-Admin-Token", Token);

        using var response = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var history = await HistoryAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual("admin", history[0].EndedBy);
    }

    [TestMethod]
    public async Task TheSweepSaysItWasTheSweep()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with
        {
            DeploymentLifetime = TimeSpan.FromDays(30),
            Retention = TimeSpan.FromDays(90)
        });

        var lambda = await fixture.CreateLambdaAsync("abandoned");

        await fixture.DeployAsync(lambda.PrivateKey);

        var databases = fixture.Application.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        await using (var database = await databases.CreateDbContextAsync())
        {
            var row = await database.Lambdas.SingleAsync(l => l.PublicKey == "abandoned");

            var old = DateTime.UtcNow.AddDays(-45);

            row.Deployed = old;
            row.Modified = old;
            row.LastSeen = old;

            await database.SaveChangesAsync();
        }

        var report = await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow);

        Assert.AreEqual(1, report.Undeployed);

        var history = await HistoryAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(ActivationEndings.Expired, history[0].EndedBy);
    }

    [TestMethod]
    public async Task TheHistoryGoesWithTheLambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        using var removed = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}");

        var databases = fixture.Application.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        await using var database = await databases.CreateDbContextAsync();

        Assert.AreEqual(0, await database.Activations.CountAsync());
    }

    #endregion

    #region Traffic

    [TestMethod]
    public async Task TrafficIsCountedPerPath()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("counted");

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create()
                         .Get("hello", () => "hi")
                         .Get("broken", () => { throw new InvalidOperationException("nope"); return "never"; });
            """);

        for (var i = 0; i < 3; i++)
        {
            using var _ = await fixture.GetAsync("/lambda/counted/hello");
        }

        using (var _ = await fixture.GetAsync("/lambda/counted/broken")) { }
        using (var _ = await fixture.GetAsync("/lambda/counted/missing")) { }

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/traffic");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var traffic = await response.GetContentAsync<LambdaTraffic>();

        Assert.AreEqual(5, traffic.Totals!.Requests);
        Assert.AreEqual(5, traffic.Minutes.Sum(m => m.Requests), "the last hour holds all of it");
        Assert.AreEqual(5, traffic.Quarters.Sum(m => m.Requests), "and so does the last day");
        Assert.HasCount(60, traffic.Minutes);
        Assert.HasCount(96, traffic.Quarters);

        Assert.AreEqual(3, traffic.Statuses.Success);
        Assert.AreEqual(1, traffic.Statuses.ServerError);
        Assert.AreEqual(1, traffic.Statuses.ClientError);

        var hello = traffic.Paths.Single(p => p.Path == "/hello");

        Assert.AreEqual(3, hello.Requests);
        Assert.AreEqual(1, traffic.Paths.Single(p => p.Path == "/broken").Failed);
    }

    [TestMethod]
    public async Task ALambdaNobodyCalledHasEmptyTraffic()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/traffic");

        var traffic = await response.GetContentAsync<LambdaTraffic>();

        Assert.IsNull(traffic.Totals);
        Assert.HasCount(60, traffic.Minutes, "the same shape to draw either way");
        Assert.AreEqual(0, traffic.Minutes.Sum(m => m.Requests));
    }

    [TestMethod]
    public void PathsAreBounded()
    {
        var telemetry = new LambdaTelemetry();

        for (var i = 0; i < 500; i++)
        {
            telemetry.Record(1, "scanned", TimeSpan.FromMilliseconds(1), 404, 0, $"/probe-{i}");
        }

        var traffic = telemetry.Describe(1);

        Assert.IsLessThanOrEqualTo(65, traffic.Paths.Count, "a scanner should not cost a row per guess");
        Assert.AreEqual(500, traffic.Paths.Sum(p => p.Requests), "but every request is still counted somewhere");
    }

    #endregion

    #region Logs

    [TestMethod]
    public async Task TheOwnerReadsTheirOwnLog()
    {
        ConsoleTee.Install();

        await using var fixture = await LambdaFixture.CreateAsync();

        var mine = await fixture.CreateLambdaAsync("mine");
        var theirs = await fixture.CreateLambdaAsync("theirs");

        await fixture.DeployAsync(mine.PrivateKey, """
            return Inline.Create().Get(() => { Console.WriteLine("mine speaks"); return "ok"; });
            """);

        await fixture.DeployAsync(theirs.PrivateKey, """
            return Inline.Create().Get(() => { Console.WriteLine("theirs speaks"); return "ok"; });
            """);

        using (var _ = await fixture.GetAsync("/lambda/mine/")) { }
        using (var _ = await fixture.GetAsync("/lambda/theirs/")) { }

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{mine.PrivateKey}/logs");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        var log = await response.GetContentAsync<OwnerLogResponse>();

        Assert.IsTrue(log.Lines.Any(l => l.Text == "mine speaks"), string.Join(" | ", log.Lines.Select(l => l.Text)));
        Assert.IsTrue(log.Lines.Any(l => l.Source == "Requests"), "the requests it answered are part of its log");
        Assert.IsFalse(log.Lines.Any(l => l.Text == "theirs speaks"), "and nothing of anybody else's");

        Assert.DoesNotContain("\"client\"", body, "the address of a visitor is not the owner's to read");
    }

    [TestMethod]
    public async Task AKeyThatChangedHandsDoesNotBringItsLogAlong()
    {
        ConsoleTee.Install();

        await using var fixture = await LambdaFixture.CreateAsync();

        var first = await fixture.CreateLambdaAsync("reused");

        await fixture.DeployAsync(first.PrivateKey, """
            return Inline.Create().Get(() => { Console.WriteLine("the first owner"); return "ok"; });
            """);

        using (var _ = await fixture.GetAsync("/lambda/reused/")) { }

        using (var _ = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{first.PrivateKey}")) { }

        var second = await fixture.CreateLambdaAsync("reused");

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{second.PrivateKey}/logs");

        var log = await response.GetContentAsync<OwnerLogResponse>();

        Assert.IsFalse(log.Lines.Any(l => l.Text.Contains("the first owner")),
                       "the lines belong to the lambda that wrote them, not to whoever holds the name now");
    }

    [TestMethod]
    public async Task TheLogCanBeFollowed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("followed");

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(() => \"ok\");");

        using (var _ = await fixture.GetAsync("/lambda/followed/")) { }

        using var first = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/logs");

        var page = await first.GetContentAsync<OwnerLogResponse>();

        using var again = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/logs?since={page.Cursor}");

        var next = await again.GetContentAsync<OwnerLogResponse>();

        Assert.IsEmpty(next.Lines, "nothing new since the cursor");
    }

    #endregion

    #region Summary

    [TestMethod]
    public async Task TheSummarySaysWhatIsGoingOn()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("summed");

        await fixture.DeployAsync(lambda.PrivateKey, LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Layout.Create().Add(Assets.App(\"site\"));"),
            new LambdaFile("site/index.html", "<!doctype html><title>summed</title>")
        ]));

        using (var _ = await fixture.GetAsync("/lambda/summed/")) { }

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.GetContentAsync<LambdaSummaryResponse>();

        Assert.AreEqual(2, summary.Live!.Version);
        Assert.AreEqual(2, summary.Latest!.Version);
        Assert.AreEqual(2, summary.Versions);

        Assert.IsNotNull(summary.Activation);
        Assert.AreEqual(2, summary.Activation.Version);

        Assert.AreEqual(1, summary.Traffic.HourRequests);
        Assert.HasCount(24, summary.Traffic.Hourly);

        Assert.AreEqual(1, summary.Storage.CodeFiles);
        Assert.AreEqual(1, summary.Storage.Assets);
        Assert.IsTrue(summary.Storage.ServesAssets, "the code asks for its assets to be served");
        Assert.IsFalse(summary.Storage.ServesWorkspace);

        Assert.AreEqual(fixture.Options.MaxCodeLength, summary.Limits.CodeCharacters);
    }

    [TestMethod]
    public async Task TheSummaryShowsWhatWentWrong()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("failing");

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create().Get(() => { throw new InvalidOperationException("the database is on fire"); return "never"; });
            """);

        using (var _ = await fixture.GetAsync("/lambda/failing/")) { }

        // a missing favicon is a warning in the log, and not a problem
        using (var _ = await fixture.GetAsync("/lambda/failing/favicon.ico")) { }

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");

        var summary = await response.GetContentAsync<LambdaSummaryResponse>();

        Assert.AreEqual(1, summary.Traffic.HourFailed);

        Assert.IsTrue(summary.RecentProblems.Any(p => p.Text.Contains("the database is on fire") || (p.Detail?.Contains("the database is on fire") ?? false)),
                      string.Join(" | ", summary.RecentProblems.Select(p => p.Text)));

        Assert.IsFalse(summary.RecentProblems.Any(p => p.Text.Contains("favicon")));
    }

    [TestMethod]
    public async Task NothingIsAnsweredWithoutTheKey()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.CreateLambdaAsync("guarded");

        foreach (var path in (string[])["summary", "traffic", "logs", "deployment/history"])
        {
            using var response = await fixture.GetAsync($"/api/v1/lambdas/not-the-key/{path}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, path);
        }

        // the public key is an address, not a key to anything
        using var byPublic = await fixture.GetAsync("/api/v1/lambdas/guarded/summary");

        Assert.AreEqual(HttpStatusCode.NotFound, byPublic.StatusCode);
    }

    [TestMethod]
    public async Task WatchingDoesNotFillTheLog()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        for (var i = 0; i < 3; i++)
        {
            using var _ = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");
        }

        var (lines, _, _) = fixture.Book.Read(0, null, Microsoft.Extensions.Logging.LogLevel.Information, 5000);

        Assert.IsFalse(lines.Any(l => l.Text.Contains("/summary")), "a page that polls should not become the log");
    }

    #endregion

    #region Lambdas from before

    /// <summary>
    /// A lambda written before versions had notes and deployments had a
    /// history: every new column empty, no activation rows. It has to be
    /// read, shown, changed and rolled back like any other.
    /// </summary>
    [TestMethod]
    public async Task ALambdaFromBeforeTheNotesStillWorks()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("from-before");

        await fixture.DeployAsync(lambda.PrivateKey, "return Content.From(Resource.FromString(\"old\"));");

        var databases = fixture.Application.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        await using (var database = await databases.CreateDbContextAsync())
        {
            // what V6 found when it ran: nothing about why, nothing about when
            await database.Database.ExecuteSqlRawAsync("UPDATE deployments SET prompt = NULL, change = NULL, origin = NULL");
            await database.Database.ExecuteSqlRawAsync("DELETE FROM activations");
        }

        using (var served = await fixture.GetAsync("/lambda/from-before/"))
        {
            Assert.AreEqual("old", await served.GetContentAsync());
        }

        foreach (var path in (string[])["", "/versions", "/versions/2", "/summary", "/traffic", "/logs", "/deployment", "/deployment/history", "/files"])
        {
            using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}{path}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, path);
        }

        // saved the way a client that has never heard of notes saves
        using (var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                   new { files = new[] { new { name = "lambda.cs", code = "return Content.From(Resource.FromString(\"new\"));" } } }))
        {
            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);
        }

        await fixture.DeployAsync(lambda.PrivateKey);

        using (var rolledBack = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/start", new DeploymentRequest(2)))
        {
            Assert.AreEqual(HttpStatusCode.OK, rolledBack.StatusCode);
        }

        using (var served = await fixture.GetAsync("/lambda/from-before/"))
        {
            Assert.AreEqual("old", await served.GetContentAsync(), "the version from before the notes deploys like any other");
        }

        var history = await HistoryAsync(fixture, lambda.PrivateKey);

        // the deployment that predates the history is simply not in it
        CollectionAssert.AreEqual(new[] { 2, 3 }, history.Select(h => h.Version).ToArray());
    }

    #endregion

    private static async Task<List<ActivationResponse>> HistoryAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/deployment/history");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<List<ActivationResponse>>();
    }

}
