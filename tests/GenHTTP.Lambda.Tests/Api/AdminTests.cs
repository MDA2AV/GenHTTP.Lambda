using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// The panel that sees every lambda, and what it takes to reach it.
/// </summary>
[TestClass]
public sealed class AdminTests
{
    private const string Token = "a-token-nobody-would-guess";

    private static Func<LambdaOptions, LambdaOptions> WithPanel => o => o with { AdminToken = Token };

    [TestMethod]
    public async Task WithoutATokenThereIsNoPanel()
    {
        // nothing configured: the route answers as though it does not exist
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.SendAsync(HttpMethod.Get, "/api/v1/admin/lambdas");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task TheWrongTokenIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var response = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas", "not-the-token");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task NoTokenAtAllIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var response = await fixture.SendAsync(HttpMethod.Get, "/api/v1/admin/lambdas");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task TheOperatorIsAnsweredWhenTheApiIsCompiled()
    {
        // the tests run the API by reflection to save time, but an
        // installation compiles it, and the code generated for a method can
        // fail where reflection does not
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel, compiled: true);

        foreach (var path in new[] { "/api/v1/admin/lambdas?page=1", "/api/v1/admin/lambdas?state=online&uses=database,drafts&view=full&created=7&sort=visited",
                                     "/api/v1/admin/overview", "/api/v1/admin/storage", "/api/v1/telemetry?minutes=5", "/api/v1/telemetry/lambdas",
                                     "/api/v1/logs?level=info", "/api/v1/admin/quiet-hours", "/api/v1/admin/optimization" })
        {
            using var response = await fixture.GetAsOperatorAsync(path);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, path);
        }
    }

    [TestMethod]
    public async Task EveryLambdaIsListedNewestFirst()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        await fixture.CreateLambdaAsync("older");
        var newer = await fixture.CreateLambdaAsync("newer");

        await fixture.DeployAsync(newer.PrivateKey);

        var listing = await ListAsync(fixture);

        Assert.AreEqual(2, listing.Total);
        Assert.AreEqual(1, listing.Deployed);
        Assert.AreEqual("newer", listing.Lambdas[0].PublicKey);
        Assert.IsNotNull(listing.Lambdas[0].ActiveVersion);
        Assert.IsNull(listing.Lambdas[1].ActiveVersion);
    }

    [TestMethod]
    public async Task TheListingCarriesEditorKeys()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var lambda = await fixture.CreateLambdaAsync("private");

        using var response = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas", Token);

        var body = await response.Content.ReadAsStringAsync();

        // deciding whether something is abusive means reading it, and the
        // editor is also where it can be emptied or corrected rather than only
        // deleted - so the panel hands out the editor link, which is the whole
        // of the credential, and the token is what stands in front of that
        Assert.Contains(lambda.PrivateKey, body);
    }

    [TestMethod]
    public async Task NobodyWithoutTheTokenSeesAnEditorKey()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var lambda = await fixture.CreateLambdaAsync("private");

        using var response = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas", "not-the-token");

        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain(lambda.PrivateKey, body, "which is the only thing keeping the listing from being a giveaway");
    }

    [TestMethod]
    public async Task TheCodeCanBeRead()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var lambda = await fixture.CreateLambdaAsync("readable");

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(() => \"the source\");");

        // the template is version one, so what was deployed is the second
        using var response = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas/readable/versions/2", Token);

        var content = await response.GetContentAsync<VersionContentResponse>();

        Assert.Contains("the source", content.Files[0].Code);
    }

    [TestMethod]
    public async Task ALambdaCanBeTakenOffline()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var lambda = await fixture.CreateLambdaAsync("noisy");

        await fixture.DeployAsync(lambda.PrivateKey);

        using var served = await fixture.GetAsync("http://noisy.localhost/");

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode);

        using var _ = await Send(fixture, HttpMethod.Post, "/api/v1/admin/lambdas/noisy/deployment/stop", Token);

        using var after = await fixture.GetAsync("http://noisy.localhost/");

        Assert.AreNotEqual(HttpStatusCode.OK, after.StatusCode);

        // taken offline, not taken away
        Assert.AreEqual(1, (await ListAsync(fixture)).Total);
    }

    [TestMethod]
    public async Task ALambdaCanBeRemoved()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var lambda = await fixture.CreateLambdaAsync("doomed");

        await fixture.DeployAsync(lambda.PrivateKey);

        using var _ = await Send(fixture, HttpMethod.Delete, "/api/v1/admin/lambdas/doomed", Token);

        Assert.AreEqual(0, (await ListAsync(fixture)).Total);

        using var gone = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}");

        Assert.AreEqual(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [TestMethod]
    public async Task ALambdaCanBeMovedToAnotherTier()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        await fixture.CreateLambdaAsync("promoted");

        using var response = await Send(fixture, HttpMethod.Put, "/api/v1/admin/lambdas/promoted/tier", Token, new TierRequest("premium"));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.GetContentAsync<AdminLambdaDetail>();

        Assert.AreEqual("Production", detail.Lambda.Tier, "premium, what production was called before, is read as it still");
        Assert.IsNull(detail.Lambda.KeptUntil, "the tier keeps it");
        CollectionAssert.AreEqual(new[] { "Sandbox", "Production", "System" }, detail.Tiers.ToArray());
    }

    [TestMethod]
    public async Task ATierThatDoesNotExistIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        await fixture.CreateLambdaAsync("hopeful");

        using var response = await Send(fixture, HttpMethod.Put, "/api/v1/admin/lambdas/hopeful/tier", Token, new TierRequest("platinum"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task TheListingCanBeNarrowedToATierAndSearchedByDomain()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        await fixture.CreateLambdaAsync("plain");

        var premium = await fixture.CreateLambdaAsync("fancy");

        using (await Send(fixture, HttpMethod.Put, "/api/v1/admin/lambdas/fancy/tier", Token, new TierRequest("Premium"))) { }
        using (await Send(fixture, HttpMethod.Put, "/api/v1/admin/lambdas/fancy/domain", Token, new DomainChangeRequest("fancy.example.org"))) { }

        using var byTier = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas?tier=Premium", Token);

        var listed = await byTier.GetContentAsync<AdminListingResponse>();

        Assert.AreEqual(1, listed.Matched);
        Assert.AreEqual("fancy.example.org", listed.Lambdas[0].Domain);
        Assert.IsTrue(listed.Lambdas[0].DomainServed);

        using var byDomain = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas?search=example.org", Token);

        Assert.AreEqual(premium.PublicKey, (await byDomain.GetContentAsync<AdminListingResponse>()).Lambdas.Single().PublicKey);
    }

    [TestMethod]
    public async Task TheListingCanBeNarrowedToWhatTheLambdasUseAndTheirState()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var online = await fixture.CreateLambdaAsync("online");
        var keeping = await fixture.CreateLambdaAsync("keeping");
        await fixture.CreateLambdaAsync("plain");

        await fixture.DeployAsync(online.PrivateKey);

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{keeping.PrivateKey}/data/database")) { }
        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{keeping.PrivateKey}/data/secrets")) { }

        async Task<IReadOnlyList<string>> Listed(string query)
        {
            using var response = await Send(fixture, HttpMethod.Get, $"/api/v1/admin/lambdas?{query}", Token);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, query);

            return [.. (await response.GetContentAsync<AdminListingResponse>()).Lambdas.Select(l => l.PublicKey)];
        }

        CollectionAssert.AreEqual(new[] { "online" }, (await Listed("state=online")).ToArray());
        CollectionAssert.AreEquivalent(new[] { "keeping", "plain" }, (await Listed("state=offline")).ToArray());
        CollectionAssert.AreEqual(new[] { "keeping" }, (await Listed("uses=database")).ToArray());
        CollectionAssert.AreEqual(new[] { "keeping" }, (await Listed("uses=Database,secrets")).ToArray(), "every feature named, in any case");
        Assert.IsEmpty(await Listed("uses=database&state=online"), "each filter narrows what the others left");
        Assert.IsEmpty(await Listed("state=expiring"), "nothing new expires within the week");
        Assert.HasCount(3, await Listed("created=1&view=full"));
        CollectionAssert.AreEqual(new[] { "keeping", "online", "plain" }, (await Listed("sort=key")).ToArray());
        CollectionAssert.AreEqual(new[] { "online", "keeping", "plain" }, (await Listed("sort=oldest")).ToArray());
    }

    [TestMethod]
    public async Task TheListingSaysWhatEachLambdaUses()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var keeping = await fixture.CreateLambdaAsync("keeping");
        var plain = await fixture.CreateLambdaAsync("plain");

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{keeping.PrivateKey}/data/secrets")) { }
        using (var off = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{plain.PrivateKey}/data/workspace")) { }

        var listing = await ListAsync(fixture);

        var uses = listing.Lambdas.Single(l => l.PublicKey == "keeping").Uses!;

        Assert.IsTrue(uses.Secrets);
        Assert.IsFalse(uses.Database);
        Assert.IsTrue(uses.Workspace, "the workspace comes switched on");
        Assert.IsNull(uses.AnalyticsSince);
        Assert.AreEqual("Full", uses.View);

        Assert.IsFalse(listing.Lambdas.Single(l => l.PublicKey == "plain").Uses!.Workspace, "until it is switched off");

        using var withWorkspace = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas?uses=workspace", Token);

        Assert.AreEqual("keeping", (await withWorkspace.GetContentAsync<AdminListingResponse>()).Lambdas.Single().PublicKey);
    }

    [TestMethod]
    public async Task AFilterThatDoesNotExistIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        foreach (var query in new[] { "state=sleeping", "uses=database,magic", "sort=price", "view=2" })
        {
            using var response = await Send(fixture, HttpMethod.Get, $"/api/v1/admin/lambdas?{query}", Token);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, query);
        }
    }

    [TestMethod]
    public async Task OneLambdaCanBeLookedAtInFull()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var lambda = await fixture.CreateLambdaAsync("detailed");

        await fixture.DeployAsync(lambda.PrivateKey);

        using (await fixture.GetAsync("http://detailed.localhost/")) { }

        using var response = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas/detailed", Token);

        var detail = await response.GetContentAsync<AdminLambdaDetail>();

        Assert.AreEqual(lambda.PrivateKey, detail.Lambda.PrivateKey);
        Assert.AreEqual(1, detail.Versions.Count);
        Assert.AreEqual(1, detail.Activations.Count);
        Assert.AreEqual(1, detail.Traffic.Totals?.Requests);
    }

    [TestMethod]
    public async Task TheDetailIsBehindTheTokenToo()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        await fixture.CreateLambdaAsync("hidden");

        using var response = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas/hidden", "not-the-token");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ALambdaCanBePutOnlineByTheOperator()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        await fixture.CreateLambdaAsync("revived");

        using var response = await Send(fixture, HttpMethod.Post, "/api/v1/admin/lambdas/revived/deployment/start", Token, new DeploymentRequest(null));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        using var served = await fixture.GetAsync("http://revived.localhost/");

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode);

        using var detail = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas/revived", Token);

        Assert.AreEqual("admin", (await detail.GetContentAsync<AdminLambdaDetail>()).Activations[0].Origin);
    }

    [TestMethod]
    public async Task ThereIsNoPageOfPricesToLinkTo()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var page = await fixture.GetAsync("/en/enterprise", "text/html");

        Assert.AreEqual(HttpStatusCode.NotFound, page.StatusCode, "the page of the enterprise edition's prices went with its switch");

        using var features = await fixture.GetAsync("/api/v1/system/features");

        Assert.AreEqual(HttpStatusCode.NotFound, features.StatusCode, "and so did what the header asked about it");
    }

    [TestMethod]
    public async Task TheSettingsNeedTheToken()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var response = await Send(fixture, HttpMethod.Put, "/api/v1/admin/settings", "not-the-token", new SettingsModel(BuildBox: false));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);

        using var settings = await Send(fixture, HttpMethod.Get, "/api/v1/admin/settings", Token);

        Assert.IsTrue((await settings.GetContentAsync<SettingsModel>()).BuildBox, "nothing was changed");
    }

    #region Helpers

    private static async Task<AdminListingResponse> ListAsync(LambdaFixture fixture)
    {
        using var response = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas", Token);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AdminListingResponse>();
    }

    private static async Task<HttpResponseMessage> Send(LambdaFixture fixture, HttpMethod method, string path, string token, object? payload = null)
    {
        using var request = fixture.Host.GetRequest(path, method);

        request.Headers.Add("X-Admin-Token", token);

        if (payload != null)
        {
            request.Content = System.Net.Http.Json.JsonContent.Create(payload, options: new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        }

        return await fixture.Host.GetResponseAsync(request);
    }

    #endregion

}
