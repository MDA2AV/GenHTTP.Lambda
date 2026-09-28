using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// The newest version is the one being worked on: saved over in place and
/// deployed again as often as it takes, while the versions before it stay as
/// they were - and what visitors get only changes when something is deployed.
/// </summary>
[TestClass]
public sealed class WorkingVersionTests
{

    #region Saving over

    [TestMethod]
    public async Task TheNewestVersionCanBeSavedOverAndKeepsItsNumber()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, Says("first"));

        using var put = await PutAsync(fixture, lambda.PrivateKey, 2, Says("second"));

        Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, await put.Content.ReadAsStringAsync());

        var saved = await put.GetContentAsync<SavedVersionResponse>();

        Assert.AreEqual(2, saved.Version, "saving over a version does not make another");
        Assert.AreEqual(2, saved.Revision, "but it is its second save");
        Assert.IsNotNull(saved.Modified);

        var versions = await VersionsAsync(fixture, lambda.PrivateKey);

        Assert.HasCount(2, versions, "the starter and the one being worked on, still");
        Assert.AreEqual(2, versions[0].Revision);
        Assert.AreEqual(1, versions[1].Revision);

        Assert.Contains("second", await CodeAsync(fixture, lambda.PrivateKey, 2));
    }

    [TestMethod]
    public async Task AnOlderVersionIsHistoryAndSaysWhatToDoInstead()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, Says("newest"));

        var before = await CodeAsync(fixture, lambda.PrivateKey, 1);

        using var put = await PutAsync(fixture, lambda.PrivateKey, 1, Says("rewritten history"));

        Assert.AreEqual(HttpStatusCode.Conflict, put.StatusCode);

        var refusal = await put.Content.ReadAsStringAsync();

        Assert.Contains("history", refusal);
        Assert.Contains("copy", refusal, "the refusal has to say how to carry on from an old version");

        Assert.AreEqual(before, await CodeAsync(fixture, lambda.PrivateKey, 1), "and the old version is as it was");

        using var missing = await PutAsync(fixture, lambda.PrivateKey, 9, Says("nowhere"));

        Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [TestMethod]
    public async Task SavingTheSameCodeAgainIsNotAnotherSave()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, Says("same"));

        using var put = await PutAsync(fixture, lambda.PrivateKey, 2, Says("same", change: "Says the same, better described"));

        var saved = await put.GetContentAsync<SavedVersionResponse>();

        Assert.AreEqual(1, saved.Revision, "nothing about the code changed, so there is nothing to deploy again");
        Assert.AreEqual("Says the same, better described", saved.Change, "what is said about it can still change");
    }

    [TestMethod]
    public async Task NotesAreKeptUnlessNewOnesAreGiven()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, Says("one", "A greeting", "Greets"));

        using (var kept = await PutAsync(fixture, lambda.PrivateKey, 2, Says("two")))
        {
            var saved = await kept.GetContentAsync<SavedVersionResponse>();

            Assert.AreEqual("Greets", saved.Change);
            Assert.AreEqual("A greeting", saved.Specification);
        }

        using (var replaced = await PutAsync(fixture, lambda.PrivateKey, 2, Says("three", change: "Greets twice")))
        {
            var saved = await replaced.GetContentAsync<SavedVersionResponse>();

            Assert.AreEqual("Greets twice", saved.Change);
            Assert.AreEqual("A greeting", saved.Specification, "a note left out is a note kept");
        }
    }

    [TestMethod]
    public async Task SomeFilesCanBeChangedInPlace()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, new VersionRequest([
            new LambdaFile(LambdaSource.EntryName, "return Content.From(Resource.FromString(Greeter.Text));"),
            new LambdaFile("Greeter.cs", "static class Greeter { public const string Text = \"before\"; }"),
            new LambdaFile("notes.txt", "to be removed")
        ]));

        using var patched = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/2?deploy=true",
            new VersionChangeRequest(null, ["notes.txt"], [new FileEdit("Greeter.cs", "\"before\"", "\"after\"")]));

        Assert.AreEqual(HttpStatusCode.OK, patched.StatusCode, await patched.Content.ReadAsStringAsync());

        var saved = await patched.GetContentAsync<SavedVersionResponse>();

        Assert.AreEqual(2, saved.Version);
        Assert.AreEqual(2, saved.Revision);
        Assert.IsTrue(saved.Deployment?.Success, "deploy=true puts it online in the same request");

        using var served = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual("after", await served.GetContentAsync());

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2");

        var content = await read.GetContentAsync<VersionContentResponse>();

        CollectionAssert.AreEqual(new[] { "lambda.cs", "Greeter.cs" }, content.Files.Select(f => f.Name).ToArray());
    }

    #endregion

    #region What is online

    [TestMethod]
    public async Task WhatIsOnlineStaysUntilItIsDeployedAgain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, SaysCode("first"));

        using (var _ = await PutAsync(fixture, lambda.PrivateKey, 2, Says("second"))) { }

        Assert.AreEqual("first", await ServedAsync(fixture, lambda.PublicKey), "saving over the version online changes nothing visitors get");

        var changed = await LambdaAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(2, changed.ActiveVersion);
        Assert.AreEqual(1, changed.ActiveRevision);
        Assert.IsTrue(changed.ActiveChanged, "and the editor can say that deploying it again changes something");

        await fixture.DeployAsync(lambda.PrivateKey);

        Assert.AreEqual("second", await ServedAsync(fixture, lambda.PublicKey), "deploying the same version again builds it again");

        var redeployed = await LambdaAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(2, redeployed.ActiveRevision);
        Assert.IsFalse(redeployed.ActiveChanged);
    }

    [TestMethod]
    public async Task ARestartServesWhatWasDeployedRatherThanWhatWasSavedSince()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, SaysCode("deployed"));

        using (var _ = await PutAsync(fixture, lambda.PrivateKey, 2, Says("saved since"))) { }

        using (var _ = await PutAsync(fixture, lambda.PrivateKey, 2, Says("saved again"))) { }

        // what a restart does to a lambda: nothing compiled, everything read again
        fixture.Deployments.Evict((await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value);

        Assert.AreEqual("deployed", await ServedAsync(fixture, lambda.PublicKey));

        await fixture.DeployAsync(lambda.PrivateKey);

        fixture.Deployments.Evict((await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value);

        Assert.AreEqual("saved again", await ServedAsync(fixture, lambda.PublicKey), "until it is deployed, and then what it is");
    }

    [TestMethod]
    public async Task ARedeployThatFailsKeepsServingTheAssetsThatWereOnline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, new VersionRequest([
            new LambdaFile(LambdaSource.EntryName, "return Layout.Create().Add(Assets.App(\"web\"));"),
            new LambdaFile("web/index.html", "<!doctype html><p>the page that is online</p>")
        ]));

        await fixture.DeployAsync(lambda.PrivateKey);

        using var put = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/2?deploy=true", new VersionRequest([
            new LambdaFile(LambdaSource.EntryName, "return Layout.Create().Add(Assets.App(\"web\")) this does not compile"),
            new LambdaFile("web/index.html", "<!doctype html><p>a page that never went online</p>")
        ]));

        var saved = await put.GetContentAsync<SavedVersionResponse>();

        Assert.IsFalse(saved.Deployment!.Success);

        using var served = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.Contains("the page that is online", await served.GetContentAsync(),
                        "the assets of a version are written out before it compiles, and have to be put back when it does not");
    }

    [TestMethod]
    public async Task TheDeploymentHistorySaysWhichSaveWasOnline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, SaysCode("first"));

        using (var _ = await PutAsync(fixture, lambda.PrivateKey, 2, Says("second"))) { }

        await fixture.DeployAsync(lambda.PrivateKey);

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/deployment/history");

        var history = await response.GetContentAsync<List<ActivationResponse>>();

        Assert.HasCount(2, history);
        Assert.AreEqual(2, history[0].Revision);
        Assert.AreEqual(1, history[1].Revision);
        Assert.AreEqual("replaced", history[1].EndedBy, "the same version deployed again replaces itself");
    }

    #endregion

    #region Copies

    [TestMethod]
    public async Task ACopyBecomesTheNewestAndLeavesTheOriginalAsItWas()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, Says("the second", "What the user wanted", "Says the second"));

        using var copied = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/1/copy", new CopyVersionRequest());

        Assert.AreEqual(HttpStatusCode.Created, copied.StatusCode, await copied.Content.ReadAsStringAsync());

        var copy = await copied.GetContentAsync<SavedVersionResponse>();

        Assert.AreEqual(3, copy.Version);
        Assert.AreEqual("A copy of version 1", copy.Change);

        Assert.AreEqual(await CodeAsync(fixture, lambda.PrivateKey, 1), await CodeAsync(fixture, lambda.PrivateKey, 3));

        // the copy is the newest now, so it is the one that can be worked on
        using (var put = await PutAsync(fixture, lambda.PrivateKey, 3, Says("carried on")))
        {
            Assert.AreEqual(HttpStatusCode.OK, put.StatusCode);
        }

        using (var put = await PutAsync(fixture, lambda.PrivateKey, 2, Says("too late")))
        {
            Assert.AreEqual(HttpStatusCode.Conflict, put.StatusCode, "and the one it replaced as the newest is history");
        }
    }

    [TestMethod]
    public async Task ACopyOfTheNewestKeepsWhatTheUserWanted()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, Says("done", "A guest book", "Adds a guest book"));

        using var copied = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/2/copy?deploy=true",
                                                   new CopyVersionRequest(Change: "Adds a way to delete entries"));

        var copy = await copied.GetContentAsync<SavedVersionResponse>();

        Assert.AreEqual(3, copy.Version);
        Assert.AreEqual("A guest book", copy.Specification);
        Assert.AreEqual("Adds a way to delete entries", copy.Change);
        Assert.IsTrue(copy.Deployment?.Success);

        Assert.AreEqual(3, (await LambdaAsync(fixture, lambda.PrivateKey)).ActiveVersion);
    }

    [TestMethod]
    public async Task ADemoCanNeitherBeSavedOverNorCopied()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("a-demo-to-be");

        await fixture.MakeDemoAsync(lambda.PublicKey);

        using (var put = await PutAsync(fixture, lambda.PrivateKey, 1, Says("changed")))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, put.StatusCode);
        }

        using (var copied = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/1/copy", new CopyVersionRequest()))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, copied.StatusCode);
        }

        Assert.HasCount(1, await VersionsAsync(fixture, lambda.PrivateKey));
    }

    #endregion

    #region Plumbing

    private static string SaysCode(string text) => $"return Content.From(Resource.FromString(\"{text}\"));";

    private static VersionRequest Says(string text, string? specification = null, string? change = null)
        => new([new LambdaFile(LambdaSource.EntryName, SaysCode(text))], specification, change);

    private static async Task SaveAsync(LambdaFixture fixture, string privateKey, VersionRequest request)
    {
        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/versions", request);

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());
    }

    private static Task<HttpResponseMessage> PutAsync(LambdaFixture fixture, string privateKey, int version, VersionRequest request)
        => fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/versions/{version}", request);

    private static async Task<List<VersionResponse>> VersionsAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/versions");

        return await response.GetContentAsync<List<VersionResponse>>();
    }

    private static async Task<string> CodeAsync(LambdaFixture fixture, string privateKey, int version)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/versions/{version}");

        var content = await response.GetContentAsync<VersionContentResponse>();

        return string.Join("\n", content.Files.Select(f => $"{f.Name}: {f.Code}"));
    }

    private static async Task<LambdaResponse> LambdaAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}");

        return await response.GetContentAsync<LambdaResponse>();
    }

    private static async Task<string> ServedAsync(LambdaFixture fixture, string publicKey)
    {
        using var served = await fixture.GetAsync($"/lambda/{publicKey}/");

        return await served.GetContentAsync();
    }

    #endregion

}
