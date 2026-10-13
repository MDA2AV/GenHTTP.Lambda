using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Git;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// The resources of a lambda kept once each, by what they hold, and named by
/// the versions and features that ship them - rather than carried inside
/// every one of them.
/// </summary>
[TestClass]
public sealed class ResourceStoreTests
{

    [TestMethod]
    public async Task AVersionNamesItsResourcesRatherThanCarryingThem()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("pages");

        var page = Encoding.UTF8.GetBytes("<h1>a page of a site</h1>");

        // not text: a NUL, and bytes no UTF-8 has
        byte[] logo = [0x89, 0x50, 0x4E, 0x47, 0x00, 0xFF, 0x10, 0x00];

        using (var uploaded = await UploadAsync(fixture, lambda.PrivateKey, Zip(("lambda.cs", "return Resources.App(\"web\");"u8.ToArray()),
                                                                                 ("resources/web/index.html", page), ("resources/web/logo.png", logo))))
        {
            Assert.AreEqual(HttpStatusCode.Created, uploaded.StatusCode, await uploaded.Content.ReadAsStringAsync());
        }

        var deployed = await fixture.DeployAsync(lambda.PrivateKey);

        Assert.IsTrue(deployed.Success, string.Join("; ", deployed.Diagnostics.Select(d => d.Message)));

        Assert.AreEqual("<h1>a page of a site</h1>", await fixture.CallAsync("http://pages.localhost/"), "served from where it went online");

        using (var served = await fixture.GetAsync("http://pages.localhost/logo.png"))
        {
            CollectionAssert.AreEqual(logo, await served.Content.ReadAsByteArrayAsync(), "and served as the bytes it is");
        }

        var stored = await File.ReadAllTextAsync(VersionFile(fixture, lambda, 2));

        Assert.IsFalse(stored.Contains("a page of a site"), "the version names the page rather than carrying it");
        StringAssert.Contains(stored, $"\"stored\":\"{Hash(page)}\"");

        CollectionAssert.AreEquivalent(new[] { Hash(page), Hash(logo) }, Kept(fixture, lambda), "and the store keeps what it holds, by its hash");

        // read back as they were sent - by the editor, by an agent, in an archive
        var files = (await fixture.VersionAsync(lambda, 2)).Files;

        Assert.AreEqual("<h1>a page of a site</h1>", files.Single(f => f.Name == "resources/web/index.html").Code);

        var picture = files.Single(f => f.Name == "resources/web/logo.png");

        Assert.AreEqual("base64", picture.Encoding);
        CollectionAssert.AreEqual(logo, Convert.FromBase64String(picture.Code));

        using var archive = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2/zip");

        using var zip = new ZipArchive(new MemoryStream(await archive.Content.ReadAsByteArrayAsync()));

        CollectionAssert.AreEqual(page, Read(zip, "resources/web/index.html"));
        CollectionAssert.AreEqual(logo, Read(zip, "resources/web/logo.png"));
    }

    [TestMethod]
    public async Task AResourceShippedUnchangedIsKeptOnce()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var style = Encoding.UTF8.GetBytes("body { color: teal }");

        foreach (var says in new[] { "one", "two" })
        {
            using var uploaded = await UploadAsync(fixture, lambda.PrivateKey, Zip(("lambda.cs", Encoding.UTF8.GetBytes(Repository.Says(says))),
                                                                                   ("resources/site.css", style)));

            Assert.AreEqual(HttpStatusCode.Created, uploaded.StatusCode, await uploaded.Content.ReadAsStringAsync());
        }

        CollectionAssert.AreEqual(new[] { Hash(style) }, Kept(fixture, lambda), "however many versions ship it");

        foreach (var version in new[] { 2, 3 })
        {
            StringAssert.Contains(await File.ReadAllTextAsync(VersionFile(fixture, lambda, version)), Hash(style), $"version {version} names it");
        }
    }

    [TestMethod]
    public async Task AVersionSavedWithItsResourcesInsideRunsAsItDid()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("inside");

        await fixture.SaveAsync(lambda, "Adds a page", new LambdaFile("lambda.cs", "return Resources.App(\"web\");"),
                                new LambdaFile("resources/web/index.html", "<p>a placeholder</p>"));

        // put back as the second layout stored a version: every resource inside it
        var envelope = JsonSerializer.Serialize(new
        {
            version = 2,
            files = new object[]
            {
                new { name = "lambda.cs", code = "return Resources.App(\"web\");" },
                new { name = "resources/web/index.html", code = "<p>kept inside</p>" }
            }
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        await File.WriteAllTextAsync(VersionFile(fixture, lambda, 2), envelope);

        var deployed = await fixture.DeployAsync(lambda.PrivateKey);

        Assert.IsTrue(deployed.Success, string.Join("; ", deployed.Diagnostics.Select(d => d.Message)));

        Assert.AreEqual("<p>kept inside</p>", await fixture.CallAsync("http://inside.localhost/"), "what it carries is what it serves");

        using (var changed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/changes?deploy=true",
                                                     new VersionChangeRequest(null, null, [new FileEdit("resources/web/index.html", "kept inside", "kept apart")])))
        {
            Assert.AreEqual(HttpStatusCode.Created, changed.StatusCode, await changed.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("<p>kept apart</p>", await fixture.CallAsync("http://inside.localhost/"));

        StringAssert.Contains(await File.ReadAllTextAsync(VersionFile(fixture, lambda, 3)), $"\"stored\":\"{Hash("<p>kept apart</p>"u8.ToArray())}\"",
                              "a version saved now names its resources");

        Assert.AreEqual(envelope, await File.ReadAllTextAsync(VersionFile(fixture, lambda, 2)), "and the one saved then is left as it was");

        // and a resource the store keeps is edited like any other: read from there, kept again
        using (var edited = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/changes?deploy=true",
                                                    new VersionChangeRequest(null, null, [new FileEdit("resources/web/index.html", "kept apart", "kept apart, and edited")])))
        {
            Assert.AreEqual(HttpStatusCode.Created, edited.StatusCode, await edited.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("<p>kept apart, and edited</p>", await fixture.CallAsync("http://inside.localhost/"));
    }

    [TestMethod]
    public async Task AResourceNoVersionNamesAnyMoreIsRemovedWithTheLast()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { MaxVersions = 2 });

        var lambda = await fixture.CreateLambdaAsync();

        await SaveNoteAsync(fixture, lambda, "one");
        await SaveNoteAsync(fixture, lambda, "two");

        // kept a while ago, rather than a moment ago by a save not written yet
        Age(fixture, lambda);

        // the newest two are kept, so the version that named "one" goes
        await SaveNoteAsync(fixture, lambda, "three");

        CollectionAssert.AreEquivalent(new[] { Hash("two"u8.ToArray()), Hash("three"u8.ToArray()) }, Kept(fixture, lambda),
                                       "what a version kept still names stays, and what none names goes");
    }

    [TestMethod]
    public async Task AResourceOnlyAFeatureShippedGoesWithTheFeature()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("drafty");

        await fixture.SaveAsync(lambda, "Adds a page", new LambdaFile("lambda.cs", "return Resources.App(\"web\");"),
                                new LambdaFile("resources/web/index.html", "<p>online</p>"));

        var feature = await fixture.CreateFeatureAsync(lambda, "Redo");

        using (var request = fixture.Host.GetRequest($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/zip?deploy=true", HttpMethod.Put))
        {
            request.Content = new ByteArrayContent(Zip(("lambda.cs", "return Resources.App(\"web\");"u8.ToArray()),
                                                       ("resources/web/index.html", "<p>in the draft</p>"u8.ToArray())));
            request.Content.Headers.ContentType = new("application/zip");

            using var put = await fixture.Host.GetResponseAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, await put.Content.ReadAsStringAsync());
        }

        using (var preview = await fixture.GetAsync($"/features/{feature.Key}/"))
        {
            Assert.AreEqual("<p>in the draft</p>", await preview.GetContentAsync(), "the preview serves what the feature keeps in the lambda's store");
        }

        CollectionAssert.AreEquivalent(new[] { Hash("<p>online</p>"u8.ToArray()), Hash("<p>in the draft</p>"u8.ToArray()) }, Kept(fixture, lambda));

        Age(fixture, lambda);

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode, await deleted.Content.ReadAsStringAsync());
        }

        CollectionAssert.AreEqual(new[] { Hash("<p>online</p>"u8.ToArray()) }, Kept(fixture, lambda), "what only the feature shipped goes with it");
    }

    [TestMethod]
    public async Task ADemoSeededAgainIsNotSavedAgain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        var before = fixture.Meta.Get("demo-crud")!.LatestVersion;

        // as on every start: its version names the resources its template carries
        await fixture.SeedDemosAsync();

        Assert.AreEqual(before, fixture.Meta.Get("demo-crud")!.LatestVersion, "a demo that is what its template holds is left as it is");

        StringAssert.Contains(await File.ReadAllTextAsync(Path.Combine(fixture.Options.CodeDirectory, fixture.Meta.GetId("demo-crud")!.ToString()!, $"v{before}.cs")),
                              "\"stored\":", "and keeps its resources in its store like any lambda");
    }

    #region Helpers

    private static async Task SaveNoteAsync(LambdaFixture fixture, LambdaResponse lambda, string note)
    {
        using var uploaded = await UploadAsync(fixture, lambda.PrivateKey, Zip(("lambda.cs", "return Resources.App(\"web\");"u8.ToArray()),
                                                                               ("resources/web/note.txt", Encoding.UTF8.GetBytes(note))));

        Assert.AreEqual(HttpStatusCode.Created, uploaded.StatusCode, await uploaded.Content.ReadAsStringAsync());
    }

    private static string StoreOf(LambdaFixture fixture, LambdaResponse lambda)
        => Path.Combine(fixture.Options.CodeDirectory, fixture.Meta.GetId(lambda.PrivateKey)!.ToString()!, "resources");

    private static string VersionFile(LambdaFixture fixture, LambdaResponse lambda, int version)
        => Path.Combine(fixture.Options.CodeDirectory, fixture.Meta.GetId(lambda.PrivateKey)!.ToString()!, $"v{version}.cs");

    /// <summary>
    /// The hashes the lambda's store keeps something under.
    /// </summary>
    private static string[] Kept(LambdaFixture fixture, LambdaResponse lambda)
    {
        var store = StoreOf(fixture, lambda);

        return Directory.Exists(store) ? [.. Directory.EnumerateFiles(store, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f)).Order(StringComparer.Ordinal)] : [];
    }

    /// <summary>
    /// Makes everything the store keeps two hours old, past the grace a save
    /// in progress is given.
    /// </summary>
    private static void Age(LambdaFixture fixture, LambdaResponse lambda)
    {
        foreach (var file in Directory.EnumerateFiles(StoreOf(fixture, lambda), "*", SearchOption.AllDirectories))
        {
            File.SetLastWriteTimeUtc(file, DateTime.UtcNow.AddHours(-2));
        }
    }

    private static string Hash(byte[] content) => Convert.ToHexStringLower(SHA256.HashData(content));

    private static byte[] Read(ZipArchive zip, string name)
    {
        using var entry = zip.GetEntry(name)!.Open();

        using var target = new MemoryStream();

        entry.CopyTo(target);

        return target.ToArray();
    }

    private static async Task<HttpResponseMessage> UploadAsync(LambdaFixture fixture, string privateKey, byte[] archive)
    {
        using var request = fixture.Host.GetRequest($"/api/v1/lambdas/{privateKey}/versions/zip", HttpMethod.Post);

        request.Content = new ByteArrayContent(archive);
        request.Content.Headers.ContentType = new("application/zip");

        return await fixture.Host.GetResponseAsync(request);
    }

    private static byte[] Zip(params (string Name, byte[] Content)[] files)
    {
        using var buffer = new MemoryStream();

        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            foreach (var (name, content) in files)
            {
                using var target = zip.CreateEntry(name).Open();

                target.Write(content);
            }
        }

        return buffer.ToArray();
    }

    #endregion

}
