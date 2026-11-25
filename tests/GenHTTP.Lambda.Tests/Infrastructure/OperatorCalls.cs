using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// What an operator does with the token: calls the API's operator routes, and
/// the tools of their endpoint.
/// </summary>
internal static class OperatorCalls
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Sends a request with the operator's token, or with another or none.
    /// </summary>
    public static async Task<HttpResponseMessage> OperateAsync(this LambdaFixture fixture, HttpMethod method, string path, HttpContent? content = null,
                                                               string? token = LambdaFixture.OperatorToken)
    {
        using var request = fixture.Host.GetRequest($"/api/v1/admin/{path}", method);

        if (token != null)
        {
            request.Headers.Add("X-Admin-Token", token);
        }

        request.Content = content;

        return await fixture.Host.GetResponseAsync(request);
    }

    /// <summary>
    /// What an operator's route answers, failing where it refused.
    /// </summary>
    public static async Task<T> OperateAsync<T>(this LambdaFixture fixture, HttpMethod method, string path, object? body = null)
    {
        using var response = await fixture.OperateAsync(method, path, body == null ? null : Body(body));

        Assert.IsTrue(response.IsSuccessStatusCode, $"{method} {path}: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");

        return await response.GetContentAsync<T>();
    }

    /// <summary>
    /// Why an operator's route refused.
    /// </summary>
    public static async Task<string> RefusedAsync(this LambdaFixture fixture, HttpMethod method, string path, object? body = null, HttpStatusCode status = HttpStatusCode.BadRequest)
    {
        using var response = await fixture.OperateAsync(method, path, body switch { null => null, byte[] bytes => new ByteArrayContent(bytes), _ => Body(body) });

        var said = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(status, response.StatusCode, $"{method} {path}: {said}");

        return said;
    }

    /// <summary>
    /// Shares a file, failing where it is refused.
    /// </summary>
    public static async Task ShareAsync(this LambdaFixture fixture, string name, byte[] content)
    {
        using var response = await fixture.OperateAsync(HttpMethod.Put, $"shared/{name}", new ByteArrayContent(content));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    public static Task ShareAsync(this LambdaFixture fixture, string name, string text) => fixture.ShareAsync(name, Encoding.UTF8.GetBytes(text));

    /// <summary>
    /// The result of a tool of the operator's endpoint - or of the public one.
    /// </summary>
    public static async Task<JsonNode> ToolAsync(this LambdaFixture fixture, string tool, JsonObject? arguments = null, string path = "/admin/mcp")
    {
        using var request = fixture.Host.GetRequest(path, HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");

        if (path == "/admin/mcp")
        {
            request.Headers.Add("X-Admin-Token", LambdaFixture.OperatorToken);
        }

        request.Content = new StringContent(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments ?? [] }
        }.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        var answer = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        return answer["result"] ?? throw new AssertFailedException($"{tool}: {answer.ToJsonString()}");
    }

    /// <summary>
    /// What a tool answered, failing where it refused.
    /// </summary>
    public static async Task<JsonNode> AnswerAsync(this LambdaFixture fixture, string tool, JsonObject? arguments = null)
    {
        var result = await fixture.ToolAsync(tool, arguments);

        Assert.IsFalse(result["isError"]?.GetValue<bool>() ?? false, $"{tool}: {result.ToJsonString()}");

        return result["structuredContent"]!;
    }

    /// <summary>
    /// The text of a tool's answer, whether it refused or not.
    /// </summary>
    public static string TextOf(JsonNode result) => result["content"]![0]!["text"]!.GetValue<string>();

    private static StringContent Body(object payload) => new(JsonSerializer.Serialize(payload, Json), Encoding.UTF8, "application/json");

}
