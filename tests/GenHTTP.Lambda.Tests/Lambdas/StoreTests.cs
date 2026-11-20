using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;
using GenHTTP.Lambda.Tests.Git;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// How the versions of a lambda are kept: a manifest each, over what they
/// hold, kept once however many versions and features hold it - and read
/// only by whatever needs it, so a version's resources weigh nothing on
/// saving, checking, compiling or describing it.
/// </summary>
[TestClass]
public sealed class StoreTests
{

    /// <summary>
    /// A resource too large to go unnoticed, of bytes nobody guesses.
    /// </summary>
    private static readonly byte[] Large = RandomNumberGenerator.GetBytes(6 * 1024 * 1024);

    private const string Serving = "return Resources.Files();";

    [TestMethod]
    public async Task AVersionIsAManifestAndWhatItHoldsIsKeptOnce()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o.WithSizes(build: 16 * 1024 * 1024));

        var lambda = await fixture.CreateLambdaAsync("keeper");

        await fixture.SaveAsync(lambda, "Ships a picture", Shipping());

        // a change of the snippet alone, which the API makes on what is kept
        await ChangeAsync(fixture, lambda, "Changes the snippet", new FileEdit(LambdaSource.EntryName, "Files()", "Files(\"\")"));

        var id = await IdOfAsync(fixture, lambda);

        var store = Path.Combine(fixture.Options.CodeDirectory, id.ToString(), "blobs");

        var kept = Directory.GetFiles(store, "*", SearchOption.AllDirectories);

        Assert.AreEqual(1, kept.Count(f => new FileInfo(f).Length > 1024 * 1024), "the picture is kept once, though two versions hold it");
        Assert.AreEqual(Large.Length, new FileInfo(kept.Single(f => new FileInfo(f).Length > 1024 * 1024)).Length, "as its bytes, not as base64");

        foreach (var version in new[] { 2, 3 })
        {
            var manifest = Path.Combine(fixture.Options.CodeDirectory, id.ToString(), $"v{version}.manifest.json");

            Assert.IsLessThan(4096, new FileInfo(manifest).Length, $"version {version} names what it holds rather than holding it");
        }

        var read = await fixture.VersionAsync(lambda, 3);

        CollectionAssert.AreEqual(Large, read.Files.Single(f => f.Name == "resources/picture.bin").Bytes, "and it reads back as it was sent");
        Assert.AreEqual("base64", read.Files.Single(f => f.Name == "resources/picture.bin").Encoding);

        // what git names it by, in either of its object formats, is known without reading it
        await using var database = await fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>().CreateDbContextAsync();

        var row = await database.Blobs.SingleAsync(b => b.LambdaId == id && b.Size == Large.Length);

        Assert.AreEqual(Convert.ToHexStringLower(SHA256.HashData(Large)), row.Hash);
        Assert.AreEqual(GitObjectId.ForBlob(Large).ToString(), row.GitSha1);
        Assert.AreEqual(Convert.ToHexStringLower(SHA256.HashData([.. Encoding.ASCII.GetBytes($"blob {Large.Length}\0"), .. Large])), row.GitSha256);
        Assert.IsNull(row.Chars, "and that it is no text");
    }

    [TestMethod]
    public async Task AResourceIsReadOnlyByWhatServesOrShipsIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o.WithSizes(build: 16 * 1024 * 1024));

        var reads = fixture.Platform.Services.GetRequiredService<ContentReads>();

        var lambda = await fixture.CreateLambdaAsync("light");

        await fixture.SaveAsync(lambda, "Ships a picture", Shipping());

        var before = reads.Resources;

        // changing the snippet of a version, and of a feature, and merging it
        await ChangeAsync(fixture, lambda, "Changes the snippet", new FileEdit(LambdaSource.EntryName, "Files()", "Files(\"\")"));

        var feature = await fixture.CreateFeatureAsync(lambda, "Another snippet");

        using (var changed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/changes",
                                                     new VersionChangeRequest(null, null, [new FileEdit(LambdaSource.EntryName, "Files(\"\")", "Files()")])))
        {
            Assert.AreEqual(HttpStatusCode.OK, changed.StatusCode, await changed.Content.ReadAsStringAsync());
        }

        using (var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/merge", new MergeFeatureRequest()))
        {
            Assert.AreEqual(HttpStatusCode.Created, merged.StatusCode, await merged.Content.ReadAsStringAsync());
        }

        // checking what it holds, and describing it
        var newest = await fixture.Meta.GetVersionAsync(lambda.PrivateKey, 4);

        Assert.IsTrue((await fixture.Meta.CheckAsync(lambda.PrivateKey, newest.Files)).Success);

        foreach (var path in new[] { "summary", "secrets", "data", "versions", "versions/4?folder=docs/" })
        {
            using var described = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/{path}");

            Assert.AreEqual(HttpStatusCode.OK, described.StatusCode, path);
        }

        Assert.AreEqual(before, reads.Resources, "nothing of that needs the picture, so nothing read it");

        // what serves it does
        Assert.IsTrue((await fixture.DeployAsync(lambda.PrivateKey)).Success);

        Assert.IsGreaterThan(before, reads.Resources);

        using var served = await fixture.GetAsync("http://light.localhost/picture.bin");

        CollectionAssert.AreEqual(Large, await served.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    public async Task AFeatureAndItsPreviewHoldWhatTheirVersionHoldsWithoutACopy()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o.WithSizes(build: 16 * 1024 * 1024));

        var lambda = await fixture.CreateLambdaAsync("brancher");

        await fixture.SaveAsync(lambda, "Ships a picture", Shipping());

        var id = await IdOfAsync(fixture, lambda);

        var feature = await fixture.CreateFeatureAsync(lambda, "Tries something");

        using (var deployed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/preview/start"))
        {
            Assert.AreEqual(HttpStatusCode.OK, deployed.StatusCode, await deployed.Content.ReadAsStringAsync());
        }

        var store = Path.Combine(fixture.Options.CodeDirectory, id.ToString(), "blobs");

        Assert.AreEqual(1, Directory.GetFiles(store, "*", SearchOption.AllDirectories).Count(f => new FileInfo(f).Length == Large.Length), "still kept once");

        var folder = Directory.GetDirectories(Path.Combine(fixture.Options.FeatureDirectory, id.ToString())).Single();

        foreach (var manifest in new[] { "files.manifest.json", "preview.manifest.json" })
        {
            Assert.IsLessThan(4096, new FileInfo(Path.Combine(folder, manifest)).Length, $"{manifest} names what it holds");
        }

        using var previewed = await fixture.GetAsync($"{feature.PreviewPath}picture.bin");

        CollectionAssert.AreEqual(Large, await previewed.Content.ReadAsByteArrayAsync(), "and the preview serves it");
    }

    [TestMethod]
    public async Task WhatNoVersionOrFeatureHoldsIsCollected()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var storage = fixture.Platform.Services.GetRequiredService<IStorageService>();

        var lambda = await fixture.CreateLambdaAsync("tidy");

        await fixture.SaveAsync(lambda, "Ships a page", new LambdaFile(LambdaSource.EntryName, Serving), new LambdaFile("resources/index.html", "<p>kept</p>"));

        var id = await IdOfAsync(fixture, lambda);

        var feature = await fixture.CreateFeatureAsync(lambda, "Drafts");

        await fixture.PutFeatureAsync(lambda, feature.Key, "A first draft", new LambdaFile(LambdaSource.EntryName, Serving), new LambdaFile("resources/index.html", "<p>first</p>"));

        await fixture.PutFeatureAsync(lambda, feature.Key, "A second draft", new LambdaFile(LambdaSource.EntryName, Serving), new LambdaFile("resources/index.html", "<p>second</p>"));

        Age(fixture, id);

        await storage.CollectAsync();

        Assert.IsTrue(Holds(fixture, id, "<p>kept</p>"), "what a version holds stays");
        Assert.IsTrue(Holds(fixture, id, "<p>second</p>"), "and what the feature holds");
        Assert.IsFalse(Holds(fixture, id, "<p>first</p>"), "what the feature held before it was saved again goes");

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}"))
        {
            Assert.IsTrue(deleted.IsSuccessStatusCode, await deleted.Content.ReadAsStringAsync());
        }

        Age(fixture, id);

        await storage.CollectAsync();

        Assert.IsFalse(Holds(fixture, id, "<p>second</p>"), "and so does what only a feature that went held");
        Assert.IsTrue(Holds(fixture, id, "<p>kept</p>"));

        await using var database = await fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>().CreateDbContextAsync();

        var rows = await database.Blobs.Where(b => b.LambdaId == id).Select(b => b.Hash).ToListAsync();

        Assert.HasCount(Directory.GetFiles(Path.Combine(fixture.Options.CodeDirectory, id.ToString(), "blobs"), "*", SearchOption.AllDirectories).Length, rows,
                        "what is said about the content goes with it");
    }

    [TestMethod]
    public async Task WhatWasJustKeptIsNotCollectedBeforeItIsNamed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var storage = fixture.Platform.Services.GetRequiredService<IStorageService>();

        var lambda = await fixture.CreateLambdaAsync("early");

        var id = await IdOfAsync(fixture, lambda);

        // kept for a save that is about to name it, as a save keeps it before its turn
        await storage.KeepAsync(id, [VersionFile.FromText("resources/index.html", "<p>on its way</p>")]);

        await storage.CollectAsync();

        Assert.IsTrue(Holds(fixture, id, "<p>on its way</p>"), "content a manifest is about to name is not taken from under it");
    }

    [TestMethod]
    public async Task AnArchiveSentBackAsItWasAddsNothingToTheStore()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o.WithSizes(build: 16 * 1024 * 1024));

        var lambda = await fixture.CreateLambdaAsync("returning");

        await fixture.SaveAsync(lambda, "Ships a picture", Shipping());

        var id = await IdOfAsync(fixture, lambda);

        var store = Path.Combine(fixture.Options.CodeDirectory, id.ToString(), "blobs");

        var before = Directory.GetFiles(store, "*", SearchOption.AllDirectories).Length;

        using var download = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2/zip");

        var archive = await download.Content.ReadAsByteArrayAsync();

        using (var zip = new ZipArchive(new MemoryStream(archive)))
        {
            Assert.AreEqual(Large.Length, zip.GetEntry("resources/picture.bin")!.Length, "the archive holds the picture as it is");
        }

        using var request = fixture.Host.GetRequest($"/api/v1/lambdas/{lambda.PrivateKey}/versions/zip", HttpMethod.Post);

        request.Content = new ByteArrayContent(archive);
        request.Content.Headers.ContentType = new("application/zip");

        using var saved = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        Assert.HasCount(before, Directory.GetFiles(store, "*", SearchOption.AllDirectories), "what came back is what the store keeps already");

        Assert.IsEmpty(Directory.GetFileSystemEntries(fixture.Options.StagingDirectory), "and nothing waits beside the stores once it is kept");

        var read = await fixture.VersionAsync(lambda, 3);

        CollectionAssert.AreEqual(Large, read.Files.Single(f => f.Name == "resources/picture.bin").Bytes);
    }

    [TestMethod]
    public async Task APushThatChangesTheCodeReadsNoResource()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o.WithSizes(build: 16 * 1024 * 1024));

        using var git = new GitClient();

        var reads = fixture.Platform.Services.GetRequiredService<ContentReads>();

        var lambda = await fixture.CreateLambdaAsync("pushed");

        await fixture.SaveAsync(lambda, "Ships a picture", Shipping());

        await git.CloneAsync(fixture.EditorUrl(lambda), "pushed");

        // what the clone fetched was copied from the store into the commits
        var before = reads.Resources;

        git.Change("pushed", "Project.cs", "Resources.Files()", "Resources.Files(\"\")");

        await git.CommitAsync("pushed", "Serves the root");

        var pushed = await git.RunAsync("pushed", "push");

        Assert.Contains("Saved version 3", pushed.Said);

        Assert.AreEqual(before, reads.Resources, "the picture is one the lambda keeps, which git named, so it was not read");

        var read = await fixture.VersionAsync(lambda, 3);

        CollectionAssert.AreEqual(Large, read.Files.Single(f => f.Name == "resources/picture.bin").Bytes);
        Assert.Contains("Files(\"\")", read.Files.Single(f => f.Name == LambdaSource.EntryName).Code);
    }

    [TestMethod]
    public async Task ADemoKeptBeforeIsNotSavedAgain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        var demo = DemoCatalog.All.First();

        var lambda = await fixture.Meta.GetAsync(demo.Key);

        var id = (await fixture.Meta.GetIdAsync(demo.Key))!.Value;

        var version = lambda!.LatestVersion!.Value;

        var code = Path.Combine(fixture.Options.CodeDirectory, id.ToString());

        // the same files, kept as they were before there was a store
        var files = (await fixture.Meta.GetVersionAsync(demo.Key, version)).Files.Select(f => f.ToSent()).ToList();

        File.Delete(Path.Combine(code, $"v{version}.manifest.json"));

        await File.WriteAllTextAsync(Path.Combine(code, $"v{version}.cs"), System.Text.Json.JsonSerializer.Serialize(new { version = 2, files }, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)));

        await fixture.SeedDemosAsync();

        Assert.AreEqual(version, (await fixture.Meta.GetAsync(demo.Key))!.LatestVersion, "what it holds is what its template holds, however it was written");
    }

    #region Helpers

    private static LambdaFile[] Shipping() =>
    [
        new(LambdaSource.EntryName, Serving),
        new("resources/picture.bin", Convert.ToBase64String(Large), "base64"),
        new("docs/product.md", "# Light\n\nShips a picture.\n")
    ];

    private static async Task ChangeAsync(LambdaFixture fixture, LambdaResponse lambda, string change, params FileEdit[] edits)
    {
        using var changed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/changes", new VersionChangeRequest(null, null, edits, Change: change));

        Assert.AreEqual(HttpStatusCode.Created, changed.StatusCode, await changed.Content.ReadAsStringAsync());
    }

    private static async Task<long> IdOfAsync(LambdaFixture fixture, LambdaResponse lambda) => (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value;

    /// <summary>
    /// Makes everything the store of a lambda keeps older than what a collection leaves alone.
    /// </summary>
    private static void Age(LambdaFixture fixture, long id)
    {
        foreach (var file in Directory.GetFiles(Path.Combine(fixture.Options.CodeDirectory, id.ToString(), "blobs"), "*", SearchOption.AllDirectories))
        {
            File.SetLastWriteTimeUtc(file, DateTime.UtcNow - ContentStore.Fresh - TimeSpan.FromMinutes(1));
        }
    }

    /// <summary>
    /// Whether the store of a lambda keeps the given text.
    /// </summary>
    private static bool Holds(LambdaFixture fixture, long id, string text)
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

        return File.Exists(Path.Combine(fixture.Options.CodeDirectory, id.ToString(), "blobs", hash[..2], hash[2..]));
    }

    #endregion

}
