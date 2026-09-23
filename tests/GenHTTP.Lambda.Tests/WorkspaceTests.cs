using System.Net;
using System.Text;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests;

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
        Assert.AreEqual(WorkspaceLimits.MaxFiles, listing.MaxFiles);
        Assert.AreEqual(WorkspaceLimits.MaxFileSize, listing.MaxFileSize);
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
        Assert.AreEqual(1234, listing.UsedBytes);
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
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await Put(fixture, lambda.PrivateKey, "big.bin", new byte[WorkspaceLimits.MaxFileSize + 1]);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsEmpty((await ListAsync(fixture, lambda.PrivateKey)).Files, "and nothing half written is left behind");
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
