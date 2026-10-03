using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Workspaces;

/// <summary>
/// The data of a lambda: what it keeps rather than what it is, shared by every
/// version and switched on and off by its owner.
/// </summary>
[TestClass]
public sealed class DataTests
{

    private const string Keeper = """
        return Inline.Create()
                     .Get("write", () => { Workspace.WriteText("note.txt", "kept"); return "written"; })
                     .Get("read", () => Workspace.Exists("note.txt") ? Workspace.ReadText("note.txt") : "nothing");
        """;

    [TestMethod]
    public async Task TheWorkspaceIsOnUntilTheOwnerSaysOtherwise()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var stores = await ListAsync(fixture, lambda.PrivateKey);

        var workspace = stores.Single(s => s.Kind == "workspace");

        Assert.AreEqual("workspace", workspace.Kind);
        Assert.IsTrue(workspace.Enabled);
        Assert.IsTrue(workspace.Default);
        Assert.IsNull(workspace.Changed, "nobody chose anything yet");
        Assert.AreEqual(fixture.Limits.WorkspaceOf(Data.Entities.LambdaTier.Free).Quota, workspace.QuotaBytes);
    }

    [TestMethod]
    public async Task DataIsSharedByEveryVersionAndLeftAloneByDeploying()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, Keeper);

        using (var _ = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/write")) { }

        // a new version, and then back to the old one: neither touches the data
        await fixture.DeployAsync(lambda.PrivateKey, Keeper.Replace("nothing", "still nothing"));

        using (var rolledBack = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/start", new DeploymentRequest(2)))
        {
            Assert.AreEqual(HttpStatusCode.OK, rolledBack.StatusCode);
        }

        using var read = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/read");

        Assert.AreEqual("kept", await read.GetContentAsync());

        var workspace = (await ListAsync(fixture, lambda.PrivateKey)).Single(s => s.Kind == "workspace");

        Assert.AreEqual(1, workspace.Items);
        Assert.AreEqual(WorkspaceLimits.Block, workspace.UsedBytes);
    }

    [TestMethod]
    public async Task SwitchingTheWorkspaceOffDeletesWhatItHeld()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await PutFileAsync(fixture, lambda.PrivateKey, "uploads/a.txt", "a");

        using var off = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/data/workspace");

        Assert.AreEqual(HttpStatusCode.OK, off.StatusCode, await off.Content.ReadAsStringAsync());

        var store = await off.GetContentAsync<DataStoreResponse>();

        Assert.IsFalse(store.Enabled);
        Assert.IsNotNull(store.Changed);
        Assert.AreEqual(0, store.Items);

        using var listed = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/files");

        var listing = await listed.GetContentAsync<WorkspaceListing>();

        Assert.IsFalse(listing.Enabled);
        Assert.IsEmpty(listing.Files);
        Assert.IsEmpty(listing.Folders);

        using var refused = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/files/b.txt", new FileRequest(Convert.ToBase64String("b"u8.ToArray())));

        Assert.AreEqual(HttpStatusCode.Conflict, refused.StatusCode, "nothing lands where the lambda has been told there is nothing");
        Assert.Contains("switched off", await refused.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task ALambdaWhoseWorkspaceIsOffIsToldSoAndWorksAgainOnceItIsOn()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, Keeper);

        using (var written = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/write"))
        {
            Assert.AreEqual(HttpStatusCode.OK, written.StatusCode);
        }

        using (var _ = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/data/workspace")) { }

        // compiled with a workspace, it is compiled again without one on its
        // next request - no deployment needed, and none would help
        using (var refused = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/write"))
        {
            Assert.AreEqual(HttpStatusCode.InternalServerError, refused.StatusCode);
        }

        using (var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary"))
        {
            var said = await summary.GetContentAsync<LambdaSummaryResponse>();

            Assert.IsTrue(said.RecentProblems.Any(p => (p.Text + p.Detail).Contains("switched off")),
                          "the owner reads why in the log: " + string.Join(" | ", said.RecentProblems.Select(p => p.Text)));

            Assert.IsFalse(said.Storage.WorkspaceEnabled);
            Assert.IsTrue(said.Storage.UsesWorkspace, "which is what the editor warns about before it is switched off");
        }

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/workspace"))
        {
            Assert.IsTrue((await on.GetContentAsync<DataStoreResponse>()).Enabled);
        }

        using (var read = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/read"))
        {
            Assert.AreEqual("nothing", await read.GetContentAsync(), "switched on again, it starts empty");
        }

        using (var written = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/write"))
        {
            Assert.AreEqual(HttpStatusCode.OK, written.StatusCode, "and can be written again");
        }
    }

    [TestMethod]
    public async Task CodeThatUsesTheWorkspaceWhileItIsBuiltDoesNotDeployWithoutOne()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using (var _ = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/data/workspace")) { }

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions?deploy=true",
                                                  LambdaFixture.Version("return Layout.Create().Add(Workspace.Files());"));

        var version = await saved.GetContentAsync<SavedVersionResponse>();

        Assert.IsFalse(version.Deployment!.Success);
        Assert.IsTrue(version.Deployment.Diagnostics.Any(d => d.Message.Contains("switched off")),
                      string.Join(" | ", version.Deployment.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    public async Task SwitchingIsIdempotent()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        for (var i = 0; i < 2; i++)
        {
            using var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/workspace");

            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        for (var i = 0; i < 2; i++)
        {
            using var off = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/data/workspace");

            Assert.AreEqual(HttpStatusCode.OK, off.StatusCode);
        }

        using var one = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/data/workspace");

        Assert.IsFalse((await one.GetContentAsync<DataStoreResponse>()).Enabled);
    }

    [TestMethod]
    public async Task AKindThereIsNotIsNotFound()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/data/mainframe");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("workspace", await response.Content.ReadAsStringAsync(), "and it says which kinds there are");
    }

    [TestMethod]
    public async Task TheDataOfADemoIsReadAndNotSwitched()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("data-of-a-demo");

        await fixture.MakeDemoAsync(lambda.PublicKey);

        Assert.HasCount(3, await ListAsync(fixture, lambda.PrivateKey));

        using var off = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/data/workspace");

        Assert.AreEqual(HttpStatusCode.Forbidden, off.StatusCode);
    }

    [TestMethod]
    public async Task AKeyThatNamesNothingIsNotFound()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/lambdas/nobody-has-this-key/data");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<List<DataStoreResponse>> ListAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/data");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<List<DataStoreResponse>>();
    }

    private static async Task PutFileAsync(LambdaFixture fixture, string privateKey, string path, string content)
    {
        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/files/{Uri.EscapeDataString(path)}",
                                                     new FileRequest(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content))));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

}
