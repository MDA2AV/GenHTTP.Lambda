using System.Net;
using System.Text;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Workspaces;

/// <summary>
/// The private directory of a lambda, as the editor sees it.
/// </summary>
[TestClass]
public sealed class WorkspaceTests
{

    [TestMethod]
    public async Task AFreshWorkspaceIsEmptyButKnowsItsQuota()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var listing = await ListAsync(fixture, lambda.PrivateKey);

        Assert.IsEmpty(listing.Files);
        Assert.AreEqual(0, listing.UsedBytes);
        Assert.AreEqual(fixture.Limits.WorkspaceOf(Data.Entities.LambdaTier.Free).Quota, listing.QuotaBytes);
    }

    [TestMethod]
    public async Task AFileSurvivesTheRoundTrip()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var content = "hello from the editor"u8.ToArray();

        var written = await WriteAsync(fixture, lambda.PrivateKey, "notes.txt", content);

        Assert.AreEqual("notes.txt", written.Path);
        Assert.AreEqual(content.Length, written.Size);

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/files/notes.txt");

        var file = await read.GetContentAsync<FileResponse>();

        CollectionAssert.AreEqual(content, Convert.FromBase64String(file.Content));
    }

    [TestMethod]
    public async Task TheListingReportsSizeAndDate()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await WriteAsync(fixture, lambda.PrivateKey, "a/deep/file.bin", new byte[1234]);

        var listing = await ListAsync(fixture, lambda.PrivateKey);

        var entry = listing.Files.Single();

        Assert.AreEqual("a/deep/file.bin", entry.Path, "nested files keep their path");
        Assert.AreEqual(1234, entry.Size);
        Assert.AreEqual(3 * WorkspaceLimits.Block, listing.UsedBytes, "the room it takes: a block for the file, and one for each of its folders");
        Assert.IsGreaterThan(DateTime.UtcNow.AddMinutes(-5), entry.Modified);
    }

    [TestMethod]
    public async Task FilesCanBeRemoved()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await WriteAsync(fixture, lambda.PrivateKey, "gone.txt", "bye"u8.ToArray());

        using var removed = await fixture.SendAsync(HttpMethod.Delete,
            $"/api/v1/lambdas/{lambda.PrivateKey}/files/gone.txt");

        Assert.IsTrue(removed.IsSuccessStatusCode);
        Assert.IsEmpty((await ListAsync(fixture, lambda.PrivateKey)).Files);
    }

    [TestMethod]
    public async Task AFileTooLargeIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { WorkspaceBytes = 4 * WorkspaceLimits.Block });

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await Put(fixture, lambda.PrivateKey, "deep/er/big.bin", new byte[4 * WorkspaceLimits.Block]);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "it would fit alone, not with the two folders it goes into");

        var listing = await ListAsync(fixture, lambda.PrivateKey);

        Assert.IsEmpty(listing.Files, "and nothing half written is left behind");
        Assert.IsEmpty(listing.Folders, "nor the folders made for it");
    }

    [TestMethod]
    public async Task ContentThatIsNotBase64IsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/files/note.txt",
                                                     new FileRequest("plain text, not base64"));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("base64", await response.Content.ReadAsStringAsync(), "and the caller is told what was expected");

        Assert.IsEmpty((await ListAsync(fixture, lambda.PrivateKey)).Files, "nothing is written from it");
    }

    [TestMethod]
    public async Task OneFileMayTakeTheWholeQuota()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { WorkspaceBytes = 4 * WorkspaceLimits.Block });

        var lambda = await fixture.CreateLambdaAsync();

        // there is no limit on the size of a file, only on the room they take
        await WriteAsync(fixture, lambda.PrivateKey, "all.bin", new byte[4 * WorkspaceLimits.Block]);

        Assert.AreEqual(4 * WorkspaceLimits.Block, (await ListAsync(fixture, lambda.PrivateKey)).UsedBytes);
    }

    [TestMethod]
    public async Task EmptyFilesAndFoldersAreNotFree()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { WorkspaceBytes = 4 * WorkspaceLimits.Block });

        var lambda = await fixture.CreateLambdaAsync();

        // however many files there are is up to the room they take - and an
        // empty one takes a block, or a loop could make them without end
        for (var i = 0; i < 3; i++)
        {
            await WriteAsync(fixture, lambda.PrivateKey, $"{i}.txt", []);
        }

        using (var folder = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/folders/empty"))
        {
            Assert.AreEqual(HttpStatusCode.OK, folder.StatusCode, "a folder takes the fourth block");
        }

        using var refused = await Put(fixture, lambda.PrivateKey, "3.txt", []);

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "and a fourth empty file does not fit");
    }

    [TestMethod]
    public async Task AnUploadThatBreaksOffIsNotKept()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var id = (fixture.Meta.GetId(lambda.PrivateKey))!.Value;

        var workspace = fixture.Application.Services.GetRequiredService<IWorkspaceService>();

        // what an engine that dropped the connection hands on: a body that
        // simply ends, a thousand bytes into the five thousand announced
        await Assert.ThrowsExactlyAsync<LambdaException>(async () =>
            await workspace.WriteAsync(id, "models/model.bin", new MemoryStream(new byte[1000]), expected: 5000));

        var listing = workspace.List(id);

        Assert.IsEmpty(listing.Files, "the part that arrived is not taken for the whole");
        Assert.IsEmpty(listing.Folders);
    }

    [TestMethod]
    public async Task ALargeFileTravelsAsItIs()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // past what is sent as base64, which only the streamed route carries
        var content = new byte[32 * 1024 * 1024 + 1];

        Random.Shared.NextBytes(content);

        using (var request = fixture.Host.GetRequest($"/api/v1/lambdas/{lambda.PrivateKey}/files/{Uri.EscapeDataString("models/large.bin")}/content", HttpMethod.Put))
        {
            request.Content = new ByteArrayContent(content);
            request.Content.Headers.ContentType = new("application/octet-stream");

            using var written = await fixture.Host.GetResponseAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, written.StatusCode, await written.Content.ReadAsStringAsync());
        }

        using (var encoded = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/files/{Uri.EscapeDataString("models/large.bin")}"))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, encoded.StatusCode, "too large to send as base64");
            Assert.Contains("/content", await encoded.Content.ReadAsStringAsync(), "and the caller is told where it is");
        }

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/files/{Uri.EscapeDataString("models/large.bin")}/content");

        Assert.AreEqual(HttpStatusCode.OK, read.StatusCode);
        Assert.AreEqual("application/octet-stream", read.Content.Headers.ContentType?.MediaType, "never rendered, whatever it holds");
        Assert.AreEqual("attachment", read.Content.Headers.ContentDisposition?.DispositionType);

        CollectionAssert.AreEqual(content, await read.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    [DataRow("../escaped.txt")]
    [DataRow("../../etc/passwd")]
    [DataRow("a/../../outside.txt")]
    public async Task NothingEscapesTheWorkspace(string path)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await Put(fixture, lambda.PrivateKey, path, "nope"u8.ToArray());

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, $"'{path}' should not resolve");
    }

    [TestMethod]
    public async Task AnotherLambdaCannotBeRead()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var mine = await fixture.CreateLambdaAsync();
        var theirs = await fixture.CreateLambdaAsync();

        await WriteAsync(fixture, theirs.PrivateKey, "secret.txt", "theirs"u8.ToArray());

        // the workspace is addressed by the editor key, so mine cannot name theirs
        var listing = await ListAsync(fixture, mine.PrivateKey);

        Assert.IsEmpty(listing.Files);
    }

    [TestMethod]
    public async Task AnUnknownKeyIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/lambdas/nosuchkeyatall/files");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task WhatALambdaWritesShowsUpInTheListing()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("writer");

        await fixture.DeployAsync(lambda.PrivateKey, """
            Workspace.WriteText("written-by-the-lambda.txt", "hello");

            return Inline.Create().Get(() => "done");
            """);

        using var _ = await fixture.GetAsync("/lambda/writer/");

        var listing = await ListAsync(fixture, lambda.PrivateKey);

        Assert.ContainsSingle(listing.Files.Where(f => f.Path == "written-by-the-lambda.txt"),
                              "the editor and the lambda share one directory");
    }

    #region Helpers

    private static async Task<WorkspaceListing> ListAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/files");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<WorkspaceListing>();
    }

    private static async Task<WorkspaceEntry> WriteAsync(LambdaFixture fixture, string privateKey, string path, byte[] content)
    {
        using var response = await Put(fixture, privateKey, path, content);

        Assert.IsTrue(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<WorkspaceEntry>();
    }

    private static async Task<HttpResponseMessage> Put(LambdaFixture fixture, string privateKey, string path, byte[] content)
        => await fixture.SendAsync(HttpMethod.Put,
               $"/api/v1/lambdas/{privateKey}/files/{Uri.EscapeDataString(path)}",
               new FileRequest(Convert.ToBase64String(content)));

    #endregion


    [TestMethod]
    public async Task AFolderCanBeMadeBeforeThereIsAnythingToPutInIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var made = await fixture.SendAsync(HttpMethod.Put,
            $"/api/v1/lambdas/{lambda.PrivateKey}/folders/logs");

        Assert.AreEqual(HttpStatusCode.OK, made.StatusCode);

        using var listed = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/files");

        var body = await listed.GetContentAsync();

        Assert.Contains("logs", body, "an empty folder has to survive being listed, or making one is pointless");

        // and it can be put back where it came from
        using var gone = await fixture.SendAsync(HttpMethod.Delete,
            $"/api/v1/lambdas/{lambda.PrivateKey}/files/logs");

        Assert.IsTrue(gone.IsSuccessStatusCode, $"removing a folder answered {(int)gone.StatusCode}");
    }

    [TestMethod]
    public async Task AFileInAFolderIsAddressedByItsEncodedPath()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var written = await fixture.SendAsync(HttpMethod.Put,
            $"/api/v1/lambdas/{lambda.PrivateKey}/files/logs%2Ftoday.txt",
            new { content = Convert.ToBase64String("hello"u8.ToArray()) });

        Assert.AreEqual(HttpStatusCode.OK, written.StatusCode);

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/files/logs%2Ftoday.txt");

        var file = await read.GetContentAsync<FileResponse>();

        Assert.AreEqual("logs/today.txt", file.Path);
        Assert.AreEqual("hello", Encoding.UTF8.GetString(Convert.FromBase64String(file.Content)));
    }

    [TestMethod]
    public async Task AFolderGoesWithWhateverIsInIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var written = await fixture.SendAsync(HttpMethod.Put,
            $"/api/v1/lambdas/{lambda.PrivateKey}/files/logs%2Ftoday.txt",
            new { content = Convert.ToBase64String("hello"u8.ToArray()) });

        Assert.AreEqual(HttpStatusCode.OK, written.StatusCode, "writing into a folder makes the folder");

        using var gone = await fixture.SendAsync(HttpMethod.Delete,
            $"/api/v1/lambdas/{lambda.PrivateKey}/files/logs");

        Assert.IsTrue(gone.IsSuccessStatusCode, $"removing a folder answered {(int)gone.StatusCode}");

        using var listed = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/files");

        Assert.DoesNotContain("today.txt", await listed.GetContentAsync());
    }

}
