using System.Net;
using System.Text;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// What a lambda may keep, which its tier decides: a premium lambda's versions
/// may be larger - its code and its resources - and its data may take more
/// room than everybody else's.
/// </summary>
[TestClass]
public sealed class TierAllowanceTests
{

    #region Versions

    [TestMethod]
    public async Task APremiumLambdaMayHaveMoreCode()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { BuildBytes = 1024, PremiumBuildBytes = 4096 });

        var lambda = await fixture.CreateLambdaAsync();

        using (var refused = await SaveAsync(fixture, lambda.PrivateKey, Coding(2000)))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "two thousand bytes of C# is more than a version of a free lambda may come to");

            var said = await refused.Content.ReadAsStringAsync();

            Assert.Contains("1 KB", said);
            Assert.Contains("premium tier may have 4 KB", said, "and whoever reads it learns there is more");
        }

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        using (var saved = await SaveAsync(fixture, lambda.PrivateKey, Coding(2000)))
        {
            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());
        }

        using var beyond = await SaveAsync(fixture, lambda.PrivateKey, Coding(5000));

        Assert.AreEqual(HttpStatusCode.BadRequest, beyond.StatusCode, "and the premium tier has a limit of its own");
        Assert.DoesNotContain("premium tier may have", await beyond.Content.ReadAsStringAsync(), "with nothing further to point to");
    }

    [TestMethod]
    public async Task TheCodeAndTheResourcesShareTheRoomOfAVersion()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { BuildBytes = 4096, PremiumBuildBytes = 4096 });

        var lambda = await fixture.CreateLambdaAsync();

        // each half fits, both together do not
        var code = new LambdaFile("Notes.cs", "// " + new string('a', 2500));
        var resource = new LambdaFile("resources/page.html", new string('b', 2500));

        using (var one = await SaveAsync(fixture, lambda.PrivateKey, [new(LambdaSource.EntryName, "return Resources.Files();"), code]))
        {
            Assert.AreEqual(HttpStatusCode.Created, one.StatusCode, await one.Content.ReadAsStringAsync());
        }

        using (var other = await SaveAsync(fixture, lambda.PrivateKey, [new(LambdaSource.EntryName, "return Resources.Files();"), resource]))
        {
            Assert.AreEqual(HttpStatusCode.Created, other.StatusCode, await other.Content.ReadAsStringAsync());
        }

        using var both = await SaveAsync(fixture, lambda.PrivateKey, [new(LambdaSource.EntryName, "return Resources.Files();"), code, resource]);

        Assert.AreEqual(HttpStatusCode.BadRequest, both.StatusCode);
        Assert.Contains("The code and the resources of a version must not exceed 4 KB together", await both.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task ALambdaMayHaveAnyNumberOfFilesAndResources()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // there was room for twelve C# files and sixty assets once; now only
        // what they come to is counted
        var files = new List<LambdaFile> { new(LambdaSource.EntryName, "return Resources.Files();") };

        files.AddRange(Enumerable.Range(1, 30).Select(i => new LambdaFile($"Type{i}.cs", $"public record Type{i}(int Value);")));
        files.AddRange(Enumerable.Range(1, 150).Select(i => new LambdaFile($"resources/www/page{i}.html", $"<p>page {i}</p>")));

        await fixture.DeployAsync(lambda.PrivateKey, LambdaSource.Serialize(files));

        using var page = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/www/page150.html");

        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode);
        Assert.AreEqual("<p>page 150</p>", await page.GetContentAsync());
    }

    [TestMethod]
    public async Task AnArchiveMayHoldAnyNumberOfFiles()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var archive = LambdaArchive.Pack([
            new LambdaFile(LambdaSource.EntryName, "return Resources.Files();"),
            .. Enumerable.Range(1, 100).Select(i => new LambdaFile($"resources/www/{i}.txt", $"{i}"))
        ]);

        using var saved = await UploadAsync(fixture, lambda.PrivateKey, archive);

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task APremiumLambdaMayShipMoreResources()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { BuildBytes = 1024, PremiumBuildBytes = 1024 * 1024 });

        var lambda = await fixture.CreateLambdaAsync();

        using (var refused = await SaveAsync(fixture, lambda.PrivateKey, Shipping(2048)))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "two kilobytes is more than a version of a free lambda may come to");

            var said = await refused.Content.ReadAsStringAsync();

            Assert.Contains("1 KB", said);
            Assert.Contains("workspace", said, "and says where a large file that is no part of the program belongs");
        }

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        using (var saved = await SaveAsync(fixture, lambda.PrivateKey, Shipping(2048)))
        {
            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());
        }

        using var beyond = await SaveAsync(fixture, lambda.PrivateKey, Shipping(1024 * 1024 + 1));

        Assert.AreEqual(HttpStatusCode.BadRequest, beyond.StatusCode, "and the premium tier has a limit of its own");
        Assert.Contains("1 MB", await beyond.Content.ReadAsStringAsync(), "said in the unit it was set in");
    }

    [TestMethod]
    public async Task AnArchiveMayHoldWhatTheTierMayShip()
    {
        // an archive is refused while it is unpacked, as soon as it holds more
        // than a lambda of the tier could - a megabyte of resources is past
        // that for a free lambda and well within it for a premium one
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { BuildBytes = 1024, PremiumBuildBytes = 2 * 1024 * 1024 });

        var lambda = await fixture.CreateLambdaAsync();

        var archive = LambdaArchive.Pack(Shipping(1024 * 1024));

        using (var refused = await UploadAsync(fixture, lambda.PrivateKey, archive))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);
        }

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        using (var saved = await UploadAsync(fixture, lambda.PrivateKey, archive))
        {
            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());
        }

        // longer in base64 than one piece of the writer, so this is read back
        // through the path that writes a value in pieces
        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2");

        var content = await read.GetContentAsync<VersionContentResponse>();

        Assert.HasCount(1024 * 1024, content.Files.Single(f => f.Name == "resources/data.bin").Bytes);
    }

    [TestMethod]
    public async Task ThePremiumTierNeverHasLessThanTheOthers()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { BuildBytes = 1024L * 1024 * 1024, DataBytes = 8L * 1024 * 1024 * 1024 });

        Assert.AreEqual(1024L * 1024 * 1024, fixture.Limits.BuildOf(LambdaTier.Premium),
                        "raising what a version of everybody's may come to must not leave premium lambdas below it");

        Assert.AreEqual(8L * 1024 * 1024 * 1024, fixture.Limits.DataOf(LambdaTier.Premium), "nor what everybody's data may take");
    }

    [TestMethod]
    public void AFileLongerThanOnePieceIsWrittenWhole()
    {
        // a surrogate pair right where the first piece would end, and a quote
        // that has to be escaped, so the pieces have to join up exactly
        var text = new string('a', 1024 * 1024 - 1) + "😀\"quoted\"" + new string('b', 2 * 1024 * 1024);

        var files = LambdaSource.Parse(LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return null;"),
            new LambdaFile("long.txt", text)
        ]));

        Assert.AreEqual(text, files[1].Code);
    }

    [TestMethod]
    public void Base64WithLineBreaksIsStillBase64()
    {
        var bytes = new byte[1000];

        Random.Shared.NextBytes(bytes);

        LambdaFile[] files =
        [
            new(LambdaSource.EntryName, "return null;"),
            new("data.bin", Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks), "base64")
        ];

        Assert.IsNull(LambdaSource.Validate(files), "some encoders wrap their lines, and decoding never minded");
        Assert.AreEqual(bytes.Length, LambdaSource.Size(files[1]));
    }

    #endregion

    #region Workspace

    [TestMethod]
    public async Task APremiumWorkspaceHasMoreRoom()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { DataBytes = 4 * WorkspaceLimits.Block, PremiumDataBytes = 16 * WorkspaceLimits.Block });

        var lambda = await fixture.CreateLambdaAsync();

        var file = new byte[8 * WorkspaceLimits.Block];

        using (var refused = await PutAsync(fixture, lambda.PrivateKey, "big.bin", file))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);
        }

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        using (var written = await PutAsync(fixture, lambda.PrivateKey, "big.bin", file))
        {
            Assert.AreEqual(HttpStatusCode.OK, written.StatusCode, await written.Content.ReadAsStringAsync());
        }

        var listing = await ListAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(16 * WorkspaceLimits.Block, listing.QuotaBytes);

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/files/big.bin");

        var downloaded = await read.GetContentAsync<FileResponse>();

        Assert.HasCount(file.Length, Convert.FromBase64String(downloaded.Content), "and it can be downloaded again");
    }

    [TestMethod]
    public async Task TheQuotaCountsEveryFileTogether()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Small);

        var lambda = await fixture.CreateLambdaAsync();

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        using (var first = await PutAsync(fixture, lambda.PrivateKey, "a.bin", new byte[2 * WorkspaceLimits.Block]))
        {
            Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
        }

        using (var second = await PutAsync(fixture, lambda.PrivateKey, "b.bin", new byte[2 * WorkspaceLimits.Block]))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, second.StatusCode, "each file fits, both together do not");
        }

        using (var replaced = await PutAsync(fixture, lambda.PrivateKey, "a.bin", new byte[2 * WorkspaceLimits.Block]))
        {
            Assert.AreEqual(HttpStatusCode.OK, replaced.StatusCode, "a file being replaced is not counted twice");
        }

        using (var third = await PutAsync(fixture, lambda.PrivateKey, "c.bin", new byte[1000]))
        {
            Assert.AreEqual(HttpStatusCode.OK, third.StatusCode, "what is left of the quota can still be used");
        }

        var listing = await ListAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(3 * WorkspaceLimits.Block, listing.UsedBytes, "a thousand bytes take a whole block");

        CollectionAssert.AreEquivalent(new[] { "a.bin", "c.bin" }, listing.Files.Select(f => f.Path).ToArray(),
                                       "and nothing that was refused is left behind");
    }

    [TestMethod]
    public async Task AWorkspaceOverItsQuotaMayBeRewrittenButNotGrow()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Small);

        var lambda = await fixture.CreateLambdaAsync("squeezed");

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        await fixture.DeployAsync(lambda.PrivateKey, Writer);

        using (var _ = await PutAsync(fixture, lambda.PrivateKey, "a.bin", new byte[2 * WorkspaceLimits.Block])) { }

        // how a workspace ends up past its quota: filled under a larger one,
        // which is what leaving the premium tier does to it
        var id = (fixture.Meta.GetId(lambda.PrivateKey))!.Value;

        await File.WriteAllBytesAsync(Path.Combine(fixture.Options.WorkspaceDirectory, id.ToString(), "x.bin"), new byte[2 * WorkspaceLimits.Block]);

        using (var same = await PutAsync(fixture, lambda.PrivateKey, "a.bin", new byte[2 * WorkspaceLimits.Block]))
        {
            Assert.AreEqual(HttpStatusCode.OK, same.StatusCode, "rewriting a file at its size takes no more room");
        }

        using (var smaller = await fixture.GetAsync("/lambda/squeezed/write?name=a.bin&size=1000"))
        {
            Assert.AreEqual(HttpStatusCode.OK, smaller.StatusCode, "and neither does the lambda shrinking it");
        }

        using (var grown = await PutAsync(fixture, lambda.PrivateKey, "a.bin", new byte[WorkspaceLimits.Block + 1]))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, grown.StatusCode, "growing it back does");
        }

        using var added = await fixture.GetAsync("/lambda/squeezed/write?name=b.bin&size=1");

        Assert.AreEqual(HttpStatusCode.InternalServerError, added.StatusCode, "and so does anything new");
    }

    [TestMethod]
    public async Task ALambdaIsHeldToTheQuotaFromInside()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Small);

        var lambda = await fixture.CreateLambdaAsync("hoarder");

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        await fixture.DeployAsync(lambda.PrivateKey, Writer);

        using (var first = await fixture.GetAsync($"/lambda/hoarder/write?name=a.bin&size={2 * WorkspaceLimits.Block}"))
        {
            Assert.AreEqual(HttpStatusCode.OK, first.StatusCode, await first.Content.ReadAsStringAsync());
        }

        using var second = await fixture.GetAsync($"/lambda/hoarder/write?name=b.bin&size={2 * WorkspaceLimits.Block}");

        Assert.AreEqual(HttpStatusCode.InternalServerError, second.StatusCode);
        Assert.Contains($"{3 * WorkspaceLimits.Block} bytes", await second.Content.ReadAsStringAsync(), "the lambda is told what its quota is");
    }

    [TestMethod]
    public async Task ALambdaCannotMakeRoomOutOfNothing()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { DataBytes = 4 * WorkspaceLimits.Block });

        var lambda = await fixture.CreateLambdaAsync("mason");

        // reading makes nothing, however deep the name; making folders and
        // empty files takes room until there is none
        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create().Get(() =>
            {
                var seen = Workspace.Exists("a/b/c/nothing.txt");

                var made = 0;

                try
                {
                    for (var i = 0; i < 100; i++)
                    {
                        if (i % 2 == 0) Workspace.CreateFolder($"f{i}"); else Workspace.WriteText($"e{i}.txt", "");
                        made++;
                    }
                }
                catch (InvalidOperationException) { }

                return $"{seen} {made} {Workspace.Folders().Length}";
            });
            """);

        using var response = await fixture.GetAsync("/lambda/mason/");

        Assert.AreEqual("False 4 2", await response.GetContentAsync(), "four blocks: two folders and two empty files, and no folders from reading");
    }

    [TestMethod]
    public async Task TextIsCountedInBytes()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { DataBytes = 2 * WorkspaceLimits.Block });

        var lambda = await fixture.CreateLambdaAsync("scribe");

        // two blocks in characters, more than two once written as bytes
        await fixture.DeployAsync(lambda.PrivateKey, $$"""
            return Inline.Create().Get(() =>
            {
                Workspace.WriteText("accents.txt", new string('é', {{WorkspaceLimits.Block + 1}}));
                return "written";
            });
            """);

        using var response = await fixture.GetAsync("/lambda/scribe/");

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public async Task MovingTheTierMovesTheLimitsOfTheRunningLambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { DataBytes = 4 * WorkspaceLimits.Block, PremiumDataBytes = 32 * WorkspaceLimits.Block });

        var lambda = await fixture.CreateLambdaAsync("mover");

        await fixture.DeployAsync(lambda.PrivateKey, Writer);

        var size = 8 * WorkspaceLimits.Block;

        using (var refused = await fixture.GetAsync($"/lambda/mover/write?name=big.bin&size={size}"))
        {
            Assert.AreEqual(HttpStatusCode.InternalServerError, refused.StatusCode, "more than a free lambda may write");
        }

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        using (var written = await fixture.GetAsync($"/lambda/mover/write?name=big.bin&size={size}"))
        {
            Assert.AreEqual(HttpStatusCode.OK, written.StatusCode, "promoted, and not deployed again");
        }

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Free);

        using var demoted = await fixture.GetAsync($"/lambda/mover/write?name=other.bin&size={size}");

        Assert.AreEqual(HttpStatusCode.InternalServerError, demoted.StatusCode, "and back again");
    }

    [TestMethod]
    public async Task ALambdaIsBuiltOnceHoweverManyRequestsArriveTogether()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("crowded");

        // the snippet counts its own starts, which is how often it was built
        await fixture.DeployAsync(lambda.PrivateKey, """
            var starts = Workspace.Exists("starts.txt") ? int.Parse(Workspace.ReadText("starts.txt")) : 0;

            Workspace.WriteText("starts.txt", (starts + 1).ToString());

            return Inline.Create().Get(() => "here");
            """);

        // built again on the next request, like every lambda after a restart
        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => fixture.GetAsync("/lambda/crowded/")));

        foreach (var response in responses)
        {
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            response.Dispose();
        }

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/files/starts.txt");

        var file = await read.GetContentAsync<FileResponse>();

        Assert.AreEqual("2", Encoding.UTF8.GetString(Convert.FromBase64String(file.Content)),
                        "once when it was deployed, and once more for the new tier - not once per request");
    }

    #endregion

    #region Summary

    [TestMethod]
    public async Task TheSummaryShowsTheAllowanceOfTheTier()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // the allowances as they were decided: versions of 32 MB and 512 MB
        // of data for everybody, and 128 MB and four gigabytes in the
        // premium tier
        var free = await SummaryAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(32L * 1024 * 1024, free.Limits.BuildBytes);
        Assert.AreEqual(512L * 1024 * 1024, free.Limits.DataBytes);

        fixture.ChangeTier(lambda.PrivateKey, LambdaTier.Premium);

        var premium = await SummaryAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(128L * 1024 * 1024, premium.Limits.BuildBytes);
        Assert.AreEqual(4096L * 1024 * 1024, premium.Limits.DataBytes);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// A premium workspace small enough to fill in a test: three blocks - and
    /// the free one too, which the premium tier never has less than.
    /// </summary>
    private static LambdaOptions Small(LambdaOptions options)
        => options with { DataBytes = 3 * WorkspaceLimits.Block, PremiumDataBytes = 3 * WorkspaceLimits.Block };

    /// <summary>
    /// Writes as many bytes as it is asked for, under the name it is given.
    /// </summary>
    private const string Writer = """
        return Inline.Create().Get("write", (string name, int size) =>
        {
            Workspace.WriteBytes(name, new byte[size]);
            return "written";
        });
        """;

    /// <summary>
    /// A lambda of exactly as many bytes of C#, most of them a comment.
    /// </summary>
    private static LambdaFile[] Coding(int characters)
    {
        const string code = "return Content.From(Resource.FromString(\"x\"));\n// ";

        return [new(LambdaSource.EntryName, code + new string('a', characters - code.Length))];
    }

    private static LambdaFile[] Shipping(int bytes) =>
    [
        new(LambdaSource.EntryName, "return Resources.Files();"),
        new("resources/data.bin", Convert.ToBase64String(new byte[bytes]), "base64")
    ];

    private static Task<HttpResponseMessage> SaveAsync(LambdaFixture fixture, string privateKey, LambdaFile[] files)
        => fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/versions", new VersionRequest(files));

    private static async Task<HttpResponseMessage> UploadAsync(LambdaFixture fixture, string privateKey, byte[] archive)
    {
        using var request = fixture.Host.GetRequest($"/api/v1/lambdas/{privateKey}/versions/zip", HttpMethod.Post);

        request.Content = new ByteArrayContent(archive);
        request.Content.Headers.ContentType = new("application/zip");

        return await fixture.Host.GetResponseAsync(request);
    }

    private static Task<HttpResponseMessage> PutAsync(LambdaFixture fixture, string privateKey, string path, byte[] content)
        => fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/files/{Uri.EscapeDataString(path)}",
                             new FileRequest(Convert.ToBase64String(content)));

    private static async Task<WorkspaceListing> ListAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/files");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<WorkspaceListing>();
    }

    private static async Task<LambdaSummaryResponse> SummaryAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/summary");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<LambdaSummaryResponse>();
    }

    #endregion

}
