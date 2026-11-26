using System.Net;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Blocking;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Operator;

/// <summary>
/// The operator's MCP endpoint at <c>/admin/mcp</c>: what the panel shows and
/// does, for an agent the operator hands the admin token as a header.
/// </summary>
[TestClass]
public sealed class OperatorMcpTests
{
    private const string Live = "return Inline.Create().Get(() => \"live\");";

    #region The door

    [TestMethod]
    public async Task TheOperatorsEndpointAsksForTheTokenAsTheApiDoes()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        using (var anonymous = await PostAsync(fixture, List(), token: null))
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        }

        using (var wrong = await PostAsync(fixture, List(), token: "not-the-token"))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, wrong.StatusCode);
        }

        using (var looking = await SendAsync(fixture, HttpMethod.Get, null, token: null))
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized, looking.StatusCode, "a GET is not let past the token either");
        }

        var tools = await ToolNamesAsync(fixture);

        Assert.Contains("list_lambdas", tools);
        Assert.Contains("customize_installation", tools, "the community edition lets the operator brand it");
    }

    [TestMethod]
    public async Task NoBrowserPageDrivesTheEndpoint()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => LambdaFixture.WithPanel(o) with { McpOrigins = ["https://apps.example.com"] });

        using var request = fixture.Host.GetRequest("/admin/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");
        request.Headers.Add("X-Admin-Token", LambdaFixture.OperatorToken);
        request.Headers.Add("Origin", "https://apps.example.com");
        request.Content = new StringContent(List().ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, "not even one of the origins the public endpoint allows");

        using var open = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        open.Headers.Add("Accept", "application/json, text/event-stream");
        open.Headers.Add("Origin", "https://apps.example.com");
        open.Content = new StringContent(List().ToJsonString(), Encoding.UTF8, "application/json");

        using var allowed = await fixture.Host.GetResponseAsync(open);

        Assert.AreEqual(HttpStatusCode.OK, allowed.StatusCode, "where the public endpoint lets it in");
    }

    [TestMethod]
    public async Task WithoutATokenThereIsNoOperatorsEndpoint()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync();

        using (var posted = await PostAsync(fixture, List(), token: null))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, posted.StatusCode, "not handed on to the panel's pages");
        }

        using (var looked = await fixture.GetAsync("/admin/mcp"))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, looked.StatusCode);
        }

        using var panel = await fixture.GetAsync("/admin/");

        Assert.AreEqual(HttpStatusCode.OK, panel.StatusCode, "the panel's pages are where they were");
    }

    [TestMethod]
    public async Task ThePanelIsServedAroundTheEndpointAsBefore()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        foreach (var path in new[] { "/admin", "/admin/", "/admin/lambdas", "/admin/lambdas/some-key", "/admin/mcpx" })
        {
            using var page = await fixture.GetAsync(path);

            Assert.AreEqual(HttpStatusCode.OK, page.StatusCode, path);
        }
    }

    [TestMethod]
    public async Task ThePublicEndpointShowsNoOperatorsTool()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");
        request.Content = new StringContent(List().ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        var listed = await response.Content.ReadAsStringAsync();

        Assert.Contains("create_lambda", listed);
        Assert.DoesNotContain("customize_installation", listed, "moved to the operator's endpoint");
        Assert.DoesNotContain("list_lambdas", listed);
    }

    [TestMethod]
    public async Task ABlockedAddressStillReachesTheOperatorsEndpoint()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => LambdaFixture.WithPanel(o) with { UnblockableNetworks = [] });

        var caller = (await CallAsync(fixture, "list_address_blocks"))["caller"]!.GetValue<string>();

        await fixture.Platform.Services.GetRequiredService<IAddressBlocks>().BlockAsync([caller], "Myself, by mistake", null);

        using (var refused = await fixture.GetAsync("/api/v1/demos"))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode, "blocked everywhere else");
        }

        using (var anonymous = await PostAsync(fixture, List(), token: null))
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode, "let through to the door, not past it");
        }

        var listed = await CallAsync(fixture, "list_address_blocks");

        var block = listed["blocks"]![0]!;

        Assert.AreEqual(caller, block["address"]!.GetValue<string>());

        await CallAsync(fixture, "remove_address_block", new JsonObject { ["id"] = block["id"]!.GetValue<long>() });

        using var again = await fixture.GetAsync("/api/v1/demos");

        Assert.AreEqual(HttpStatusCode.OK, again.StatusCode, "the operator lifted their own block from the endpoint");
    }

    [TestMethod]
    public async Task AHostNobodyHasIsNotAnsweredThere()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => LambdaFixture.WithPanel(o) with { PublicUrl = "https://apps.example.com" });

        using var elsewhere = await PostAsync(fixture, List(), host: "nobody.example.org");

        Assert.AreEqual(HttpStatusCode.NotFound, elsewhere.StatusCode);
    }

    #endregion

    #region What it offers

    [TestMethod]
    public async Task EveryRouteOfTheOperatorsApiHasATool()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        using var described = await fixture.GetAsync("/api/v1/openapi.json");

        var paths = JsonNode.Parse(await described.Content.ReadAsStringAsync())!["paths"]!.AsObject();

        var routes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (documented, operations) in paths)
        {
            var path = documented.TrimEnd('/');

            // the operator's, and the public one the panel opens a shared file by
            if (!(path.StartsWith("/admin", StringComparison.Ordinal) || path.StartsWith("/telemetry", StringComparison.Ordinal) || path.StartsWith("/logs", StringComparison.Ordinal)
                  || path.StartsWith("/shared/", StringComparison.Ordinal)))
            {
                continue;
            }

            foreach (var (method, _) in operations!.AsObject())
            {
                if (method is "get" or "put" or "post" or "delete" or "patch")
                {
                    routes.Add($"{method.ToUpperInvariant()} {path}");
                }
            }
        }

        CollectionAssert.AreEquivalent(Tools.Keys.ToArray(), routes.ToArray(),
                                       "a route of the operator's API without a tool here - or the other way round - is added to this map with its tool");

        var offered = await ToolNamesAsync(fixture);

        CollectionAssert.AreEquivalent(Tools.Values.Distinct().ToArray(), offered.ToArray(), "every tool is one of the panel's");
    }

    /// <summary>
    /// What of the operator's endpoint does what each route of the operator's
    /// API does - several routes to one tool where one tool takes what the
    /// routes split.
    /// </summary>
    private static readonly Dictionary<string, string> Tools = new()
    {
        ["GET /admin/lambdas"] = "list_lambdas",
        ["GET /admin/lambdas/{publicKey}"] = "read_lambda",
        ["GET /admin/lambdas/{publicKey}/versions/{version}"] = "read_version",
        ["POST /admin/lambdas/{publicKey}/deployment/start"] = "deploy_lambda",
        ["POST /admin/lambdas/{publicKey}/deployment/stop"] = "undeploy_lambda",
        ["PUT /admin/lambdas/{publicKey}/tier"] = "change_tier",
        ["PUT /admin/lambdas/{publicKey}/domain"] = "change_domain",
        ["PUT /admin/lambdas/{publicKey}/connectivity"] = "decide_connectivity",
        ["POST /admin/lambdas/{publicKey}/block"] = "block_lambda",
        ["DELETE /admin/lambdas/{publicKey}/block"] = "unblock_lambda",
        ["DELETE /admin/lambdas/{publicKey}"] = "delete_lambda",
        ["GET /admin/overview"] = "read_overview",
        ["GET /admin/storage"] = "read_storage",
        ["POST /admin/storage/measure"] = "read_storage",
        ["GET /admin/blocks"] = "list_address_blocks",
        ["DELETE /admin/blocks/{id}"] = "remove_address_block",
        ["GET /admin/connectivity"] = "list_connectivity",
        ["GET /admin/settings"] = "read_settings",
        ["PUT /admin/settings"] = "change_settings",
        ["GET /admin/limits"] = "read_limits",
        ["PUT /admin/limits"] = "change_limits",
        ["GET /admin/guide"] = "read_guide",
        ["PUT /admin/guide"] = "change_guide_section",
        ["DELETE /admin/guide"] = "remove_guide_section",
        ["GET /admin/agent"] = "read_agent_settings",
        ["PUT /admin/agent"] = "change_agent_settings",
        ["GET /admin/agent/runs"] = "list_agent_runs",
        ["GET /admin/agent/usage"] = "read_agent_usage",
        ["GET /admin/integrations"] = "list_integrations",
        ["GET /admin/usage-report"] = "read_usage_report",
        ["GET /admin/quiet-hours"] = "read_quiet_hours",
        ["GET /admin/optimization"] = "read_optimization",
        ["GET /admin/branding"] = "customize_installation",
        ["PUT /admin/branding"] = "customize_installation",
        ["DELETE /admin/branding"] = "customize_installation",
        ["PUT /admin/branding/logo"] = "customize_installation",
        ["DELETE /admin/branding/logo"] = "customize_installation",
        ["GET /admin/branding/preview"] = "customize_installation",
        ["PUT /admin/branding/colors"] = "customize_installation",
        ["PUT /admin/branding/fonts"] = "customize_installation",
        ["GET /admin/shared"] = "list_shared_files",
        ["PUT /admin/shared/{name}"] = "write_shared_file",
        ["DELETE /admin/shared/{name}"] = "delete_shared_file",
        ["GET /shared/{name}"] = "read_shared_file",
        ["GET /telemetry"] = "read_telemetry",
        ["GET /telemetry/lambdas"] = "list_activity",
        ["GET /logs"] = "read_logs",
        ["GET /logs/callers"] = "list_callers"
    };

    [TestMethod]
    public async Task ReadsSayTheyChangeNothingAndWhatCannotBeUndoneIsDestructive()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var listed = await RpcAsync(fixture, List());

        string[] destructive = ["delete_lambda", "remove_guide_section", "delete_shared_file", "customize_installation"];

        foreach (var tool in listed["result"]!["tools"]!.AsArray())
        {
            var name = tool!["name"]!.GetValue<string>();
            var annotations = tool["annotations"]!;

            var reads = name.StartsWith("read_", StringComparison.Ordinal) || name.StartsWith("list_", StringComparison.Ordinal);

            Assert.AreEqual(reads, annotations["readOnlyHint"]!.GetValue<bool>(), name);
            Assert.AreEqual(destructive.Contains(name), annotations["destructiveHint"]!.GetValue<bool>(), name);
            Assert.IsFalse(annotations["openWorldHint"]!.GetValue<bool>(), name);
        }
    }

    [TestMethod]
    public async Task WhereTheHostOffersNoBrandingThereIsNoToolForIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var tools = await ToolNamesAsync(fixture);

        Assert.Contains("read_overview", tools);
        Assert.DoesNotContain("customize_installation", tools);
    }

    #endregion

    #region Running the lambdas

    [TestMethod]
    public async Task AnOperatorsAgentTakesALambdaThroughWhatThePanelDoes()
    {
        // the fixture's own address is recorded where it is not one of the
        // installation's own networks
        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => LambdaFixture.WithPanel(o) with { UnblockableNetworks = [] });

        var lambda = await fixture.CreateLambdaAsync("shop");

        await fixture.DeployAsync(lambda.PrivateKey, Live);

        var page = await CallAsync(fixture, "list_lambdas", new JsonObject { ["search"] = "shop" });

        Assert.AreEqual("shop", page["lambdas"]![0]!["publicKey"]!.GetValue<string>());

        var read = await CallAsync(fixture, "read_lambda", new JsonObject { ["publicKey"] = "shop" });

        Assert.AreEqual(read["lambda"]!["newest"]!.GetValue<int>(), read["lambda"]!["online"]!.GetValue<int>(), "what was put online last");
        Assert.AreEqual(1, read["editorAddresses"]!["count"]!.GetValue<int>(), "the address it was made from, to block");
        Assert.IsGreaterThan(0L, read["keeps"]!["versionAllowance"]!.GetValue<long>(), "what it keeps against what it may");
        Assert.IsGreaterThan(0L, read["keeps"]!["codeBytes"]!.GetValue<long>());
        Assert.IsNotNull(read["problems"]!.AsArray(), "what went wrong lately, as the panel's page shows it");

        var moved = await CallAsync(fixture, "change_tier", new JsonObject { ["publicKey"] = "shop", ["tier"] = "Production" });

        Assert.AreEqual("Production", moved["lambda"]!["tier"]!.GetValue<string>());

        var domain = await CallAsync(fixture, "change_domain", new JsonObject { ["publicKey"] = "shop", ["domain"] = "shop.example.com" });

        Assert.AreEqual("shop.example.com", domain["lambda"]!["domain"]!.GetValue<string>());

        var narrowed = await CallAsync(fixture, "list_lambdas", new JsonObject
        {
            ["tier"] = "Production",
            ["state"] = "online",
            ["view"] = "full",
            ["uses"] = new JsonArray("domain"),
            ["sort"] = "changed"
        });

        Assert.AreEqual("shop", narrowed["lambdas"]![0]!["publicKey"]!.GetValue<string>(), "every filter the panel has");
        Assert.AreEqual(1, narrowed["matched"]!.GetValue<int>());

        var other = await CallAsync(fixture, "list_lambdas", new JsonObject { ["tier"] = "Sandbox" });

        Assert.AreEqual(0, other["matched"]!.GetValue<int>());

        var unknown = await ToolAsync(fixture, "list_lambdas", new JsonObject { ["uses"] = new JsonArray("rockets") });

        Assert.Contains("There is no feature 'rockets'", unknown["result"]!["content"]![0]!["text"]!.GetValue<string>(), "a refusal that names what there is");

        var offline = await CallAsync(fixture, "undeploy_lambda", new JsonObject { ["publicKey"] = "shop" });

        Assert.IsNull(offline["lambda"]!["online"], "nothing online any more");

        var online = await CallAsync(fixture, "deploy_lambda", new JsonObject { ["publicKey"] = "shop" });

        Assert.IsTrue(online["ok"]!.GetValue<bool>());

        var blocked = await CallAsync(fixture, "block_lambda", new JsonObject { ["publicKey"] = "shop", ["reason"] = "Phishing" });

        Assert.AreEqual("Phishing", blocked["lambda"]!["blocked"]!["reason"]!.GetValue<string>());

        using (var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new VersionRequest([new LambdaFile("lambda.cs", Live)])))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode, "the owner can change nothing while it is blocked");
        }

        await CallAsync(fixture, "unblock_lambda", new JsonObject { ["publicKey"] = "shop" });

        await CallAsync(fixture, "delete_lambda", new JsonObject { ["publicKey"] = "shop" });

        var gone = await ToolAsync(fixture, "read_lambda", new JsonObject { ["publicKey"] = "shop" });

        Assert.IsTrue(gone["result"]!["isError"]!.GetValue<bool>(), "a refusal that says why, not a failed call");

        var said = Lines(fixture);

        Assert.Contains("Set tier of lambda shop to Production by operator", said, "logged in the panel's words");
        Assert.Contains("Blocked lambda shop addresses 0 by operator", said);
        Assert.Contains("Deleted lambda shop by operator", said);
        Assert.Contains("Read lambda shop by operator", said, "reads are logged as every MCP read is");
    }

    [TestMethod]
    public async Task NoAnswerHoldsAnEditorKeyOrTheToken()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("private-things");

        await fixture.DeployAsync(lambda.PrivateKey, Live);

        foreach (var (tool, arguments) in new (string, JsonObject)[]
                 {
                     ("list_lambdas", new JsonObject()),
                     ("read_lambda", new JsonObject { ["publicKey"] = "private-things" }),
                     ("read_overview", new JsonObject()),
                     ("read_logs", new JsonObject { ["level"] = "trace", ["limit"] = 1000 }),
                     ("deploy_lambda", new JsonObject { ["publicKey"] = "private-things" })
                 })
        {
            var answer = (await ToolAsync(fixture, tool, arguments)).ToJsonString();

            Assert.DoesNotContain(lambda.PrivateKey, answer, tool);
            Assert.DoesNotContain(LambdaFixture.OperatorToken, answer, tool);
        }

        Assert.IsFalse(fixture.Book.Read(0, null, LogLevel.Trace, 50_000).Lines.Any(l => l.Text.Contains(LambdaFixture.OperatorToken, StringComparison.Ordinal)),
                       "no line logs the token");
    }

    [TestMethod]
    public async Task AVersionIsReadAFileAtATime()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("readable");

        await fixture.DeployAsync(lambda.PrivateKey, Live);

        var newest = (await CallAsync(fixture, "read_lambda", new JsonObject { ["publicKey"] = "readable" }))["lambda"]!["newest"]!.GetValue<int>();

        var version = await CallAsync(fixture, "read_version", new JsonObject { ["publicKey"] = "readable", ["version"] = newest });

        Assert.Contains("lambda.cs", version["files"]!.AsArray().Select(f => f!["path"]!.GetValue<string>()).ToList());
        Assert.IsNull(version["files"]![0]!["text"], "named, not read");

        var file = await CallAsync(fixture, "read_version", new JsonObject { ["publicKey"] = "readable", ["version"] = newest, ["file"] = "lambda.cs" });

        Assert.Contains(Live, file["text"]!.GetValue<string>());

        var missing = await ToolAsync(fixture, "read_version", new JsonObject { ["publicKey"] = "readable", ["version"] = newest, ["file"] = "nope.cs" });

        Assert.IsTrue(missing["result"]!["isError"]!.GetValue<bool>());
    }

    [TestMethod]
    public async Task TheOperatorDecidesWhatWaitsToConnect()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("weather");

        using (var asked = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/connectivity", new ConnectivityRequest("api.weather.example for the forecast")))
        {
            Assert.AreEqual(HttpStatusCode.OK, asked.StatusCode, await asked.Content.ReadAsStringAsync());
        }

        var waiting = await CallAsync(fixture, "list_connectivity");

        Assert.AreEqual("api.weather.example for the forecast", waiting["waiting"]![0]!["purpose"]!.GetValue<string>());

        var decided = await CallAsync(fixture, "decide_connectivity", new JsonObject { ["publicKey"] = "weather", ["allow"] = true, ["note"] = "Fine" });

        Assert.AreEqual("Allowed", decided["lambda"]!["connectivity"]!["status"]!.GetValue<string>());
        Assert.AreEqual("weather", (await CallAsync(fixture, "list_connectivity"))["allowed"]![0]!["publicKey"]!.GetValue<string>());
    }

    #endregion

    #region The installation

    [TestMethod]
    public async Task ALimitIsChangedWithoutRepeatingTheOthers()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var before = await CallAsync(fixture, "read_limits");

        var changed = await CallAsync(fixture, "change_limits", new JsonObject
        {
            ["versions"] = 7,
            ["sandbox"] = new JsonObject { ["analyticsDays"] = 3, ["label"] = "Trial" }
        });

        Assert.AreEqual(7, changed["versions"]!.GetValue<int>());
        Assert.AreEqual(3, changed["sandbox"]!["analyticsDays"]!.GetValue<int>());
        Assert.AreEqual("Trial", changed["sandbox"]!["label"]!.GetValue<string>());
        Assert.AreEqual(before["features"]!.GetValue<int>(), changed["features"]!.GetValue<int>(), "what was not named stays");
        Assert.AreEqual(before["sandbox"]!["buildBytes"]!.GetValue<long>(), changed["sandbox"]!["buildBytes"]!.GetValue<long>());

        Assert.Contains("Changed limit limits.versions from", string.Join('\n', Lines(fixture)), "in the words of the panel's API");

        var refused = await ToolAsync(fixture, "change_limits", new JsonObject { ["versions"] = -1 });

        Assert.IsTrue(refused["result"]!["isError"]!.GetValue<bool>());
        Assert.AreEqual(7, (await CallAsync(fixture, "read_limits"))["versions"]!.GetValue<int>(), "nothing was changed");
    }

    [TestMethod]
    public async Task TheSettingsTheGuideAndTheBuildAgentAreTheOperators()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var settings = await CallAsync(fixture, "change_settings", new JsonObject { ["profile"] = "enterprise" });

        Assert.AreEqual("enterprise", settings["profile"]!.GetValue<string>());
        Assert.IsTrue(settings["optimizeDatabases"]!.GetValue<bool>(), "left as it was");

        await CallAsync(fixture, "change_guide_section", new JsonObject { ["title"] = "House style", ["text"] = "Answer in Dutch." });

        var guide = await CallAsync(fixture, "read_guide", new JsonObject { ["whole"] = true });

        Assert.AreEqual("House style", guide["section"]!["title"]!.GetValue<string>());
        Assert.Contains("Answer in Dutch.", guide["guide"]!.GetValue<string>(), "agents read it");

        await CallAsync(fixture, "remove_guide_section");

        Assert.IsNull((await CallAsync(fixture, "read_guide"))["section"]);

        var agent = await CallAsync(fixture, "change_agent_settings", new JsonObject { ["builds"] = false });

        Assert.IsFalse(agent["builds"]!.GetValue<bool>());
        Assert.IsTrue(agent["changes"]!.GetValue<bool>(), "left as it was");

        // without an agent there is no option to decide anything about
        var unoffered = await ToolAsync(fixture, "change_agent_settings", new JsonObject { ["efforts"] = new JsonArray("low"), ["effort"] = "high" });

        Assert.IsTrue(unoffered["result"]!["isError"]!.GetValue<bool>(), "an option there is none of is refused");
        Assert.IsFalse((await CallAsync(fixture, "read_agent_settings"))["agentConfigured"]!.GetValue<bool>());
    }

    [TestMethod]
    public async Task EverythingThePanelShowsIsReadAndBounded()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("counted");

        await fixture.DeployAsync(lambda.PrivateKey, Live);

        for (var i = 0; i < 3; i++)
        {
            using var visit = await fixture.GetAsync("/lambda/counted/");
        }

        var overview = await CallAsync(fixture, "read_overview");

        Assert.AreEqual(1, overview["census"]!["lambdas"]!.GetValue<int>());

        var logs = await CallAsync(fixture, "read_logs", new JsonObject { ["limit"] = 2 });

        Assert.HasCount(2, logs["lines"]!.AsArray());
        Assert.IsGreaterThan(0L, logs["cursor"]!.GetValue<long>());

        var activity = await CallAsync(fixture, "list_activity", new JsonObject { ["limit"] = 1 });

        Assert.AreEqual("counted", activity["lambdas"]![0]!["publicKey"]!.GetValue<string>());

        var telemetry = await CallAsync(fixture, "read_telemetry", new JsonObject { ["minutes"] = 5 });

        Assert.IsNotNull(telemetry["server"]!["engine"]);

        foreach (var tool in new[] { "read_storage", "list_integrations", "read_usage_report", "read_quiet_hours", "read_optimization", "list_callers", "read_settings" })
        {
            await CallAsync(fixture, tool);
        }

        var measured = await CallAsync(fixture, "read_storage", new JsonObject { ["measure"] = true });

        Assert.IsNotNull(measured["measured"]);

        var integrations = await CallAsync(fixture, "list_integrations");

        Assert.AreEqual("database", integrations["integrations"]![0]!["kind"]!.GetValue<string>());

        var outside = await ToolAsync(fixture, "read_logs", new JsonObject { ["limit"] = 5000 });

        Assert.IsTrue(outside["result"]?["isError"]?.GetValue<bool>() == true || outside["error"] != null, "more than an answer carries is refused");
    }

    [TestMethod]
    public async Task TheOperatorsAgentNamesTheInstallation()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var named = await CallAsync(fixture, "customize_installation", new JsonObject { ["name"] = "Acme Apps" });

        Assert.AreEqual("Acme Apps", named["name"]!.GetValue<string>());

        var read = await CallAsync(fixture, "customize_installation");

        Assert.AreEqual("Acme Apps", read["name"]!.GetValue<string>(), "without arguments it reads");

        Assert.Contains("Changed branding name from 'GenHTTP Lambda' to 'Acme Apps' by operator", Lines(fixture));
    }

    #endregion

    #region Helpers

    private static JsonObject List() => new() { ["jsonrpc"] = "2.0", ["id"] = 1, ["method"] = "tools/list", ["params"] = new JsonObject() };

    private static async Task<IReadOnlyList<string>> ToolNamesAsync(LambdaFixture fixture)
        => [.. (await RpcAsync(fixture, List()))["result"]!["tools"]!.AsArray().Select(t => t!["name"]!.GetValue<string>())];

    /// <summary>
    /// What a tool answered, as structured content, failing where it refused.
    /// </summary>
    private static async Task<JsonNode> CallAsync(LambdaFixture fixture, string tool, JsonObject? arguments = null)
    {
        var answer = await ToolAsync(fixture, tool, arguments ?? new JsonObject());

        var result = answer["result"] ?? throw new AssertFailedException($"{tool}: {answer.ToJsonString()}");

        Assert.IsFalse(result["isError"]?.GetValue<bool>() ?? false, $"{tool}: {answer.ToJsonString()}");

        return result["structuredContent"]!;
    }

    private static Task<JsonObject> ToolAsync(LambdaFixture fixture, string tool, JsonObject arguments)
        => RpcAsync(fixture, new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments }
        });

    private static async Task<JsonObject> RpcAsync(LambdaFixture fixture, JsonObject message)
    {
        using var response = await PostAsync(fixture, message);

        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);

        return (JsonObject)JsonNode.Parse(body)!;
    }

    private static Task<HttpResponseMessage> PostAsync(LambdaFixture fixture, JsonObject message, string? token = LambdaFixture.OperatorToken, string? host = null)
        => SendAsync(fixture, HttpMethod.Post, message, token, host);

    private static async Task<HttpResponseMessage> SendAsync(LambdaFixture fixture, HttpMethod method, JsonObject? message, string? token = LambdaFixture.OperatorToken,
                                                             string? host = null)
    {
        using var request = fixture.Host.GetRequest("/admin/mcp", method);

        request.Headers.Add("Accept", "application/json, text/event-stream");

        if (token != null)
        {
            request.Headers.Add("X-Admin-Token", token);
        }

        if (host != null)
        {
            request.Headers.Host = host;
        }

        if (message != null)
        {
            request.Content = new StringContent(message.ToJsonString(), Encoding.UTF8, "application/json");
        }

        return await fixture.Host.GetResponseAsync(request);
    }

    private static IReadOnlyList<string> Lines(LambdaFixture fixture)
        => [.. fixture.Book.Read(0, null, LogLevel.Information, 50_000).Lines.Select(l => l.Text)];

    #endregion

}
