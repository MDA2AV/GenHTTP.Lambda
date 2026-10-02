using System.Net;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Diagnostics;

/// <summary>
/// What an operator reads about what was done through the API and the MCP
/// tools: the operation, the lambda by its public key, and the arguments worth
/// knowing - never the editor key, and never the value of a secret.
/// </summary>
[TestClass]
public sealed class OperationLogTests
{

    [TestMethod]
    public async Task WhatIsDoneThroughTheApiIsLoggedByThePublicKey()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("logged-lambda");

        await fixture.DeployAsync(lambda.PrivateKey, "return Content.From(Resource.FromString(\"hello\"));");

        using var secrets = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/secrets");

        Assert.AreEqual(HttpStatusCode.OK, secrets.StatusCode);

        using var secret = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/SHOP_KEY", new SecretRequest("a-value-nobody-reads"));

        Assert.AreEqual(HttpStatusCode.OK, secret.StatusCode);

        using var source = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/source", new SourceRequest("MIT", null));

        Assert.AreEqual(HttpStatusCode.OK, source.StatusCode);

        var logged = Logged(fixture, "Resource");

        Assert.Contains("Created lambda logged-lambda template (none) view Full", logged);
        Assert.Contains("Saved lambda logged-lambda version 2 files 1", logged);
        Assert.Contains("Deployed lambda logged-lambda version 2", logged);
        Assert.Contains("Enabled secrets of lambda logged-lambda", logged);
        Assert.Contains("Set secret SHOP_KEY of lambda logged-lambda", logged);
        Assert.Contains("Published source of lambda logged-lambda license MIT", logged);

        AssertNothingSecret(fixture, lambda.PrivateKey, "a-value-nobody-reads");
    }

    [TestMethod]
    public async Task WhatAnAgentDoesThroughMcpIsLogged()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var made = Structured(await CallToolAsync(fixture, "create_lambda", new JsonObject { ["acceptTerms"] = true, ["publicKey"] = "agent-made" }));

        var privateKey = made["privateKey"]!.GetValue<string>();

        await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = privateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "lambda.cs", ["code"] = "return Content.From(Resource.FromString(\"hello\"));" }),
            ["deploy"] = true
        });

        var created = Structured(await CallToolAsync(fixture, "create_feature", new JsonObject { ["privateKey"] = privateKey, ["name"] = "Dark mode" }));

        var feature = created["feature"]!["feature"]!.GetValue<string>();

        await CallToolAsync(fixture, "enable_data", new JsonObject { ["privateKey"] = privateKey, ["kind"] = "secrets" });

        await CallToolAsync(fixture, "set_secret", new JsonObject { ["privateKey"] = privateKey, ["name"] = "SHOP_KEY", ["value"] = "a-value-nobody-reads" });

        await CallToolAsync(fixture, "set_secret", new JsonObject { ["privateKey"] = privateKey, ["feature"] = feature, ["name"] = "SANDBOX_KEY", ["value"] = "a-value-nobody-reads" });

        await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = privateKey });

        await CallToolAsync(fixture, "delete_feature", new JsonObject { ["privateKey"] = privateKey, ["feature"] = feature });

        var logged = Logged(fixture, "McpTools");

        Assert.Contains("Created lambda agent-made template (none) view Full", logged);
        Assert.Contains("Saved lambda agent-made version 2 files 1", logged);
        Assert.Contains("Deployed lambda agent-made version 2", logged);
        Assert.Contains("Created feature 'Dark mode' of lambda agent-made base 2", logged);
        Assert.Contains("Enabled secrets of lambda agent-made", logged);
        Assert.Contains("Set secret SHOP_KEY of lambda agent-made", logged);
        Assert.Contains("Set secret SANDBOX_KEY of feature 'Dark mode' of lambda agent-made", logged);
        Assert.Contains("Read lambda agent-made version 2", logged);
        Assert.Contains("Deleted feature 'Dark mode' of lambda agent-made", logged);

        AssertNothingSecret(fixture, privateKey, "a-value-nobody-reads");

        Assert.IsFalse(Said(fixture).Any(l => l.Contains(feature, StringComparison.Ordinal)), "a feature is named by its name, never by its key");
    }

    [TestMethod]
    public async Task WhatSomebodyAsksToHaveBuiltIsLoggedInFull()
    {
        await using var agent = FakeAgent.Start();

        await using var fixture = await LambdaFixture.CreateAsync(o => o with { AgentUrl = agent.Url, AgentToken = "a secret" });

        const string prompt = "A pub quiz with twenty questions about the history of the city, and a leaderboard for the teams";

        using var build = await fixture.SendAsync(HttpMethod.Post, "/api/v1/builds", new BuildRequest(prompt));

        Assert.AreEqual(HttpStatusCode.Accepted, build.StatusCode);

        Assert.IsTrue(Logged(fixture, "BuildResource").Any(l => l.EndsWith(prompt, StringComparison.Ordinal)),
                      "the prompt is the one argument that is logged however long it is");
    }

    #region Helpers

    /// <summary>
    /// What was logged by the sources whose name ends as given.
    /// </summary>
    private static List<string> Logged(LambdaFixture fixture, string source)
        => [.. fixture.Book.Read(0, null, LogLevel.Information, 50_000).Lines.Where(l => l.Source.EndsWith(source, StringComparison.Ordinal)).Select(l => l.Text)];

    private static void AssertNothingSecret(LambdaFixture fixture, string privateKey, string value)
    {
        var everything = Said(fixture);

        Assert.IsFalse(everything.Any(l => l.Contains(privateKey, StringComparison.Ordinal)), "the editor key is never logged");
        Assert.IsFalse(everything.Any(l => l.Contains(value, StringComparison.Ordinal)), "and neither is the value of a secret");
    }

    /// <summary>
    /// Everything the server said, apart from the request lines: they carry
    /// the path as it was requested, editor key and all, and these tests are
    /// about the lines that say what was done.
    /// </summary>
    private static List<string> Said(LambdaFixture fixture)
        => [.. fixture.Book.Read(0, null, LogLevel.Trace, 50_000).Lines.Where(l => l.Source != "Requests").Select(l => l.Text + " " + l.Detail)];

    private static JsonObject Structured(JsonObject answer) => (JsonObject)answer["result"]!["structuredContent"]!;

    private static async Task<JsonObject> CallToolAsync(LambdaFixture fixture, string tool, JsonObject arguments)
    {
        var message = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments }
        };

        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");
        request.Content = new StringContent(message.ToJsonString(), System.Text.Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        var answer = (JsonObject)JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.AreNotEqual(true, answer["result"]?["isError"]?.GetValue<bool>(), $"{tool} failed: {answer.ToJsonString()}");

        return answer;
    }

    #endregion

}
