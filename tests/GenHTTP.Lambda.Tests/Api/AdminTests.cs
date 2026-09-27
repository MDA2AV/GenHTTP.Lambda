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

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
                        "and it looks the same as having no panel at all");
    }

    [TestMethod]
    public async Task NoTokenAtAllIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var response = await fixture.SendAsync(HttpMethod.Get, "/api/v1/admin/lambdas");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
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

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
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

        using var served = await fixture.GetAsync("/lambda/noisy/");

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode);

        using var _ = await Send(fixture, HttpMethod.Post, "/api/v1/admin/lambdas/noisy/deployment/stop", Token);

        using var after = await fixture.GetAsync("/lambda/noisy/");

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

        Assert.AreEqual("Premium", detail.Lambda.Tier);
        Assert.IsNull(detail.Lambda.KeptUntil, "the tier keeps it");
        CollectionAssert.AreEqual(new[] { "Free", "Premium" }, detail.Tiers.ToArray());
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
    public async Task OneLambdaCanBeLookedAtInFull()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var lambda = await fixture.CreateLambdaAsync("detailed");

        await fixture.DeployAsync(lambda.PrivateKey);

        using (await fixture.GetAsync("/lambda/detailed/")) { }

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

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ALambdaCanBePutOnlineByTheOperator()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        await fixture.CreateLambdaAsync("revived");

        using var response = await Send(fixture, HttpMethod.Post, "/api/v1/admin/lambdas/revived/deployment/start", Token, new DeploymentRequest(null));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        using var served = await fixture.GetAsync("/lambda/revived/");

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode);

        using var detail = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas/revived", Token);

        Assert.AreEqual("admin", (await detail.GetContentAsync<AdminLambdaDetail>()).Activations[0].Origin);
    }

    [TestMethod]
    public async Task TheEnterprisePageIsLinkedByDefault()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var response = await fixture.GetAsync("/api/v1/system/features");

        Assert.IsTrue((await response.GetContentAsync<FeaturesResponse>()).Enterprise);
    }

    [TestMethod]
    public async Task TheEnterprisePageCanBeUnlinked()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var change = await Send(fixture, HttpMethod.Put, "/api/v1/admin/settings", Token, new SettingsModel(false));

        Assert.AreEqual(HttpStatusCode.OK, change.StatusCode);
        Assert.IsFalse((await change.GetContentAsync<SettingsModel>()).EnterprisePage);

        using var features = await fixture.GetAsync("/api/v1/system/features");

        Assert.IsFalse((await features.GetContentAsync<FeaturesResponse>()).Enterprise);

        using var settings = await Send(fixture, HttpMethod.Get, "/api/v1/admin/settings", Token);

        Assert.IsFalse((await settings.GetContentAsync<SettingsModel>()).EnterprisePage);

        // unlinked, not removed
        using var page = await fixture.GetAsync("/enterprise", "text/html");

        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode);
    }

    [TestMethod]
    public async Task TheSettingsNeedTheToken()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var response = await Send(fixture, HttpMethod.Put, "/api/v1/admin/settings", "not-the-token", new SettingsModel(false));

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        using var features = await fixture.GetAsync("/api/v1/system/features");

        Assert.IsTrue((await features.GetContentAsync<FeaturesResponse>()).Enterprise);
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
