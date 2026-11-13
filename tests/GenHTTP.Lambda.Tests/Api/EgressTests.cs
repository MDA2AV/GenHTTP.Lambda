using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// Whether a lambda may connect to other servers: refused unless its tier
/// includes it or the operator allowed what its owner asked for.
/// </summary>
[TestClass]
public sealed class EgressTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Code that makes a client and never calls out with it: the tests have
    /// no network to call, and compiling is what is decided here.
    /// </summary>
    private const string Connecting = "var http = new System.Net.Http.HttpClient();\nreturn Inline.Create().Get(() => $\"connects {http.Timeout.TotalSeconds}\");";

    private const string Quiet = "return Inline.Create().Get(() => \"quiet\");";

    #region What is refused

    [TestMethod]
    [DataRow("var http = new System.Net.Http.HttpClient();\nreturn Inline.Create();", "a client named in full")]
    [DataRow("using System.Net.Http;\n\nreturn Inline.Create();", "the namespace")]
    [DataRow("System.Net.Http.HttpClient http = new();\nreturn Inline.Create();", "a target-typed new")]
    [DataRow("using System.Net.Sockets;\n\nvar client = new TcpClient();\nreturn Inline.Create();", "a socket")]
    [DataRow("var addresses = System.Net.Dns.GetHostAddresses(\"example.com\");\nreturn Inline.Create();", "a name looked up")]
    [DataRow("var socket = new System.Net.WebSockets.ClientWebSocket();\nreturn Inline.Create();", "a WebSocket client")]
    [DataRow("var mail = new System.Net.Mail.SmtpClient(\"smtp.example.com\");\nreturn Inline.Create();", "a mail client")]
    [DataRow("var request = System.Net.WebRequest.Create(\"https://example.com/\");\nreturn Inline.Create();", "a request of old")]
    [DataRow("var page = System.Xml.Linq.XDocument.Load(\"https://example.com/feed.xml\");\nreturn Inline.Create();", "XML read from an address")]
    [DataRow("var reader = new System.Xml.XmlTextReader(\"https://example.com/feed.xml\");\nreturn Inline.Create();", "an XML reader made with an address")]
    [DataRow("var pipe = new System.IO.Pipes.NamedPipeClientStream(\".\", \"/var/run/docker.sock\");\nreturn Inline.Create();", "a socket of the machine")]
    [DataRow("return Inline.Create().Add(BearerAuthentication.Create().Issuer(\"https://login.example.com/\"));", "the keys of a token's issuer")]
    public async Task CodeThatConnectsIsRefusedWhereTheLambdaMayNot(string code, string how)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync(code);

        Assert.IsFalse(outcome.Success, how);

        var refusal = outcome.Diagnostics.FirstOrDefault(d => d.Id == "LAMBDA0003");

        Assert.IsNotNull(refusal, $"{how}: refused for want of egress, not for a type that is not there - {string.Join("; ", outcome.Diagnostics.Select(d => $"{d.Id} {d.Message}"))}");
        Assert.Contains("request_egress", refusal.Message, $"{how}: says how to ask for it");
        Assert.Contains("from the lambda's page", refusal.Message, $"{how}: and what to do instead");
    }

    [TestMethod]
    public async Task WhatSharesANameWithWhatConnectsIsLeftAlone()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // an address family, a mail address, XML from a string, encoding for
        // a page - and names of the lambda's own - connect to nothing
        var outcome = await fixture.Deployments.ValidateAsync("""
            using System.Net.Sockets;

            var mail = new System.Net.Mail.MailAddress("someone@example.com");
            var feed = System.Xml.Linq.XDocument.Parse("<feed/>");
            var safe = System.Net.WebUtility.HtmlEncode("<b>");

            return Inline.Create().Get((IRequest request) => new Ping(request.Client.Address?.AddressFamily == AddressFamily.InterNetworkV6, mail.Host, feed.Root!.Name.LocalName, safe));

            record Ping(bool Six, string Host, string Root, string Safe);
            """);

        Assert.IsTrue(outcome.Success, string.Join("; ", outcome.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    public async Task ALambdaThatMayConnectCompilesWhatDoes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync("""
            using System.Net.Http;
            using System.Net.Sockets;

            var http = new HttpClient();
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var feed = System.Xml.Linq.XDocument.Load("https://example.com/feed.xml");

            return Content.From(Resource.FromString("networking is available"));
            """, settings: new BuildSettings(WorkspaceLimits.Standard, Egress: true));

        Assert.IsTrue(outcome.Success, string.Join("; ", outcome.Diagnostics.Select(d => d.Message)));
    }

    #endregion

    #region Asking, and deciding

    [TestMethod]
    public async Task AnOwnerAsksAndTheOperatorAllowsIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("weather");

        Assert.IsFalse(lambda.Egress!.Allowed, "a new lambda may not connect");
        Assert.IsNull(lambda.Egress.Status);

        // what connects is saved, and refused as it goes online
        var refused = await DeployAsync(fixture, lambda.PrivateKey, Connecting);

        Assert.IsFalse(refused.Success);
        Assert.IsTrue(refused.Diagnostics.Any(d => d.Id == "LAMBDA0003"));

        var asked = await AskAsync(fixture, lambda.PrivateKey, "  api.open-meteo.com, for the forecast on the front page  ");

        Assert.AreEqual("Requested", asked.Status);
        Assert.AreEqual("api.open-meteo.com, for the forecast on the front page", asked.Purpose);
        Assert.IsFalse(asked.Allowed, "asking is not being allowed");

        var waiting = await ListAsync(fixture);

        var listed = waiting.Requested.Single(e => e.PublicKey == "weather");

        Assert.AreEqual("api.open-meteo.com, for the forecast on the front page", listed.Egress.Purpose, "the operator reads what it is for");
        Assert.IsTrue(listed.Connects, "and that its newest version connects");

        var detail = await DecideAsync(fixture, "weather", new EgressDecision(true, "Fine for the forecast"));

        Assert.IsTrue(detail.Lambda.Egress!.Allowed);
        Assert.AreEqual("Allowed", detail.Lambda.Egress.Status);

        var owner = await OwnerSeesAsync(fixture, lambda.PrivateKey);

        Assert.IsTrue(owner.Egress!.Allowed);
        Assert.AreEqual("Fine for the forecast", owner.Egress.Note, "the owner reads what the operator said");

        Assert.IsTrue((await DeployAsync(fixture, lambda.PrivateKey, null)).Success, "what was refused goes online now");

        Assert.AreEqual("connects 100", await ServedAsync(fixture, "http://weather.localhost/"));

        var listing = await ListAsync(fixture);

        Assert.IsFalse(listing.Requested.Any(e => e.PublicKey == "weather"));
        Assert.IsTrue(listing.Allowed.Any(e => e.PublicKey == "weather"), "what was allowed stays listed, to be looked at again");
    }

    [TestMethod]
    public async Task TakingItBackStopsTheCodeThatConnectsWithoutADeploy()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("relay");

        await AskAsync(fixture, lambda.PrivateKey, "api.example.com, for the prices");
        await DecideAsync(fixture, "relay", new EgressDecision(true));

        await fixture.DeployAsync(lambda.PrivateKey, Connecting);

        Assert.AreEqual("connects 100", await ServedAsync(fixture, "http://relay.localhost/"));

        await DecideAsync(fixture, "relay", new EgressDecision(false, "It fetches whatever address it is given"));

        using (var stopped = await fixture.GetAsync("http://relay.localhost/"))
        {
            Assert.AreNotEqual(HttpStatusCode.OK, stopped.StatusCode, "built again without egress, it no longer compiles");
        }

        var owner = await OwnerSeesAsync(fixture, lambda.PrivateKey);

        Assert.IsFalse(owner.Egress!.Allowed);
        Assert.AreEqual("Declined", owner.Egress.Status);
        Assert.AreEqual("It fetches whatever address it is given", owner.Egress.Note);

        // and allowed again, it is built again with it - still without a deploy
        await DecideAsync(fixture, "relay", new EgressDecision(true));

        Assert.AreEqual("connects 100", await ServedAsync(fixture, "http://relay.localhost/"));
    }

    [TestMethod]
    public async Task APremiumLambdaMayConnectWithoutAsking()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("shop");

        await TierAsync(fixture, "shop", "Premium");

        var owner = await OwnerSeesAsync(fixture, lambda.PrivateKey);

        Assert.IsTrue(owner.Egress!.Allowed);
        Assert.IsTrue(owner.Egress.Included, "the tier includes it");

        await fixture.DeployAsync(lambda.PrivateKey, Connecting);

        Assert.AreEqual("connects 100", await ServedAsync(fixture, "http://shop.localhost/"));

        var asked = await AskAsync(fixture, lambda.PrivateKey, "api.stripe.com, for payments");

        Assert.IsNull(asked.Status, "there is nothing to ask for");
        Assert.IsFalse((await ListAsync(fixture)).Requested.Any(e => e.PublicKey == "shop"));

        // out of the tier, it is built again without it
        await TierAsync(fixture, "shop", "Free");

        using var stopped = await fixture.GetAsync("http://shop.localhost/");

        Assert.AreNotEqual(HttpStatusCode.OK, stopped.StatusCode);
    }

    [TestMethod]
    public async Task APreviewIsBuiltAsTheLambdaIs()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("drafts");

        await fixture.DeployAsync(lambda.PrivateKey, Quiet);

        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Prices"));

        var feature = await created.GetContentAsync<FeatureResponse>();

        var at = $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}";

        using (var saved = await fixture.SendAsync(HttpMethod.Put, $"{at}/files", new FeatureFilesRequest([new LambdaFile(LambdaSource.EntryName, Connecting)])))
        {
            Assert.AreEqual(HttpStatusCode.OK, saved.StatusCode, await saved.Content.ReadAsStringAsync());
        }

        using (var refused = await fixture.SendAsync(HttpMethod.Post, $"{at}/preview/start"))
        {
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, refused.StatusCode, "a preview may connect only where the lambda may");
        }

        await DecideAsync(fixture, "drafts", new EgressDecision(true));

        using var previewed = await fixture.SendAsync(HttpMethod.Post, $"{at}/preview/start");

        Assert.AreEqual(HttpStatusCode.OK, previewed.StatusCode, await previewed.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task ARequestSaysWhatItIsFor()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("vague");

        using var empty = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/egress", new EgressRequest("   "));

        Assert.AreEqual(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.IsNull((await OwnerSeesAsync(fixture, lambda.PrivateKey)).Egress!.Status, "nothing was asked");
    }

    [TestMethod]
    public async Task ADemoAndABlockedLambdaCannotAsk()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var demo = await fixture.CreateLambdaAsync("showroom");

        await fixture.MakeDemoAsync("showroom");

        var blocked = await fixture.CreateLambdaAsync("blocked");

        using (var block = await AsOperatorAsync(fixture, HttpMethod.Post, "/api/v1/admin/lambdas/blocked/block", new BlockRequest("Phishing")))
        {
            Assert.AreEqual(HttpStatusCode.OK, block.StatusCode);
        }

        using (var asked = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{demo.PrivateKey}/egress", new EgressRequest("anything")))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, asked.StatusCode, "a demo is changed by nobody");
        }

        using (var asked = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{blocked.PrivateKey}/egress", new EgressRequest("anything")))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, asked.StatusCode, "nor is a blocked lambda");
        }

        using (var allowed = await AsOperatorAsync(fixture, HttpMethod.Put, "/api/v1/admin/lambdas/blocked/egress", new EgressDecision(true)))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, allowed.StatusCode, "nothing about a blocked lambda changes but its block");
        }

        using (var allowed = await AsOperatorAsync(fixture, HttpMethod.Put, "/api/v1/admin/lambdas/showroom/egress", new EgressDecision(true)))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, allowed.StatusCode, "a demo never connects");
        }
    }

    [TestMethod]
    public async Task TheOwnerMayTakeTheRequestBackAndAskAgainAfterADecline()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("again");

        await AskAsync(fixture, lambda.PrivateKey, "api.example.com");

        using (var withdrawn = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/egress"))
        {
            Assert.AreEqual(HttpStatusCode.OK, withdrawn.StatusCode);
            Assert.IsNull((await withdrawn.GetContentAsync<EgressResponse>()).Status);
        }

        Assert.IsFalse((await ListAsync(fixture)).Requested.Any(e => e.PublicKey == "again"), "the operator has nothing left to decide");

        await AskAsync(fixture, lambda.PrivateKey, "api.example.com");

        await DecideAsync(fixture, "again", new EgressDecision(false, "Say which data, and what for"));

        var anew = await AskAsync(fixture, lambda.PrivateKey, "api.example.com/prices, to show today's prices of the shop");

        Assert.AreEqual("Requested", anew.Status, "asked again, it waits anew");
        Assert.IsNull(anew.Note, "and what was said about the last request does not answer this one");
    }

    [TestMethod]
    public async Task DecliningWhatNobodyAskedForIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        await fixture.CreateLambdaAsync("nothing");

        using var declined = await AsOperatorAsync(fixture, HttpMethod.Put, "/api/v1/admin/lambdas/nothing/egress", new EgressDecision(false));

        Assert.AreEqual(HttpStatusCode.Conflict, declined.StatusCode);
    }

    [TestMethod]
    public async Task TheOverviewSaysTheCodeConnects()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("overview");

        Assert.IsFalse((await SummaryAsync(fixture, lambda.PrivateKey)).Connects);

        // saved, refused going online - and said, so the owner knows what to ask for
        await DeployAsync(fixture, lambda.PrivateKey, Connecting);

        Assert.IsTrue((await SummaryAsync(fixture, lambda.PrivateKey)).Connects);

        // a string or a comment that mentions a client connects to nothing
        await DeployAsync(fixture, lambda.PrivateKey, "// new HttpClient()\nreturn Inline.Create().Get(() => \"HttpClient\");");

        Assert.IsFalse((await SummaryAsync(fixture, lambda.PrivateKey)).Connects);
    }

    #endregion

    #region Agents

    [TestMethod]
    public async Task AnAgentIsToldHowToAskForIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("agent");

        var written = Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "lambda.cs", ["code"] = Connecting }),
            ["deploy"] = true
        }));

        Assert.Contains("request_egress", written.ToJsonString(), "the refusal says how to ask");

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey }));

        Assert.IsFalse(read["egress"]!["allowed"]!.GetValue<bool>());
        Assert.Contains("request_egress", read["egressNote"]!.GetValue<string>(), "and so does the lambda whose code connects");

        var asked = Structured(await CallToolAsync(fixture, "request_egress", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["purpose"] = "api.open-meteo.com, for the forecast"
        }));

        Assert.AreEqual("Requested", asked["egress"]!["status"]!.GetValue<string>());
        Assert.Contains("tell the user", asked["next"]!.GetValue<string>(), "an agent tells its user rather than waits");

        var where = Structured(await CallToolAsync(fixture, "request_egress", new JsonObject { ["privateKey"] = lambda.PrivateKey }));

        Assert.AreEqual("Requested", where["egress"]!["status"]!.GetValue<string>(), "with only the key, it reads where it stands");

        var guide = Structured(await CallToolAsync(fixture, "platform_guide", new JsonObject()));

        Assert.Contains("request_egress", guide["egress"]!["asking"]!.GetValue<string>());
        Assert.Contains("fetch()", guide["egress"]!["browserFirst"]!.GetValue<string>(), "what a browser may fetch is fetched there");
    }

    #endregion

    #region Helpers

    private static async Task<EgressResponse> AskAsync(LambdaFixture fixture, string privateKey, string purpose)
    {
        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/egress", new EgressRequest(purpose));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<EgressResponse>();
    }

    private static async Task<AdminLambdaDetail> DecideAsync(LambdaFixture fixture, string publicKey, EgressDecision decision)
    {
        using var response = await AsOperatorAsync(fixture, HttpMethod.Put, $"/api/v1/admin/lambdas/{publicKey}/egress", decision);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<AdminLambdaDetail>();
    }

    private static async Task TierAsync(LambdaFixture fixture, string publicKey, string tier)
    {
        using var response = await AsOperatorAsync(fixture, HttpMethod.Put, $"/api/v1/admin/lambdas/{publicKey}/tier", new TierRequest(tier));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private static async Task<AdminEgressListing> ListAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/egress");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AdminEgressListing>();
    }

    private static async Task<LambdaResponse> OwnerSeesAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<LambdaResponse>();
    }

    private static async Task<LambdaSummaryResponse> SummaryAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/summary");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<LambdaSummaryResponse>();
    }

    /// <summary>
    /// Saves the code, when there is any, and tries to put the newest version online.
    /// </summary>
    private static async Task<DeploymentOutcomeResponse> DeployAsync(LambdaFixture fixture, string privateKey, string? code)
    {
        if (code != null)
        {
            using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/versions", LambdaFixture.Version(code));

            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);
        }

        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/deployment/start", new DeploymentRequest(null));

        return await response.GetContentAsync<DeploymentOutcomeResponse>();
    }

    private static async Task<HttpResponseMessage> AsOperatorAsync(LambdaFixture fixture, HttpMethod method, string path, object? body = null)
    {
        using var request = fixture.Host.GetRequest(path, method);

        request.Headers.Add("X-Admin-Token", LambdaFixture.OperatorToken);

        if (body != null)
        {
            request.Content = JsonContent.Create(body, options: Json);
        }

        return await fixture.Host.GetResponseAsync(request);
    }

    private static async Task<string> ServedAsync(LambdaFixture fixture, string address)
    {
        using var served = await fixture.GetAsync(address);

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode, address);

        return await served.GetContentAsync();
    }

    private static JsonObject Structured(JsonObject answer) => (JsonObject)answer["result"]!["structuredContent"]!;

    private static async Task<JsonObject> CallToolAsync(LambdaFixture fixture, string tool, JsonObject arguments)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");

        var message = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments }
        };

        request.Content = new StringContent(message.ToJsonString(), System.Text.Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        return (JsonObject)JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
    }

    #endregion

}
