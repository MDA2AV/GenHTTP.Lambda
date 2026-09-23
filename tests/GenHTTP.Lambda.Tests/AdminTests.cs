using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests;

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

    #region Helpers

    private static async Task<AdminListingResponse> ListAsync(LambdaFixture fixture)
    {
        using var response = await Send(fixture, HttpMethod.Get, "/api/v1/admin/lambdas", Token);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AdminListingResponse>();
    }

    private static async Task<HttpResponseMessage> Send(LambdaFixture fixture, HttpMethod method, string path, string token)
    {
        using var request = fixture.Host.GetRequest(path, method);

        request.Headers.Add("X-Admin-Token", token);

        return await fixture.Host.GetResponseAsync(request);
    }

    #endregion

}
