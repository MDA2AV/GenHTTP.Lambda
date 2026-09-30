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
                "Hosts small C# web services: a snippet returns a GenHTTP handler, which is served at a public address.",
                "",
                "Start with platform_guide. Then list_demos and read the demo closest to what the user wants:",
                "demos are finished lambdas whose keys are public and read only, so read_lambda, list_files and",
                "read_logs work on them as on your own. Follow their patterns.",
                "",
                "A new lambda: create_lambda, then write_code with deploy: true (check_code first if unsure).",
                "Nothing is reachable before deploy.",
                "",
                "Changing a lambda that exists - above all one that is online: work in a feature. Versions never",
                "change once saved; a feature is where work happens. create_feature branches off the newest",
                "version with a copy of its files and of its data; change_code and write_code with feature change",
                "it in place as often as it takes, and deploy: true puts it online at its own preview address -",
                "never the lambda's. Test it there (read_logs with feature). When it does what was asked,",
                "merge_feature makes it the next version; deploy: true puts that online. Only a feature based on",
                "the newest version can be merged: if others were saved since, bring their changes in yourself",
                "and move its base with update_feature.",
                "",
                "A version is the program: code and assets, the whole front end included. Data - the database,",
                "the workspace and the secrets - is what the program keeps: shared by every version, untouched",
                "by deploys, rollbacks and merges, gone only with the lambda. A feature tries itself out on a copy",
                "of the data, which its merge throws away. Records go in the database and uploaded files in the",
                "workspace, never in assets; the front end goes in assets, never in the data.",
                "",
                "Keep records - entries, accounts, orders, scores - in the database, a SQLite file of the lambda's",
                "own: switch it on with enable_data (kind 'database'), ship the schema as SQL migrations in",
                "migrations/ (V1__Create_items.sql, then V2__..., never editing one that was applied) and apply",
                "them with Evolve when the lambda starts. Read and write with Database.GetConnection() and plain",
                "SQL with parameters, synchronously, a connection per request. read_database shows the tables and",
                "rows; demo-crud does all of it.",
                "",
                "Every version also keeps what is written about it, in .lambda/ beside the program - never",
                "compiled, never served: docs/product.md (what the app is, who it is for, why it exists and what",
                "people do with it, in the user's terms), docs/decisions.md (the technical decisions and why) and",
                "tests/README.md (how to test it automatically), with the scripts and data the tests use in",
                "tests/. Read them before you change a lambda. Write them with a new lambda, and update what a",
                "change affects in the same save - in a feature they are merged with the code. Run the tests",
                "against the feature's preview before you merge it.",
                "",
                "API keys, passwords and tokens are secrets, never code: read them with Secret.Read(\"NAME\").",
                "Switch secrets on with enable_data, and let the user set the values under Data > Secrets in the",
                "editor - read_lambda and list_secrets list the names the code reads that have no value yet.",
                "set_secret stores a value the user gave you; nobody can read one back.",
                "",
                "Link with relative paths only (\"api/items\", \"app.css\" - no leading slash, never /lambda/...):",
                "a lambda also answers at the root of a domain of its own, where absolute paths break, and a",
                "feature at /features/{feature}/, where /lambda/... is the live lambda with its real data.",
                "",
                "Pass specification (what the user wants and why, in their words where you can - not your own",
                "instructions) and change (one line on what it does) with every write: the owner reads them in",
                "the version history. After deploying, read_logs shows how it answers.",
                "",
                "create_lambda returns an editor key. It cannot be recovered and grants write access:",
                "give it to the user and do not publish it.",
                "",
                "Building for somebody who does not write code? create_lambda with view: \"Simple\" opens its",
                "editor on the app, how it is doing and a box to ask for a change, without the code, files and",
                "versions. update_lambda changes that default later; each person can still switch for themselves.",
                "",
                "If you can make HTTP requests, prefer the REST API at https://genhttp.dev/api/v1/openapi.json:",
                "same functionality, fewer tokens, since files are sent directly. A feature can be downloaded",
                "and put back as a zip, so you can edit locally and push as often as it takes - the zip holds",
                ".lambda/ too, so zip the folder's contents with it ('zip -r ../f.zip .', not '*'). Many",
                "environments cannot reach it; then use these tools."
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
