using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Web;

/// <summary>
/// The lambdas the operator listed in the sitemap of the installation: named
/// there by the root of their address while they are online, left out while
/// they are not, and listed by nobody but the operator.
/// </summary>
[TestClass]
public sealed class LambdaSitemapTests
{

    private const string Hello = "return Content.From(Resource.FromString(\"hello\"));";

    private const string Shop = "https://genhttp.dev/lambda/shop/";

    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    private static readonly XNamespace Xhtml = "http://www.w3.org/1999/xhtml";

    /// <summary>
    /// An installation with a public address, so it has a sitemap, and with a
    /// panel, so it has an operator.
    /// </summary>
    private static Func<LambdaOptions, LambdaOptions> Site(string? publicUrl = "https://genhttp.dev")
        => options => LambdaFixture.WithPanel(options) with { PublicUrl = publicUrl };

    [TestMethod]
    public async Task NoLambdaIsInTheSitemapUntilTheOperatorListsIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        var lambda = await fixture.CreateLambdaAsync("shop");

        await fixture.DeployAsync(lambda.PrivateKey, Hello);

        Assert.IsFalse((await SitemapAsync(fixture)).Any(l => l.Contains("/lambda/", StringComparison.Ordinal)), "being online is not enough");

        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/lambdas/shop");

        var detail = await response.GetContentAsync<AdminLambdaDetail>();

        Assert.IsFalse(detail.Sitemap.Listed);
        Assert.AreEqual(Shop, detail.Sitemap.Address, "what it would be listed as");
    }

    [TestMethod]
    public async Task ALambdaTheOperatorListedIsInTheSitemapAtTheRootOfItsAddress()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        var lambda = await fixture.CreateLambdaAsync("shop");

        await fixture.DeployAsync(lambda.PrivateKey, Hello);

        Assert.IsTrue((await ListAsync(fixture, "shop", true)).Sitemap.Listed);

        var sitemap = await ReadSitemapAsync(fixture);

        var entry = sitemap.Descendants(Ns + "url").Single(u => u.Element(Ns + "loc")!.Value == Shop);

        Assert.IsNull(entry.Element(Ns + "lastmod"), "what it serves changes with its data, which the platform cannot date");
        Assert.IsEmpty(entry.Elements(Xhtml + "link").ToList(), "it is in none of the languages of the site");

        Assert.Contains("Set sitemap of lambda shop to True by operator", Logged(fixture));

        Assert.IsFalse((await ListAsync(fixture, "shop", false)).Sitemap.Listed);

        CollectionAssert.DoesNotContain(await SitemapAsync(fixture), Shop, "taken out again");
    }

    [TestMethod]
    public async Task ALambdaIsInTheSitemapOnlyWhileItIsOnline()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        var lambda = await fixture.CreateLambdaAsync("shop");

        await ListAsync(fixture, "shop", true);

        CollectionAssert.DoesNotContain(await SitemapAsync(fixture), Shop, "its address answers with an error while it is offline");

        await fixture.DeployAsync(lambda.PrivateKey, Hello);

        CollectionAssert.Contains(await SitemapAsync(fixture), Shop);

        using var stopped = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/stop");

        Assert.AreEqual(HttpStatusCode.OK, stopped.StatusCode);

        CollectionAssert.DoesNotContain(await SitemapAsync(fixture), Shop);

        await fixture.DeployAsync(lambda.PrivateKey);

        CollectionAssert.Contains(await SitemapAsync(fixture), Shop, "listed is listed until the operator says otherwise");
    }

    [TestMethod]
    public async Task TheSitemapFollowsALambdaToItsNewKey()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        var lambda = await fixture.CreateLambdaAsync("shop");

        await fixture.DeployAsync(lambda.PrivateKey, Hello);

        await ListAsync(fixture, "shop", true);

        using var moved = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}", new UpdateLambdaRequest("store"));

        Assert.AreEqual(HttpStatusCode.OK, moved.StatusCode);

        var listed = await SitemapAsync(fixture);

        CollectionAssert.Contains(listed, "https://genhttp.dev/lambda/store/");
        CollectionAssert.DoesNotContain(listed, Shop);
    }

    [TestMethod]
    public async Task OnlyTheOperatorListsALambdaInTheSitemap()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        var lambda = await fixture.CreateLambdaAsync("shop");

        using var anonymous = await fixture.SendAsync(HttpMethod.Put, "/api/v1/admin/lambdas/shop/sitemap", new SitemapRequest(true));

        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        using var guessed = await SendAsOperatorAsync(fixture, HttpMethod.Put, "/api/v1/admin/lambdas/shop/sitemap", new SitemapRequest(true), "not-the-token");

        Assert.AreEqual(HttpStatusCode.Forbidden, guessed.StatusCode);

        // nor is the owner told about it when they read their lambda
        using var owned = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}");

        Assert.AreEqual(HttpStatusCode.OK, owned.StatusCode);
        Assert.DoesNotContain("sitemap", (await owned.Content.ReadAsStringAsync()).ToLowerInvariant());
    }

    [TestMethod]
    public async Task ADemoIsListedLikeAnyOtherLambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site());

        var lambda = await fixture.CreateLambdaAsync("shop");

        await fixture.DeployAsync(lambda.PrivateKey, Hello);

        await fixture.MakeDemoAsync("shop");

        Assert.IsTrue((await ListAsync(fixture, "shop", true)).Sitemap.Listed, "what changes is the sitemap, not the program");

        CollectionAssert.Contains(await SitemapAsync(fixture), Shop);
    }

    [TestMethod]
    public async Task WithoutAPublicAddressThereIsNoSitemapToListALambdaIn()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Site(publicUrl: null));

        await fixture.CreateLambdaAsync("shop");

        var detail = await ListAsync(fixture, "shop", true);

        Assert.IsTrue(detail.Sitemap.Listed, "kept for when the installation has one");
        Assert.IsNull(detail.Sitemap.Address, "so the operator is told there is nothing to list it in");
    }

    #region Helpers

    private static async Task<AdminLambdaDetail> ListAsync(LambdaFixture fixture, string publicKey, bool listed)
    {
        using var response = await SendAsOperatorAsync(fixture, HttpMethod.Put, $"/api/v1/admin/lambdas/{publicKey}/sitemap", new SitemapRequest(listed));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AdminLambdaDetail>();
    }

    private static async Task<XDocument> ReadSitemapAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsync("/sitemap.xml");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return XDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Every address the sitemap lists.
    /// </summary>
    private static async Task<List<string>> SitemapAsync(LambdaFixture fixture)
        => [.. (await ReadSitemapAsync(fixture)).Descendants(Ns + "loc").Select(l => l.Value)];

    private static async Task<HttpResponseMessage> SendAsOperatorAsync(LambdaFixture fixture, HttpMethod method, string path, object payload,
                                                                       string token = LambdaFixture.OperatorToken)
    {
        using var request = fixture.Host.GetRequest(path, method);

        request.Headers.Add("X-Admin-Token", token);

        request.Content = JsonContent.Create(payload, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return await fixture.Host.GetResponseAsync(request);
    }

    /// <summary>
    /// What the operator's actions logged.
    /// </summary>
    private static List<string> Logged(LambdaFixture fixture)
        => [.. fixture.Book.Read(0, null, LogLevel.Information, 50_000).Lines
                  .Where(l => l.Source.EndsWith("AdminResource", StringComparison.Ordinal))
                  .Select(l => l.Text)];

    #endregion

}
