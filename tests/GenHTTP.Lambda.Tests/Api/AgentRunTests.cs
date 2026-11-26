using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Services.Agents;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// The agent as an integration: whatever runs behind it, the platform keeps
/// its runs, offers its options, takes and hands back files, and says what a
/// run did from what its calls did at the platform's doors.
/// </summary>
[TestClass]
public sealed class AgentRunTests
{

    #region Options

    [TestMethod]
    public async Task TheOptionsOfAnyAgentAreOfferedAndWhatItWritesIsMarkedWithThem()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var offer = await OfferAsync(fixture);

        CollectionAssert.AreEqual(new[] { "engine", "tone" }, offer.Options.Select(o => o.Name).ToArray(), "the integration's options, named as it names them");
        Assert.AreEqual("swift", offer.Options[0].Default);
        Assert.IsFalse(offer.Options[0].Values.Any(v => v.Name == "thorough"), "behind a password there is not");
        Assert.IsEmpty(offer.Models, "this agent has no model to choose");

        var lambda = await fixture.CreateLambdaAsync("options");

        var state = await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Make it warmer", Options: new Dictionary<string, string> { ["tone"] = "warm" }));

        CollectionAssert.AreEquivalent(new Dictionary<string, string> { ["engine"] = "swift", ["tone"] = "warm" }, agent.Last.Handed.Options.ToDictionary(),
                                       "every option handed on, the defaults where nobody chose");

        await AgentCalls.ToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["files"] = AgentCalls.Code() }, agent.Last.Credential);

        using var versions = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions");

        var newest = JsonNode.Parse(await versions.Content.ReadAsStringAsync())![0]!;

        Assert.AreEqual("build-agent", newest["writer"]!.GetValue<string>());
        Assert.AreEqual("warm", newest["options"]!["tone"]!.GetValue<string>(), "what the run wrote says what it ran on");
        Assert.IsNull(newest["model"], "and nothing of an option it does not have");

        Assert.AreEqual("warm", state.Job!.Options!["tone"]);
    }

    [TestMethod]
    public async Task AValueBehindThePasswordIsForWhoeverKnowsItAndRunsWithoutLimits()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent, password: "open sesame");

        Assert.IsTrue((await OfferAsync(fixture)).Options[0].Values.Single(v => v.Name == "thorough").Password);

        using var wrong = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds",
                                                  new BuildRequest("A pub quiz", Options: new Dictionary<string, string> { ["engine"] = "thorough" }, Password: "nope"));

        Assert.AreEqual(HttpStatusCode.Forbidden, wrong.StatusCode);
        Assert.IsEmpty(agent.Received, "refused before the agent is asked");

        await BuildAsync(fixture, new BuildRequest("A pub quiz", Options: new Dictionary<string, string> { ["engine"] = "thorough" }, Password: "open sesame"));

        Assert.IsTrue(agent.Last.Handed.Limits.Unbounded, "with the password, the limits for such runs - none, as nobody set any");

        await BuildAsync(fixture, new BuildRequest("A pub quiz"));

        Assert.AreEqual(TimeSpan.FromMinutes(5), agent.Last.Handed.Limits.Time, "without, what the agent holds a run to by default");
        Assert.AreEqual(40, agent.Last.Handed.Limits.Steps);
    }

    [TestMethod]
    public async Task TheOperatorDecidesTheOptionsAndTheLimitsOfAnyAgent()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent, password: "open sesame");

        using var saved = await fixture.SendAsOperatorAsync(HttpMethod.Put, "/api/v1/admin/agent", new AgentSettingsRequest(
            Options: [new AgentOptionRequest("engine", [new AgentModelRequest("thorough", "open")], "thorough")],
            Limits: new AgentLimitsModel(120, 0)));

        Assert.AreEqual(HttpStatusCode.OK, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        var settings = await saved.GetContentAsync<AgentSettingsResponse>();

        Assert.AreEqual("thorough", settings.Options!.Single(o => o.Name == "engine").Default);
        Assert.AreEqual("Test agent", settings.Agent!.Agent, "what the agent is, for the privacy statement");
        Assert.AreEqual("localhost", settings.Agent.Destinations.Single().Address, "and where it sends what");
        Assert.IsTrue(settings.LimitSupport!.Time);

        await BuildAsync(fixture, new BuildRequest("A pub quiz"));

        Assert.AreEqual("thorough", agent.Last.Handed.Options["engine"], "taken from behind the password and made the default");
        Assert.AreEqual(TimeSpan.FromMinutes(2), agent.Last.Handed.Limits.Time);
        Assert.IsNull(agent.Last.Handed.Limits.Steps, "zero is no limit");

        using var refused = await fixture.SendAsOperatorAsync(HttpMethod.Put, "/api/v1/admin/agent", new AgentSettingsRequest(
            Options: [new AgentOptionRequest("engine", [new AgentModelRequest("thorough", "off")])]));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "the default has to be everybody's");

        using var unknown = await fixture.SendAsOperatorAsync(HttpMethod.Put, "/api/v1/admin/agent", new AgentSettingsRequest(
            Options: [new AgentOptionRequest("brain", [new AgentModelRequest("big", "open")])]));

        Assert.AreEqual(HttpStatusCode.BadRequest, unknown.StatusCode, "an option the agent does not have");
    }

    #endregion

    #region What a run did

    [TestMethod]
    public async Task WhatARunDidIsReadFromItsCallsRatherThanFromWhatTheAgentSaid()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("claims");
        await fixture.DeployAsync(lambda.PrivateKey);

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Add a dark mode", Deploy: false));

        var run = agent.Last;

        var feature = await AgentCalls.ToolAsync(fixture, "create_feature", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["name"] = "Dark mode" }, run.Credential);
        var key = feature["feature"]!["feature"]!.GetValue<string>();

        await AgentCalls.ToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey, ["feature"] = key, ["files"] = AgentCalls.Code("dark"), ["deploy"] = true
        }, run.Credential);

        // what the agent says it did is its own account, and nothing more
        run.End(AgentEnding.Completed, "I put the dark mode online for everybody.");

        var result = (await AgentCalls.EndedAsync(fixture, lambda.PrivateKey)).Job!.Result!;

        Assert.IsTrue(result.Ok);
        Assert.AreEqual(key, result.Feature, "the draft the calls wrote into");
        Assert.AreEqual("Dark mode", result.FeatureName);
        Assert.IsTrue(result.Preview, "online at its preview");
        Assert.IsFalse(result.Deployed, "and nothing put online, whatever the agent said");
        Assert.AreEqual("I put the dark mode online for everybody.", result.Summary, "its words are shown as its words");
        Assert.IsTrue(result.Undo!.Undoes.Contains("draft"), "a draft it started can be thrown away again");
    }

    [TestMethod]
    public async Task AFailedRunShowsTheDraftItLeftAndClaimsNothingElse()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("half");
        await fixture.DeployAsync(lambda.PrivateKey);

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Add a shop"));

        var run = agent.Last;

        var feature = await AgentCalls.ToolAsync(fixture, "create_feature", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["name"] = "Shop" }, run.Credential);
        var key = feature["feature"]!["feature"]!.GetValue<string>();

        await AgentCalls.ToolAsync(fixture, "change_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["feature"] = key, ["files"] = AgentCalls.Code("shop") },
                                   run.Credential);

        run.End(AgentEnding.TimedOut, detail: "Out of time");

        var job = (await AgentCalls.EndedAsync(fixture, lambda.PrivateKey)).Job!;

        Assert.AreEqual("done", job.State, "it saved something, so what it did is shown as done - cut short");
        Assert.AreEqual("timeout", job.Result!.Reason);
        Assert.AreEqual(key, job.Result.Feature, "the half-finished draft stays a draft");
        Assert.IsFalse(job.Result.Deployed);

        using var features = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features");

        Assert.Contains(key, await features.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task ABuildHandsOverTheKeysOfTheLambdaItsCallsCreated()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var id = await BuildAsync(fixture, new BuildRequest("A guest book for our wedding"));

        var run = agent.Last;

        Assert.AreEqual(AgentTask.Build, run.Handed.Task);
        Assert.IsNull(run.Handed.Lambda, "nothing to work on yet");

        var created = await AgentCalls.ToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true, ["publicKey"] = "wedding" }, run.Credential);

        await AgentCalls.ToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = created["privateKey"]!.GetValue<string>(), ["files"] = AgentCalls.Code("welcome"), ["deploy"] = true
        }, run.Credential);

        run.End(AgentEnding.Completed, "A guest book is online.");

        var built = await AgentCalls.BuiltAsync(fixture, id);

        Assert.AreEqual("done", built.State);
        Assert.IsTrue(built.Result!.Ok);
        Assert.IsTrue(built.Result.Deployed);
        Assert.AreEqual("wedding", built.Result.PublicKey);
        Assert.AreEqual(created["privateKey"]!.GetValue<string>(), built.Result.PrivateKey, "from the platform's records of what the run created");
        Assert.EndsWith($"/editor/{created["privateKey"]!.GetValue<string>()}", built.Result.EditorUrl!);
    }

    [TestMethod]
    public async Task ABuildThatWroteNothingIsNoSuccessWhateverItSaid()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var id = await BuildAsync(fixture, new BuildRequest("A game"));

        var run = agent.Last;

        // the starter is online, which is not what anybody asked for
        await AgentCalls.ToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true }, run.Credential);

        run.End(AgentEnding.Completed, "Your game is ready!");

        var built = await AgentCalls.BuiltAsync(fixture, id);

        Assert.AreEqual("failed", built.State);
        Assert.IsFalse(built.Result!.Ok);
        Assert.IsNotNull(built.Result.PrivateKey, "the lambda it made is still theirs");
    }

    #endregion

    #region Asking, waiting, carrying on

    [TestMethod]
    public async Task ARunThatAsksIsAnsweredByTheNextMessage()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("asking");

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Make it look nicer"));

        var first = agent.Last;

        first.End(AgentEnding.Asked, "Which colours do you like?");

        var asked = (await AgentCalls.EndedAsync(fixture, lambda.PrivateKey)).Job!;

        Assert.AreEqual("done", asked.State);
        Assert.IsTrue(asked.Result!.Asked);
        Assert.AreEqual("Which colours do you like?", asked.Result.Question);

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Blue and white", Continues: asked.Id));

        var second = agent.Last.Handed;

        Assert.AreEqual(AgentContinuationKind.Answer, second.Continues!.Kind);
        Assert.AreEqual("Make it look nicer", second.Continues.Asked);
        Assert.AreEqual("Which colours do you like?", second.Continues.Question);
        Assert.AreEqual("Blue and white", second.Message);
        Assert.AreEqual("Which colours do you like?", second.Conversation.Last().Reply, "the conversation is the platform's, from its records");
    }

    [TestMethod]
    public async Task ABuildThatAskedBeforeItMadeAnAppPassesItsFilesToTheBuildThatAnswers()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var logo = await AttachAsync(fixture, "logo.png", "image/png", Png());

        var asked = await BuildAsync(fixture, new BuildRequest("A page for our club, with this logo", Attachments: [logo.Id]));

        agent.Last.End(AgentEnding.Asked, "Which events should it list?");

        var question = await AgentCalls.BuiltAsync(fixture, asked);

        Assert.IsTrue(question.Result!.Asked);
        Assert.IsNull(question.Result.PrivateKey, "it asked before it made anything");

        await BuildAsync(fixture, new BuildRequest("The summer party and the tournament", Continues: asked));

        var answer = agent.Last.Handed;

        // the app does not exist yet, so the logo is nowhere but with the build that asked
        var earlier = answer.Attachments.Single();

        Assert.AreEqual("logo.png", earlier.Name);
        Assert.IsTrue(earlier.Earlier);
        Assert.AreEqual(AgentContinuationKind.Answer, answer.Continues!.Kind);
    }

    [TestMethod]
    public async Task WhatTheAppWaitsForIsSaidAndCarriedOnFromWithoutTypingAgain()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("waiting");

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Show the weather of Vienna"));

        var run = agent.Last;

        await AgentCalls.ToolAsync(fixture, "request_connectivity", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey, ["purpose"] = "api.weather.example, for the forecast"
        }, run.Credential);

        run.End(AgentEnding.Completed, "It waits for permission to use the weather service.");

        var ended = (await AgentCalls.EndedAsync(fixture, lambda.PrivateKey)).Job!;

        var waits = ended.Result!.WaitsFor!.Single();

        Assert.AreEqual("operator", waits.Who, "read from the lambda, not from what the agent said");
        Assert.AreEqual("connectivity", waits.What);
        Assert.Contains("connectivity", ended.Result.Settings!);

        // once it is decided, carrying on needs nothing typed again
        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest(null, Continues: ended.Id));

        Assert.AreEqual("Show the weather of Vienna", agent.Last.Handed.Message);
        Assert.AreEqual(AgentContinuationKind.Unblocked, agent.Last.Handed.Continues!.Kind);
    }

    [TestMethod]
    public async Task WaitingIsToldApartFromWorkingAndWhy()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("queue");

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Add a page"));

        var run = agent.Last;

        run.Queue(3);

        var queued = await AgentCalls.UntilAsync(() => AgentCalls.StateAsync(fixture, lambda.PrivateKey), s => s.Job?.Wait?.Position == 3);

        Assert.AreEqual("queued", queued.Job!.State);
        Assert.AreEqual("queue", queued.Job.Wait!.Reason);

        run.Start();
        run.Wait(AgentWaitReason.RateLimited, DateTimeOffset.UtcNow.AddMinutes(2));

        var held = await AgentCalls.UntilAsync(() => AgentCalls.StateAsync(fixture, lambda.PrivateKey), s => s.Job?.Wait?.Reason == "rate-limited");

        Assert.AreEqual("running", held.Job!.State, "begun, and held up");
        Assert.IsNotNull(held.Job.Wait!.Until);

        run.Start();

        await AgentCalls.UntilAsync(() => AgentCalls.StateAsync(fixture, lambda.PrivateKey), s => s.Job is { State: "running", Wait: null });
    }

    #endregion

    #region Undoing

    [TestMethod]
    public async Task UndoPutsBackWhatWasOnlineDiscardsItsDraftAndSaysWhatItLeft()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("undone");
        await fixture.DeployAsync(lambda.PrivateKey);

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Change everything"));

        var run = agent.Last;

        await AgentCalls.ToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["files"] = AgentCalls.Code("new"), ["deploy"] = true },
                                   run.Credential);
        await AgentCalls.ToolAsync(fixture, "create_feature", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["name"] = "Later" }, run.Credential);
        await AgentCalls.ToolAsync(fixture, "enable_data", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["kind"] = "database" }, run.Credential);

        run.End(AgentEnding.Completed, "Done.");

        var offer = (await AgentCalls.EndedAsync(fixture, lambda.PrivateKey)).Job!.Result!.Undo!;

        Assert.IsTrue(offer.Possible);
        CollectionAssert.IsSubsetOf(new[] { "online", "draft" }, offer.Undoes.ToArray());
        CollectionAssert.IsSubsetOf(new[] { "versions", "data" }, offer.Keeps.ToArray(), "what would delete data stays, and says so");

        using var undone = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/agent/undo", new UndoRequest());

        Assert.AreEqual(HttpStatusCode.OK, undone.StatusCode, await undone.Content.ReadAsStringAsync());

        var outcome = await undone.GetContentAsync<UndoOutcome>();

        CollectionAssert.IsSubsetOf(new[] { "online", "draft" }, outcome.Undone.ToArray());
        Assert.AreEqual(1, outcome.Online, "what was online before is again");

        using var features = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features");

        Assert.AreEqual("[]", await features.Content.ReadAsStringAsync(), "and its draft is gone");

        var after = (await AgentCalls.StateAsync(fixture, lambda.PrivateKey)).Job!.Result!.Undo!;

        Assert.IsFalse(after.Possible, "once undone, there is nothing more to undo");
        Assert.Contains("undone", after.Keeps);
    }

    [TestMethod]
    public async Task UndoLeavesWhatSomebodyElseChangedSince()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("since");
        await fixture.DeployAsync(lambda.PrivateKey);

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("New front page"));

        await AgentCalls.ToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["files"] = AgentCalls.Code("agent"), ["deploy"] = true },
                                   agent.Last.Credential);

        agent.Last.End(AgentEnding.Completed);

        await AgentCalls.EndedAsync(fixture, lambda.PrivateKey);

        // the owner puts another version online meanwhile
        await fixture.DeployAsync(lambda.PrivateKey, "return Content.From(Resource.FromString(\"owner\"));");

        using var undone = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/agent/undo", new UndoRequest());

        var outcome = await undone.GetContentAsync<UndoOutcome>();

        Assert.Contains("changed:online", outcome.Kept);
        Assert.AreEqual(3, outcome.Online, "the owner's version stays online");
    }

    #endregion

    #region Files

    [TestMethod]
    public async Task AttachmentsAreKeptOfferedLaterAndPutIntoTheAppByReference()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var offer = await OfferAsync(fixture);

        Assert.AreEqual(3, offer.Attachments!.MaxCount, "the page says what it takes before anything is sent");

        var lambda = await fixture.CreateLambdaAsync("logo");

        var picture = Png();

        var logo = await AttachAsync(fixture, "logo.png", "image/png", picture);

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Use this logo", Attachments: [logo.Id]));

        var run = agent.Last;

        var handed = run.Handed.Attachments.Single();

        Assert.AreEqual("logo.png", handed.Name);
        Assert.IsFalse(handed.Earlier);
        Assert.AreEqual($"attachments/{logo.Id}", handed.Path);

        // the agent sees the picture through the platform, as a client
        using (var read = await RawToolAsync(fixture, "read_attachment", new JsonObject { ["attachment"] = logo.Id }, run.Credential))
        {
            var content = JsonNode.Parse(await read.Content.ReadAsStringAsync())!["result"]!["content"]!.AsArray();

            Assert.AreEqual("image", content[0]!["type"]!.GetValue<string>(), "a picture the model sees");
        }

        // and puts it into the app by its id, without carrying its bytes
        await AgentCalls.ToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "resources/web/logo.png", ["attachment"] = logo.Id })
        }, run.Credential);

        using (var version = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2"))
        {
            var saved = JsonNode.Parse(await version.Content.ReadAsStringAsync())!["files"]!.AsArray().Single(f => f!["name"]!.GetValue<string>() == "resources/web/logo.png")!;

            Assert.AreEqual("base64", saved["encoding"]!.GetValue<string>());
            CollectionAssert.AreEqual(picture, Convert.FromBase64String(saved["code"]!.GetValue<string>()), "byte for byte");
        }

        run.End(AgentEnding.Completed);

        await AgentCalls.EndedAsync(fixture, lambda.PrivateKey);

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Make the logo from before bigger"));

        var earlier = agent.Last.Handed.Attachments.Single();

        Assert.AreEqual(logo.Id, earlier.Id, "a later message can refer to it");
        Assert.IsTrue(earlier.Earlier);
        Assert.AreEqual("logo.png", agent.Last.Handed.Conversation.Last().Attachments!.Single());
    }

    [TestMethod]
    public async Task AnAttachmentIsServedToBeShownOrSavedNeverRun()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var page = await AttachAsync(fixture, "evil.html", "text/html", Encoding.UTF8.GetBytes("<script>alert(1)</script>"));

        using var served = await fixture.GetAsync($"/api/v1/attachments/{page.Id}");

        Assert.AreEqual("application/octet-stream", served.Content.Headers.ContentType!.MediaType, "never a page of this site");
        Assert.AreEqual("attachment", served.Content.Headers.ContentDisposition?.DispositionType ?? served.Headers.GetValues("Content-Disposition").First().Split(';')[0]);
        Assert.Contains("sandbox", served.Headers.GetValues("Content-Security-Policy").Single());
        Assert.AreEqual("nosniff", served.Headers.GetValues("X-Content-Type-Options").Single());

        var picture = await AttachAsync(fixture, "photo.png", "image/png", Png());

        using var shown = await fixture.GetAsync($"/api/v1/attachments/{picture.Id}");

        Assert.AreEqual("image/png", shown.Content.Headers.ContentType!.MediaType, "a picture is shown");
    }

    [TestMethod]
    public async Task WhatTheAgentDoesNotTakeIsNeitherOfferedNorTaken()
    {
        var agent = new FakeAgentProvider { Capabilities = FakeAgentProvider.Everything with { Attachments = null, ReturnsFiles = false, Asks = false } };
        await using var fixture = await WithAgentAsync(agent);

        var offer = await OfferAsync(fixture);

        Assert.IsNull(offer.Attachments, "no button for files");
        Assert.IsFalse(offer.Asks);

        using var upload = await RawAttachAsync(fixture, "logo.png", "image/png", Png());

        Assert.AreEqual(HttpStatusCode.NotFound, upload.StatusCode);

        var typed = await AttachmentRefusalAsync(new FakeAgentProvider(), "notes.exe", "application/x-msdownload");

        Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, typed);
    }

    [TestMethod]
    public async Task FilesTheAgentHandsBackAreKeptWithTheRun()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var id = await BuildAsync(fixture, new BuildRequest("A recipe card"));

        agent.Last.Start().Return("card.md", "text/markdown", Encoding.UTF8.GetBytes("# Pancakes"), "The recipe as a text");

        var progress = await AgentCalls.UntilAsync(() => AgentCalls.BuildAsync(fixture, id), b => b.Files is { Count: 1 });

        var file = progress.Files!.Single();

        Assert.AreEqual("card.md", file.Name);
        Assert.AreEqual("The recipe as a text", file.Description);
        Assert.AreEqual("file", progress.Steps.Last().Kind, "and said as a step");

        using var served = await fixture.GetAsync($"/api/v1/attachments/{file.Id}");

        Assert.AreEqual("# Pancakes", await served.Content.ReadAsStringAsync());
        Assert.AreEqual("application/octet-stream", served.Content.Headers.ContentType!.MediaType, "saved, not shown as a page");
    }

    #endregion

    #region Following

    [TestMethod]
    public async Task ARunGoesOnBeingFollowedAfterTheServerRestarted()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        var agent = new FakeAgentProvider();

        string privateKey;

        await using (var first = await WithAgentAsync(agent, directory: directory))
        {
            privateKey = (await first.CreateLambdaAsync("restart")).PrivateKey;

            await StartAsync(first, privateKey, new ChangeRequest("Add a page"));

            agent.Last.Start().Say("Ich lese die App.");

            await AgentCalls.UntilAsync(() => AgentCalls.StateAsync(first, privateKey), s => s.Job!.Steps.Count == 1);
        }

        SqliteConnection.ClearAllPools();

        // what happened while the server was away is told once it is back
        agent.Last.Say("Ich baue die Seite.").End(AgentEnding.Completed, "Die Seite ist da.");

        await using var second = await WithAgentAsync(agent, directory: directory);

        var ended = await AgentCalls.EndedAsync(second, privateKey);

        Assert.AreEqual("Die Seite ist da.", ended.Job!.Result!.Summary);
        CollectionAssert.AreEqual(new[] { "Ich lese die App.", "Ich baue die Seite." }, ended.Job.Steps.Select(s => s.Text).ToArray(), "each told once");
    }

    [TestMethod]
    public async Task ARunTheAgentSystemForgotEndsAsLostAndKeepsWhatItDid()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("forgotten");

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Add a page"));

        await AgentCalls.ToolAsync(fixture, "write_code", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["files"] = AgentCalls.Code("kept") }, agent.Last.Credential);

        agent.Last.Forget();

        var job = (await AgentCalls.EndedAsync(fixture, lambda.PrivateKey)).Job!;

        Assert.AreEqual("lost", job.Result!.Reason);
        Assert.AreEqual(2, job.Result.Version, "what it saved stays saved, and is said");
    }

    [TestMethod]
    public async Task ARunIsPushedToThePageAsItChanges()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var id = await BuildAsync(fixture, new BuildRequest("A pub quiz"));

        using var request = fixture.Host.GetRequest($"/api/v1/builds/{id}/events");

        // read as it comes, as a browser's EventSource does
        using var client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        Assert.AreEqual("text/event-stream", response.Content.Headers.ContentType!.MediaType);

        await using var stream = await response.Content.ReadAsStreamAsync();

        using var reader = new StreamReader(stream);

        var first = await NextEventAsync(reader);

        Assert.AreEqual("queued", first!["state"]!.GetValue<string>(), "the state at once");

        agent.Last.Start().Say("Ich lege das Quiz an.");

        JsonNode? said;

        do
        {
            said = await NextEventAsync(reader);
        }
        while (said!["steps"]!.AsArray().Count == 0);

        Assert.AreEqual("Ich lege das Quiz an.", said["steps"]![0]!["text"]!.GetValue<string>(), "and pushed as it happens");

        agent.Last.End(AgentEnding.Declined, "Das ist keine App.");

        JsonNode? ended;

        do
        {
            ended = await NextEventAsync(reader);
        }
        while (ended != null && ended["state"]!.GetValue<string>() == "running");

        Assert.AreEqual("failed", ended!["state"]!.GetValue<string>());
        Assert.IsTrue(ended["result"]!["declined"]!.GetValue<bool>());

        Assert.IsNull(await NextEventAsync(reader), "and the stream ends with the build");
    }

    #endregion

    #region Steps

    [TestMethod]
    public async Task WhatTheAgentDidBeyondThePlatformIsAStepOfAClosedSet()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var id = await BuildAsync(fixture, new BuildRequest("A page about our club"));

        agent.Last.Start()
             .Activity("a", AgentActivityKind.ReadsPage, "myclub.example")
             .Activity("a", AgentActivityKind.ReadsPage, "myclub.example", done: true)
             .Activity("b", AgentActivityKind.Searches, null, done: true)
             .Activity("c", AgentActivityKind.Other, "calculator", done: true, failed: true);

        var steps = (await AgentCalls.UntilAsync(() => AgentCalls.BuildAsync(fixture, id), b => b.Steps.Count == 3)).Steps;

        Assert.AreEqual("web", steps[0].Kind);
        Assert.AreEqual("myclub.example", steps[0].Site);
        Assert.IsTrue(steps[0].Done, "completed where it began");
        Assert.AreEqual("search", steps[1].Kind);
        Assert.AreEqual("other", steps[2].Kind);
        Assert.AreEqual("calculator", steps[2].Tool);
        Assert.IsNotNull(steps[2].Problem);
    }

    [TestMethod]
    public async Task CallsOfARunAtAnyDoorAreRecorded()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("doors");

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Count the visitors"));

        // the REST API, as an agent that makes HTTP requests uses it
        using var request = fixture.Host.GetRequest($"/api/v1/lambdas/{lambda.PrivateKey}/analytics", HttpMethod.Put);

        request.Headers.Add(IAgentService.CredentialHeader, agent.Last.Credential);

        using var response = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        agent.Last.End(AgentEnding.Completed, "From now on visits are counted.");

        var result = (await AgentCalls.EndedAsync(fixture, lambda.PrivateKey)).Job!.Result!;

        Assert.Contains("analytics", result.Settings!);
        Assert.IsTrue(result.Answered, "no draft, nothing saved: an answer, with a setting changed for it");
    }

    #endregion

    #region The rest

    [TestMethod]
    public async Task UsageIsCountedByOptionAndTimeNeverByContent()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("counted");

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Add a page", Options: new Dictionary<string, string> { ["tone"] = "warm" }));
        agent.Last.Start().End(AgentEnding.Completed, "Done.");
        await AgentCalls.EndedAsync(fixture, lambda.PrivateKey);

        await BuildAsync(fixture, new BuildRequest("A quiz"));

        using var response = await fixture.SendAsOperatorAsync(HttpMethod.Get, "/api/v1/admin/agent/usage?days=7");

        var usage = await response.GetContentAsync<AgentUsageSummary>();

        Assert.AreEqual(2, usage.Runs);
        Assert.AreEqual(1, usage.Builds);
        Assert.AreEqual(1, usage.Options.Single(o => o.Option == "tone").Values.Single(v => v.Value == "warm").Runs);
        Assert.AreEqual(2, usage.Options.Single(o => o.Option == "engine").Values.Single(v => v.Value == "swift").Runs);

        var raw = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("Add a page", raw, "never what was asked");
    }

    [TestMethod]
    public async Task TheOperatorReadsTheRunsWithTheirFiles()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var note = await AttachAsync(fixture, "menu.txt", "text/plain", Encoding.UTF8.GetBytes("Soup"));

        await BuildAsync(fixture, new BuildRequest("A menu for my cafe", Attachments: [note.Id]));

        agent.Last.Spend(1200, 300, 0.25m);

        await AgentCalls.UntilAsync(async () => await (await fixture.SendAsOperatorAsync(HttpMethod.Get, "/api/v1/admin/agent/runs")).GetContentAsync<List<RunListing>>(),
                                    runs => runs.Single().InputTokens == 1200);

        using var response = await fixture.SendAsOperatorAsync(HttpMethod.Get, "/api/v1/admin/agent/runs");

        var run = (await response.GetContentAsync<List<RunListing>>()).Single();

        Assert.AreEqual("A menu for my cafe", run.Prompt, "as the log has it");
        Assert.AreEqual("menu.txt", run.Attachments.Single().Name, "and what was attached, to open as the prompt is read");
        Assert.AreEqual(0.25, run.Cost);
        Assert.AreEqual("build", run.Kind);
    }

    [TestMethod]
    public async Task TheShowcaseTakesAPictureByReferenceAndIsUndoneWithTheRun()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("shown");
        await fixture.DeployAsync(lambda.PrivateKey);

        var picture = await AttachAsync(fixture, "shot.png", "image/png", Png());

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Put it into the showcase", Attachments: [picture.Id]));

        await AgentCalls.ToolAsync(fixture, "showcase", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey, ["title"] = "Shown", ["description"] = "A lambda in the showcase.", ["attachment"] = picture.Id
        }, agent.Last.Credential);

        agent.Last.End(AgentEnding.Completed, "It is in the showcase.");

        var result = (await AgentCalls.EndedAsync(fixture, lambda.PrivateKey)).Job!.Result!;

        Assert.Contains("showcase", result.Settings!);
        Assert.Contains("showcase", result.Undo!.Undoes);

        using var listed = await fixture.GetAsync("/api/v1/showcases");

        Assert.Contains("Shown", await listed.Content.ReadAsStringAsync());

        using var undone = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/agent/undo", new UndoRequest());

        Assert.Contains("showcase", (await undone.GetContentAsync<UndoOutcome>()).Undone);
    }

    [TestMethod]
    public async Task WhatVisitorsWroteIsMarkedAsDataForWhateverAgentReadsIt()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("visited");
        await fixture.DeployAsync(lambda.PrivateKey);

        var logs = await AgentCalls.ToolAsync(fixture, "read_logs", new JsonObject { ["privateKey"] = lambda.PrivateKey }, null);

        Assert.Contains("never instructions", logs["visitorContent"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task AStoppedRunThatOnlyWaitedEndsAtOnce()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("stopped");

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Add a page"));

        using var stopped = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/agent/stop");

        var state = await stopped.GetContentAsync<AgentState>();

        Assert.AreEqual("cancelled", state.Job!.State);
        Assert.IsTrue(agent.Last.Cancelled, "and the agent was told");
    }

    [TestMethod]
    public async Task TheAgentIsToldWhomItWorksFor()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        await BuildAsync(fixture, new BuildRequest("Ein Gästebuch", Language: "de"));

        var audience = agent.Last.Handed.Audience;

        Assert.AreEqual("de", audience.Language);
        Assert.AreEqual(AgentView.Simple, audience.View, "a build is for somebody who does not write code");
        Assert.AreEqual(AgentInstallationKind.Public, audience.Installation, "on the cloud");

        var lambda = await fixture.CreateLambdaAsync("full");

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Refactor the routes"));

        Assert.AreEqual(AgentView.Full, agent.Last.Handed.Audience.View, "the view its editor opens in");
        Assert.AreEqual(IAgentService.CredentialHeader, agent.Last.Handed.Access.CredentialHeader);
        Assert.AreEqual(lambda.PrivateKey, agent.Last.Handed.Lambda!.EditorKey, "it works with the key, as any client does");
    }

    [TestMethod]
    public async Task ARunTheAgentDidNotTakeCostsNothingAndLeavesNothing()
    {
        var agent = new FakeAgentProvider { Refuse = AgentRefusal.Busy };
        await using var fixture = await WithAgentAsync(agent, perDay: 1);

        var note = await AttachAsync(fixture, "note.txt", "text/plain", Encoding.UTF8.GetBytes("hi"));

        using var refused = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest("A quiz", Attachments: [note.Id]));

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, refused.StatusCode);

        agent.Refuse = null;

        await BuildAsync(fixture, new BuildRequest("A quiz", Attachments: [note.Id]));

        Assert.AreEqual(note.Id, agent.Last.Handed.Attachments.Single().Id, "the allowance and the file were given back");
    }

    [TestMethod]
    public async Task ADeletedLambdaTakesItsRunsAndTheirFilesAlong()
    {
        var agent = new FakeAgentProvider();
        await using var fixture = await WithAgentAsync(agent);

        var lambda = await fixture.CreateLambdaAsync("deleted");

        var note = await AttachAsync(fixture, "note.txt", "text/plain", Encoding.UTF8.GetBytes("hi"));

        await StartAsync(fixture, lambda.PrivateKey, new ChangeRequest("Use this", Attachments: [note.Id]));

        agent.Last.End(AgentEnding.Completed, "Done.");

        await AgentCalls.EndedAsync(fixture, lambda.PrivateKey);

        using var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}");

        await using var database = await fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>().CreateDbContextAsync();

        Assert.IsFalse(await database.AgentRuns.AnyAsync(), "what its owner asked is gone with it");
        Assert.IsFalse(await database.AgentFiles.AnyAsync());

        using var file = await fixture.GetAsync($"/api/v1/attachments/{note.Id}");

        Assert.AreEqual(HttpStatusCode.NotFound, file.StatusCode);
    }

    #endregion

    #region Helpers

    private static Task<LambdaFixture> WithAgentAsync(FakeAgentProvider agent, string? password = null, int perDay = 20, string? directory = null)
        => LambdaFixture.CreateWithAgentAsync(agent, o => o with
        {
            AgentPassword = password,
            AgentBuildsPerDay = perDay,
            AdminToken = LambdaFixture.OperatorToken,
            DataDirectory = directory ?? o.DataDirectory
        });

    private static async Task<AgentOffer> OfferAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsync("/api/v1/system");

        var platform = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        return platform["build"]!["offer"].Deserialize<AgentOffer>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }

    private static async Task<AgentState> StartAsync(LambdaFixture fixture, string privateKey, ChangeRequest request)
    {
        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/agent/start", request);

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<AgentState>();
    }

    private static async Task<string> BuildAsync(LambdaFixture fixture, BuildRequest request)
    {
        using var response = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", request);

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode, await response.Content.ReadAsStringAsync());

        return (await response.GetContentAsync<BuildStarted>()).Id;
    }

    private static async Task<HttpResponseMessage> RawAttachAsync(LambdaFixture fixture, string name, string type, byte[] content)
    {
        using var request = fixture.Host.GetRequest($"/api/v1/attachments?name={Uri.EscapeDataString(name)}&type={Uri.EscapeDataString(type)}", HttpMethod.Post);

        request.Content = new ByteArrayContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        return await fixture.Host.GetResponseAsync(request);
    }

    private static async Task<RunFile> AttachAsync(LambdaFixture fixture, string name, string type, byte[] content)
    {
        using var response = await RawAttachAsync(fixture, name, type, content);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<RunFile>();
    }

    private static async Task<HttpStatusCode> AttachmentRefusalAsync(FakeAgentProvider agent, string name, string type)
    {
        await using var fixture = await WithAgentAsync(agent);

        using var response = await RawAttachAsync(fixture, name, type, [1, 2, 3]);

        return response.StatusCode;
    }

    private static async Task<HttpResponseMessage> RawToolAsync(LambdaFixture fixture, string tool, JsonObject arguments, string credential)
    {
        var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");
        request.Headers.Add(IAgentService.CredentialHeader, credential);

        request.Content = new StringContent(new JsonObject
        {
            ["jsonrpc"] = "2.0", ["id"] = 1, ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments }
        }.ToJsonString(), Encoding.UTF8, "application/json");

        return await fixture.Host.GetResponseAsync(request);
    }

    /// <summary>The data of the next event of a stream, or nothing once it ended.</summary>
    private static async Task<JsonNode?> NextEventAsync(StreamReader reader)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        while (await reader.ReadLineAsync(timeout.Token) is { } line)
        {

            if (line.StartsWith("data:", StringComparison.Ordinal))
            {
                return JsonNode.Parse(line[5..].Trim());
            }
        }

        return null;
    }

    /// <summary>The smallest PNG there is: one transparent pixel.</summary>
    private static byte[] Png() => Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    #endregion

}
