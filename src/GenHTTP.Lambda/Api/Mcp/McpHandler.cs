using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.IO;


namespace GenHTTP.Lambda.Api.Mcp;

/// <summary>
/// The Model Context Protocol endpoint, so an agent can build something here
/// and put it online without a person driving the editor.
/// </summary>
/// <remarks>
/// One path, taking POST. Every message is JSON-RPC 2.0: a request is answered
/// with one JSON object, and a notification is acknowledged with 202 and no
/// body. The specification also allows answering over server sent events, and
/// this does not - nothing it does takes long enough to be worth streaming,
/// and a client has to support both.
///
/// No session either. Everything a call needs is in its arguments, chiefly the
/// editor key, so there is nothing worth keeping between two of them and
/// nothing to lose when a process restarts.
/// </remarks>
public sealed class McpHandler : IHandler
{
    private readonly McpTools _tools;

    private readonly HashSet<string> _origins;

    public McpHandler(McpTools tools, IEnumerable<string> origins)
    {
        _tools = tools;
        _origins = new HashSet<string>(origins, StringComparer.OrdinalIgnoreCase);
    }

    public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Header.Method == RequestMethod.Get)
        {
            // the specification allows a server to offer a stream here and to
            // say so with 405 when it does not
            return Plain(request, ResponseStatus.MethodNotAllowed,
                         "This endpoint answers POST with JSON-RPC. It offers no event stream.");
        }

        if (request.Header.Method != RequestMethod.Post)
        {
            return Plain(request, ResponseStatus.MethodNotAllowed, "POST a JSON-RPC message.");
        }

        /*
         * Origin is checked because a browser sends one and a page on some
         * other site could otherwise drive this endpoint with somebody's
         * credentials in the background. An agent speaking HTTP sends no
         * Origin at all and is unaffected.
         */
        var origin = request.Header.Headers.GetEntry("Origin");

        if (origin != null && !Allowed(origin))
        {
            return Plain(request, ResponseStatus.Forbidden, "That origin is not allowed to use this endpoint.");
        }

        var version = request.Header.Headers.GetEntry("MCP-Protocol-Version");

        if (version != null && !McpProtocol.Versions.Contains(version))
        {
            return Plain(request, ResponseStatus.BadRequest, $"This server does not speak MCP '{version}'.");
        }

        JsonNode? message;

        try
        {
            var content = request.HasBody ? request.GetBody() : null;

            var body = content == null ? [] : (await content.AsMemoryAsync()).ToArray();

            message = body.Length == 0 ? null : JsonNode.Parse(Encoding.UTF8.GetString(body));

            if (message == null)
            {
                return Json(request, ResponseStatus.BadRequest,
                            McpProtocol.Error(null, McpProtocol.InvalidRequest, "A message is one JSON-RPC object."));
            }
        }
        catch (Exception)
        {
            return Json(request, ResponseStatus.BadRequest,
                        McpProtocol.Error(null, McpProtocol.ParseError, "That is not JSON."));
        }

        if (message is not JsonObject call)
        {
            return Json(request, ResponseStatus.BadRequest,
                        McpProtocol.Error(null, McpProtocol.InvalidRequest, "A message is one JSON-RPC object."));
        }

        var id = call["id"];

        var method = call["method"]?.GetValue<string>();

        if (method == null)
        {
            return Json(request, ResponseStatus.BadRequest,
                        McpProtocol.Error(id, McpProtocol.InvalidRequest, "A message needs a method."));
        }

        // a notification has no id and is owed no answer
        if (id == null)
        {
            return Plain(request, ResponseStatus.Accepted, string.Empty);
        }

        var parameters = call["params"] as JsonObject ?? [];

        var answer = await AnswerAsync(method, id, parameters, Origin(request));

        return Json(request, ResponseStatus.Ok, answer);
    }

    private async ValueTask<JsonObject> AnswerAsync(string method, JsonNode id, JsonObject parameters, string origin)
    {
        switch (method)
        {
            case "initialize":
                return McpProtocol.Result(id, Introduce(parameters));

            case "ping":
                return McpProtocol.Result(id, new JsonObject());

            case "tools/list":
                return McpProtocol.Result(id, new JsonObject { ["tools"] = _tools.Describe() });

            case "tools/call":
                {
                    var name = parameters["name"]?.GetValue<string>();

                    if (name == null)
                    {
                        return McpProtocol.Error(id, McpProtocol.InvalidParams, "A call needs the name of a tool.");
                    }

                    var arguments = parameters["arguments"] as JsonObject ?? [];

                    return McpProtocol.Result(id, await _tools.CallAsync(name, arguments, origin));
                }

            case "resources/list":
                return McpProtocol.Result(id, new JsonObject { ["resources"] = new JsonArray() });

            case "prompts/list":
                return McpProtocol.Result(id, new JsonObject { ["prompts"] = new JsonArray() });

            default:
                return McpProtocol.Error(id, McpProtocol.MethodNotFound, $"This server has no method '{method}'.");
        }
    }

    /// <summary>
    /// What this server is and what it can do.
    /// </summary>
    /// <remarks>
    /// The version asked for is echoed back when it is one we speak, and the
    /// newest one we speak is offered otherwise. That is the negotiation the
    /// specification describes: the client then decides whether it can live
    /// with the answer.
    /// </remarks>
    private static JsonObject Introduce(JsonObject parameters)
    {
        var asked = parameters["protocolVersion"]?.GetValue<string>();

        return new JsonObject
        {
            ["protocolVersion"] = asked != null && McpProtocol.Versions.Contains(asked) ? asked : McpProtocol.Latest,
            ["capabilities"] = new JsonObject
            {
                ["tools"] = new JsonObject()
            },
            ["serverInfo"] = new JsonObject
            {
                ["name"] = "genhttp-lambda",
                ["title"] = "GenHTTP Lambda",
                ["version"] = "1.0.0"
            },
            ["instructions"] = string.Join('\n',
            [
                "Hosts small C# web services: a snippet returns a GenHTTP handler, which is served at a public address.",
                "",
                "Start with platform_guide, then read a running example (list_examples, read_example).",
                "",
                "Flow: create_lambda, then write_code with deploy: true (check_code first if unsure). Nothing is",
                "reachable before deploy. For later changes use change_code, which takes only the files or the",
                "lines that change.",
                "",
                "Pass prompt (what you were asked) and change (one line on what it does) with every write:",
                "the owner reads them in the version history. After deploying, read_logs shows how it answers.",
                "",
                "create_lambda returns an editor key. It cannot be recovered and grants write access:",
                "give it to the user and do not publish it.",
                "",
                "If you can make HTTP requests, prefer the REST API at https://genhttp.dev/api/v1/openapi.json:",
                "same functionality, fewer tokens, since files are sent directly. A version can be downloaded",
                "and uploaded as a zip, so you can edit locally and push once. Many environments cannot",
                "reach it; then use these tools."
            ])
        };
    }

    /// <summary>
    /// The scheme and host the caller used, so the links handed back can be followed as they are.
    /// </summary>
    /// <remarks>
    /// A proxy in front terminates TLS and says so in its forwarding headers;
    /// without one, the connection itself tells.
    /// </remarks>
    private static string Origin(IRequest request)
    {
        var forwarded = request.Header.Headers.GetForwardings().FirstOrDefault();

        var host = forwarded?.Host ?? request.Header.Headers.GetEntry("Host");

        if (string.IsNullOrEmpty(host))
        {
            return string.Empty;
        }

        var protocol = forwarded?.Protocol ?? request.Client.Protocol;

        return $"{(protocol == ClientProtocol.Https ? "https" : "http")}://{host}";
    }

    private bool Allowed(string origin)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var parsed))
        {
            return false;
        }

        return _origins.Contains(parsed.Host);
    }

    private static IResponse Json(IRequest request, ResponseStatus status, JsonObject payload)
        => Answer(request, status, payload.ToJsonString(McpProtocol.Format), "application/json");

    private static IResponse Plain(IRequest request, ResponseStatus status, string message)
        => Answer(request, status, message, "text/plain; charset=utf-8");

    private static IResponse Answer(IRequest request, ResponseStatus status, string body, string type)
        => request.Respond()
                  .Status(status)
                  .Content(Resource.FromString(body).Type(new ContentType(type)).Build())
                  .Build();

}

/// <summary>
/// Puts the endpoint into a layout.
/// </summary>
public sealed class McpHandlerBuilder(McpTools tools, IEnumerable<string> origins) : IHandlerBuilder<McpHandlerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    public McpHandlerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);

        return this;
    }

    public IHandler Build() => Concerns.Chain(_concerns, new McpHandler(tools, origins));

}
