using GenHTTP.Api.Content;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Hosting;

using GenHTTP.Modules.Mcp;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// The Model Context Protocol endpoint, so an agent can build something here
/// and put it online without a person driving the editor.
/// </summary>
/// <remarks>
/// GenHTTP's MCP module speaks the protocol: one path taking POST, answered
/// with one JSON object per request and never with an event stream, a
/// notification acknowledged with 202, the stateless revision of the protocol
/// and the ones before it that start with a handshake - which it answers
/// without keeping a session. That suits this server: everything a call needs
/// is in its arguments, chiefly the editor key, so there is nothing worth
/// keeping between two of them and nothing to lose when a process restarts.
///
/// The tools are <see cref="McpTools"/>, made once from the platform's
/// services (see <see cref="McpServer"/>).
/// </remarks>
public static class McpEndpoint
{

    /// <summary>
    /// The endpoint, answering with the given tools.
    /// </summary>
    internal static IHandlerBuilder Create(LambdaOptions options, McpTools instance)
    {
        var tools = Tools.Create()
                         .Server("genhttp-lambda", "1.0.0", "GenHTTP Lambda")
                         .Instructions(InstructionsFor(options))
                         .Add(instance);

        foreach (var origin in Origins(options))
        {
            tools.AllowOrigin(origin);
        }

        return tools;
    }

    /// <summary>
    /// The web pages a browser may drive the endpoint from.
    /// </summary>
    /// <remarks>
    /// Only browsers send an Origin, and only a browser can be made to send a
    /// request somebody else wrote: the module refuses every origin it was not
    /// given, and an agent speaking HTTP sends none.
    /// </remarks>
    internal static IEnumerable<string> Origins(LambdaOptions options) => options.McpOrigins.Select(McpOrigins.Of).OfType<string>();

    /// <summary>
    /// What an agent is told before it calls anything.
    /// </summary>
    /// <remarks>
    /// Short: how to get from nothing to something online, how a lambda that
    /// exists is changed, and the rules an agent would otherwise break before
    /// it reads the guide. The rest is in <c>platform_guide</c> and the tools'
    /// descriptions.
    /// </remarks>
    /// <summary>
    /// The instructions as this installation gives them: with its own address
    /// where it has one, and the path below it where it does not.
    /// </summary>
    internal static string InstructionsFor(LambdaOptions options)
        => Instructions.Replace("{home}", options.PublicUrl is { } url ? url + "/" : "/", StringComparison.Ordinal);

    internal static readonly string Instructions = string.Join('\n',
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
        "A version is the program and never changes: its code - its .cs files, compiled in any folder, and",
        "any other file, kept and never compiled or served - and its resources in resources/, what",
        "it reads and serves (Resources.App(\"web\") serves resources/web/): the front end, its migrations. Data",
        "- the database, the workspace, the secrets - is shared by every version and untouched by deploys",
        "and merges; a feature works on a copy. Records go in the database and uploads in the workspace,",
        "never in resources.",
        "",
        "Database: enable_data 'database'; the schema as Evolve migrations in resources/migrations/",
        "(V1__Create_items.sql, never editing one that was applied, never EF's migrations or EnsureCreated);",
        "records through Entity Framework Core on Database.GetConnection(), used synchronously -",
        "never their Async forms. demo-crud shows it.",
        "",
        "Never wait for a task (.Result, .Wait(), .GetAwaiter().GetResult()): it deadlocks and is refused.",
        "Await it, or call the synchronous method.",
        "",
        "Link with relative paths only (\"api/items\", no leading slash, never the full address): a lambda",
        "answers at the root of its own address, but a feature at /features/{feature}/, where either leaves it.",
        "",
        "A page meant to be found or shared - a website, a shop, a landing page - gets a title, a meta",
        "description, a favicon and a social preview, whose og:image takes a full address: platform_guide,",
        "beingFound.",
        "",
        "Never poll for changes (fetch on a timer to refresh a page): push them from the server with",
        "server-sent events (demo-live) or a websocket (demo-game) - platform_guide, liveUpdates.",
        "",
        "We ask for a small \"Made with GenHTTP Lambda\" line at the foot of the pages you build - a nofollow",
        "link, plain text on a domain of its own - unless the user does not want it: platform_guide, backlink.",
        "",
        "API keys and passwords: Secret.Read(\"NAME\") and enable_data 'secrets'; the user enters the value",
        "in the editor.",
        "",
        "Pass specification (what the user asked for and why, in their words) and change (one line for the",
        "owner) with every write.",
        "",
        "Every version keeps docs/product.md (what the app is and for whom), docs/decisions.md (the",
        "technical decisions and why) and tests/README.md (how to test it) in its code. Read them before",
        "changing a lambda; write them with a new one and update what a change affects in the same save.",
        "Keep them in proportion: a small lambda needs a few lines each and one quick check, not a test",
        "suite - they grow with the app.",
        "",
        "What you build resources or code from with a tool goes in a folder of the code, built by you and",
        "saved with what it built - nothing is built here: platform_guide, code.",
        "",
        "If you can make HTTP requests, the REST API ({home}api/v1/openapi.json) does the same",
        "with fewer tokens: download a feature as a zip, edit locally, put it back.",
        "",
        "If you can run git, clone the lambda instead (read_lambda's gitUrl) - AGENTS.md in it says how: a",
        "commit pushed to main is a version, a branch pushed is a feature - platform_guide, git."
    ]);

}
