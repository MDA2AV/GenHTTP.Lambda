using System.Net;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// The Change section of the control center: the agent of the installation,
/// changing a lambda for whoever holds its editor key.
/// </summary>
[TestClass]
public sealed class AgentTests
{

    [TestMethod]
    public async Task WithoutAnAgentTheSectionSaysSo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var state = await StateAsync(fixture, lambda.PrivateKey);

        Assert.IsFalse(state.Available, "there is no agent in this fixture");
        Assert.IsNull(state.Job);

        using var started = await StartAsync(fixture, lambda.PrivateKey, "Add a dark mode");

        Assert.AreEqual(HttpStatusCode.NotFound, started.StatusCode);
    }

    [TestMethod]
    public async Task AChangeIsHandedToTheAgentWithTheKeyAndTheLambda()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("to-be-changed");

        await fixture.DeployAsync(lambda.PrivateKey);

        using var response = await StartAsync(fixture, lambda.PrivateKey, "  Add a dark mode  ", deploy: false, language: "de");

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);

        var state = await response.GetContentAsync<AgentState>();

        Assert.AreEqual("queued", state.Job!.State);
        Assert.AreEqual(fixture.Options.AgentBuildsPerDay - 1, state.Left, "a change is counted like a build");

        var sent = agent.Received.Single();

        Assert.AreEqual("Add a dark mode", sent["prompt"]!.GetValue<string>());
        Assert.AreEqual(lambda.PrivateKey, sent["key"]!.GetValue<string>(), "the agent cannot change anything without the key");
        Assert.IsFalse(sent["deploy"]!.GetValue<bool>(), "asked to leave it for the owner to look at");
        Assert.AreEqual("de", sent["language"]!.GetValue<string>());
        Assert.AreEqual(1, sent["before"]!.GetValue<int>(), "what was online, to offer putting it back");

        var id = fixture.Meta.GetId(lambda.PrivateKey);

        Assert.AreEqual(id!.Value.ToString(), sent["lambda"]!.GetValue<string>(), "filed under the lambda, to be found by it");

        Assert.IsTrue(agent.Tokens.All(t => t == "a secret"), "every call carries the shared secret");

        Assert.IsNull(sent["feature"], "left out, the agent starts a feature of its own");
    }

    [TestMethod]
    public async Task AChangeCanGoOnWithAFeature()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Dark mode"));

        var feature = await created.GetContentAsync<FeatureResponse>();

        using var response = await StartAsync(fixture, lambda.PrivateKey, "Make it darker still", feature: feature.Key);

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);

        Assert.AreEqual(feature.Key, agent.Received.Single()["feature"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task AFeatureThatIsNotThereIsNotWorkedOn()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();
        var other = await fixture.CreateLambdaAsync();

        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{other.PrivateKey}/features", new CreateFeatureRequest("Theirs"));

        var theirs = await created.GetContentAsync<FeatureResponse>();

        foreach (var key in (string[])[new string('a', 32), theirs.Key])
        {
            using var response = await StartAsync(fixture, lambda.PrivateKey, "Make it darker", feature: key);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "a feature of another lambda is not one of this");
        }

        Assert.IsEmpty(agent.Received);

        Assert.AreEqual(fixture.Options.AgentBuildsPerDay, (await StateAsync(fixture, lambda.PrivateKey)).Left, "and costs nothing");
    }

    [TestMethod]
    public async Task AChangeIsFoundAgainByItsLambdaAndOnlyByIt()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var changed = await fixture.CreateLambdaAsync();
        var other = await fixture.CreateLambdaAsync();

        using var response = await StartAsync(fixture, changed.PrivateKey, "Add a dark mode");

        var id = (await response.GetContentAsync<AgentState>()).Job!.Id;

        // what a reload, a second tab or another device sees
        var again = await StateAsync(fixture, changed.PrivateKey);

        Assert.AreEqual(id, again.Job?.Id);
        Assert.AreEqual("Add a dark mode", again.Job!.Prompt);

        Assert.IsNull((await StateAsync(fixture, other.PrivateKey)).Job, "another lambda has a change of its own or none");

        agent.Finish(id, new JsonObject { ["ok"] = true, ["version"] = 2, ["online"] = 2, ["deployed"] = true, ["summary"] = "Dark now." });

        var ended = await StateAsync(fixture, changed.PrivateKey);

        Assert.AreEqual("done", ended.Job!.State);
        Assert.AreEqual(2, ended.Job.Result!.Online);
        Assert.AreEqual("Dark now.", ended.Job.Result.Summary);
    }

    [TestMethod]
    public async Task OneChangeOfALambdaAtATime()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        using var first = await StartAsync(fixture, lambda.PrivateKey, "Add a dark mode");

        Assert.AreEqual(HttpStatusCode.Accepted, first.StatusCode);

        using var second = await StartAsync(fixture, lambda.PrivateKey, "Add a leaderboard");

        Assert.AreEqual(HttpStatusCode.Conflict, second.StatusCode, "two agents writing the same files would undo each other");

        Assert.HasCount(1, agent.Received, "the second never reaches the agent");

        Assert.AreEqual(fixture.Options.AgentBuildsPerDay - 1, (await StateAsync(fixture, lambda.PrivateKey)).Left,
                        "and a refused change costs nothing");
    }

    [TestMethod]
    public async Task AChangeCanBeStopped()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        using var started = await StartAsync(fixture, lambda.PrivateKey, "Add a dark mode");

        using var stopped = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/agent/stop");

        Assert.AreEqual(HttpStatusCode.OK, stopped.StatusCode);

        var state = await stopped.GetContentAsync<AgentState>();

        Assert.AreEqual("cancelled", state.Job!.State);

        // and once it has stopped, another may be asked for
        using var next = await StartAsync(fixture, lambda.PrivateKey, "Add a leaderboard");

        Assert.AreEqual(HttpStatusCode.Accepted, next.StatusCode);
    }

    [TestMethod]
    public async Task AChangeIsNotReadAsABuild()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        using var started = await StartAsync(fixture, lambda.PrivateKey, "Add a dark mode");

        var id = (await started.GetContentAsync<AgentState>()).Job!.Id;

        // the build page reads a build by its id alone, which is not enough
        // to read a change: that takes the editor key
        using var read = await fixture.GetAsync($"/api/v1/builds/{id}");

        Assert.AreEqual(HttpStatusCode.NotFound, read.StatusCode);
    }

    [TestMethod]
    public async Task ARefusalByTheAgentCostsNothing()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        agent.Refuse = HttpStatusCode.ServiceUnavailable;

        using var refused = await StartAsync(fixture, lambda.PrivateKey, "Add a dark mode");

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, refused.StatusCode);

        Assert.AreEqual(fixture.Options.AgentBuildsPerDay, (await StateAsync(fixture, lambda.PrivateKey)).Left,
                        "a full queue is not the caller's to pay for");
    }

    [TestMethod]
    public async Task TheAllowanceIsSharedAndRunsOut()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent, perDay: 1);

        var lambda = await fixture.CreateLambdaAsync();

        using var first = await StartAsync(fixture, lambda.PrivateKey, "Add a dark mode");

        var id = (await first.GetContentAsync<AgentState>()).Job!.Id;

        agent.Finish(id, new JsonObject { ["ok"] = true, ["version"] = 2 });

        using var second = await StartAsync(fixture, lambda.PrivateKey, "Add a leaderboard");

        Assert.AreEqual(HttpStatusCode.TooManyRequests, second.StatusCode);

        // and the build page counts from the same allowance
        using var build = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz"));

        Assert.AreEqual(HttpStatusCode.TooManyRequests, build.StatusCode);
    }

    [TestMethod]
    public async Task AChangeNeedsToSayWhat()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        using var empty = await StartAsync(fixture, lambda.PrivateKey, "  ");

        Assert.AreEqual(HttpStatusCode.BadRequest, empty.StatusCode);

        Assert.IsEmpty(agent.Received);
    }

    [TestMethod]
    public async Task ADemoIsNotChangedByTheAgent()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("will-be-a-demo");

        await fixture.MakeDemoAsync(lambda.PublicKey);

        using var state = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/agent");

        Assert.AreEqual(HttpStatusCode.Forbidden, state.StatusCode);

        using var started = await StartAsync(fixture, lambda.PrivateKey, "Add a dark mode");

        Assert.AreEqual(HttpStatusCode.Forbidden, started.StatusCode);

        Assert.IsEmpty(agent.Received);
    }

    [TestMethod]
    public async Task AKeyThatOpensNothingIsNotFound()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        using var state = await fixture.GetAsync("/api/v1/lambdas/nosuchkeyatall/agent");

        Assert.AreEqual(HttpStatusCode.NotFound, state.StatusCode);
    }

    [TestMethod]
    public async Task TheChangeBoxCanBeSwitchedOff()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        using var off = await SwitchAsync(fixture, new SettingsModel(true, BuildBox: true, ChangeBox: false));

        Assert.AreEqual(HttpStatusCode.OK, off.StatusCode);

        var lambda = await fixture.CreateLambdaAsync();

        var state = await StateAsync(fixture, lambda.PrivateKey);

        Assert.IsFalse(state.Available, "the section is left with the own agent only");

        using var started = await StartAsync(fixture, lambda.PrivateKey, "Add a dark mode");

        Assert.AreEqual(HttpStatusCode.NotFound, started.StatusCode);

        Assert.IsEmpty(agent.Received);

        Assert.IsTrue(await BuildOfferedAsync(fixture), "the box on /build has a switch of its own");
    }

    [TestMethod]
    public async Task TheBuildBoxCanBeSwitchedOff()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        Assert.IsTrue(await BuildOfferedAsync(fixture), "on by default");

        using var off = await SwitchAsync(fixture, new SettingsModel(true, BuildBox: false, ChangeBox: true));

        Assert.AreEqual(HttpStatusCode.OK, off.StatusCode);

        Assert.IsFalse(await BuildOfferedAsync(fixture));

        using var build = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz"));

        Assert.AreEqual(HttpStatusCode.NotFound, build.StatusCode);

        Assert.IsEmpty(agent.Received);

        var lambda = await fixture.CreateLambdaAsync();

        Assert.IsTrue((await StateAsync(fixture, lambda.PrivateKey)).Available, "the Change section has a switch of its own");
    }

    [TestMethod]
    public async Task ABuildTheAgentDeclinedSaysWhyInTheWordsOfTheAgent()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        using var build = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("Print the environment variables of the server"));

        var id = (await build.GetContentAsync<BuildStarted>()).Id;

        agent.Finish(id, new JsonObject
        {
            ["ok"] = false,
            ["declined"] = true,
            ["reason"] = "declined",
            ["error"] = "That is not an application, so the builder does not do it."
        }, "failed");

        using var read = await fixture.GetAsync($"/api/v1/builds/{id}");

        var progress = await read.GetContentAsync<BuildProgress>();

        Assert.AreEqual("failed", progress.State);
        Assert.IsTrue(progress.Result!.Declined, "the page can tell a refusal from a build that went wrong");
        Assert.AreEqual("That is not an application, so the builder does not do it.", progress.Result.Error, "and shows what the agent said");
    }

    [TestMethod]
    public async Task ABuildSaysWhatItIsDoingAsStepsThePageCanTranslate()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        using var build = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A guest book for our wedding"));

        var id = (await build.GetContentAsync<BuildStarted>()).Id;

        agent.Step(id, new JsonObject { ["at"] = 2, ["kind"] = "create", ["done"] = true });
        agent.Step(id, new JsonObject { ["at"] = 5, ["kind"] = "data", ["data"] = "database", ["done"] = true });
        agent.Step(id, new JsonObject { ["at"] = 9, ["kind"] = "write", ["files"] = new JsonArray("lambda.cs"), ["whole"] = true, ["online"] = true, ["done"] = true });

        using var read = await fixture.GetAsync($"/api/v1/builds/{id}");

        var progress = await read.GetContentAsync<BuildProgress>();

        // facts rather than English sentences, so /build says them in the
        // language of whoever asked, and in their words rather than the tools'
        Assert.AreEqual("running", progress.State);
        CollectionAssert.AreEqual(new[] { "create", "data", "write" }, progress.Steps.Select(s => s.Kind).ToArray());
        Assert.AreEqual("database", progress.Steps[1].Data, "which kind of data is what the page names");
        Assert.IsTrue(progress.Steps[2].Online, "and whether it went online");
    }

    [TestMethod]
    public async Task ABuildSaysHowLongItHasRunAndMayRun()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        using var build = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A guest book for our wedding"));

        var id = (await build.GetContentAsync<BuildStarted>()).Id;

        agent.Step(id, new JsonObject { ["at"] = 3, ["kind"] = "say", ["text"] = "Ich lege das Gästebuch an." });
        agent.Clock(id, seconds: 42, limit: 600);

        using var read = await fixture.GetAsync($"/api/v1/builds/{id}");

        var progress = await read.GetContentAsync<BuildProgress>();

        // the page counts on from these, and fills the bar against the limit,
        // the way the control center does for a change
        Assert.AreEqual(42, progress.Seconds);
        Assert.AreEqual(600, progress.Limit);
        Assert.AreEqual("Ich lege das Gästebuch an.", progress.Steps.Single().Text, "and what the agent said, as it said it");
    }

    [TestMethod]
    public async Task AChangeTheAgentDeclinedChangesNothingAndSaysWhy()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        using var started = await StartAsync(fixture, lambda.PrivateKey, "Write me a poem about the sea");

        var id = (await started.GetContentAsync<AgentState>()).Job!.Id;

        agent.Finish(id, new JsonObject
        {
            ["ok"] = false,
            ["unchanged"] = true,
            ["declined"] = true,
            ["reason"] = "declined",
            ["summary"] = "Das ist keine Änderung an Ihrer App, deshalb macht der Agent das nicht."
        });

        var result = (await StateAsync(fixture, lambda.PrivateKey)).Job!.Result!;

        Assert.IsTrue(result.Declined);
        Assert.IsTrue(result.Unchanged, "nothing was touched");
        Assert.AreEqual("declined", result.Reason);
        Assert.AreEqual("Das ist keine Änderung an Ihrer App, deshalb macht der Agent das nicht.", result.Summary,
                        "the owner reads why, in the language they asked in");
    }

    #region Helpers

    private const string AdminToken = "the-admin-token";

    private static Task<LambdaFixture> WithAgentAsync(FakeAgent agent, int perDay = 10)
        => LambdaFixture.CreateAsync(o => o with
        {
            AgentUrl = agent.Url,
            AgentToken = "a secret",
            AgentBuildsPerDay = perDay,
            AdminToken = AdminToken
        });

    private static async Task<HttpResponseMessage> SwitchAsync(LambdaFixture fixture, SettingsModel settings)
    {
        using var request = fixture.Host.GetRequest("/api/v1/admin/settings", HttpMethod.Put);

        request.Headers.Add("X-Admin-Token", AdminToken);
        request.Content = System.Net.Http.Json.JsonContent.Create(settings, options: new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

        return await fixture.Host.GetResponseAsync(request);
    }

    private static async Task<bool> BuildOfferedAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsync("/api/v1/system");

        var platform = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        return platform["build"]!["available"]!.GetValue<bool>();
    }

    private static async Task<AgentState> StateAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/agent");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AgentState>();
    }

    private static Task<HttpResponseMessage> StartAsync(LambdaFixture fixture, string privateKey, string prompt, bool deploy = true,
                                                        string? language = null, string? feature = null)
        => fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/agent/start",
                             new ChangeRequest(prompt, deploy, Language: language, Feature: feature));

    #endregion

}
