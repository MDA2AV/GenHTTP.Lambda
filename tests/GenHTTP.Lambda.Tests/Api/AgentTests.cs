using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Agents;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
    public async Task ARunnerOfThisReleaseIsHandedTheRunAndFollowedAsItStreams()
    {
        await using var agent = FakeAgent.Start();

        agent.Streams = true;

        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("streamed");

        await fixture.DeployAsync(lambda.PrivateKey);

        using var response = await StartAsync(fixture, lambda.PrivateKey, "Make it blue", language: "de");

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);

        var id = (await response.GetContentAsync<AgentState>()).Job!.Id;

        // what a runner of this release is handed besides what one from before was
        var sent = agent.Received.Single();

        Assert.AreEqual(2, sent["protocol"]!.GetValue<int>());
        Assert.AreEqual(id, sent["run"]!.GetValue<string>());
        Assert.AreEqual(600, sent["timeout"]!.GetValue<int>(), "what the runner gives a run, where the operator set nothing");
        Assert.AreEqual(60, sent["maxTurns"]!.GetValue<int>());
        Assert.IsNotNull(sent["view"]);

        // and what it streams is what the pages read
        agent.Emit(id, new JsonObject { ["type"] = "started" });
        agent.Emit(id, new JsonObject { ["type"] = "said", ["text"] = "Ich lese die App." });
        agent.Emit(id, new JsonObject
        {
            ["type"] = "returned", ["name"] = "farben.md", ["mediaType"] = "text/markdown", ["content"] = Convert.ToBase64String("# Blau"u8.ToArray())
        });
        agent.Emit(id, new JsonObject { ["type"] = "spent", ["inputTokens"] = 1200, ["outputTokens"] = 80 });
        agent.Emit(id, new JsonObject { ["type"] = "ended", ["ending"] = "asked", ["message"] = "Welches Blau?" });

        var ended = (await AgentCalls.EndedAsync(fixture, lambda.PrivateKey)).Job!;

        Assert.IsTrue(ended.Result!.Asked);
        Assert.AreEqual("Welches Blau?", ended.Result.Question);
        Assert.AreEqual("farben.md", ended.Files!.Single().Name);
        CollectionAssert.Contains(ended.Steps.Select(s => s.Text).ToList(), "Ich lese die App.");

        // the offer follows what the runner says it can do
        using var offer = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/agent");

        var said = JsonNode.Parse(await offer.Content.ReadAsStringAsync())!["offer"]!;

        Assert.IsTrue(said["asks"]!.GetValue<bool>());
        Assert.IsTrue(said["returnsFiles"]!.GetValue<bool>());
        Assert.AreEqual(3, said["attachments"]!["maxCount"]!.GetValue<int>());
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

        // filed under a tag of the lambda, which a runner from before needs to
        // run one change of it at a time - and under the platform's own id
        StringAssert.Matches(sent["lambda"]!.GetValue<string>(), new System.Text.RegularExpressions.Regex("^[0-9]{1,18}$"));
        Assert.AreEqual(state.Job.Id, sent["run"]!.GetValue<string>());

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

        // what the agent does, it does through the platform's doors - which is
        // where what the page says came of it is read from
        await AgentCalls.ToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = changed.PrivateKey, ["files"] = AgentCalls.Code("dark"), ["deploy"] = true },
                                   agent.Received.Single()["ticket"]!.GetValue<string>());

        agent.Finish(id, new JsonObject { ["ok"] = true, ["summary"] = "Dark now." });

        var ended = await AgentCalls.EndedAsync(fixture, changed.PrivateKey);

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

        await AgentCalls.EndedAsync(fixture, lambda.PrivateKey);

        using var second = await StartAsync(fixture, lambda.PrivateKey, "Add a leaderboard");

        Assert.AreEqual(HttpStatusCode.TooManyRequests, second.StatusCode);

        // and the build page counts from the same allowance
        using var build = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz"));

        Assert.AreEqual(HttpStatusCode.TooManyRequests, build.StatusCode);
    }

    [TestMethod]
    public async Task ABuildSaysHowManyAreLeftToday()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent, perDay: 3);

        using var build = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz"));

        Assert.AreEqual(HttpStatusCode.Accepted, build.StatusCode);

        var started = await build.GetContentAsync<BuildStarted>();

        Assert.AreEqual(2, started.Left, "the workspace shows what is left before any app is known");
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

        using var off = await SwitchAsync(fixture, new AgentSettingsRequest(Changes: false));

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

        using var off = await SwitchAsync(fixture, new AgentSettingsRequest(Builds: false));

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

        var progress = await AgentCalls.BuiltAsync(fixture, id);

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

        // what the agent does through the platform's doors, the platform
        // records itself, whichever agent it is
        var ticket = agent.Received.Single()["ticket"]!.GetValue<string>();

        var created = await AgentCalls.ToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true, ["publicKey"] = "guest-book" }, ticket);
        var key = created["privateKey"]!.GetValue<string>();

        await AgentCalls.ToolAsync(fixture, "enable_data", new JsonObject { ["privateKey"] = key, ["kind"] = "database" }, ticket);
        await AgentCalls.ToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = key, ["files"] = AgentCalls.Code(), ["deploy"] = true }, ticket);

        var progress = await AgentCalls.UntilAsync(() => AgentCalls.BuildAsync(fixture, id), b => b.Steps.Count(s => s.Done == true) == 3);

        // facts rather than English sentences, so /build says them in the
        // language of whoever asked, and in their words rather than the tools'
        Assert.AreEqual("running", progress.State);
        CollectionAssert.AreEqual(new[] { "create", "data", "write" }, progress.Steps.Select(s => s.Kind).ToArray());
        Assert.AreEqual("database", progress.Steps[1].Data, "which kind of data is what the page names");
        Assert.IsTrue(progress.Steps[2].Online, "and whether it went online");
    }

    [TestMethod]
    public async Task APageTheAgentReadIsAStepThatNamesItsSite()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        using var build = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("Look at myoldhomepage.com and make a new version of it"));

        var id = (await build.GetContentAsync<BuildStarted>()).Id;

        agent.Step(id, new JsonObject { ["at"] = 4, ["kind"] = "web", ["site"] = "myoldhomepage.com", ["done"] = true });

        var step = (await AgentCalls.UntilAsync(() => AgentCalls.BuildAsync(fixture, id), b => b.Steps.Any(s => s.Done == true))).Steps.Single();

        // the site, which is what the person named, so /build can say "Looking
        // at myoldhomepage.com" - never the address, which may hold anything
        Assert.AreEqual("web", step.Kind);
        Assert.AreEqual("myoldhomepage.com", step.Site);
        Assert.IsTrue(step.Done);
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

        var progress = await AgentCalls.UntilAsync(() => AgentCalls.BuildAsync(fixture, id), b => b.Steps.Count > 0);

        // the page counts on from these - counted by the platform from when
        // the agent says it began - and fills the bar against the limit it
        // gave the run, the way the control center does for a change
        Assert.IsTrue(progress.Seconds is >= 0 and <= 60, $"{progress.Seconds} seconds");
        Assert.AreEqual(600, progress.Limit, "what a runner from before holds a run to");
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

        var result = (await AgentCalls.EndedAsync(fixture, lambda.PrivateKey)).Job!.Result!;

        Assert.IsTrue(result.Declined);
        Assert.IsTrue(result.Unchanged, "nothing was touched");
        Assert.AreEqual("declined", result.Reason);
        Assert.AreEqual("Das ist keine Änderung an Ihrer App, deshalb macht der Agent das nicht.", result.Summary,
                        "the owner reads why, in the language they asked in");
    }

    [TestMethod]
    public async Task TheConversationSoFarIsHandedToTheAgentCutToSize()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        // more turns than the agent is handed, one that says nothing, and one far too long
        var history = Enumerable.Range(1, 11)
                                .Select(n => new ConversationTurn($"Question {n}", $"Answer {n}"))
                                .Append(new ConversationTurn("   ", "Nothing was asked"))
                                .Append(new ConversationTurn(new string('x', 5000), new string('y', 5000)))
                                .ToList();

        using var started = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/agent/start",
                                                    new ChangeRequest("And last month?", History: history));

        Assert.AreEqual(HttpStatusCode.Accepted, started.StatusCode);

        var sent = (JsonArray)agent.Received.Single()["history"]!;

        Assert.HasCount(8, sent, "the newest turns, as many as the agent is handed");
        Assert.AreEqual("Question 5", sent[0]!["prompt"]!.GetValue<string>(), "oldest first");
        Assert.AreEqual("Answer 11", sent[6]!["reply"]!.GetValue<string>());
        Assert.AreEqual(600, sent[7]!["prompt"]!.GetValue<string>().Length, "an earlier message is cut");
        Assert.AreEqual(1000, sent[7]!["reply"]!.GetValue<string>().Length, "and so is an answer");
        Assert.IsFalse(sent.Any(turn => turn!["reply"]?.GetValue<string>() == "Nothing was asked"), "a turn that asked nothing is left out");

        Assert.AreEqual("And last month?", agent.Received.Single()["prompt"]!.GetValue<string>(), "the message is the message");
    }

    /// <summary>
    /// The runner takes a body of a bounded size. A message in Japanese with
    /// the conversation before it, every letter escaped as six characters,
    /// came to several times what it was, and the runner dropped it.
    /// </summary>
    [TestMethod]
    public async Task AConversationInAnyLanguageFitsWhatTheRunnerTakes()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        var history = Enumerable.Range(0, 8).Select(_ => new ConversationTurn(new string('あ', 600), new string('い', 1000))).ToList();

        using var started = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/agent/start",
                                                    new ChangeRequest(new string('う', 2000), History: history));

        Assert.AreEqual(HttpStatusCode.Accepted, started.StatusCode);

        var body = agent.Bodies.Single();
        var text = System.Text.Encoding.UTF8.GetString(body);

        Assert.IsLessThan(64 * 1024, body.Length, "what the runner takes, in bytes (LONGEST_BODY in builder.mjs)");
        Assert.DoesNotContain("\\u3042", text, "the words travel as they are, not as escapes");
        Assert.Contains(new string('う', 2000), text);
    }

    [TestMethod]
    public async Task WithoutAConversationNothingButTheMessageIsHandedOn()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        using var started = await StartAsync(fixture, lambda.PrivateKey, "Add a dark mode");

        Assert.IsNull(agent.Received.Single()["history"], "the Change section of the editor sends none");
    }

    [TestMethod]
    public async Task AQuestionIsAnsweredRatherThanMadeAChange()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        using var started = await StartAsync(fixture, lambda.PrivateKey, "How many people came this week?");

        var id = (await started.GetContentAsync<AgentState>()).Job!.Id;

        // the agent switches analytics on through the tool, as it would
        await AgentCalls.ToolAsync(fixture, "analytics", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["enable"] = true },
                                   agent.Received.Single()["ticket"]!.GetValue<string>());

        agent.Finish(id, new JsonObject
        {
            ["ok"] = true,
            ["summary"] = "Analytics were off, so I switched them on: from now on every visit is counted."
        });

        var job = (await AgentCalls.EndedAsync(fixture, lambda.PrivateKey)).Job!;

        Assert.IsTrue(job.Result!.Answered, "the page shows an answer, not a change");
        Assert.AreEqual("analytics", job.Result.Settings!.Single(), "and the settings it changed, read off the tools");
        Assert.AreEqual("analytics", job.Steps.Single().Kind);
    }

    [TestMethod]
    public async Task EveryModelButTheOneBehindThePasswordIsOfferedAtFirst()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var offer = await OfferAsync(fixture);

        CollectionAssert.AreEqual(new[] { "opus", "sonnet", "haiku" }, offer.Models.Select(m => m.Name).ToArray(),
                                  "Fable is behind the password, and there is none");
        Assert.AreEqual("Opus 5.5", offer.Models[0].Label);
        Assert.IsTrue(offer.Models.All(m => !m.Password));
        Assert.AreEqual("opus", offer.Model);
        CollectionAssert.AreEqual(new[] { "low", "medium", "high", "xhigh", "max" }, offer.Efforts.ToArray());
        Assert.AreEqual("high", offer.Effort);

        var lambda = await fixture.CreateLambdaAsync();

        var change = (await StateAsync(fixture, lambda.PrivateKey)).Offer;

        CollectionAssert.AreEqual(offer.Models.ToArray(), change.Models.ToArray(), "the Change section offers the same");
        CollectionAssert.AreEqual(offer.Efforts.ToArray(), change.Efforts.ToArray());
    }

    [TestMethod]
    public async Task ABuildRunsOnTheModelAndTheEffortItAsksFor()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        using var chosen = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz", " Sonnet ", Effort: "LOW"));

        Assert.AreEqual(HttpStatusCode.Accepted, chosen.StatusCode);

        var started = await chosen.GetContentAsync<BuildStarted>();

        Assert.AreEqual("sonnet", started.Model);
        Assert.AreEqual("low", started.Effort);

        using var plain = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A guestbook"));

        Assert.AreEqual(HttpStatusCode.Accepted, plain.StatusCode);

        var sent = agent.Received.ToArray();

        Assert.AreEqual("sonnet", sent[0]["model"]!.GetValue<string>());
        Assert.AreEqual("low", sent[0]["effort"]!.GetValue<string>());
        Assert.IsFalse(sent[0]["unbounded"]!.GetValue<bool>(), "on the clock, like everybody's");

        Assert.AreEqual("opus", sent[1]["model"]!.GetValue<string>(), "where nobody chose, the default model");
        Assert.AreEqual("high", sent[1]["effort"]!.GetValue<string>(), "and the default effort");
    }

    [TestMethod]
    public async Task AChangeRunsOnTheModelAndTheEffortItAsksFor()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/agent/start",
                                                     new ChangeRequest("Add a dark mode", Model: "haiku", Effort: "xhigh"));

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);

        var job = (await response.GetContentAsync<AgentState>()).Job!;

        Assert.AreEqual("haiku", job.Model);
        Assert.AreEqual("xhigh", job.Effort, "the change says what it runs at, as the agent took it");

        var sent = agent.Received.Single();

        Assert.AreEqual("haiku", sent["model"]!.GetValue<string>());
        Assert.AreEqual("xhigh", sent["effort"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TheModelBehindThePasswordIsForWhoeverKnowsIt()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent, password: "open sesame");

        var fable = (await OfferAsync(fixture)).Models.Single(m => m.Name == "fable");

        Assert.AreEqual("Fable 5.2", fable.Label);
        Assert.IsTrue(fable.Password, "the page asks for it");

        using var wrong = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz", "fable", "open says me"));

        Assert.AreEqual(HttpStatusCode.Forbidden, wrong.StatusCode);
        Assert.IsEmpty(agent.Received, "refused before the agent is asked");

        using var right = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz", " Fable ", "open sesame"));

        Assert.AreEqual(HttpStatusCode.Accepted, right.StatusCode);

        var sent = agent.Received.Single();

        Assert.AreEqual("fable", sent["model"]!.GetValue<string>());
        Assert.IsTrue(sent["unbounded"]!.GetValue<bool>(), "with the password, without the clock");

        var lambda = await fixture.CreateLambdaAsync();

        Assert.AreEqual(fixture.Options.AgentBuildsPerDay - 1, (await StateAsync(fixture, lambda.PrivateKey)).Left,
                        "a wrong password costs nothing");
    }

    [TestMethod]
    public async Task WithoutAPasswordTheModelBehindOneIsOfferedToNobody()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        Assert.IsFalse((await OfferAsync(fixture)).Models.Any(m => m.Name == "fable"));

        var lambda = await fixture.CreateLambdaAsync();

        using var guessed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/agent/start",
                                                    new ChangeRequest("Add a dark mode", Model: "fable", Password: ""));

        Assert.AreEqual(HttpStatusCode.BadRequest, guessed.StatusCode, "not offered, rather than open to an empty password");

        using var unknown = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/agent/start",
                                                    new ChangeRequest("Add a dark mode", Model: "gpt"));

        Assert.AreEqual(HttpStatusCode.BadRequest, unknown.StatusCode);

        Assert.IsEmpty(agent.Received);
    }

    [TestMethod]
    public async Task OnlyAModelBehindThePasswordRunsWithoutTheClock()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent, password: "open sesame");

        using var moved = await SwitchAsync(fixture, new AgentSettingsRequest(Models:
        [
            new("opus", "open"), new("sonnet", "open"), new("haiku", "password"), new("fable", "open")
        ]));

        Assert.AreEqual(HttpStatusCode.OK, moved.StatusCode);

        using var fable = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz", "fable"));

        Assert.AreEqual(HttpStatusCode.Accepted, fable.StatusCode, "Fable is everybody's now");

        using var haiku = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz", "haiku", "open sesame"));

        Assert.AreEqual(HttpStatusCode.Accepted, haiku.StatusCode);

        var sent = agent.Received.ToArray();

        Assert.IsFalse(sent[0]["unbounded"]!.GetValue<bool>(), "taken from behind the password, Fable is on the clock like every other");
        Assert.IsTrue(sent[1]["unbounded"]!.GetValue<bool>(), "and the model put behind it runs without it");

        using var plain = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz", "haiku"));

        Assert.AreEqual(HttpStatusCode.Forbidden, plain.StatusCode);
    }

    [TestMethod]
    public async Task WhatTheOperatorDoesNotOfferIsRefusedBeforeAnythingIsSpent()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        using var narrowed = await SwitchAsync(fixture, new AgentSettingsRequest(
            Models: [new("opus", "open"), new("sonnet", "off"), new("haiku", "open"), new("fable", "off")],
            Efforts: ["low", "medium", "high"]));

        Assert.AreEqual(HttpStatusCode.OK, narrowed.StatusCode);

        var offer = await OfferAsync(fixture);

        CollectionAssert.AreEqual(new[] { "opus", "haiku" }, offer.Models.Select(m => m.Name).ToArray());
        CollectionAssert.AreEqual(new[] { "low", "medium", "high" }, offer.Efforts.ToArray());

        using var sonnet = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz", "sonnet"));

        Assert.AreEqual(HttpStatusCode.BadRequest, sonnet.StatusCode);

        using var max = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz", Effort: "max"));

        Assert.AreEqual(HttpStatusCode.BadRequest, max.StatusCode);

        using var turbo = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz", Effort: "turbo"));

        Assert.AreEqual(HttpStatusCode.BadRequest, turbo.StatusCode);

        Assert.IsEmpty(agent.Received, "the agent is not asked");

        var lambda = await fixture.CreateLambdaAsync();

        Assert.AreEqual(fixture.Options.AgentBuildsPerDay, (await StateAsync(fixture, lambda.PrivateKey)).Left, "and nothing is counted");
    }

    [TestMethod]
    public async Task TheDefaultsAreOfferedToEverybody()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent, password: "open sesame");

        using var hidden = await SwitchAsync(fixture, new AgentSettingsRequest(Models:
        [
            new("opus", "password"), new("sonnet", "open"), new("haiku", "open"), new("fable", "password")
        ]));

        Assert.AreEqual(HttpStatusCode.BadRequest, hidden.StatusCode, "the default model is everybody's");

        using var none = await SwitchAsync(fixture, new AgentSettingsRequest(Efforts: []));

        Assert.AreEqual(HttpStatusCode.BadRequest, none.StatusCode, "something has to be offered");

        using var unoffered = await SwitchAsync(fixture, new AgentSettingsRequest(Efforts: ["low", "medium"]));

        Assert.AreEqual(HttpStatusCode.BadRequest, unoffered.StatusCode, "the default effort is offered");

        using var unknown = await SwitchAsync(fixture, new AgentSettingsRequest(Model: "gpt"));

        Assert.AreEqual(HttpStatusCode.BadRequest, unknown.StatusCode);

        Assert.AreEqual("opus", (await OfferAsync(fixture)).Model, "nothing was changed");

        using var moved = await SwitchAsync(fixture, new AgentSettingsRequest(Model: "haiku", Effort: "low",
            Models: [new("opus", "password"), new("sonnet", "open"), new("haiku", "open"), new("fable", "password")]));

        Assert.AreEqual(HttpStatusCode.OK, moved.StatusCode, "with another default, the first may go behind the password");

        var offer = await OfferAsync(fixture);

        Assert.AreEqual("haiku", offer.Model);
        Assert.AreEqual("low", offer.Effort);
        Assert.IsTrue(offer.Models.Single(m => m.Name == "opus").Password);

        using var build = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A pub quiz"));

        Assert.AreEqual(HttpStatusCode.Accepted, build.StatusCode);
        Assert.AreEqual("haiku", agent.Received.Single()["model"]!.GetValue<string>());
        Assert.AreEqual("low", agent.Received.Single()["effort"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task ThePanelSaysWhatTheRunnerSignsInWithAndRunsAs()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent, password: "open sesame");

        agent.SignsInWith = "api-key";

        using var response = await fixture.SendAsOperatorAsync(HttpMethod.Get, "/api/v1/admin/agent");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var settings = await response.GetContentAsync<AgentSettingsResponse>();

        Assert.IsTrue(settings.Builds);
        Assert.IsTrue(settings.Changes);
        Assert.IsTrue(settings.PasswordSet);
        Assert.AreEqual(agent.Url, settings.Url);
        Assert.AreEqual(fixture.Options.AgentBuildsPerDay, settings.PerDay);

        Assert.AreEqual("password", settings.Models.Single(m => m.Name == "fable").Access);
        Assert.IsTrue(settings.Models.Single(m => m.Name == "fable").Offered);
        Assert.IsTrue(settings.Efforts.All(e => e.Offered));

        var runner = settings.Runner!;

        Assert.IsTrue(runner.Reachable);

        string? Fact(string label) => runner.Facts?.FirstOrDefault(f => f.Label == label)?.Value;

        Assert.AreEqual("an API key", Fact("Signs in with"));
        Assert.AreEqual("claude-fable-5-2", Fact("Model fable"));
        Assert.AreEqual("2.1.329", Fact("Claude Code"));
        Assert.AreEqual("named", settings.Agent!.Web, "whether a build may read the sites its request names, as the agent declares it");
        Assert.IsNull(Fact("Reads pages"), "and not said twice");

        var raw = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("open sesame", raw, "whether there is a password, never what it is");
        Assert.DoesNotContain("a secret", raw, "nor the token the server sends the runner");
    }

    [TestMethod]
    public async Task ThePanelSaysHowManyBuildsRunAtOnceAndWhoSharesTheRunner()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        var alone = (await (await fixture.SendAsOperatorAsync(HttpMethod.Get, "/api/v1/admin/agent")).GetContentAsync<AgentSettingsResponse>()).Runner!;

        Assert.IsNull(alone.Concurrency, "a runner from before says nothing of it");
        Assert.IsNull(alone.Facts?.FirstOrDefault(f => f.Label == "Installations"), "a runner of this installation alone says nothing of others");

        agent.Concurrency = 4;
        agent.Installations = 3;

        var shared = (await (await fixture.SendAsOperatorAsync(HttpMethod.Get, "/api/v1/admin/agent")).GetContentAsync<AgentSettingsResponse>()).Runner!;

        Assert.AreEqual(4, shared.Concurrency);
        Assert.AreEqual("3", shared.Facts?.Single(f => f.Label == "Installations").Value);
    }

    [TestMethod]
    public async Task ThePanelSaysWhenTheRunnerDoesNotAnswer()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { AgentUrl = "http://127.0.0.1:9/", AdminToken = AdminToken });

        var settings = await (await fixture.SendAsOperatorAsync(HttpMethod.Get, "/api/v1/admin/agent")).GetContentAsync<AgentSettingsResponse>();

        Assert.IsFalse(settings.Runner!.Reachable);
        Assert.IsFalse(settings.PasswordSet);
        Assert.IsFalse(settings.Models.Single(m => m.Name == "fable").Offered, "behind a password there is not");
    }

    [TestMethod]
    public async Task TheSwitchesKeepWhatWasSavedBeforeTheyMoved()
    {
        await using var agent = FakeAgent.Start();
        await using var fixture = await WithAgentAsync(agent);

        // where the site's settings kept them, before the agent had a section of its own
        await using (var database = await fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>().CreateDbContextAsync())
        {
            database.Settings.Add(new SettingEntity { Key = "build-box", Value = "false" });
            await database.SaveChangesAsync();
        }

        await fixture.Platform.Services.GetRequiredService<AgentSettingsStore>().LoadAsync();

        Assert.IsFalse(await BuildOfferedAsync(fixture), "switched off before, off still");

        using var on = await SwitchAsync(fixture, new AgentSettingsRequest(Builds: true));

        Assert.IsTrue(await BuildOfferedAsync(fixture));

        await using (var database = await fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>().CreateDbContextAsync())
        {
            Assert.AreEqual("true", (await database.Settings.FindAsync("build-box"))!.Value, "and kept in the same row");
        }

        var settings = await on.GetContentAsync<AgentSettingsResponse>();

        Assert.AreEqual("password", settings.Models.Single(m => m.Name == "fable").Access, "the rest as it was");
        Assert.AreEqual("high", settings.Effort);
    }

    #region Helpers

    private const string AdminToken = LambdaFixture.OperatorToken;

    private static Task<LambdaFixture> WithAgentAsync(FakeAgent agent, int perDay = 10, string? password = null)
        => LambdaFixture.CreateAsync(o => o with
        {
            AgentUrl = agent.Url,
            AgentToken = "a secret",
            AgentBuildsPerDay = perDay,
            AgentPassword = password,
            AdminToken = AdminToken
        });

    private static async Task<HttpResponseMessage> SwitchAsync(LambdaFixture fixture, AgentSettingsRequest settings)
    {
        using var request = fixture.Host.GetRequest("/api/v1/admin/agent", HttpMethod.Put);

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

    private static async Task<AgentOffer> OfferAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsync("/api/v1/system");

        var platform = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        return platform["build"]!["offer"].Deserialize<AgentOffer>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
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
