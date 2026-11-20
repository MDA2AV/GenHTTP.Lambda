using System.Net;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Data.Sqlite;
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
        Assert.AreEqual("http://quiz.localhost:8080/", listing.Entries[0].Address);

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
    public async Task ThePictureIsAFileAndNotARecord()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("framed");

        await PutAsync(fixture, lambda.PrivateKey, new ShowcaseRequest("Title", "Description.", Convert.ToBase64String(Png)));

        var (id, kept, size) = Row(fixture.Options, "framed");

        Assert.AreEqual(0, kept, "no picture is kept in the database");
        Assert.AreEqual(Png.Length, size);

        CollectionAssert.AreEqual(Png, File.ReadAllBytes(PictureFile(fixture.Options, id)));

        await PutAsync(fixture, lambda.PrivateKey, new ShowcaseRequest("Title", "Description.", Convert.ToBase64String(Gif)));

        CollectionAssert.AreEqual(Gif, File.ReadAllBytes(PictureFile(fixture.Options, id)), "a new picture replaces the one before");
        Assert.AreEqual(Gif.Length, Row(fixture.Options, "framed").Size);
    }

    [TestMethod]
    public async Task APictureKeptInTheDatabaseBeforeIsMovedOutAsThePlatformStarts()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        long id;

        await using (var first = await LambdaFixture.CreateAsync(o => o with { DataDirectory = directory }))
        {
            var lambda = await first.CreateLambdaAsync("older");

            await first.DeployAsync(lambda.PrivateKey);

            await PutAsync(first, lambda.PrivateKey, new ShowcaseRequest("Title", "Description.", Convert.ToBase64String(Png)));

            id = Row(first.Options, "older").Id;
        }

        // as an installation from before kept it: the picture in its row, no file
        var options = new LambdaOptions { DataDirectory = directory };

        File.Delete(PictureFile(options, id));

        using (var connection = new SqliteConnection($"Data Source={options.DatabaseFile};Pooling=false"))
        {
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = "UPDATE showcases SET image = $image, image_bytes = 0";
            command.Parameters.AddWithValue("$image", Png);

            command.ExecuteNonQuery();
        }

        await using var second = await LambdaFixture.CreateAsync(o => o with { DataDirectory = directory });

        var (_, kept, size) = Row(second.Options, "older");

        Assert.AreEqual(0, kept, "the row no longer holds it");
        Assert.AreEqual(Png.Length, size, "but says how large it is");

        CollectionAssert.AreEqual(Png, File.ReadAllBytes(PictureFile(second.Options, id)));

        var listing = await ListAsync(second);

        Assert.AreEqual(Png.Length, listing.Entries[0].ImageBytes);

        using var image = await second.GetAsync(listing.Entries[0].ImagePath);

        CollectionAssert.AreEqual(Png, await image.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    public async Task ThePictureGoesWithItsEntryAndWithItsLambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var removed = await fixture.CreateLambdaAsync("removed");
        var deleted = await fixture.CreateLambdaAsync("deleted");

        await PutAsync(fixture, removed.PrivateKey, new ShowcaseRequest("Title", "Description.", Convert.ToBase64String(Png)));
        await PutAsync(fixture, deleted.PrivateKey, new ShowcaseRequest("Title", "Description.", Convert.ToBase64String(Png)));

        var removedFile = PictureFile(fixture.Options, Row(fixture.Options, "removed").Id);
        var deletedFile = PictureFile(fixture.Options, Row(fixture.Options, "deleted").Id);

        using (var response = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{removed.PrivateKey}/showcase"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        using (var response = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{deleted.PrivateKey}"))
        {
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        Assert.IsFalse(File.Exists(removedFile), "taken out of the showcase");
        Assert.IsFalse(File.Exists(deletedFile), "deleted with its lambda");
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

        var telemetry = fixture.Platform.Services.GetRequiredService<LambdaTelemetry>();

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

        var first = await (await fixture.GetAsync("/api/v1/showcases/?take=2")).GetContentAsync<Page<ShowcaseResponse>>();

        Assert.HasCount(2, first.Entries);
        Assert.AreEqual(2, first.Next);

        var second = await (await fixture.GetAsync($"/api/v1/showcases/?skip={first.Next}&take=2")).GetContentAsync<Page<ShowcaseResponse>>();

        Assert.HasCount(1, second.Entries);
        Assert.IsNull(second.Next);
    }

    [TestMethod]
    public async Task AnAgentIsSentToTheApiRatherThanATool()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // as base64 in a tool call the picture was too large for the agents
        // that tried, so there is no tool to try: the guide sends them to the
        // API with the picture in a file
        CollectionAssert.DoesNotContain(await ToolsAsync(fixture), "showcase");

        var showcase = Guide.Section(await Guide.ReadAsync(fixture), "showcase");

        Assert.Contains("api/v1/lambdas/{privateKey}/showcase", showcase, "where to send it");
        Assert.Contains("never goes through a tool call, your output or a command line", showcase, "a picture does not fit into one");
        Assert.Contains("curl -T entry.json", showcase, "it goes from a file");
        Assert.Contains("the user does it in the editor's Showcase section", showcase, "and an agent that cannot make requests says where the user does it");
    }

    [TestMethod]
    public async Task APictureAsLargeAsTheLimitIsTaken()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        var own = await (await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/showcase")).GetContentAsync<OwnShowcaseResponse>();

        // a screenshot that does not compress, as large as it may be
        var picture = new byte[own.Limits.ImageBytes];

        Random.Shared.NextBytes(picture);
        Png.CopyTo(picture, 0);

        var saved = await PutAsync(fixture, lambda.PrivateKey, new ShowcaseRequest("Greeting", "Says hello to whoever opens it.", Convert.ToBase64String(picture)));

        Assert.AreEqual(picture.Length, saved.ImageBytes);

        using var image = await fixture.GetAsync(saved.ImagePath);

        CollectionAssert.AreEqual(picture, await image.Content.ReadAsByteArrayAsync());
    }

    #region Helpers

    /// <summary>
    /// What the platform's records say of the entry of a lambda: its id, how
    /// many bytes of its picture the row holds, how large it says it is.
    /// </summary>
    private static (long Id, long Kept, long Size) Row(LambdaOptions options, string publicKey)
    {
        using var connection = new SqliteConnection($"Data Source={options.DatabaseFile};Pooling=false");

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT s.lambda_id, length(s.image), s.image_bytes FROM showcases s JOIN lambdas l ON l.id = s.lambda_id WHERE l.public_key = $key";
        command.Parameters.AddWithValue("$key", publicKey);

        using var reader = command.ExecuteReader();

        Assert.IsTrue(reader.Read(), $"'{publicKey}' has an entry");

        return (reader.GetInt64(0), reader.IsDBNull(1) ? 0 : reader.GetInt64(1), reader.GetInt64(2));
    }

    private static string PictureFile(LambdaOptions options, long lambdaId)
        => Path.Combine(options.DataDirectory, "showcase", lambdaId.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private static async Task<ShowcaseResponse> PutAsync(LambdaFixture fixture, string privateKey, ShowcaseRequest request)
    {
        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/showcase", request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<ShowcaseResponse>();
    }

    private static async Task<Page<ShowcaseResponse>> ListAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsync("/api/v1/showcases/");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<Page<ShowcaseResponse>>();
    }

    private static async Task<List<string>> ToolsAsync(LambdaFixture fixture)
    {
        using var response = await fixture.SendAsync(HttpMethod.Post, "/mcp", new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/list",
            ["params"] = new JsonObject()
        });

        var answer = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        return [.. answer["result"]!["tools"]!.AsArray().Select(t => t!["name"]!.GetValue<string>())];
    }

    #endregion

}
