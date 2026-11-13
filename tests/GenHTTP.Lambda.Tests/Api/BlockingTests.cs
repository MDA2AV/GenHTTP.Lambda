using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Blocking;
using GenHTTP.Lambda.Tests.Git;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// What the operator does about abuse: block a lambda, and the addresses it
/// was changed from.
/// </summary>
[TestClass]
public sealed class BlockingTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string Live = "return Inline.Create().Get(() => \"live\");";

    /// <summary>
    /// The fixture's requests come from loopback, which is one of the
    /// installation's own networks and never recorded or blocked - so the
    /// tests about addresses take it off the list.
    /// </summary>
    private static Func<LambdaOptions, LambdaOptions> Reachable => o => LambdaFixture.WithPanel(o) with { UnblockableNetworks = [] };

    #region Blocking a lambda

    [TestMethod]
    public async Task ABlockedLambdaGoesOfflineAndRefusesEveryChange()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("phish");

        await fixture.DeployAsync(lambda.PrivateKey, Live);

        Assert.AreEqual("live", await ServedAsync(fixture, "http://phish.localhost/"));

        var detail = await BlockAsync(fixture, "phish", new BlockRequest("Asks for bank passwords"));

        Assert.IsNull(detail.Lambda.ActiveVersion, "blocking takes it offline");
        Assert.IsNotNull(detail.Lambda.Blocked);
        Assert.AreEqual("Asks for bank passwords", detail.Block?.Reason);

        using (var served = await fixture.GetAsync("http://phish.localhost/"))
        {
            Assert.AreNotEqual(HttpStatusCode.OK, served.StatusCode, "nobody is served what it was blocked for");
        }

        var at = $"/api/v1/lambdas/{lambda.PrivateKey}";

        // every door its owner has, each refused the same way
        var attempts = new (HttpMethod Method, string Path, object? Body)[]
        {
            (HttpMethod.Post, $"{at}/versions", LambdaFixture.Version(Live)),
            (HttpMethod.Post, $"{at}/deployment/start", new DeploymentRequest(null)),
            (HttpMethod.Post, $"{at}/features", new CreateFeatureRequest("Again")),
            (HttpMethod.Put, $"{at}/data/database", null),
            (HttpMethod.Put, $"{at}/secrets/TOKEN", new SecretRequest("value")),
            (HttpMethod.Put, $"{at}/files/page.html", new FileRequest("PGgxPkhpPC9oMT4=")),
            (HttpMethod.Put, $"{at}/source", new SourceRequest("MIT", null)),
            (HttpMethod.Put, $"{at}/analytics", null),
            (HttpMethod.Delete, at, null)
        };

        foreach (var (method, path, body) in attempts)
        {
            using var refused = await fixture.SendAsync(method, path, body);

            Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode, $"{method} {path}");

            var error = await refused.GetContentAsync<ErrorResponse>();

            Assert.AreEqual("Blocked", error.Error, $"{method} {path}");
            Assert.Contains("blocked by the operator", error.Message, $"{method} {path}");
        }

        // what it has can still be read, and taken away
        using var read = await fixture.GetAsync(at);

        Assert.AreEqual(HttpStatusCode.OK, read.StatusCode);
        Assert.IsNotNull((await read.GetContentAsync<LambdaResponse>()).Blocked);

        using var export = await fixture.GetAsync($"{at}/export");

        Assert.AreEqual(HttpStatusCode.OK, export.StatusCode);
    }

    [TestMethod]
    public async Task AnAgentIsToldTheLambdaIsBlocked()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("phish");

        await BlockAsync(fixture, "phish", new BlockRequest());

        var answer = await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "lambda.cs", ["code"] = Live }),
            ["deploy"] = true
        });

        Assert.IsTrue(answer["result"]!["isError"]!.GetValue<bool>());

        var said = answer["result"]!["content"]![0]!["text"]!.GetValue<string>();

        Assert.Contains("blocked by the operator", said);
        Assert.Contains("Trying again will not help", said, "so an agent stops rather than goes on trying");
    }

    [TestMethod]
    public async Task TheOwnerIsToldWhyTheLambdaWasBlocked()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("phish");
        var quiet = await fixture.CreateLambdaAsync("quiet");

        await BlockAsync(fixture, "phish", new BlockRequest("  Asks for bank passwords  "));
        await BlockAsync(fixture, "quiet", new BlockRequest());

        var blocked = await OwnerSeesAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual("Asks for bank passwords", blocked.BlockReason, "the operator's words, as they wrote them");
        Assert.IsNull((await OwnerSeesAsync(fixture, quiet.PrivateKey)).BlockReason, "no reason given, none shown");

        using var lifted = await AsOperatorAsync(fixture, HttpMethod.Delete, "/api/v1/admin/lambdas/phish/block");

        Assert.AreEqual(HttpStatusCode.OK, lifted.StatusCode);
        Assert.IsNull((await OwnerSeesAsync(fixture, lambda.PrivateKey)).BlockReason, "it goes with the block");
    }

    [TestMethod]
    public async Task LiftingTheBlockLetsTheOwnerChangeItAgainButLeavesItOffline()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("mistake");

        await fixture.DeployAsync(lambda.PrivateKey, Live);

        await BlockAsync(fixture, "mistake", new BlockRequest("Looked like phishing"));

        using (var lifted = await AsOperatorAsync(fixture, HttpMethod.Delete, "/api/v1/admin/lambdas/mistake/block"))
        {
            Assert.AreEqual(HttpStatusCode.OK, lifted.StatusCode);

            var detail = await lifted.GetContentAsync<AdminLambdaDetail>();

            Assert.IsNull(detail.Lambda.Blocked);
            Assert.IsNull(detail.Block);
            Assert.IsNull(detail.Lambda.ActiveVersion, "lifting the block does not put it back online");
        }

        await fixture.DeployAsync(lambda.PrivateKey);

        Assert.AreEqual("live", await ServedAsync(fixture, "http://mistake.localhost/"));
    }

    [TestMethod]
    public async Task OnlyTheOperatorCanDeleteABlockedLambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("phish");

        await BlockAsync(fixture, "phish", new BlockRequest());

        using (var owner = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}"))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, owner.StatusCode);
        }

        using (var removed = await AsOperatorAsync(fixture, HttpMethod.Delete, "/api/v1/admin/lambdas/phish"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, removed.StatusCode);
        }

        Assert.IsNull(fixture.Meta.Get(lambda.PrivateKey));
    }

    [TestMethod]
    public async Task ThePreviewsOfItsFeaturesGoOfflineWithIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("phish");

        await fixture.DeployAsync(lambda.PrivateKey, Live);

        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Draft"));

        var feature = await created.GetContentAsync<FeatureResponse>();

        using (var previewed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/preview/start"))
        {
            Assert.AreEqual(HttpStatusCode.OK, previewed.StatusCode, await previewed.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("live", await ServedAsync(fixture, feature.PreviewPath));

        await BlockAsync(fixture, "phish", new BlockRequest());

        using var gone = await fixture.GetAsync(feature.PreviewPath);

        Assert.AreNotEqual(HttpStatusCode.OK, gone.StatusCode, "a draft runs the same code at an address of its own");
    }

    [TestMethod]
    public async Task ABlockedLambdaIsNotRemovedForGoingUnused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("phish");

        var detail = await BlockAsync(fixture, "phish", new BlockRequest());

        Assert.IsNull(detail.Lambda.KeptUntil, "removed by itself, its key would be free again for whoever it was blocked for");

        fixture.Meta.RunMaintenance(DateTime.UtcNow.AddYears(1));

        Assert.IsNotNull(fixture.Meta.Get(lambda.PrivateKey));
    }

    [TestMethod]
    public async Task ADemoCannotBeBlocked()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        await fixture.CreateLambdaAsync("almost-a-demo");

        await fixture.MakeDemoAsync("almost-a-demo");

        using var refused = await AsOperatorAsync(fixture, HttpMethod.Post, "/api/v1/admin/lambdas/almost-a-demo/block", new BlockRequest());

        Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode);
    }

    [TestMethod]
    public async Task APushToABlockedLambdaIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("phish");

        await git.CloneAsync(fixture.EditorUrl(lambda), "phish");

        await BlockAsync(fixture, "phish", new BlockRequest());

        git.Write("phish", "resources/page.html", "<h1>Again</h1>");

        await git.CommitAsync("phish", "Puts it back");

        var pushed = await git.TryAsync("phish", "push");

        Assert.IsFalse(pushed.Success, pushed.ToString());
        Assert.Contains("blocked by the operator", pushed.Said);

        // what it has can still be taken away
        await git.CloneAsync(fixture.EditorUrl(lambda), "again");
    }

    [TestMethod]
    public async Task ThePublishedSourceOfABlockedLambdaIsNotFoundUntilTheBlockIsLifted()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("opened");

        await fixture.PublishAsync(lambda);

        await BlockAsync(fixture, "opened", new BlockRequest());

        using (var hidden = await fixture.GetAsync("/api/v1/sources/opened"))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, hidden.StatusCode);
        }

        using (var _ = await AsOperatorAsync(fixture, HttpMethod.Delete, "/api/v1/admin/lambdas/opened/block")) { }

        using var back = await fixture.GetAsync("/api/v1/sources/opened");

        Assert.AreEqual(HttpStatusCode.OK, back.StatusCode, "the publication was kept");
    }

    #endregion

    #region The addresses it was changed from

    [TestMethod]
    public async Task TheAddressesALambdaWasChangedFromAreListed()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Reachable);

        var lambda = await fixture.CreateLambdaAsync("made-here");

        await fixture.DeployAsync(lambda.PrivateKey, Live);

        var detail = await DetailAsync(fixture, "made-here");

        var editor = detail.EditorAddresses?.SingleOrDefault();

        Assert.IsNotNull(editor, "created, saved and put online from one address");
        Assert.IsFalse(editor.Blocked);
        Assert.AreEqual(detail.You, editor.Range, "the fixture's own address, which the panel marks");

        // the operator's own doings in the panel are not where it came from
        await BlockAsync(fixture, "made-here", new BlockRequest());

        Assert.HasCount(1, (await DetailAsync(fixture, "made-here")).EditorAddresses!);

        // and they go with the lambda
        using var removed = await AsOperatorAsync(fixture, HttpMethod.Delete, "/api/v1/admin/lambdas/made-here");

        Assert.AreEqual(HttpStatusCode.NoContent, removed.StatusCode);
    }

    [TestMethod]
    public async Task NoAddressOfTheInstallationsOwnNetworksIsRecordedOrBlocked()
    {
        // loopback, as the build agent's network would be
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("by-the-agent");

        await fixture.DeployAsync(lambda.PrivateKey, Live);

        var detail = await DetailAsync(fixture, "by-the-agent");

        Assert.IsEmpty(detail.EditorAddresses!);
        Assert.IsNull(detail.You);

        using var refused = await AsOperatorAsync(fixture, HttpMethod.Post, "/api/v1/admin/lambdas/by-the-agent/block", new BlockRequest("Spam", ["10.1.2.3"]));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Contains("never blocked", (await refused.GetContentAsync<ErrorResponse>()).Message);

        Assert.IsNull((await DetailAsync(fixture, "by-the-agent")).Lambda.Blocked, "nothing is blocked when an address is refused");
    }

    #endregion

    #region Blocking addresses

    [TestMethod]
    public async Task ABlockedAddressIsRefusedEverywhereButThePanel()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Reachable);

        var lambda = await fixture.CreateLambdaAsync("phish");

        await fixture.DeployAsync(lambda.PrivateKey, Live);

        var editors = (await DetailAsync(fixture, "phish")).EditorAddresses!.Select(e => e.Address).ToList();

        await BlockAsync(fixture, "phish", new BlockRequest("Phishing", editors));

        foreach (var address in new[] { "/", "/api/v1/demos", "http://phish.localhost/", "/editor/" + lambda.PrivateKey })
        {
            using var refused = await fixture.GetAsync(address);

            Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode, address);
        }

        // the operator who blocked their own address can still get to the panel
        using (var panel = await fixture.GetAsync("/admin/blocks"))
        {
            Assert.AreEqual(HttpStatusCode.OK, panel.StatusCode);
        }

        var listing = await BlocksAsync(fixture);

        var block = listing.Blocks.Single();

        Assert.AreEqual(listing.You, block.Address);
        Assert.AreEqual("Phishing", block.Reason);
        Assert.AreEqual("phish", block.Lambda);
        Assert.IsGreaterThanOrEqualTo(4L, block.Refused);
        Assert.AreEqual(block.Created + TimeSpan.FromDays(14), block.Expires);

        using (var lifted = await AsOperatorAsync(fixture, HttpMethod.Delete, $"/api/v1/admin/blocks/{block.Id}"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, lifted.StatusCode);
        }

        using var again = await fixture.GetAsync("/api/v1/demos");

        Assert.AreEqual(HttpStatusCode.OK, again.StatusCode);
    }

    [TestMethod]
    public async Task TheTokenIsStillAskedForFromABlockedAddress()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Reachable);

        var you = (await BlocksAsync(fixture)).You!;

        Blocks(fixture).Block([you], "Myself, by mistake", null);

        using var anonymous = await fixture.GetAsync("/api/v1/admin/blocks");

        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode, "let through to the panel's door, not past it");
    }

    [TestMethod]
    public async Task AnIPv6AddressIsBlockedWithItsNetwork()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Reachable);

        var blocks = Blocks(fixture);

        var made = blocks.Block(["2001:db8:1:2::5", "203.0.113.7"], "Spam", null);

        CollectionAssert.AreEquivalent(new[] { "2001:db8:1:2::/64", "203.0.113.7" }, made.Select(b => b.Address).ToArray());

        // a device picks another address within its /64 every day or so
        Assert.IsTrue(blocks.Covers(IPAddress.Parse("2001:db8:1:2:abcd::1")));
        Assert.IsFalse(blocks.Covers(IPAddress.Parse("2001:db8:1:3::5")));

        // an IPv4 client on a dual stack socket is the same client
        Assert.IsTrue(blocks.Covers(IPAddress.Parse("::ffff:203.0.113.7")));
        Assert.IsFalse(blocks.Covers(IPAddress.Parse("203.0.113.8")));
    }

    [TestMethod]
    public async Task BlocksAreLetGoOfAfterTwoWeeks()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Reachable);

        var blocks = Blocks(fixture);

        blocks.Block(["203.0.113.7"], "Spam", null);

        Assert.AreEqual(0, blocks.Expire(DateTime.UtcNow.AddDays(13)));
        Assert.HasCount(1, (await BlocksAsync(fixture)).Blocks);

        Assert.AreEqual(1, blocks.Expire(DateTime.UtcNow.AddDays(15)));
        Assert.IsEmpty((await BlocksAsync(fixture)).Blocks);
        Assert.IsFalse(blocks.Covers(IPAddress.Parse("203.0.113.7")));
    }

    [TestMethod]
    public async Task BlockingAnAddressAgainStartsItsTimeAgain()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Reachable);

        var blocks = Blocks(fixture);

        var first = blocks.Block(["203.0.113.7"], "Spam", "one").Single();

        var second = blocks.Block(["203.0.113.7"], "Phishing", "two").Single();

        Assert.AreEqual(first.Id, second.Id);
        Assert.IsGreaterThanOrEqualTo(first.Created, second.Created);
        Assert.AreEqual("Phishing", second.Reason);
        Assert.AreEqual("two", second.PublicKey);
    }

    #endregion

    #region Helpers

    private static IAddressBlocks Blocks(LambdaFixture fixture) => fixture.Platform.Services.GetRequiredService<IAddressBlocks>();

    private static async Task<AdminLambdaDetail> BlockAsync(LambdaFixture fixture, string publicKey, BlockRequest body)
    {
        using var response = await AsOperatorAsync(fixture, HttpMethod.Post, $"/api/v1/admin/lambdas/{publicKey}/block", body);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<AdminLambdaDetail>();
    }

    private static async Task<AdminLambdaDetail> DetailAsync(LambdaFixture fixture, string publicKey)
    {
        using var response = await fixture.GetAsOperatorAsync($"/api/v1/admin/lambdas/{publicKey}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AdminLambdaDetail>();
    }

    private static async Task<LambdaResponse> OwnerSeesAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<LambdaResponse>();
    }

    private static async Task<AddressBlockListing> BlocksAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/blocks");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AddressBlockListing>();
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
