using System.Net;
using System.Text.Json.Nodes;

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

        foreach (var wanted in (string[])["create_lambda", "write_code", "check_code", "deploy", "read_lambda",
                                          "read_logs", "upload_file", "list_files", "delete_file", "platform_guide"])
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
        Assert.AreEqual(0, (await fixture.Meta.CountAsync()).Lambdas, "and no lambda was made anyway");
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

        var versions = await fixture.Meta.GetVersionsAsync(privateKey);

        Assert.AreEqual("a page that says hello", versions[0].Specification);
        Assert.AreEqual("Answers every request with hello", versions[0].Change);
        Assert.AreEqual("agent", versions[0].Origin, "what came through MCP says so");

        // and the next agent to open it can read why
        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey }));

        Assert.AreEqual("Answers every request with hello", read["change"]!.GetValue<string>());
        Assert.AreEqual("Answers every request with hello", read["history"]![0]!["change"]!.GetValue<string>());

        Structured(await CallToolAsync(fixture, "deploy", new JsonObject { ["privateKey"] = privateKey }));

        var history = await fixture.Meta.GetActivationsAsync(privateKey);

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

        var versions = await fixture.Meta.GetVersionsAsync(privateKey);

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

        // an agent has no Storage panel, so the guide has to name the tool
        // that puts a file there and say which way round to prefer
        Assert.Contains("Workspace.App()", guide, "and that a front end can live in the workspace instead");
        Assert.Contains("upload_file", guide, "the guide has to name the tool that puts a file in the workspace");
        // both ways are ordinary, so the guide has to describe both and say
        // what decides between them rather than naming a winner
        Assert.Contains("withTheCode", guide);
        Assert.Contains("inTheWorkspace", guide);
        Assert.Contains("whenToPreferIt", guide, "each way has to say what it is for");
    }

    [TestMethod]
    public async Task AnAgentCanUploadAFrontEndAndHaveItServed()
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
