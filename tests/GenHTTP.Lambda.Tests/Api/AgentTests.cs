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

        var id = await fixture.Meta.GetIdAsync(lambda.PrivateKey);

        Assert.AreEqual(id!.Value.ToString(), sent["lambda"]!.GetValue<string>(), "filed under the lambda, to be found by it");

        Assert.IsTrue(agent.Tokens.All(t => t == "a secret"), "every call carries the shared secret");
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

    #region Helpers

    private static Task<LambdaFixture> WithAgentAsync(FakeAgent agent, int perDay = 10)
        => LambdaFixture.CreateAsync(o => o with
        {
            AgentUrl = agent.Url,
            AgentToken = "a secret",
            AgentBuildsPerDay = perDay
        });

    private static async Task<AgentState> StateAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/agent");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AgentState>();
    }

    private static Task<HttpResponseMessage> StartAsync(LambdaFixture fixture, string privateKey, string prompt, bool deploy = true,
                                                        string? language = null)
        => fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/agent/start",
                             new ChangeRequest(prompt, deploy, Language: language));

    #endregion

}
