using System.Text.Json;
using System.Text.Json.Nodes;

namespace GenHTTP.Lambda.Api.Mcp;

/// <summary>
/// The bits of the Model Context Protocol this server speaks.
/// </summary>
/// <remarks>
/// MCP is JSON-RPC 2.0 over a single HTTP endpoint. A server may answer a
/// request either with one JSON object or with a stream of server sent events;
/// this one always answers with the object, because everything it does is a
/// question and an answer and nothing here takes long enough to stream.
/// </remarks>
public static class McpProtocol
{

    /// <summary>
    /// The revisions of the protocol this understands, newest first.
    /// </summary>
    /// <remarks>
    /// A client asking for one of these is answered with the same one. Anything
    /// else is answered with the newest, which the client may then refuse - that
    /// is the negotiation the specification describes, and it is better than
    /// failing outright on a version we might well be compatible with.
    /// </remarks>
    public static readonly string[] Versions = ["2025-06-18", "2025-03-26"];

    public static string Latest => Versions[0];

    public static readonly JsonSerializerOptions Format = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    #region Errors

    public const int ParseError = -32700;

    public const int InvalidRequest = -32600;

    public const int MethodNotFound = -32601;

    public const int InvalidParams = -32602;

    public const int InternalError = -32603;

    #endregion

    #region Envelopes

    /// <summary>
    /// A successful answer to a request.
    /// </summary>
    public static JsonObject Result(JsonNode? id, JsonNode? result) => new()
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id?.DeepClone(),
        ["result"] = result
    };

    /// <summary>
    /// A failed one. This is for the protocol going wrong - a method that does
    /// not exist, arguments that do not fit. A tool that runs and fails says so
    /// in its own result instead, so the model can read what went wrong and try
    /// something else rather than the conversation ending in an error.
    /// </summary>
    public static JsonObject Error(JsonNode? id, int code, string message, JsonNode? data = null)
    {
        var error = new JsonObject
        {
            ["code"] = code,
            ["message"] = message
        };

        if (data != null)
        {
            error["data"] = data;
        }

        return new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id?.DeepClone(),
            ["error"] = error
        };
    }

    /// <summary>
    /// What a tool gives back: text for the model to read, and the same thing
    /// as data for anything that would rather parse it.
    /// </summary>
    public static JsonObject Say(object payload, bool failed = false)
    {
        var text = JsonSerializer.Serialize(payload, Format);

        return new JsonObject
        {
            ["content"] = new JsonArray(new JsonObject
            {
                ["type"] = "text",
                ["text"] = text
            }),
            ["structuredContent"] = JsonNode.Parse(text),
            ["isError"] = failed
        };
    }

    /// <summary>
    /// What a tool gives back when it could not do the thing it was asked.
    /// </summary>
    public static JsonObject Refuse(string message) => Say(new { ok = false, problem = message }, failed: true);

    #endregion

}
