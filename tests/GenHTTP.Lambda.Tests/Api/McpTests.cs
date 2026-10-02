using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// The endpoint an agent builds things through.
/// </summary>
[TestClass]
public sealed class McpTests
{

    [TestMethod]
    public async Task ItIntroducesItself()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var answer = await CallAsync(fixture, "initialize", new JsonObject
        {
            ["protocolVersion"] = "2025-06-18",
            ["capabilities"] = new JsonObject(),
            ["clientInfo"] = new JsonObject { ["name"] = "a test", ["version"] = "1" }
        });

        var result = answer["result"]!;

        Assert.AreEqual("2025-06-18", result["protocolVersion"]!.GetValue<string>(),
                        "a version it speaks is answered with the same version");

        Assert.IsNotNull(result["capabilities"]!["tools"], "it has tools, so it has to say so");
        Assert.IsNotNull(result["serverInfo"]!["name"]);
        Assert.Contains("create_lambda", result["instructions"]!.GetValue<string>(),
                        "the instructions have to say how to get from nothing to something online");
    }

    [TestMethod]
    public async Task AVersionItDoesNotKnowIsAnsweredWithOneItDoes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var answer = await CallAsync(fixture, "initialize", new JsonObject { ["protocolVersion"] = "1999-01-01" });

        Assert.AreEqual("2025-06-18", answer["result"]!["protocolVersion"]!.GetValue<string>(),
                        "the client decides whether it can live with that, rather than being refused outright");
    }

    [TestMethod]
    public async Task ANotificationIsAcknowledgedAndNotAnswered()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await PostAsync(fixture, new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["method"] = "notifications/initialized"
        });

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
        Assert.IsEmpty(await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task EveryToolSaysWhatItTakes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var tools = (JsonArray)(await CallAsync(fixture, "tools/list", new JsonObject()))["result"]!["tools"]!;

        var named = tools.Select(t => t!["name"]!.GetValue<string>()).ToList();

        foreach (var wanted in (string[])["create_lambda", "update_lambda", "write_code", "change_code", "create_feature", "update_feature", "merge_feature",
                                          "delete_feature", "check_code", "deploy", "read_lambda", "read_logs", "upload_file", "list_files",
                                          "delete_file", "platform_guide"])
        {
            Assert.Contains(wanted, named);
        }

        foreach (var tool in tools)
        {
            Assert.IsNotEmpty(tool!["description"]!.GetValue<string>(), "a tool nobody can read is a tool nobody calls");
            Assert.AreEqual("object", tool["inputSchema"]!["type"]!.GetValue<string>());
        }
    }

    [TestMethod]
    public async Task EveryToolSaysWhatItDoesToThePlatform()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var tools = (JsonArray)(await CallAsync(fixture, "tools/list", new JsonObject()))["result"]!["tools"]!;

        foreach (var tool in tools)
        {
            Assert.IsNotEmpty(tool!["title"]!.GetValue<string>());
            Assert.IsNotNull(tool["annotations"]?["readOnlyHint"], "without hints a client has to assume the worst of every tool");
        }

        var reading = tools.Where(t => t!["annotations"]!["readOnlyHint"]!.GetValue<bool>())
                           .Select(t => t!["name"]!.GetValue<string>())
                           .Order()
                           .ToList();

        CollectionAssert.AreEqual(new[] { "check_code", "list_demos", "list_files", "list_secrets", "platform_guide", "read_database", "read_lambda", "read_logs" },
                                  reading, "these look and change nothing, so a client may call them without asking");

        var delete = tools.Single(t => t!["name"]!.GetValue<string>() == "delete_file")!;

        Assert.IsTrue(delete["annotations"]!["destructiveHint"]!.GetValue<bool>());
    }

    [TestMethod]
    public async Task ALargeLambdaIsReadByNameUntilAFileIsAskedFor()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        // more than one answer carries: every file in full used to overflow
        // what a client accepts, and then the agent saw nothing at all
        var large = "public static class Lore\n{\n    public const string Text = \"" + new string('a', 40_000) + "\";\n}\n";

        await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["files"] = new JsonArray(
                new JsonObject { ["name"] = "lambda.cs", ["code"] = "return Content.From(Resource.FromString(Lore.Text));" },
                new JsonObject { ["name"] = "Lore.cs", ["code"] = large })
        });

        var listed = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey }));

        Assert.IsTrue(listed["filesOmitted"]!.GetValue<bool>());
        Assert.IsNotNull(listed["note"]);

        var lore = ((JsonArray)listed["files"]!).Single(f => f!["name"]!.GetValue<string>() == "Lore.cs")!;

        Assert.IsNull(lore["code"], "over the budget, the answer lists files rather than carrying them");
        Assert.AreEqual(large.Length, lore["length"]!.GetValue<int>());

        var one = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey, ["file"] = "Lore.cs" }));

        var only = ((JsonArray)one["files"]!).Single()!;

        Assert.AreEqual("Lore.cs", only["name"]!.GetValue<string>());
        Assert.AreEqual(large, only["code"]!.GetValue<string>(), "a file asked for comes back in full, however large");
    }

    [TestMethod]
    public async Task AnUnknownMethodIsAProtocolError()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var answer = await CallAsync(fixture, "does/not/exist", new JsonObject());

        Assert.AreEqual(-32601, answer["error"]!["code"]!.GetValue<int>());
    }

    [TestMethod]
    public async Task AToolThatRefusesSaysSoInItsResult()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // a tool that fails must not fail the call: the model has to be able to
        // read what went wrong and try again
        var answer = await CallToolAsync(fixture, "deploy", new JsonObject { ["privateKey"] = "not-a-key" });

        Assert.IsNull(answer["error"], "a tool going wrong is not the protocol going wrong");

        var result = answer["result"]!;

        Assert.IsTrue(result["isError"]!.GetValue<bool>());
        Assert.IsNotEmpty(result["content"]![0]!["text"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task NothingIsCreatedWithoutTheTermsBeingAccepted()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var answer = await CallToolAsync(fixture, "create_lambda", new JsonObject());

        Assert.IsTrue(answer["result"]!["isError"]!.GetValue<bool>());
        Assert.AreEqual(0, fixture.Meta.Count().Lambdas, "and no lambda was made anyway");
    }

    [TestMethod]
    public async Task AnAgentCanBuildForSomebodyWhoWritesNoCode()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject
        {
            ["acceptTerms"] = true,
            ["publicKey"] = "for-the-owner",
            ["view"] = "Simple"
        }));

        Assert.AreEqual("Simple", made["view"]!.GetValue<string>());

        var privateKey = made["privateKey"]!.GetValue<string>();

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey }));

        Assert.AreEqual("Simple", read["view"]!.GetValue<string>(), "reading it says which view it opens in");

        var full = Structured(await CallToolAsync(fixture, "update_lambda", new JsonObject { ["privateKey"] = privateKey, ["view"] = "full" }));

        Assert.AreEqual("Full", full["view"]!.GetValue<string>());
        Assert.AreEqual("Full", (fixture.Meta.Get(privateKey))!.View, "and it is kept");
    }

    [TestMethod]
    public async Task AnAgentIsToldWhichViewsThereAre()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true }));

        Assert.AreEqual("Full", made["view"]!.GetValue<string>(), "a lambda opens in the full view unless asked otherwise");

        var privateKey = made["privateKey"]!.GetValue<string>();

        var refused = await CallToolAsync(fixture, "update_lambda", new JsonObject { ["privateKey"] = privateKey, ["view"] = "minimal" });

        Assert.IsTrue(refused["result"]!["isError"]!.GetValue<bool>());
        Assert.Contains("Simple", Structured(refused)["problem"]!.GetValue<string>(), "the refusal names the views there are");

        var missing = await CallToolAsync(fixture, "update_lambda", new JsonObject { ["privateKey"] = privateKey });

        Assert.IsTrue(missing["result"]!["isError"]!.GetValue<bool>(), "there is nothing to change without a view");

        var wrong = await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true, ["view"] = "minimal" });

        Assert.IsTrue(wrong["result"]!["isError"]!.GetValue<bool>());
        Assert.AreEqual(1, fixture.Meta.Count().Lambdas, "and no lambda was made for it");
    }

    [TestMethod]
    public async Task AnAgentCanGetFromNothingToSomethingOnline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // create
        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject
        {
            ["acceptTerms"] = true,
            ["publicKey"] = "agent-built"
        }));

        Assert.IsTrue(made["ok"]!.GetValue<bool>());

        var privateKey = made["privateKey"]!.GetValue<string>();

        // write, in two files, to prove that reaches the compiler as two
        var files = new JsonArray(
            new JsonObject
            {
                ["name"] = "lambda.cs",
                ["code"] = "return Inline.Create().Get(() => Greeter.Greet());"
            },
            new JsonObject
            {
                ["name"] = "Greeter.cs",
                ["code"] = "static class Greeter { public static Greeting Greet() => new Greeting(\"built by an agent\"); }\n\npublic record Greeting(string Text);"
            });

        var checked_ = Structured(await CallToolAsync(fixture, "check_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["files"] = files.DeepClone()
        }));

        Assert.IsTrue(checked_["compiles"]!.GetValue<bool>(), checked_.ToJsonString());

        var written = Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["files"] = files.DeepClone()
        }));

        var deployed = Structured(await CallToolAsync(fixture, "deploy", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["version"] = written["version"]!.GetValue<int>()
        }));

        Assert.IsTrue(deployed["ok"]!.GetValue<bool>(), deployed.ToJsonString());

        // and the thing it built actually answers, at a link that can be followed as it is
        var address = new Uri(deployed["publicUrl"]!.GetValue<string>());

        Assert.IsTrue(address.IsAbsoluteUri);

        using var served = await fixture.GetAsync(address.PathAndQuery);

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode);
        Assert.Contains("built by an agent", await served.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task AChangeSendsOnlyWhatChangesAndCanDeployAtOnce()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject
        {
            ["acceptTerms"] = true,
            ["publicKey"] = "changed-by-agent"
        }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        var written = Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["deploy"] = true,
            ["files"] = new JsonArray(
                new JsonObject { ["name"] = "lambda.cs", ["code"] = "return Content.From(Resource.FromString(Greeter.Text));" },
                new JsonObject { ["name"] = "Greeter.cs", ["code"] = "static class Greeter { public const string Text = \"first\"; }" },
                new JsonObject { ["name"] = "notes.txt", ["code"] = "to be removed" })
        }));

        Assert.IsTrue(written["ok"]!.GetValue<bool>(), written.ToJsonString());
        Assert.IsNotNull(written["publicUrl"], "deploy: true answers the way deploy does");

        var changed = Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["deploy"] = true,
            ["remove"] = new JsonArray("notes.txt"),
            ["edits"] = new JsonArray(new JsonObject { ["file"] = "Greeter.cs", ["find"] = "\"first\"", ["replace"] = "\"second\"" })
        }));

        Assert.IsTrue(changed["ok"]!.GetValue<bool>(), changed.ToJsonString());

        using var served = await fixture.GetAsync("/lambda/changed-by-agent/");

        Assert.AreEqual("second", await served.Content.ReadAsStringAsync());

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey }));

        var names = ((JsonArray)read["files"]!).Select(f => f!["name"]!.GetValue<string>()).ToList();

        CollectionAssert.AreEqual(new[] { "lambda.cs", "Greeter.cs" }, names, "the file not named stays, the removed one is gone");
    }

    [TestMethod]
    public async Task AChangeCanBeCompiledWithoutGoingOnline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true, ["publicKey"] = "checked-first" }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        var online = Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["deploy"] = true,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "lambda.cs", ["code"] = "return Content.From(Resource.FromString(\"first\"));" })
        }))["version"]!.GetValue<int>();

        var broken = Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["check"] = true,
            ["edits"] = new JsonArray(new JsonObject { ["file"] = "lambda.cs", ["find"] = "\"first\"", ["replace"] = "\"second\" +" })
        }));

        Assert.IsTrue(broken["ok"]!.GetValue<bool>(), "the version is saved either way");
        Assert.AreEqual(online + 1, broken["version"]!.GetValue<int>());
        Assert.IsFalse(broken["compiles"]!.GetValue<bool>());
        Assert.IsNotEmpty((JsonArray)broken["diagnostics"]!, "and says what is wrong without the files being sent again");

        var fixedUp = Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["check"] = true,
            ["edits"] = new JsonArray(new JsonObject { ["file"] = "lambda.cs", ["find"] = "\"second\" +", ["replace"] = "\"second\"" })
        }));

        Assert.IsTrue(fixedUp["compiles"]!.GetValue<bool>(), fixedUp.ToJsonString());

        using var served = await fixture.GetAsync("/lambda/checked-first/");

        Assert.AreEqual("first", await served.Content.ReadAsStringAsync(), "checking puts nothing online");
    }

    [TestMethod]
    public async Task ADeploymentThatIsRefusedNamesTheVersionAndWhatIsStillOnline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true, ["publicKey"] = "still-online" }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        var online = Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["deploy"] = true,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "lambda.cs", ["code"] = "return Content.From(Resource.FromString(\"works\"));" })
        }))["version"]!.GetValue<int>();

        var refused = Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["deploy"] = true,
            ["edits"] = new JsonArray(new JsonObject { ["file"] = "lambda.cs", ["find"] = ");", ["replace"] = ")" })
        }));

        Assert.IsFalse(refused["ok"]!.GetValue<bool>());
        Assert.AreEqual(online + 1, refused["version"]!.GetValue<int>(), "the version was saved, and which one it is matters");
        Assert.AreEqual(online, refused["stillOnline"]!.GetValue<int>(), "and nothing went offline");
        Assert.IsNotEmpty((JsonArray)refused["diagnostics"]!);
    }

    [TestMethod]
    public async Task AnEditThatMatchesTwiceIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "lambda.cs", ["code"] = "// a\n// a\nreturn Content.From(Resource.FromString(\"x\"));" })
        });

        var answer = await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["edits"] = new JsonArray(new JsonObject { ["file"] = "lambda.cs", ["find"] = "// a", ["replace"] = "// b" })
        });

        Assert.IsTrue(answer["result"]!["isError"]!.GetValue<bool>());
        Assert.Contains("more than once", Structured(answer)["problem"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task AnAgentSaysWhyAndItIsKept()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        var written = Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["specification"] = "a page that says hello",
            ["change"] = "Answers every request with hello",
            ["files"] = new JsonArray(new JsonObject { ["name"] = "lambda.cs", ["code"] = "return Inline.Create().Get(() => \"hello\");" })
        }));

        Assert.IsTrue(written["ok"]!.GetValue<bool>(), written.ToJsonString());

        var versions = fixture.Meta.GetVersions(privateKey);

        Assert.AreEqual("a page that says hello", versions[0].Specification);
        Assert.AreEqual("Answers every request with hello", versions[0].Change);
        Assert.AreEqual("agent", versions[0].Origin, "what came through MCP says so");

        // and the next agent to open it can read why
        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey }));

        Assert.AreEqual("Answers every request with hello", read["change"]!.GetValue<string>());
        Assert.AreEqual("Answers every request with hello", read["history"]![0]!["change"]!.GetValue<string>());

        Structured(await CallToolAsync(fixture, "deploy", new JsonObject { ["privateKey"] = privateKey }));

        var history = fixture.Meta.GetActivations(privateKey);

        Assert.AreEqual("agent", history[0].Origin);
    }

    [TestMethod]
    public async Task AnAgentCanReadHowItsLambdaIsDoing()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject
        {
            ["acceptTerms"] = true,
            ["publicKey"] = "observed"
        }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["files"] = new JsonArray(new JsonObject
            {
                ["name"] = "lambda.cs",
                ["code"] = "return Inline.Create().Get(() => { throw new InvalidOperationException(\"it broke here\"); return \"never\"; });"
            })
        }));

        Structured(await CallToolAsync(fixture, "deploy", new JsonObject { ["privateKey"] = privateKey }));

        using (var _ = await fixture.GetAsync("/lambda/observed/")) { }

        var logs = Structured(await CallToolAsync(fixture, "read_logs", new JsonObject { ["privateKey"] = privateKey }));

        Assert.IsTrue(logs["ok"]!.GetValue<bool>(), logs.ToJsonString());
        Assert.AreEqual(1, logs["traffic"]!["lastHour"]!["serverErrors"]!.GetValue<int>());
        Assert.Contains("it broke here", logs["lines"]!.ToJsonString(), "the exception is what an agent needs to see");
        Assert.DoesNotContain("\"client\"", logs.ToJsonString());

        var refused = await CallToolAsync(fixture, "read_logs", new JsonObject { ["privateKey"] = "not-a-key" });

        Assert.IsTrue(refused["result"]!["isError"]!.GetValue<bool>());
    }

    [TestMethod]
    public async Task AChangeKeepsItsNoteToo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        var changed = Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["change"] = "Adds a readme",
            ["files"] = new JsonArray(new JsonObject { ["name"] = "readme.txt", ["code"] = "hello" })
        }));

        Assert.IsTrue(changed["ok"]!.GetValue<bool>(), changed.ToJsonString());

        var versions = fixture.Meta.GetVersions(privateKey);

        Assert.AreEqual("Adds a readme", versions[0].Change);
        Assert.AreEqual("agent", versions[0].Origin);
    }

    [TestMethod]
    public async Task CodeThatDoesNotCompileComesBackWithTheFileAndLine()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true }));

        var result = Structured(await CallToolAsync(fixture, "check_code", new JsonObject
        {
            ["privateKey"] = made["privateKey"]!.GetValue<string>(),
            ["files"] = new JsonArray(
                new JsonObject { ["name"] = "lambda.cs", ["code"] = "return Inline.Create().Get(() => Broken.Thing());" },
                new JsonObject { ["name"] = "Broken.cs", ["code"] = "static class Broken\n{\n    public static string Thing() => nope;\n}" })
        }));

        Assert.IsFalse(result["compiles"]!.GetValue<bool>());

        var complaint = ((JsonArray)result["diagnostics"]!).First(d => d!["file"]!.GetValue<string>() == "Broken.cs");

        Assert.AreEqual(3, complaint!["line"]!.GetValue<int>(),
                        "an agent fixing this needs to be told which file and which line");
    }

    [TestMethod]
    public async Task AgentsAreToldToLinkRelatively()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var guide = Structured(await CallToolAsync(fixture, "platform_guide", new JsonObject()));

        // a lambda can answer at the root of a domain of its own, where a
        // path starting with /lambda/ or / points at nothing of the lambda's
        Assert.Contains("relative", guide["paths"]!["rule"]!.GetValue<string>());
        Assert.Contains("domain", guide["paths"]!["why"]!.GetValue<string>());

        var initialized = await CallAsync(fixture, "initialize", new JsonObject());

        Assert.Contains("relative paths", initialized["result"]!["instructions"]!.GetValue<string>(),
                        "and the instructions every agent reads say it before the guide does");
    }

    [TestMethod]
    public async Task TheGuideSaysTheThingsThatCatchPeopleOut()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var guide = Structured(await CallToolAsync(fixture, "platform_guide", new JsonObject())).ToJsonString();

        Assert.Contains("Workspace", guide);
        Assert.Contains("anonymous", guide, "returning an anonymous type from a route is the first thing anybody hits");
        Assert.Contains("Inline.Create", guide);

        // an agent shipping a front end has to be told it can put it in a
        // folder, or it will flatten everything into the root and wonder why
        Assert.Contains("Assets.App(\\u0022site\\u0022)", guide,
                        "the guide has to say a folder can be served by naming it");

        // an agent has no Data panel, so the guide has to name the tool that
        // puts a file there - and say that the front end is not what goes there
        Assert.Contains("upload_file", guide, "the guide has to name the tool that puts a file in the workspace");

        var parsed = Structured(await CallToolAsync(fixture, "platform_guide", new JsonObject()));

        Assert.Contains("Assets.App", parsed["servingAFrontEnd"]!["rule"]!.GetValue<string>(),
                        "the front end is part of the program, and shipped with it");
        Assert.Contains("workspace", parsed["servingAFrontEnd"]!["notFromTheWorkspace"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TheGuideSaysHowVersionsFeaturesAndDataLive()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var guide = Structured(await CallToolAsync(fixture, "platform_guide", new JsonObject()));

        // said before anything else about files, because it decides where
        // every one of them goes and how a change is made
        var names = guide.Select(p => p.Key).ToList();

        Assert.IsLessThan(names.IndexOf("assets"), names.IndexOf("lifecycle"), "the lifecycle comes before the details");

        var lifecycle = guide["lifecycle"]!;

        Assert.Contains("never changes", lifecycle["versions"]!["immutable"]!.GetValue<string>());
        Assert.Contains("online", lifecycle["features"]!["when"]!.GetValue<string>(), "a lambda in use is changed in a feature");
        Assert.Contains("merge_feature", lifecycle["features"]!["how"]!.GetValue<string>());
        Assert.Contains("copy", lifecycle["features"]!["data"]!.GetValue<string>());
        Assert.Contains("update_feature", lifecycle["features"]!["rebasing"]!.GetValue<string>(), "and how to get a feature that fell behind merged");
        Assert.Contains("Nothing merges or rebases for you", lifecycle["features"]!["rebasing"]!.GetValue<string>());

        Assert.Contains("every version", lifecycle["data"]!["what"]!.GetValue<string>());
        Assert.Contains("merge", lifecycle["data"]!["lifetime"]!.GetValue<string>());
        Assert.Contains("enable_data", lifecycle["data"]!["optIn"]!.GetValue<string>(), "an agent switches on what it builds needs");
        Assert.Contains("only the owner", lifecycle["data"]!["optIn"]!.GetValue<string>(), "and never switches anything off, which deletes it");

        Assert.Contains("front end", lifecycle["whereThingsGo"]!["theProgram"]!.GetValue<string>());
        Assert.Contains("accounts", lifecycle["whereThingsGo"]!["theRecords"]!.GetValue<string>());
        Assert.Contains("upload", lifecycle["whereThingsGo"]!["theFiles"]!.GetValue<string>());

        Assert.Contains("No feature needed", lifecycle["flows"]!["newLambda"]!.GetValue<string>(), "the flow without features stays");
        Assert.Contains("create_feature", lifecycle["flows"]!["changeALambda"]!.GetValue<string>());

        var initialized = await CallAsync(fixture, "initialize", new JsonObject());

        var instructions = initialized["result"]!["instructions"]!.GetValue<string>();

        Assert.Contains("create_feature", instructions, "the instructions every agent reads say it before the guide does");
        Assert.Contains("merge_feature", instructions);
        Assert.Contains("never in assets", instructions);
    }

    [TestMethod]
    public async Task APreviewThatLinksToTheLambdaByItsFullPathIsWarnedAbout()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true, ["publicKey"] = "linked" }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        var feature = Structured(await CallToolAsync(fixture, "create_feature", new JsonObject { ["privateKey"] = privateKey, ["name"] = "Page" }))
                      ["feature"]!["feature"]!.GetValue<string>();

        var linked = Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["feature"] = feature,
            ["deploy"] = true,
            ["files"] = new JsonArray(
                new JsonObject { ["name"] = "lambda.cs", ["code"] = "return Layout.Create().Add(Assets.App(\"web\"));" },
                new JsonObject { ["name"] = "web/index.html", ["code"] = "<script>fetch('/lambda/linked/api/items', { method: 'POST' })</script>" })
        }));

        Assert.IsTrue(linked["ok"]!.GetValue<bool>(), linked.ToJsonString());
        Assert.Contains("live lambda", linked["warning"]!.GetValue<string>(), "from the preview, that is the real data");

        var relative = Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["feature"] = feature,
            ["deploy"] = true,
            ["edits"] = new JsonArray(new JsonObject { ["file"] = "web/index.html", ["find"] = "/lambda/linked/api/items", ["replace"] = "api/items" })
        }));

        Assert.IsNull(relative["warning"], "a relative path stays in the preview");

        var deleted = Structured(await CallToolAsync(fixture, "delete_feature", new JsonObject { ["privateKey"] = privateKey, ["feature"] = $" {feature.ToUpperInvariant()} " }));

        Assert.AreEqual(feature, deleted["deleted"]!.GetValue<string>(), "said by the key it has, however it was written");
    }

    [TestMethod]
    public async Task APreviewPictureByItsFullAddressIsNotWarnedAbout()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true, ["publicKey"] = "shared" }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        var feature = Structured(await CallToolAsync(fixture, "create_feature", new JsonObject { ["privateKey"] = privateKey, ["name"] = "Preview" }))
                      ["feature"]!["feature"]!.GetValue<string>();

        // social networks do not resolve a relative og:image, and the page
        // never follows it - so the agent told to write it is not told off
        var named = Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["feature"] = feature,
            ["deploy"] = true,
            ["files"] = new JsonArray(
                new JsonObject { ["name"] = "lambda.cs", ["code"] = "return Layout.Create().Add(Assets.App(\"web\"));" },
                new JsonObject
                {
                    ["name"] = "web/index.html",
                    ["code"] = "<head><meta property=\"og:image\" content=\"https://example.com/lambda/shared/preview.png\">"
                             + "<link rel=\"canonical\" href=\"https://example.com/lambda/shared/\"><link rel=\"icon\" href=\"icon.svg\"></head>"
                })
        }));

        Assert.IsTrue(named["ok"]!.GetValue<bool>(), named.ToJsonString());
        Assert.IsNull(named["warning"], "naming the address is not linking there");

        var linked = Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["feature"] = feature,
            ["deploy"] = true,
            ["edits"] = new JsonArray(new JsonObject { ["file"] = "web/index.html", ["find"] = "href=\"icon.svg\"", ["replace"] = "href=\"/lambda/shared/icon.svg\"" })
        }));

        Assert.Contains("live lambda", linked["warning"]!.GetValue<string>(), "an icon the page loads is a link like any other");
    }

    [TestMethod]
    public async Task AgentsAreToldToMakeAPublicPageFindable()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var initialized = await CallAsync(fixture, "initialize", new JsonObject());

        var instructions = initialized["result"]!["instructions"]!.GetValue<string>();

        Assert.Contains("meant to be found", instructions, "said on connecting, before anything is built");
        Assert.Contains("beingFound", instructions, "and where the guide says how");

        var guide = Structured(await CallToolAsync(fixture, "platform_guide", new JsonObject()));

        var found = guide["beingFound"]!;

        Assert.Contains("<title>", found["title"]!.GetValue<string>());
        Assert.Contains("description", found["description"]!.GetValue<string>());
        Assert.Contains("icon", found["icon"]!.GetValue<string>());
        Assert.Contains("og:image", found["socialPreview"]!.GetValue<string>());
        Assert.Contains("domainUrl", found["fullAddress"]!.GetValue<string>(), "a social network does not resolve a relative picture");
        Assert.Contains("canonical", found["canonical"]!.GetValue<string>(), "a lambda on a domain is listed under the domain");
        Assert.Contains("In proportion", found["howMuch"]!.GetValue<string>());

        Assert.Contains("beingFound", guide["paths"]!["exception"]!.GetValue<string>(), "the rule about relative paths names its exception");
    }

    [TestMethod]
    public async Task AnAgentChangesALambdaInAFeatureWithoutTouchingIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject
        {
            ["acceptTerms"] = true,
            ["publicKey"] = "in-a-feature"
        }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        var deployed = Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["deploy"] = true,
            ["change"] = "Greets",
            ["files"] = new JsonArray(new JsonObject { ["name"] = "lambda.cs", ["code"] = "return Content.From(Resource.FromString(\"live\"));" })
        }));

        Assert.Contains("create_feature", deployed["next"]!.GetValue<string>(), "an agent that just put something online is told how the next change is made");

        var created = Structured(await CallToolAsync(fixture, "create_feature", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["name"] = "Louder greeting",
            ["specification"] = "Make the greeting louder"
        }));

        Assert.IsTrue(created["ok"]!.GetValue<bool>(), created.ToJsonString());

        var feature = created["feature"]!["feature"]!.GetValue<string>();

        Assert.AreEqual(2, created["feature"]!["base"]!.GetValue<int>());
        Assert.IsTrue(created["feature"]!["mergeable"]!.GetValue<bool>());

        var previewUrl = new Uri(created["feature"]!["previewUrl"]!.GetValue<string>());

        Assert.AreEqual($"/features/{feature}/", previewUrl.AbsolutePath);

        // two attempts, both in the feature, both tried at its own address
        foreach (var attempt in (string[])["LIVE?", "LIVE!"])
        {
            var changed = Structured(await CallToolAsync(fixture, "change_code", new JsonObject
            {
                ["privateKey"] = privateKey,
                ["feature"] = feature,
                ["deploy"] = true,
                ["change"] = "Greets louder",
                ["files"] = new JsonArray(new JsonObject { ["name"] = "lambda.cs", ["code"] = $"return Content.From(Resource.FromString(\"{attempt}\"));" })
            }));

            Assert.IsTrue(changed["ok"]!.GetValue<bool>(), changed.ToJsonString());
            Assert.IsNull(changed["onlineUntil"], "a preview is not the lambda going online");

            using var preview = await fixture.GetAsync(previewUrl.AbsolutePath);

            Assert.AreEqual(attempt, await preview.GetContentAsync());

            using var live = await fixture.GetAsync("/lambda/in-a-feature/");

            Assert.AreEqual("live", await live.GetContentAsync(), "the lambda goes on serving its visitors");
        }

        var logs = Structured(await CallToolAsync(fixture, "read_logs", new JsonObject { ["privateKey"] = privateKey, ["feature"] = feature }));

        var texts = ((JsonArray)logs["lines"]!).Select(l => l!["text"]!.GetValue<string>()).ToList();

        Assert.IsTrue(texts.Any(t => t.Contains($"/features/{feature}/")), string.Join(" | ", texts));
        Assert.IsFalse(texts.Any(t => t.Contains("/lambda/in-a-feature/")), "what the lambda's visitors caused is not the feature's");

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey }));

        Assert.AreEqual(2, read["latestVersion"]!.GetValue<int>(), "however often a feature is changed, no version is made");
        Assert.AreEqual(feature, ((JsonArray)read["features"]!).Single()!["feature"]!.GetValue<string>());

        var merged = Structured(await CallToolAsync(fixture, "merge_feature", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["feature"] = feature,
            ["deploy"] = true
        }));

        Assert.IsTrue(merged["ok"]!.GetValue<bool>(), merged.ToJsonString());
        Assert.AreEqual(3, merged["version"]!.GetValue<int>());

        using (var live = await fixture.GetAsync("/lambda/in-a-feature/"))
        {
            Assert.AreEqual("LIVE!", await live.GetContentAsync());
        }

        using (var gone = await fixture.GetAsync(previewUrl.AbsolutePath))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, gone.StatusCode, "a merged feature is gone, its preview with it");
        }

        var after = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey }));

        Assert.IsEmpty((JsonArray)after["features"]!);
        Assert.AreEqual("Greets louder", after["change"]!.GetValue<string>(), "the version keeps what the feature said about itself");
        Assert.AreEqual("Make the greeting louder", after["specification"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task AFeatureBehindTheNewestVersionIsMergedOnlyOnceItsBaseMoves()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "lambda.cs", ["code"] = "return Content.From(Resource.FromString(\"two\"));" })
        }));

        var feature = Structured(await CallToolAsync(fixture, "create_feature", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["name"] = "Something"
        }))["feature"]!["feature"]!.GetValue<string>();

        Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["feature"] = feature,
            ["edits"] = new JsonArray(new JsonObject { ["file"] = "lambda.cs", ["find"] = "two", ["replace"] = "two, with something" })
        }));

        // somebody else saves a version meanwhile
        Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "Other.cs", ["code"] = "static class Other { }" })
        }));

        var refused = await CallToolAsync(fixture, "merge_feature", new JsonObject { ["privateKey"] = privateKey, ["feature"] = feature });

        Assert.IsTrue(refused["result"]!["isError"]!.GetValue<bool>());

        var problem = Structured(refused)["problem"]!.GetValue<string>();

        Assert.Contains("version 3", problem);
        Assert.Contains("update_feature", problem, "the refusal says how to get there");

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey, ["feature"] = feature }));

        Assert.IsFalse(read["feature"]!["mergeable"]!.GetValue<bool>());
        Assert.AreEqual(3, ((JsonArray)read["feature"]!["newerVersions"]!).Single()!["version"]!.GetValue<int>(), "and which versions it has to take in");
        Assert.Contains("two, with something", ((JsonArray)read["files"]!)[0]!["code"]!.GetValue<string>(), "the files are the feature's");

        // brought in by hand, and said so
        Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["feature"] = feature,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "Other.cs", ["code"] = "static class Other { }" })
        }));

        var moved = Structured(await CallToolAsync(fixture, "update_feature", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["feature"] = feature,
            ["base"] = 3
        }));

        Assert.IsTrue(moved["feature"]!["mergeable"]!.GetValue<bool>());

        var merged = Structured(await CallToolAsync(fixture, "merge_feature", new JsonObject { ["privateKey"] = privateKey, ["feature"] = feature }));

        Assert.IsTrue(merged["ok"]!.GetValue<bool>(), merged.ToJsonString());
        Assert.AreEqual(4, merged["version"]!.GetValue<int>());
        Assert.IsNull(merged["onlineUntil"], "merged without deploy, nothing went online");
    }

    [TestMethod]
    public async Task AFeatureTriesItselfOutOnACopyOfTheData()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true, ["publicKey"] = "copied-data" }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["deploy"] = true,
            ["files"] = new JsonArray(new JsonObject
            {
                ["name"] = "lambda.cs",
                ["code"] = "return Inline.Create().Get(() => Workspace.ReadText(\"note.txt\")).Post(\"wipe\", () => { Workspace.WriteText(\"note.txt\", \"wiped\"); return \"ok\"; });"
            })
        }));

        Structured(await CallToolAsync(fixture, "upload_file", new JsonObject { ["privateKey"] = privateKey, ["path"] = "note.txt", ["content"] = "real" }));

        var created = Structured(await CallToolAsync(fixture, "create_feature", new JsonObject { ["privateKey"] = privateKey, ["name"] = "Wiping" }));

        var feature = created["feature"]!["feature"]!.GetValue<string>();

        Structured(await CallToolAsync(fixture, "deploy", new JsonObject { ["privateKey"] = privateKey, ["feature"] = feature }));

        using (var copy = await fixture.GetAsync($"/features/{feature}/"))
        {
            Assert.AreEqual("real", await copy.GetContentAsync(), "the feature starts with a copy of what the lambda keeps");
        }

        using (var _ = await fixture.SendAsync(HttpMethod.Post, $"/features/{feature}/wipe")) { }

        using (var live = await fixture.GetAsync("/lambda/copied-data/"))
        {
            Assert.AreEqual("real", await live.GetContentAsync(), "and whatever it does to the copy, the lambda's own is as it was");
        }

        var listed = Structured(await CallToolAsync(fixture, "list_files", new JsonObject { ["privateKey"] = privateKey, ["feature"] = feature }));

        Assert.AreEqual("note.txt", ((JsonArray)listed["files"]!).Single()!["path"]!.GetValue<string>());

        var uploaded = Structured(await CallToolAsync(fixture, "upload_file", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["feature"] = feature,
            ["path"] = "extra.txt",
            ["content"] = "only in the feature"
        }));

        Assert.Contains("copy", uploaded["note"]!.GetValue<string>());

        var real = Structured(await CallToolAsync(fixture, "list_files", new JsonObject { ["privateKey"] = privateKey }));

        Assert.HasCount(1, (JsonArray)real["files"]!, "nothing put into the copy reaches the lambda's data");

        Structured(await CallToolAsync(fixture, "delete_feature", new JsonObject { ["privateKey"] = privateKey, ["feature"] = feature }));

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey }));

        Assert.IsEmpty((JsonArray)read["features"]!);
    }

    [TestMethod]
    public async Task AFeatureThatDoesNotCompileIsNotMerged()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        var feature = Structured(await CallToolAsync(fixture, "create_feature", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["name"] = "Broken"
        }))["feature"]!["feature"]!.GetValue<string>();

        var checkedOnly = Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["feature"] = feature,
            ["check"] = true,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "lambda.cs", ["code"] = "return this is not csharp;" })
        }));

        Assert.IsFalse(checkedOnly["compiles"]!.GetValue<bool>());

        var refused = await CallToolAsync(fixture, "merge_feature", new JsonObject { ["privateKey"] = privateKey, ["feature"] = feature });

        Assert.IsTrue(refused["result"]!["isError"]!.GetValue<bool>());
        Assert.IsNotEmpty((JsonArray)Structured(refused)["diagnostics"]!);

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey }));

        Assert.HasCount(1, (JsonArray)read["features"]!, "the feature stays, to be fixed");
        Assert.AreEqual(1, read["latestVersion"]!.GetValue<int>(), "and no version was made of it");
    }

    [TestMethod]
    public async Task AnAgentIsToldWhenTheWorkspaceIsOff()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using (var _ = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/data/workspace")) { }

        var listed = Structured(await CallToolAsync(fixture, "list_files", new JsonObject { ["privateKey"] = lambda.PrivateKey }));

        Assert.IsFalse(listed["enabled"]!.GetValue<bool>());
        Assert.Contains("ask the user", listed["note"]!.GetValue<string>());

        var refused = await CallToolAsync(fixture, "upload_file", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["path"] = "a.txt",
            ["content"] = "a"
        });

        Assert.IsTrue(refused["result"]!["isError"]!.GetValue<bool>());
        Assert.Contains("switched off", Structured(refused)["problem"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task AFileUploadedByAnAgentIsServedAtOnce()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject
        {
            ["acceptTerms"] = true,
            ["publicKey"] = "uploaded-by-agent"
        }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        // one deploy, and never another for a change to the front end
        Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["files"] = new JsonArray(new JsonObject
            {
                ["name"] = "lambda.cs",
                ["code"] = "return Layout.Create().Add(Workspace.App());"
            })
        }));

        var deployed = Structured(await CallToolAsync(fixture, "deploy", new JsonObject
        {
            ["privateKey"] = privateKey
        }));

        Assert.IsTrue(deployed["ok"]!.GetValue<bool>(), deployed.ToJsonString());

        var written = Structured(await CallToolAsync(fixture, "upload_file", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["path"] = "index.html",
            ["content"] = "<!doctype html><title>by an agent</title><h1>uploaded, not deployed</h1>"
        }));

        Assert.IsTrue(written["ok"]!.GetValue<bool>(), written.ToJsonString());

        // no second deploy
        using var served = await fixture.GetAsync("/lambda/uploaded-by-agent/");

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode);
        Assert.Contains("uploaded, not deployed", await served.GetContentAsync());

        // and an image, which is the only way one gets in at all
        var gif = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

        Structured(await CallToolAsync(fixture, "upload_file", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["path"] = "dot.gif",
            ["content"] = Convert.ToBase64String(gif),
            ["encoding"] = "base64"
        }));

        using var image = await fixture.GetAsync("/lambda/uploaded-by-agent/dot.gif");

        Assert.AreEqual(HttpStatusCode.OK, image.StatusCode);
        CollectionAssert.AreEqual(gif, await image.Content.ReadAsByteArrayAsync(),
                                  "a base64 upload has to arrive as the bytes it stood for");

        var listed = Structured(await CallToolAsync(fixture, "list_files", new JsonObject
        {
            ["privateKey"] = privateKey
        }));

        var names = ((JsonArray)listed["files"]!).Select(f => f!["path"]!.GetValue<string>()).ToList();

        Assert.Contains("index.html", names);
        Assert.Contains("dot.gif", names);

        Structured(await CallToolAsync(fixture, "delete_file", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["path"] = "dot.gif"
        }));

        /*
         * It answers 200 after being deleted, because the application answers
         * every address that names no file with its shell. What says it has
         * gone is that the answer is the page rather than the image.
         */
        using var gone = await fixture.GetAsync("/lambda/uploaded-by-agent/dot.gif");

        Assert.AreEqual("text/html", gone.Content.Headers.ContentType?.MediaType,
                        "and it can be taken away again");
    }

    [TestMethod]
    public async Task ADemoIsReadWithTheToolsAnAgentUsesOnItsOwnLambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        var listed = Structured(await CallToolAsync(fixture, "list_demos", new JsonObject()));

        var demo = ((JsonArray)listed["demos"]!).Select(d => (JsonObject)d!).First(d => d["id"]!.GetValue<string>() == "demo-crud");

        var key = demo["privateKey"]!.GetValue<string>();

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = key }));

        var names = ((JsonArray)read["files"]!).Select(f => f!["name"]!.GetValue<string>()).ToList();

        Assert.Contains("Store.cs", names, "an agent should see every file, not just the snippet");
        Assert.Contains("web/index.html", names, "and the front end in its folder");
        Assert.AreEqual("Demo", read["tier"]!.GetValue<string>());

        var files = Structured(await CallToolAsync(fixture, "list_files", new JsonObject { ["privateKey"] = key }));

        Assert.IsNotNull(files["files"], "what it stores at runtime is readable too");
    }

    [TestMethod]
    public async Task ADemoCannotBeChangedByAnAgent()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        var write = await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = "demo-crud",
            ["edits"] = new JsonArray(new JsonObject { ["file"] = "lambda.cs", ["find"] = "limit: 200", ["replace"] = "limit: 1" }),
            ["deploy"] = true
        });

        Assert.IsTrue(write["result"]!["isError"]!.GetValue<bool>());
        Assert.Contains("create_lambda", Structured(write)["problem"]!.GetValue<string>(), "the refusal says what to do instead");

        var upload = await CallToolAsync(fixture, "upload_file", new JsonObject
        {
            ["privateKey"] = "demo-crud",
            ["path"] = "tasks.json",
            ["content"] = "[]"
        });

        Assert.IsTrue(upload["result"]!["isError"]!.GetValue<bool>(), "nor can what it stores be replaced");

        var feature = await CallToolAsync(fixture, "create_feature", new JsonObject
        {
            ["privateKey"] = "demo-crud",
            ["name"] = "Mine now"
        });

        Assert.IsTrue(feature["result"]!["isError"]!.GetValue<bool>(), "nor can a feature be started on it");

        var copy = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject
        {
            ["template"] = "demo-crud",
            ["acceptTerms"] = true
        }));

        var own = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = copy["privateKey"]!.GetValue<string>() }));

        Assert.AreEqual("Free", own["tier"]!.GetValue<string>(), "a copy of a demo is an ordinary lambda of one's own");
    }

    [TestMethod]
    public async Task AGetIsToldThereIsNoStream()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/mcp");

        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode,
                        "which is how the specification says to decline one");
    }

    [TestMethod]
    public async Task ABrowserFromSomewhereElseIsTurnedAway()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Origin", "https://not-this-server.example");
        request.Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"ping\"}");

        using var response = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode,
                        "an agent sends no origin; only a browser does, and only a browser can be aimed by somebody else");
    }

    [TestMethod]
    public async Task AProtocolVersionItDoesNotKnowIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("MCP-Protocol-Version", "1999-01-01");
        request.Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"ping\"}");

        using var response = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task AFileTooLargeToSendIsLeftToTheArchive()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // well within what any lambda may ship, and more than an answer should
        // carry however it is asked for
        var large = new string('a', 1024 * 1024 + 1);

        using (var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new VersionRequest([
                   new LambdaFile(LambdaSource.EntryName, "return Assets.Files();"),
                   new LambdaFile("large.txt", large)
               ])))
        {
            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());
        }

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["file"] = "large.txt" }));

        var file = ((JsonArray)read["files"]!).Single()!;

        Assert.IsNull(file["code"], "a file this large is named rather than sent");
        Assert.AreEqual(large.Length, file["length"]!.GetValue<int>());
        Assert.IsTrue(read["filesOmitted"]!.GetValue<bool>());
        Assert.Contains("/zip", read["note"]!.GetValue<string>(), "and the agent is told where to get it");
    }

    [TestMethod]
    public async Task ReadingALambdaSaysWhatItsTierAllows()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var free = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey }))["limits"]!;

        Assert.AreEqual(fixture.Options.MaxCodeLengthOf(LambdaTier.Free), free["codeCharacters"]!.GetValue<int>());
        Assert.AreEqual(fixture.Options.MaxAssetBytesOf(LambdaTier.Free), free["assetBytes"]!.GetValue<int>());
        Assert.AreEqual(fixture.Options.WorkspaceOf(LambdaTier.Free).Quota, free["workspaceBytes"]!.GetValue<long>());

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        var premium = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey }))["limits"]!;

        Assert.AreEqual(fixture.Options.MaxCodeLengthOf(LambdaTier.Premium), premium["codeCharacters"]!.GetValue<int>());
        Assert.AreEqual(fixture.Options.MaxAssetBytesOf(LambdaTier.Premium), premium["assetBytes"]!.GetValue<int>());
        Assert.AreEqual(fixture.Options.WorkspaceOf(LambdaTier.Premium).Quota, premium["workspaceBytes"]!.GetValue<long>());
    }

    [TestMethod]
    public async Task TheGuideExplainsWhatEachTierAllows()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var guide = Structured(await CallToolAsync(fixture, "platform_guide", new JsonObject()));

        var limits = guide["limits"]!;

        Assert.Contains("premium", limits["tiers"]!.GetValue<string>(), "which tier a lambda is in, and who decides it");

        var free = limits["free"]!.GetValue<string>();
        var premium = limits["premium"]!.GetValue<string>();

        Assert.Contains("1,048,576 characters", free);
        Assert.Contains("Assets: 32 MB", free);
        Assert.Contains("Workspace: 256 MB", free);

        Assert.Contains("10,485,760 characters", premium);
        Assert.Contains("Assets: 128 MB", premium);
        Assert.Contains("Workspace: 2 GB", premium);

        Assert.IsNull(guide["moreThanOneFile"]!["limit"], "nothing counts the C# files any more");
        Assert.AreEqual(JsonValueKind.String, guide["assets"]!["limits"]!["count"]!.GetValueKind(), "nor the assets");
    }

    [TestMethod]
    public async Task AgentsAreToldToKeepRecordsInTheDatabase()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // said where an agent decides it: on connecting, in the guide, and
        // where it picks the tool that saves the code
        var initialized = await CallAsync(fixture, "initialize", new JsonObject());

        var instructions = initialized["result"]!["instructions"]!.GetValue<string>();

        Assert.Contains("Database.GetConnection()", instructions);
        Assert.Contains("Evolve", instructions);
        Assert.Contains("Entity Framework Core", instructions, "records are read and written through it");
        Assert.Contains("never their Async forms", instructions, "and synchronously");

        var guide = Structured(await CallToolAsync(fixture, "platform_guide", new JsonObject()));

        Assert.Contains("database", guide["lifecycle"]!["whereThingsGo"]!["theRecords"]!.GetValue<string>());
        Assert.Contains("Evolve", guide["database"]!["migrations"]!["example"]!.GetValue<string>());
        Assert.Contains("synchronously", guide["database"]!["usage"]!["synchronous"]!.GetValue<string>());
        Assert.Contains("UseSqlite(connection, contextOwnsConnection: true)", guide["database"]!["entityFramework"]!["connection"]!.GetValue<string>()
                                                                             + string.Join(" ", guide["database"]!["surface"]!.AsArray().Select(s => s!.GetValue<string>())));
        Assert.Contains("SaveChangesAsync", guide["database"]!["entityFramework"]!["synchronous"]!.GetValue<string>(), "named, as what not to call");
        Assert.Contains("EnsureCreated", guide["database"]!["notAllowed"]!.GetValue<string>(), "the schema is Evolve's");

        var tools = (JsonArray)(await CallAsync(fixture, "tools/list", new JsonObject()))["result"]!["tools"]!;

        string Describe(string name) => tools.Single(t => t!["name"]!.GetValue<string>() == name)!["description"]!.GetValue<string>();

        Assert.Contains("database", Describe("write_code"));
        Assert.Contains("'database'", Describe("enable_data"));
        Assert.Contains("Read only", Describe("read_database"), "and nothing but the lambda writes to it");
    }

    [TestMethod]
    public async Task AgentsAreToldToKeepLargeDataInTheWorkspace()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // an agent decides where a model goes while it reads the guide and
        // while it picks a tool, so both have to say it
        var guide = Structured(await CallToolAsync(fixture, "platform_guide", new JsonObject()));

        var rule = guide["lifecycle"]!["whereThingsGo"]!["theFiles"]!.GetValue<string>();

        Assert.Contains("model", rule);
        Assert.Contains("workspace", rule);
        Assert.Contains("upload_file", guide["workspace"]!["fromOutside"]!.GetValue<string>());
        Assert.Contains("workspace", guide["assets"]!["size"]!.GetValue<string>(), "and the assets say where their large files belong");

        var tools = (JsonArray)(await CallAsync(fixture, "tools/list", new JsonObject()))["result"]!["tools"]!;

        string Describe(string name) => tools.Single(t => t!["name"]!.GetValue<string>() == name)!["description"]!.GetValue<string>();

        Assert.Contains("upload_file", Describe("write_code"));
        Assert.Contains("model", Describe("upload_file"));

        // and while it picks one, which way round the program and its data go
        Assert.Contains("front end", Describe("write_code"));
        Assert.Contains("Not for the front end", Describe("upload_file"));
        Assert.Contains("every version", Describe("list_files") + Describe("upload_file"));
    }

    [TestMethod]
    public async Task AnAgentPublishesASourceOnlyWhenItIsAsked()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true, ["publicKey"] = "shared" }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        // asked with only the key, it says how things are and changes nothing
        var state = Structured(await CallToolAsync(fixture, "open_source", new JsonObject { ["privateKey"] = privateKey }));

        Assert.IsFalse(state["published"]!.GetValue<bool>());
        Assert.Contains("only if the user asked", state["note"]!.GetValue<string>());

        using (var nothing = await fixture.GetAsync("/api/v1/sources/shared"))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, nothing.StatusCode);
        }

        var published = Structured(await CallToolAsync(fixture, "open_source", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["license"] = "apache-2.0",
            ["author"] = "Jane Doe"
        }));

        Assert.IsTrue(published["published"]!.GetValue<bool>());
        Assert.AreEqual("Apache-2.0", published["license"]!.GetValue<string>(), "the identifier however it was capitalised");
        Assert.AreEqual("Jane Doe", published["holder"]!.GetValue<string>());
        Assert.EndsWith("/source/shared", published["url"]!.GetValue<string>());
        Assert.Contains("never write keys", published["note"]!.GetValue<string>(), StringComparison.OrdinalIgnoreCase);

        // and from then on, whoever reads the lambda is told its files are public
        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey }));

        Assert.AreEqual("Apache-2.0", read["openSource"]!["license"]!.GetValue<string>());
        Assert.Contains("public", read["openSource"]!["note"]!.GetValue<string>());

        var wrong = await CallToolAsync(fixture, "open_source", new JsonObject { ["privateKey"] = privateKey, ["license"] = "proprietary" });

        Assert.IsTrue(wrong["result"]!["isError"]!.GetValue<bool>());

        var removed = Structured(await CallToolAsync(fixture, "open_source", new JsonObject { ["privateKey"] = privateKey, ["remove"] = true }));

        Assert.IsFalse(removed["published"]!.GetValue<bool>());

        var after = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey }));

        Assert.IsNull(after["openSource"]);
    }

    [TestMethod]
    public async Task TheGuideSaysWhatAPublishedSourceHoldsAndWhatItNeverDoes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var guide = Structured(await CallToolAsync(fixture, "platform_guide", new JsonObject()));

        var open = guide["openSource"]!;

        Assert.Contains("Only when the user asks", open["when"]!.GetValue<string>());
        Assert.Contains("database", open["neverPublished"]!.GetValue<string>());
        Assert.Contains("lambda.cs", open["startingFromOne"]!.GetValue<string>(), "how an agent makes a lambda of a download");

        var tools = (JsonArray)(await CallAsync(fixture, "tools/list", new JsonObject()))["result"]!["tools"]!;

        var tool = tools.Single(t => t!["name"]!.GetValue<string>() == "open_source")!;

        Assert.Contains("never its data", tool["description"]!.GetValue<string>());
        Assert.IsFalse(tool["annotations"]!["readOnlyHint"]!.GetValue<bool>());
    }

    #region Plumbing

    private static JsonObject Structured(JsonObject answer) => (JsonObject)answer["result"]!["structuredContent"]!;

    private static Task<JsonObject> CallToolAsync(LambdaFixture fixture, string tool, JsonObject arguments)
        => CallAsync(fixture, "tools/call", new JsonObject { ["name"] = tool, ["arguments"] = arguments });

    private static async Task<JsonObject> CallAsync(LambdaFixture fixture, string method, JsonObject parameters)
    {
        using var response = await PostAsync(fixture, new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = method,
            ["params"] = parameters
        });

        return (JsonObject)JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
    }

    private static async Task<HttpResponseMessage> PostAsync(LambdaFixture fixture, JsonObject message)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");
        request.Content = new StringContent(message.ToJsonString(), System.Text.Encoding.UTF8, "application/json");

        return await fixture.Host.GetResponseAsync(request);
    }

    #endregion

}
