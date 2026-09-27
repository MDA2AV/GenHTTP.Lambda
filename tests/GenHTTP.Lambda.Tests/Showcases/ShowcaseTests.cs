using System.Net;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Showcases;

/// <summary>
/// The page where owners present their lambdas, and the entry each of them keeps.
/// </summary>
[TestClass]
public sealed class ShowcaseTests
{

    /// <summary>
    /// The signature of a PNG and a little after it, which is all the sniffing reads.
    /// </summary>
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];

    private static readonly byte[] Gif = "GIF89a-and-then-some"u8.ToArray();

    [TestMethod]
    public async Task AnOnlineLambdaIsListedWithItsPicture()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("quiz");

        await fixture.DeployAsync(lambda.PrivateKey);

        var saved = await PutAsync(fixture, lambda.PrivateKey, new ShowcaseRequest("Pub quiz", "Scores for the Tuesday quiz.", Convert.ToBase64String(Png)));

        Assert.AreEqual("Pub quiz", saved.Title);
        Assert.AreEqual("image/png", saved.ImageType);
        Assert.IsTrue(saved.Online);

        var listing = await ListAsync(fixture);

        Assert.AreEqual(1, listing.Total);
        Assert.AreEqual("quiz", listing.Entries[0].PublicKey);
        Assert.AreEqual("/lambda/quiz/", listing.Entries[0].Path);

        using var image = await fixture.GetAsync(listing.Entries[0].ImagePath);

        Assert.AreEqual(HttpStatusCode.OK, image.StatusCode);
        Assert.AreEqual("image/png", image.Content.Headers.ContentType?.MediaType);
        CollectionAssert.AreEqual(Png, await image.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    public async Task AnOfflineLambdaIsNotListed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await PutAsync(fixture, lambda.PrivateKey, new ShowcaseRequest("Draft", "Not online yet.", Convert.ToBase64String(Png)));

        Assert.AreEqual(0, (await ListAsync(fixture)).Total, "an entry for a link that does not answer is an advertisement for a broken link");

        var own = await (await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/showcase")).GetContentAsync<OwnShowcaseResponse>();

        Assert.IsNotNull(own.Showcase, "the owner still sees the entry");
        Assert.IsFalse(own.Showcase.Online);
    }

    [TestMethod]
    public async Task ALambdaWithoutAnEntryHasNone()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var own = await (await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/showcase")).GetContentAsync<OwnShowcaseResponse>();

        Assert.IsNull(own.Showcase);
        Assert.IsGreaterThan(0, own.Limits.ImageBytes);
    }

    [TestMethod]
    public async Task AChangeKeepsThePictureAndMovesItsAddress()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var first = await PutAsync(fixture, lambda.PrivateKey, new ShowcaseRequest("One", "First words.", Convert.ToBase64String(Gif)));

        var second = await PutAsync(fixture, lambda.PrivateKey, new ShowcaseRequest("Two", "Better words.", null));

        Assert.AreEqual("Two", second.Title);
        Assert.AreEqual("image/gif", second.ImageType, "no new picture keeps the old one");
        Assert.AreNotEqual(first.ImagePath, second.ImagePath, "the picture is cached for good, so a change has to be a new address");
    }

    [TestMethod]
    public async Task ANewEntryNeedsAPicture()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/showcase",
                                                     new ShowcaseRequest("Title", "Description.", null));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task OnlyPicturesAreAccepted()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var script = Convert.ToBase64String("<script>alert(1)</script>"u8.ToArray());

        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/showcase",
                                                     new ShowcaseRequest("Title", "Description.", script));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "the picture is served from this origin, so what it is must not be taken on trust");
    }

    [TestMethod]
    public async Task ATitleHasALimit()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/showcase",
                                                     new ShowcaseRequest(new string('a', 61), "Description.", Convert.ToBase64String(Png)));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task AnUnknownKeyCannotShowcaseAnything()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.SendAsync(HttpMethod.Put, "/api/v1/lambdas/not-a-key/showcase",
                                                     new ShowcaseRequest("Title", "Description.", Convert.ToBase64String(Png)));

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task RemovingTakesItOffThePage()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        await PutAsync(fixture, lambda.PrivateKey, new ShowcaseRequest("Title", "Description.", Convert.ToBase64String(Png)));

        using var removed = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/showcase");

        Assert.IsTrue(removed.IsSuccessStatusCode);
        Assert.AreEqual(0, (await ListAsync(fixture)).Total);
    }

    [TestMethod]
    public async Task ADeletedLambdaTakesItsEntryWithIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("gone");

        await PutAsync(fixture, lambda.PrivateKey, new ShowcaseRequest("Title", "Description.", Convert.ToBase64String(Png)));

        using var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}");

        Assert.IsTrue(deleted.IsSuccessStatusCode);

        using var image = await fixture.GetAsync("/api/v1/showcases/gone/image");

        Assert.AreEqual(HttpStatusCode.NotFound, image.StatusCode);
    }

    [TestMethod]
    public async Task TheBusierLambdaComesFirst()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var quiet = await fixture.CreateLambdaAsync("quiet");
        var busy = await fixture.CreateLambdaAsync("busy");

        foreach (var lambda in (LambdaResponse[])[quiet, busy])
        {
            await fixture.DeployAsync(lambda.PrivateKey);
            await PutAsync(fixture, lambda.PrivateKey, new ShowcaseRequest(lambda.PublicKey, "Description.", Convert.ToBase64String(Png)));
        }

        var telemetry = fixture.Application.Services.GetRequiredService<LambdaTelemetry>();

        var id = await fixture.Meta.GetIdAsync(busy.PrivateKey);

        for (var i = 0; i < 200; i++)
        {
            telemetry.Record(id!.Value, "busy", TimeSpan.FromMilliseconds(1), 200, 10);
        }

        var listing = await ListAsync(fixture);

        Assert.AreEqual("busy", listing.Entries[0].PublicKey);
    }

    [TestMethod]
    public async Task ItIsPaged()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        for (var i = 0; i < 3; i++)
        {
            var lambda = await fixture.CreateLambdaAsync();

            await fixture.DeployAsync(lambda.PrivateKey);
            await PutAsync(fixture, lambda.PrivateKey, new ShowcaseRequest($"Entry {i}", "Description.", Convert.ToBase64String(Png)));
        }

        var first = await (await fixture.GetAsync("/api/v1/showcases/?take=2")).GetContentAsync<ShowcaseListingResponse>();

        Assert.HasCount(2, first.Entries);
        Assert.AreEqual(2, first.Next);

        var second = await (await fixture.GetAsync($"/api/v1/showcases/?skip={first.Next}&take=2")).GetContentAsync<ShowcaseListingResponse>();

        Assert.HasCount(1, second.Entries);
        Assert.IsNull(second.Next);
    }

    [TestMethod]
    public async Task AnAgentCanShowcaseALambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        var added = await ToolAsync(fixture, new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["title"] = "Guestbook",
            ["description"] = "Leave a line for the couple.",
            ["image"] = Convert.ToBase64String(Png)
        });

        Assert.IsTrue(added["showcased"]!.GetValue<bool>());
        Assert.AreEqual(1, (await ListAsync(fixture)).Total);

        var changed = await ToolAsync(fixture, new JsonObject { ["privateKey"] = lambda.PrivateKey, ["title"] = "Wedding guestbook" });

        Assert.AreEqual("Wedding guestbook", changed["title"]!.GetValue<string>());
        Assert.AreEqual("Leave a line for the couple.", changed["description"]!.GetValue<string>(), "what is not named stays as it is");

        var removed = await ToolAsync(fixture, new JsonObject { ["privateKey"] = lambda.PrivateKey, ["remove"] = true });

        Assert.IsFalse(removed["showcased"]!.GetValue<bool>());
        Assert.AreEqual(0, (await ListAsync(fixture)).Total);
    }

    #region Helpers

    private static async Task<ShowcaseResponse> PutAsync(LambdaFixture fixture, string privateKey, ShowcaseRequest request)
    {
        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/showcase", request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<ShowcaseResponse>();
    }

    private static async Task<ShowcaseListingResponse> ListAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsync("/api/v1/showcases/");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<ShowcaseListingResponse>();
    }

    private static async Task<JsonObject> ToolAsync(LambdaFixture fixture, JsonObject arguments)
    {
        using var response = await fixture.SendAsync(HttpMethod.Post, "/mcp", new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = "showcase", ["arguments"] = arguments }
        });

        var answer = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        return (JsonObject)answer["result"]!["structuredContent"]!;
    }

    #endregion

}
