using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests;

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

        foreach (var wanted in (string[])["create_lambda", "write_code", "check_code", "deploy", "read_lambda", "platform_guide"])
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

        // and the thing it built actually answers
        using var served = await fixture.GetAsync(deployed["publicUrl"]!.GetValue<string>());

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode);
        Assert.Contains("built by an agent", await served.Content.ReadAsStringAsync());
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
    }

    [TestMethod]
    public async Task TheExampleOfAFolderServedAsASiteCanBeRead()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var read = Structured(await CallToolAsync(fixture, "read_example", new JsonObject { ["id"] = "site" }));

        var files = (JsonArray)read["files"]!;

        var names = files.Select(f => f!["name"]!.GetValue<string>()).ToList();

        Assert.Contains("site/index.html", names, "an agent should see the folder, not just the code");
        Assert.Contains("site/app.css", names);
        Assert.Contains("site/app.js", names);
    }

    [TestMethod]
    public async Task AnExampleCanBeReadInFull()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var listed = Structured(await CallToolAsync(fixture, "list_examples", new JsonObject()));

        Assert.IsNotEmpty((JsonArray)listed["examples"]!);

        var read = Structured(await CallToolAsync(fixture, "read_example", new JsonObject { ["id"] = "shop" }));

        var files = (JsonArray)read["files"]!;

        Assert.IsTrue(files.Count > 1, "the shop is several files and an agent should see all of them");
        Assert.IsNotEmpty(files[0]!["code"]!.GetValue<string>());
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
