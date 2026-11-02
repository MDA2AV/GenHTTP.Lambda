using System.Net;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Execution;

/// <summary>
/// What a lambda can say about itself to whoever calls it: tools for an AI
/// agent, and the description of its API.
/// </summary>
[TestClass]
public sealed class DescriptionTests
{

    [TestMethod]
    public async Task ALambdaCanOfferToolsToAnAgent()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("abacus");

        // no using: the module and what describes a tool are imported for every lambda
        await fixture.DeployAsync(lambda.PrivateKey, """
            var tools = Tools.Create()
                             .Server("abacus", "1.0.0")
                             .Add<Abacus>();

            return Layout.Create().Add("mcp", tools);

            public sealed class Abacus
            {

                [Tool(ReadOnly = true)]
                [Summary("Adds two numbers")]
                public int Add([Summary("The first")] int a, [System.ComponentModel.DataAnnotations.Range(0, 10)] int b) => a + b;

            }
            """);

        var listed = await CallAsync(fixture, "http://abacus.localhost/mcp", "tools/list", new JsonObject());

        var tool = listed["result"]!["tools"]![0]!;

        Assert.AreEqual("add", tool["name"]!.GetValue<string>());
        Assert.AreEqual("Adds two numbers", tool["description"]!.GetValue<string>());
        Assert.AreEqual(10, tool["inputSchema"]!["properties"]!["b"]!["maximum"]!.GetValue<int>(), "what the lambda allows is said to the agent");

        var called = await CallAsync(fixture, "http://abacus.localhost/mcp", "tools/call", new JsonObject
        {
            ["name"] = "add",
            ["arguments"] = new JsonObject { ["a"] = 2, ["b"] = 3 }
        });

        Assert.AreEqual("5", called["result"]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task ALambdaDescribesItsApiInItsOwnWords()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("greeter");

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Layout.Create()
                         .AddService<Greetings>("greetings")
                         .Add(ApiDescription.Create());

            [Tag("Greetings")]
            public sealed class Greetings
            {

                [ResourceMethod]
                [Summary("Greets whoever asks")]
                [Remarks("Always in English.")]
                public string Greet() => "hello";

            }
            """);

        using var response = await fixture.GetAsync("http://greeter.localhost/openapi.json");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var document = JsonNode.Parse(await response.GetContentAsync())!;

        var operation = document["paths"]!["/greetings/"]!["get"]!;

        Assert.AreEqual("Greets whoever asks", operation["summary"]!.GetValue<string>());
        Assert.AreEqual("Always in English.", operation["description"]!.GetValue<string>());
    }

    private static async Task<JsonObject> CallAsync(LambdaFixture fixture, string address, string method, JsonObject parameters)
    {
        using var response = await fixture.SendAsync(HttpMethod.Post, address, new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = method,
            ["params"] = parameters
        });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return (JsonObject)JsonNode.Parse(await response.GetContentAsync())!;
    }

}
