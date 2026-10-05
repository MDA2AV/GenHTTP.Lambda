using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Web;

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

        var answer = await AnswerAsync(method, id, parameters, RequestOrigin.Of(request));

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
                "Hosts small C# web services: a snippet returns a GenHTTP handler, served at a public address.",
                "",
                "Start with platform_guide, then list_demos and read the demo closest to the task with read_lambda",
                "(their keys are public and read only). Follow their patterns.",
                "",
                "A new lambda: create_lambda, then write_code with deploy: true - nothing is reachable before. Give",
                "the user the editor key it returns: it cannot be recovered and grants write access.",
                "",
                "A lambda that exists is changed in a feature: create_feature, then change_code or write_code with",
                "feature and deploy: true to try it at its preview address (read_logs with feature), then",
                "merge_feature with deploy: true. Only a feature based on the newest version merges; if others were",
                "saved since, bring their changes in and move its base with update_feature.",
                "",
                "A version is the program - code and assets, the front end included - and never changes. Data - the",
                "database, the workspace, the secrets - is shared by every version and untouched by deploys and",
                "merges; a feature works on a copy. Records go in the database and uploads in the workspace,",
                "never in assets.",
                "",
                "Database: enable_data 'database'; the schema as Evolve migrations in migrations/",
                "(V1__Create_items.sql, never editing one that was applied, never EF's migrations or EnsureCreated);",
                "records through Entity Framework Core on Database.GetConnection(), used synchronously -",
                "never their Async forms. demo-crud shows it.",
                "",
                "Never wait for a task (.Result, .Wait(), .GetAwaiter().GetResult()): it deadlocks and is refused.",
                "Await it, or call the synchronous method.",
                "",
                "Link with relative paths only (\"api/items\", no leading slash, never /lambda/...): a lambda also",
                "answers at the root of a domain of its own, and a feature at /features/{feature}/.",
                "",
                "A page meant to be found or shared - a website, a shop, a landing page - gets a title, a meta",
                "description, a favicon and a social preview, whose og:image takes a full address: platform_guide,",
                "beingFound.",
                "",
                "Never poll for changes (fetch on a timer to refresh a page): push them from the server with",
                "server-sent events (demo-live) or a websocket (demo-game) - platform_guide, liveUpdates.",
                "",
                "We ask for a small \"Made with GenHTTP Lambda\" line at the foot of the pages you build - a link,",
                "plain text on a domain of its own - unless the user does not want it: platform_guide, backlink.",
                "",
                "API keys and passwords: Secret.Read(\"NAME\") and enable_data 'secrets'; the user enters the value",
                "in the editor.",
                "",
                "Pass specification (what the user asked for and why, in their words) and change (one line for the",
                "owner) with every write.",
                "",
                "Every version keeps .lambda/docs/product.md (what the app is and for whom), .lambda/docs/decisions.md",
                "(the technical decisions and why) and .lambda/tests/README.md (how to test it). Read them before",
                "changing a lambda; write them with a new one and update what a change affects in the same save.",
                "Keep them in proportion: a small lambda needs a few lines each and one quick check, not a test",
                "suite - they grow with the app.",
                "",
                "What you build the assets or code from with a build tool goes in .lambda/dev/ (dev/ in a clone), built by",
                "you and saved with what it built - nothing is built here: platform_guide, development.",
                "",
                "If you can make HTTP requests, the REST API (https://genhttp.dev/api/v1/openapi.json) does the same",
                "with fewer tokens: download a feature as a zip, edit locally, put it back ('zip -r ../f.zip .', so",
                ".lambda/ comes along).",
                "",
                "If you can run git, clone the lambda instead (read_lambda's gitUrl) - AGENTS.md in it says how: a",
                "commit pushed to main is a version, a branch pushed is a feature - platform_guide, git."
            ])
        };
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
