using System.Net;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Services.Agents;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// What a test does as the agent of a run - calls to the platform's doors
/// with the run's credential, as any agent makes them - and how it waits for
/// what the platform makes of a run, which it records as the agent system
/// tells it, a moment later.
/// </summary>
internal static class AgentCalls
{

    /// <summary>
    /// Calls a tool of the platform's MCP, as the agent of a run does - with
    /// its credential, or as nobody known without.
    /// </summary>
    public static async Task<JsonNode> ToolAsync(LambdaFixture fixture, string tool, JsonObject arguments, string? credential, bool allowError = false)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");

        if (credential != null)
        {
            request.Headers.Add(IAgentService.CredentialHeader, credential);
        }

        request.Content = new StringContent(new JsonObject
        {
            ["jsonrpc"] = "2.0", ["id"] = 1, ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments }
        }.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        var result = JsonNode.Parse(await response.Content.ReadAsStringAsync())!["result"]!;

        if (!allowError)
        {
            Assert.IsFalse(result["isError"]?.GetValue<bool>() ?? false, result.ToJsonString());
        }

        return result["structuredContent"] ?? result;
    }

    /// <summary>
    /// The lambda.cs of a lambda that answers with a word.
    /// </summary>
    public static JsonArray Code(string word = "hello")
        => [new JsonObject { ["name"] = "lambda.cs", ["code"] = $"return Content.From(Resource.FromString(\"{word}\"));" }];

    /// <summary>
    /// Reads what the platform says of a run until it says what the test waits
    /// for - it records what the agent system tells as it is told - or fails
    /// after a while.
    /// </summary>
    public static async Task<T> UntilAsync<T>(Func<Task<T>> read, Func<T, bool> done, string? what = null)
    {
        var until = DateTime.UtcNow + TimeSpan.FromSeconds(15);

        while (true)
        {
            var current = await read();

            if (done(current))
            {
                return current;
            }

            if (DateTime.UtcNow > until)
            {
                Assert.Fail($"Waited in vain{(what != null ? " for " + what : "")}");
            }

            await Task.Delay(30);
        }
    }

    /// <summary>
    /// The state of a lambda's runs, as the editor reads it.
    /// </summary>
    public static async Task<AgentState> StateAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/agent");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AgentState>();
    }

    /// <summary>
    /// How a build is getting on, as /build reads it.
    /// </summary>
    public static async Task<BuildProgress> BuildAsync(LambdaFixture fixture, string id)
    {
        using var response = await fixture.GetAsync($"/api/v1/builds/{id}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<BuildProgress>();
    }

    /// <summary>
    /// The state once the run of a lambda ended.
    /// </summary>
    public static Task<AgentState> EndedAsync(LambdaFixture fixture, string privateKey)
        => UntilAsync(() => StateAsync(fixture, privateKey), s => s.Job is { State: not ("queued" or "running") }, "the run to end");

    /// <summary>
    /// The build once it ended.
    /// </summary>
    public static Task<BuildProgress> BuiltAsync(LambdaFixture fixture, string id)
        => UntilAsync(() => BuildAsync(fixture, id), b => b.State is not ("queued" or "running"), "the build to end");

}
