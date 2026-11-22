using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Services.Background;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.UsageReports;
using GenHTTP.Lambda.Tests.Infrastructure;


using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// What the build agent of the installation writes is known to be its work,
/// on the model and effort of its job - by the ticket the server issued for
/// the job, which its calls carry - and nobody else's is.
/// </summary>
[TestClass]
public sealed class AgentWriterTests
{

    private const string Snippet = "return Inline.Create().Get(() => \"written\");";

    #region Tests

    [TestMethod]
    public async Task WhatTheBuildAgentSavesIsMarkedWithTheModelAndEffortOfItsJob()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("marked");

        var ticket = await TicketAsync(fixture, agent, lambda.PrivateKey, "sonnet", "low");

        await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = Files(),
            ["change"] = "says written"
        }, ticket);

        var newest = (await VersionsAsync(fixture, lambda.PrivateKey))[0]!;

        Assert.AreEqual("agent", newest["origin"]!.GetValue<string>(), "it came through MCP, as any agent's does");
        Assert.AreEqual("build-agent", newest["writer"]!.GetValue<string>());
        Assert.AreEqual("sonnet", newest["model"]!.GetValue<string>(), "the model of the job, as the server issued it");
        Assert.AreEqual("low", newest["effort"]!.GetValue<string>());

        Assert.IsFalse(fixture.Book.Read(0, null, LogLevel.Trace, 50_000).Lines.Any(l => l.Text.Contains(ticket, StringComparison.Ordinal)),
                       "no line logs the ticket");

        var template = (await VersionsAsync(fixture, lambda.PrivateKey))[^1]!;

        Assert.IsNull(template["writer"], "what the lambda started as was nobody's work");
        Assert.IsNull(template["model"]);
    }

    [TestMethod]
    public async Task ADraftOfTheBuildAgentPutOnlineByItsOwnerIsTheAgentsWork()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("drafted");

        await fixture.DeployAsync(lambda.PrivateKey);

        var ticket = await TicketAsync(fixture, agent, lambda.PrivateKey, "opus", "max");

        var feature = (await CallToolAsync(fixture, "create_feature", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["name"] = "Darker" }, ticket))["feature"]!["feature"]!.GetValue<string>();

        await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["feature"] = feature,
            ["files"] = Files(),
            ["change"] = "makes it darker"
        }, ticket);

        var draft = await FeatureAsync(fixture, lambda.PrivateKey, feature);

        Assert.AreEqual("build-agent", draft["writer"]!.GetValue<string>(), "the draft says who wrote what it holds");
        Assert.AreEqual("opus", draft["model"]!.GetValue<string>());
        Assert.AreEqual("max", draft["effort"]!.GetValue<string>());

        // the owner puts it online in the editor, through the API
        using (var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature}/merge", new MergeFeatureRequest()))
        {
            Assert.IsTrue(merged.IsSuccessStatusCode, await merged.Content.ReadAsStringAsync());
        }

        var newest = (await VersionsAsync(fixture, lambda.PrivateKey))[0]!;

        Assert.AreEqual("api", newest["origin"]!.GetValue<string>(), "it came through the API");
        Assert.AreEqual("build-agent", newest["writer"]!.GetValue<string>(), "but the agent wrote it");
        Assert.AreEqual("opus", newest["model"]!.GetValue<string>());
        Assert.AreEqual("max", newest["effort"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task AnOwnerSavingIntoTheAgentsDraftMakesItTheirs()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("taken-over");

        var ticket = await TicketAsync(fixture, agent, lambda.PrivateKey, "haiku", "medium");

        var feature = (await CallToolAsync(fixture, "create_feature", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["name"] = "Shared" }, ticket))["feature"]!["feature"]!.GetValue<string>();

        await CallToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["feature"] = feature, ["files"] = Files() }, ticket);

        // the owner changes what the agent wrote, in the editor
        using (var saved = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature}/files", new FeatureFilesRequest(
                   [new LambdaFile("lambda.cs", "return Inline.Create().Get(() => \"mine\");")])))
        {
            Assert.AreEqual(HttpStatusCode.OK, saved.StatusCode, await saved.Content.ReadAsStringAsync());
        }

        Assert.IsNull((await FeatureAsync(fixture, lambda.PrivateKey, feature))["writer"], "the last save was the owner's");

        using (var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature}/merge", new MergeFeatureRequest()))
        {
            Assert.IsTrue(merged.IsSuccessStatusCode, await merged.Content.ReadAsStringAsync());
        }

        var newest = (await VersionsAsync(fixture, lambda.PrivateKey))[0]!;

        Assert.IsNull(newest["writer"], "so the version is the owner's");
        Assert.IsNull(newest["model"]);
    }

    [TestMethod]
    public async Task AnAgentOfSomebodysOwnIsNoWriter()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("own-agent");

        // one that claims to be the build agent, and one that says nothing
        foreach (var ticket in new[] { "0123456789abcdef0123456789abcdef", null })
        {
            await CallToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["files"] = Files() }, ticket);

            var newest = (await VersionsAsync(fixture, lambda.PrivateKey))[0]!;

            Assert.AreEqual("agent", newest["origin"]!.GetValue<string>());
            Assert.IsNull(newest["writer"], "only a ticket the server issued makes the build agent the writer");
            Assert.IsNull(newest["model"]);
            Assert.IsNull(newest["effort"]);
        }
    }

    [TestMethod]
    public Task TheReportAndThePanelCountWhatTheBuildAgentWroteByModelAndEffort() => CountedAsync(null);

    [TestMethod]
    public async Task TheReportAndThePanelCountWhatTheBuildAgentWroteOnPostgreSql()
    {
        await using var database = await TestDatabase.CreateAsync();

        await CountedAsync(database.Integration);
    }

    private static async Task CountedAsync(IIntegrationBuilder<IDatabaseProvider>? database)
    {
        await using var agent = FakeAgent.Start();
        await using var collector = FakeCollector.Start();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => o with
        {
            AgentUrl = agent.Url,
            AgentToken = "a secret",
            AdminToken = LambdaFixture.OperatorToken,
            UsageReportUrl = collector.Url
        }, database);

        var lambda = await fixture.CreateLambdaAsync("counted");

        var ticket = await TicketAsync(fixture, agent, lambda.PrivateKey, "sonnet", "high");

        await CallToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["files"] = Files() }, ticket);
        await CallToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["files"] = Files() }, ticket);

        // and one of an agent of somebody's own
        await CallToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["files"] = Files() }, null);

        await fixture.Platform.Services.GetRequiredService<IUsageReportService>().SendAsync();

        var activity = JsonNode.Parse(collector.Reports.Last().Body)!["activity"]!;

        Assert.AreEqual(3, activity["versions"]!["mcp"]!.GetValue<int>(), "every agent's saves through MCP, the build agent's among them");
        Assert.AreEqual(2, activity["buildAgent"]!.GetValue<int>());
        Assert.AreEqual(2, activity["models"]!["sonnet"]!.GetValue<int>());
        Assert.AreEqual(0, activity["models"]!["opus"]!.GetValue<int>(), "every model is there, at zero where none was used");
        Assert.HasCount(4, activity["models"]!.AsObject());
        Assert.AreEqual(2, activity["efforts"]!["high"]!.GetValue<int>());
        Assert.HasCount(5, activity["efforts"]!.AsObject());

        using var overview = await fixture.GetAsOperatorAsync("/api/v1/admin/overview");

        var census = JsonNode.Parse(await overview.Content.ReadAsStringAsync())!["lambdas"]!;

        Assert.AreEqual(2, census["agentModels"]!["sonnet"]!.GetValue<int>());
        Assert.AreEqual(2, census["agentEfforts"]!["high"]!.GetValue<int>());
    }

    [TestMethod]
    public async Task WhatThePublicSeesOfAVersionSaysNothingOfItsWriter()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("published");

        var ticket = await TicketAsync(fixture, agent, lambda.PrivateKey, "sonnet", "low");

        await CallToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["files"] = Files(), ["change"] = "says written" }, ticket);

        using (var published = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/source", new SourceRequest("MIT", null)))
        {
            Assert.AreEqual(HttpStatusCode.OK, published.StatusCode, await published.Content.ReadAsStringAsync());
        }

        using var source = await fixture.GetAsync("/api/v1/sources/published");

        var body = await source.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, source.StatusCode, body);
        Assert.DoesNotContain("sonnet", body, "the public source names no model");
        Assert.DoesNotContain("build-agent", body);
    }

    [TestMethod]
    public async Task ATicketOutlivesARestartOfTheServer()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        await using var agent = FakeAgent.Start();

        string ticket, privateKey;

        await using (var first = await WithAgentAsync(agent, directory))
        {
            privateKey = (await first.CreateLambdaAsync("restarted")).PrivateKey;

            ticket = await TicketAsync(first, agent, privateKey, "haiku", "high");
        }

        SqliteConnection.ClearAllPools();

        // a server that never issued it in its own memory - another one, or
        // the same one after a restart - knows the ticket from the records
        await using var second = await WithAgentAsync(agent, directory);

        await CallToolAsync(second, "write_code", new JsonObject { ["privateKey"] = privateKey, ["files"] = Files() }, ticket);

        var newest = (await VersionsAsync(second, privateKey))[0]!;

        Assert.AreEqual("build-agent", newest["writer"]!.GetValue<string>());
        Assert.AreEqual("haiku", newest["model"]!.GetValue<string>());
        Assert.AreEqual("high", newest["effort"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task OnlyAHashOfATicketIsKept()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("hashed");

        var ticket = await TicketAsync(fixture, agent, lambda.PrivateKey, "opus", "low");

        await using var database = await Databases(fixture).CreateDbContextAsync();

        var kept = await database.AgentTickets.AsNoTracking().ToListAsync();

        Assert.HasCount(1, kept);
        Assert.AreEqual(Convert.ToHexStringLower(SHA256.HashData(Encoding.ASCII.GetBytes(ticket))), kept[0].Hash,
                        "what the records hold cannot be presented as the ticket");
        Assert.AreEqual(AgentModel.Opus, kept[0].Model);
        Assert.AreEqual(AgentEffort.Low, kept[0].Effort);
    }

    [TestMethod]
    public async Task AnExpiredTicketMarksNothingAndIsRemoved()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("expired");

        var ticket = await TicketAsync(fixture, agent, lambda.PrivateKey, "sonnet", "medium");

        await using (var database = await Databases(fixture).CreateDbContextAsync())
        {
            await database.AgentTickets.ExecuteUpdateAsync(t => t.SetProperty(e => e.Expires, DateTime.UtcNow.AddMinutes(-1)));
        }

        await CallToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["files"] = Files() }, ticket);

        Assert.IsNull((await VersionsAsync(fixture, lambda.PrivateKey))[0]!["writer"], "a ticket a day old is nobody's");

        var job = fixture.Platform.Services.GetServices<IBackgroundJob>().OfType<AgentTicketJob>().Single();

        await job.ExecuteAsync(CancellationToken.None);

        await using (var database = await Databases(fixture).CreateDbContextAsync())
        {
            Assert.IsFalse(await database.AgentTickets.AnyAsync(), "and the job takes it away");
        }
    }

    #endregion

    #region Helpers

    private static Task<LambdaFixture> WithAgentAsync(FakeAgent agent, string? directory = null)
        => LambdaFixture.CreateAsync(o => o with
        {
            AgentUrl = agent.Url,
            AgentToken = "a secret",
            AdminToken = LambdaFixture.OperatorToken,
            DataDirectory = directory ?? o.DataDirectory
        });

    private static IDbContextFactory<LambdaDbContext> Databases(LambdaFixture fixture)
        => fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

    private static JsonArray Files() => new(new JsonObject { ["name"] = "lambda.cs", ["code"] = Snippet });

    /// <summary>
    /// Asks the agent for a change on the given model and effort, and returns
    /// the ticket the runner was handed with it.
    /// </summary>
    private static async Task<string> TicketAsync(LambdaFixture fixture, FakeAgent agent, string privateKey, string model, string effort)
    {
        using var started = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/agent/start",
                                                    new ChangeRequest("Make it so", Model: model, Effort: effort));

        Assert.AreEqual(HttpStatusCode.Accepted, started.StatusCode, await started.Content.ReadAsStringAsync());

        var sent = agent.Received.Last();

        Assert.AreEqual(model, sent["model"]!.GetValue<string>());

        var ticket = sent["ticket"]!.GetValue<string>();

        Assert.AreEqual(32, ticket.Length);

        return ticket;
    }

    private static async Task<JsonArray> VersionsAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/versions");

        return JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsArray();
    }

    private static async Task<JsonNode> FeatureAsync(LambdaFixture fixture, string privateKey, string feature)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/features");

        return JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsArray().Single(f => f!["key"]!.GetValue<string>() == feature)!;
    }

    private static async Task<JsonNode> CallToolAsync(LambdaFixture fixture, string tool, JsonObject arguments, string? ticket)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");

        if (ticket != null)
        {
            request.Headers.Add("X-Lambda-Agent", ticket);
        }

        request.Content = new StringContent(new JsonObject
        {
            ["jsonrpc"] = "2.0", ["id"] = 1, ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments }
        }.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        var result = JsonNode.Parse(await response.Content.ReadAsStringAsync())!["result"]!;

        Assert.IsFalse(result["isError"]?.GetValue<bool>() ?? false, result.ToJsonString());

        return result["structuredContent"]!;
    }

    #endregion

}
