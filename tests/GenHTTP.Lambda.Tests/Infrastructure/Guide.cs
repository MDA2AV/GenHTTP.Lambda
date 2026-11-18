using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// Reads platform_guide the way an agent does, and the sections the tools
/// point to as "platform_guide, &lt;section&gt;".
/// </summary>
internal static class Guide
{

    /// <summary>
    /// What the guide answers, as the model reads it.
    /// </summary>
    public static async Task<string> ReadAsync(LambdaFixture fixture) => Text(await AnswerAsync(fixture));

    /// <summary>
    /// The whole answer of the tool call, as it went over the wire.
    /// </summary>
    public static async Task<JsonObject> AnswerAsync(LambdaFixture fixture)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");

        request.Content = new StringContent(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = "platform_guide", ["arguments"] = new JsonObject() }
        }.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        return (JsonObject)JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
    }

    /// <summary>
    /// The text of an answer.
    /// </summary>
    public static string Text(JsonObject answer) => answer["result"]!["content"]![0]!["text"]!.GetValue<string>();

    /// <summary>
    /// A section of the guide - from its heading to the next heading of the
    /// same level or above.
    /// </summary>
    public static string Section(string guide, string name)
    {
        foreach (var level in (string[])["##", "###"])
        {
            var heading = $"\n{level} {name}\n";

            var start = guide.IndexOf(heading, StringComparison.Ordinal);

            if (start < 0)
            {
                continue;
            }

            var body = start + heading.Length;

            var end = guide.Length;

            for (var depth = 1; depth <= level.Length; depth++)
            {
                var next = guide.IndexOf($"\n{new string('#', depth)} ", body, StringComparison.Ordinal);

                if (next >= 0 && next < end)
                {
                    end = next;
                }
            }

            return guide[body..end];
        }

        throw new AssertFailedException($"The guide has no section '{name}'.");
    }

}
