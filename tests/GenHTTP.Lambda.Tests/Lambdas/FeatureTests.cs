using System.Net;
using System.Text;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// Features: a change worked on beside a lambda, with its own copy of the
/// files and the data and an address of its own, merged into the next version
/// once it is right - while the lambda goes on as it was.
/// </summary>
[TestClass]
public sealed class FeatureTests
{

    #region Starting one

    [TestMethod]
    public async Task AFeatureStartsAsACopyOfTheNewestVersion()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, Says("two"));

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Something", "What the user wants");

        Assert.AreEqual("Something", feature.Name);
        Assert.AreEqual("What the user wants", feature.Specification);
        Assert.AreEqual(2, feature.Base);
        Assert.AreEqual(2, feature.Newest);
        Assert.IsTrue(feature.Mergeable);
        Assert.IsFalse(feature.Online, "nothing is online until its preview is started");
        Assert.AreEqual($"/features/{feature.Key}/", feature.PreviewPath);
        Assert.AreEqual(32, feature.Key.Length, "the key is what opens the preview, so nobody guesses it");

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}");

        var content = await read.GetContentAsync<FeatureContentResponse>();

        Assert.Contains("two", content.Files.Single().Code);

        using var listed = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features");

        Assert.AreEqual(feature.Key, (await listed.GetContentAsync<List<FeatureResponse>>()).Single().Key);
    }

    [TestMethod]
    public async Task AFeatureCanStartFromAnOlderVersion()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, Says("two"));

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "From the start", @base: 1);

        Assert.AreEqual(1, feature.Base);
        Assert.IsFalse(feature.Mergeable, "and is then behind the newest until it says otherwise");

        using var missing = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Nowhere", Base: 9));

        Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [TestMethod]
    public async Task AFeatureNeedsAName()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        foreach (var name in (string?[])[null, "   ", new string('x', FeatureService.MaxName + 1), "two\nlines"])
        {
            using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest(name));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, name ?? "null");
        }
    }

    [TestMethod]
    public async Task ALambdaMayHaveOnlySoManyFeatures()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { MaxFeatures = 2 });

        var lambda = await fixture.CreateLambdaAsync();

        await CreateAsync(fixture, lambda.PrivateKey, "One");
        var second = await CreateAsync(fixture, lambda.PrivateKey, "Two");

        using (var third = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Three")))
        {
            Assert.AreEqual(HttpStatusCode.Conflict, third.StatusCode);
            Assert.Contains("Merge or delete", await third.Content.ReadAsStringAsync());
        }

        using (var _ = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{second.Key}")) { }

        await CreateAsync(fixture, lambda.PrivateKey, "Three");
    }

    [TestMethod]
    public async Task ADemoHasNoFeatures()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("feature-of-a-demo");

        await fixture.MakeDemoAsync(lambda.PublicKey);

        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Mine"));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Working on it

    [TestMethod]
    public async Task SavingAFeatureChangesNeitherTheLambdaNorItsVersions()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, SaysCode("live"));

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Change");

        for (var i = 0; i < 3; i++)
        {
            using var put = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/files",
                                                    new FeatureFilesRequest(Says($"attempt {i}").Files, Change: "Says something else"));

            Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, await put.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("live", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/"));

        using var versions = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions");

        Assert.HasCount(2, await versions.GetContentAsync<List<VersionResponse>>());

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}");

        var content = await read.GetContentAsync<FeatureContentResponse>();

        Assert.Contains("attempt 2", content.Files.Single().Code);
        Assert.AreEqual("Says something else", content.Feature.Change);
    }

    [TestMethod]
    public async Task SomeFilesOfAFeatureCanBeChanged()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, new VersionRequest([
            new LambdaFile(LambdaSource.EntryName, "return Content.From(Resource.FromString(Greeter.Text));"),
            new LambdaFile("Greeter.cs", "static class Greeter { public const string Text = \"before\"; }"),
            new LambdaFile("notes.txt", "to be removed")
        ]));

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Greeting");

        using var changed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/changes?deploy=true",
            new VersionChangeRequest(null, ["notes.txt"], [new FileEdit("Greeter.cs", "\"before\"", "\"after\"")]));

        Assert.AreEqual(HttpStatusCode.OK, changed.StatusCode, await changed.Content.ReadAsStringAsync());

        var saved = await changed.GetContentAsync<FeatureSavedResponse>();

        Assert.IsTrue(saved.Preview?.Success);
        Assert.IsTrue(saved.Feature.Online);

        Assert.AreEqual("after", await ServedAsync(fixture, feature.PreviewPath));
    }

    [TestMethod]
    public async Task AFeatureIsHeldToWhatAVersionMayHold()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { BuildBytes = 100 });

        var lambda = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Too much");

        using var put = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/files",
                                                new FeatureFilesRequest([new LambdaFile(LambdaSource.EntryName, SaysCode(new string('x', 200)))]));

        Assert.AreEqual(HttpStatusCode.BadRequest, put.StatusCode, "refused when saved, rather than when it is merged");
    }

    [TestMethod]
    public async Task AFeatureCanBeRenamedAndDescribed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Draft", "Wanted");

        using var patched = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}",
                                                    new UpdateFeatureRequest(Name: "Leaderboard", Change: "Adds a leaderboard"));

        var updated = await patched.GetContentAsync<FeatureResponse>();

        Assert.AreEqual("Leaderboard", updated.Name);
        Assert.AreEqual("Adds a leaderboard", updated.Change);
        Assert.AreEqual("Wanted", updated.Specification, "what is left out stays");

        using var cleared = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}",
                                                    new UpdateFeatureRequest(Specification: ""));

        var emptied = await cleared.GetContentAsync<FeatureResponse>();

        Assert.IsNull(emptied.Specification, "what is sent empty is cleared");
        Assert.AreEqual("Adds a leaderboard", emptied.Change);
    }

    [TestMethod]
    public async Task AFeatureSaysWhetherItsPreviewServesTheLatestSave()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Current");

        Assert.IsFalse(feature.Current, "nothing is online yet");
        Assert.AreEqual(1, feature.Revision, "the files it began with");

        var started = await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        Assert.IsTrue(started.Feature.Current);

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key, Says("newer").Files!);

        using (var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}"))
        {
            var saved = (await read.GetContentAsync<FeatureContentResponse>()).Feature;

            Assert.IsTrue(saved.Online);
            Assert.IsFalse(saved.Current, "the preview serves an earlier save");
            Assert.AreEqual(2, saved.Revision);
        }

        Assert.IsTrue((await StartAsync(fixture, lambda.PrivateKey, feature.Key)).Feature.Current);

        using var renamed = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}",
                                                    new UpdateFeatureRequest(Name: "Renamed"));

        var after = await renamed.GetContentAsync<FeatureResponse>();

        Assert.IsTrue(after.Current, "a new name changes nothing the preview serves");
        Assert.AreEqual(2, after.Revision);
    }

    #endregion

    #region Its preview

    [TestMethod]
    public async Task ThePreviewAnswersAtItsOwnAddressAndIsNotIndexed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, SaysCode("live"));

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Page");

        using (var offline = await fixture.GetAsync(feature.PreviewPath))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, offline.StatusCode, "not before it is started");
        }

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key, [
            new LambdaFile(LambdaSource.EntryName, "return Layout.Create().Add(\"api\", Inline.Create().Get(() => \"from the api\")).Add(Resources.App(\"web\"));"),
            new LambdaFile("resources/web/index.html", "<!doctype html><p>the feature's page</p>")
        ]);

        var started = await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        Assert.IsTrue(started.Success);
        Assert.IsTrue(started.Feature.Online);
        Assert.IsNotNull(started.Feature.Previewed);

        using (var page = await fixture.GetAsync(feature.PreviewPath))
        {
            Assert.Contains("the feature's page", await page.GetContentAsync());
            Assert.IsTrue(page.Headers.TryGetValues("X-Robots-Tag", out var robots) && robots.Single().Contains("noindex"),
                          "a preview is handed around to try something, not to be found");
            Assert.IsTrue(page.Headers.TryGetValues("Referrer-Policy", out var referrer) && referrer.Single() == "no-referrer",
                          "and its address, which opens it, is not passed on to what it links to");
        }

        // relative links resolve against the preview, like they do against the lambda
        Assert.AreEqual("from the api", await ServedAsync(fixture, $"{feature.PreviewPath}api"));

        using (var stopped = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/preview/stop"))
        {
            Assert.IsFalse((await stopped.GetContentAsync<FeatureResponse>()).Online);
        }

        using (var offline = await fixture.GetAsync(feature.PreviewPath))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, offline.StatusCode);
        }
    }

    [TestMethod]
    public async Task ThePreviewServesWhatWasDeployedUntilItIsDeployedAgain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Preview");

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key, Says("deployed").Files!);

        await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key, Says("saved since").Files!);

        Assert.AreEqual("deployed", await ServedAsync(fixture, feature.PreviewPath), "saving alone changes nothing a visitor of the preview gets");

        // what a restart does: nothing compiled, everything read again
        fixture.Deployments.EvictAll((fixture.Meta.GetId(lambda.PrivateKey))!.Value);

        Assert.AreEqual("deployed", await ServedAsync(fixture, feature.PreviewPath), "not even after a restart");

        await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        Assert.AreEqual("saved since", await ServedAsync(fixture, feature.PreviewPath));
    }

    [TestMethod]
    public async Task APreviewThatDoesNotBuildKeepsServingWhatItDid()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Page");

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key, [
            new LambdaFile(LambdaSource.EntryName, "return Layout.Create().Add(Resources.App(\"web\"));"),
            new LambdaFile("resources/web/index.html", "<!doctype html><p>the page that is online</p>")
        ]);

        await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key, [
            new LambdaFile(LambdaSource.EntryName, "return Layout.Create().Add(Resources.App(\"web\")) this does not compile"),
            new LambdaFile("resources/web/index.html", "<!doctype html><p>a page that never went online</p>")
        ]);

        using (var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/preview/start"))
        {
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
            Assert.IsNotEmpty((await refused.GetContentAsync<FeaturePreviewResponse>()).Diagnostics);
        }

        Assert.Contains("the page that is online", await ServedAsync(fixture, feature.PreviewPath),
                        "the resources are written out before the code compiles, and are put back when it does not");
    }

    [TestMethod]
    public async Task APreviewIsKeptOutOfTheLambdasFiguresAndLog()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(() => { Console.WriteLine(\"printed by the lambda\"); return \"live\"; });");

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Printing");

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key,
                            [new LambdaFile(LambdaSource.EntryName, "return Inline.Create().Get(() => { Console.WriteLine(\"printed by the feature\"); return \"preview\"; });")]);

        await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        Assert.AreEqual("live", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/"));

        for (var i = 0; i < 3; i++)
        {
            Assert.AreEqual("preview", await ServedAsync(fixture, feature.PreviewPath));
        }

        using (var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary"))
        {
            Assert.AreEqual(1, (await summary.GetContentAsync<LambdaSummaryResponse>()).Traffic.HourRequests, "only the lambda's own visitor is counted");
        }

        using (var own = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/logs"))
        {
            var lines = (await own.GetContentAsync<OwnerLogResponse>()).Lines.Select(l => l.Text).ToList();

            Assert.IsTrue(lines.Any(l => l.Contains("printed by the lambda")), string.Join(" | ", lines));
            Assert.IsFalse(lines.Any(l => l.Contains("printed by the feature") || l.Contains("/features/")), "the preview is not in the lambda's log");
        }

        using (var preview = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/logs"))
        {
            var lines = (await preview.GetContentAsync<OwnerLogResponse>()).Lines.Select(l => l.Text).ToList();

            Assert.IsTrue(lines.Any(l => l.Contains("printed by the feature")), string.Join(" | ", lines));
            Assert.IsTrue(lines.Any(l => l.Contains($"/features/{feature.Key}/")));
            Assert.IsFalse(lines.Any(l => l.Contains("printed by the lambda")), "nor the lambda in the preview's");
        }
    }

    [TestMethod]
    public async Task AnAddressNobodyHandedOutIsNotFound()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        foreach (var path in (string[])["/features/", "/features/nothing-like-a-key/", $"/features/{new string('a', 32)}/"])
        {
            using var response = await fixture.GetAsync(path);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, path);
        }
    }

    #endregion

    #region Its data

    [TestMethod]
    public async Task AFeatureWorksOnACopyOfTheData()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, Keeper);

        using (var _ = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/folders/empty")) { }

        await PutFileAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/files/note.txt", "real");

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Keeper");

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key, [new LambdaFile(LambdaSource.EntryName, Keeper)]);

        await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        Assert.AreEqual("real", await ServedAsync(fixture, $"{feature.PreviewPath}read"), "it starts with a copy of what the lambda keeps");

        using (var listing = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/workspace"))
        {
            var copy = await listing.GetContentAsync<WorkspaceListing>();

            Assert.Contains("empty", copy.Folders, "empty folders included");
        }

        using (var _ = await fixture.SendAsync(HttpMethod.Post, $"{feature.PreviewPath}write")) { }

        Assert.AreEqual("written by the code", await ServedAsync(fixture, $"{feature.PreviewPath}read"));
        Assert.AreEqual("real", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/read"), "the lambda's own data is as it was");

        await PutFileAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/workspace/extra.txt", "only here");

        using (var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/workspace/extra.txt"))
        {
            Assert.AreEqual("only here", Encoding.UTF8.GetString(Convert.FromBase64String((await read.GetContentAsync<FileResponse>()).Content)));
        }

        using (var own = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/files"))
        {
            Assert.IsFalse((await own.GetContentAsync<WorkspaceListing>()).Files.Any(f => f.Path == "extra.txt"));
        }

        using (var data = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/data"))
        {
            Assert.AreEqual(2, (await data.GetContentAsync<List<DataStoreResponse>>()).Single(s => s.Kind == "workspace").Items, "its data counts its copy");
        }

        // a fresh copy of the real thing, and the preview reads it again
        using (var refreshed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/data/refresh"))
        {
            Assert.AreEqual(HttpStatusCode.OK, refreshed.StatusCode);
        }

        Assert.AreEqual("real", await ServedAsync(fixture, $"{feature.PreviewPath}read"));
    }

    [TestMethod]
    public async Task SwitchingTheWorkspaceOffEmptiesTheCopiesToo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await PutFileAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/files/note.txt", "real");

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Copy");

        using (var _ = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/data/workspace")) { }

        using var listing = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/workspace");

        var copy = await listing.GetContentAsync<WorkspaceListing>();

        Assert.IsFalse(copy.Enabled);
        Assert.IsEmpty(copy.Files);
    }

    #endregion

    #region Merging

    [TestMethod]
    public async Task MergingMakesTheFeatureTheNextVersionAndLeavesTheDataAlone()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, Keeper);

        await PutFileAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/files/note.txt", "real");

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Shouting", "Say it louder");

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key, [new LambdaFile(LambdaSource.EntryName, Keeper.Replace("Workspace.ReadText(\"note.txt\")", "Workspace.ReadText(\"note.txt\").ToUpperInvariant()"))],
                            change: "Reads the note out loud");

        await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        using (var _ = await fixture.SendAsync(HttpMethod.Post, $"{feature.PreviewPath}write")) { }

        using var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/merge",
                                                   new MergeFeatureRequest(Deploy: true));

        Assert.AreEqual(HttpStatusCode.Created, merged.StatusCode, await merged.Content.ReadAsStringAsync());

        var result = await merged.GetContentAsync<FeatureMergeResponse>();

        Assert.IsTrue(result.Merged);
        Assert.AreEqual(3, result.Version!.Version);
        Assert.AreEqual("Reads the note out loud", result.Version.Change, "the version says what the feature said");
        Assert.AreEqual("Say it louder", result.Version.Specification);
        Assert.IsTrue(result.Deployment!.Success);

        Assert.AreEqual("REAL", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/read"),
                        "the merged code runs against the lambda's own data, not against what the preview wrote");

        using (var gone = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}"))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, gone.StatusCode);
        }

        using (var preview = await fixture.GetAsync(feature.PreviewPath))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, preview.StatusCode);
        }

        var directory = Path.Combine(fixture.Options.FeatureDirectory, (fixture.Meta.GetId(lambda.PrivateKey))!.Value.ToString());

        Assert.IsFalse(Directory.Exists(directory) && Directory.EnumerateFileSystemEntries(directory).Any(), "its files and its copy of the data are gone");
    }

    [TestMethod]
    public async Task MergingWithoutDeployingLeavesWhatIsOnline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, SaysCode("live"));

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Later");

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key, Says("later").Files!);

        using var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/merge", new MergeFeatureRequest());

        var result = await merged.GetContentAsync<FeatureMergeResponse>();

        Assert.AreEqual(3, result.Version!.Version);
        Assert.IsNull(result.Deployment);
        Assert.AreEqual("Merges the feature 'Later'", result.Version.Change, "and says where it came from when nobody said more");

        Assert.AreEqual("live", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/"));
    }

    [TestMethod]
    public async Task OnlyAFeatureBasedOnTheNewestVersionIsMerged()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var first = await CreateAsync(fixture, lambda.PrivateKey, "First");
        var second = await CreateAsync(fixture, lambda.PrivateKey, "Second");

        await PutFilesAsync(fixture, lambda.PrivateKey, first.Key, Says("first").Files!);
        await PutFilesAsync(fixture, lambda.PrivateKey, second.Key, Says("second").Files!);

        using (var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{first.Key}/merge", new MergeFeatureRequest()))
        {
            Assert.AreEqual(HttpStatusCode.Created, merged.StatusCode);
        }

        using (var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{second.Key}/merge", new MergeFeatureRequest()))
        {
            Assert.AreEqual(HttpStatusCode.Conflict, refused.StatusCode, "merging it would undo what the first one did");

            var message = await refused.Content.ReadAsStringAsync();

            Assert.Contains("version 2", message);
            Assert.Contains("base", message, "and the refusal says how to get it merged");
        }

        using (var listed = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features"))
        {
            var left = (await listed.GetContentAsync<List<FeatureResponse>>()).Single();

            Assert.IsFalse(left.Mergeable);
            Assert.AreEqual(1, left.Base);
            Assert.AreEqual(2, left.Newest);
        }

        // whoever works on it brings version 2 in, and says so
        using (var moved = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{second.Key}", new UpdateFeatureRequest(Base: 2)))
        {
            Assert.IsTrue((await moved.GetContentAsync<FeatureResponse>()).Mergeable);
        }

        using (var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{second.Key}/merge", new MergeFeatureRequest()))
        {
            Assert.AreEqual(HttpStatusCode.Created, merged.StatusCode);
            Assert.AreEqual(3, (await merged.GetContentAsync<FeatureMergeResponse>()).Version!.Version);
        }

        using (var missing = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{first.Key}", new UpdateFeatureRequest(Base: 3)))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode, "a merged feature is gone");
        }
    }

    [TestMethod]
    public async Task AFeatureIsNotMergedWhileItDoesNotCompileOrChangesNothing()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Nothing yet");

        using (var unchanged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/merge", new MergeFeatureRequest()))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, unchanged.StatusCode);
            Assert.Contains("nothing to merge", await unchanged.Content.ReadAsStringAsync());
        }

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key, [new LambdaFile(LambdaSource.EntryName, "return this is not csharp;")]);

        using (var broken = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/merge", new MergeFeatureRequest()))
        {
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, broken.StatusCode);

            var result = await broken.GetContentAsync<FeatureMergeResponse>();

            Assert.IsFalse(result.Merged);
            Assert.IsNotEmpty(result.Diagnostics);
        }

        using var versions = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions");

        Assert.HasCount(1, await versions.GetContentAsync<List<VersionResponse>>(), "no version is made of either");

        using var kept = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}");

        Assert.AreEqual(HttpStatusCode.OK, kept.StatusCode, "and the feature stays, to be fixed");
    }

    #endregion

    #region Ending

    [TestMethod]
    public async Task ADeletedFeatureTakesItsPreviewAndDataWithIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, SaysCode("live"));

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Unwanted");

        await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}"))
        {
            Assert.IsTrue(deleted.IsSuccessStatusCode);
        }

        using (var preview = await fixture.GetAsync(feature.PreviewPath))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, preview.StatusCode);
        }

        Assert.AreEqual("live", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/"), "the lambda is not touched");

        using var listed = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features");

        Assert.IsEmpty(await listed.GetContentAsync<List<FeatureResponse>>());

        var directory = Path.Combine(fixture.Options.FeatureDirectory, (fixture.Meta.GetId(lambda.PrivateKey))!.Value.ToString());

        Assert.IsFalse(Directory.Exists(directory) && Directory.EnumerateFileSystemEntries(directory).Any(), "its files and its copy of the data are gone");
    }

    [TestMethod]
    public async Task DeletingALambdaTakesItsFeaturesAlong()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Doomed");

        await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        var id = (fixture.Meta.GetId(lambda.PrivateKey))!.Value;

        using (var _ = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}")) { }

        using var preview = await fixture.GetAsync(feature.PreviewPath);

        Assert.AreEqual(HttpStatusCode.NotFound, preview.StatusCode);
        Assert.IsFalse(Directory.Exists(Path.Combine(fixture.Options.FeatureDirectory, id.ToString())));
    }

    [TestMethod]
    public async Task APreviewNobodyWorksOnGoesOffline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Forgotten");

        await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        var features = fixture.Application.Services.GetRequiredService<IFeatureService>();

        Assert.AreEqual(0, features.RunMaintenance(DateTime.UtcNow), "not while it is being worked on");

        Assert.AreEqual(1, features.RunMaintenance(DateTime.UtcNow + fixture.Options.DeploymentLifetime + TimeSpan.FromHours(1)));

        using var preview = await fixture.GetAsync(feature.PreviewPath);

        Assert.AreEqual(HttpStatusCode.NotFound, preview.StatusCode);

        using var kept = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}");

        Assert.AreEqual(HttpStatusCode.OK, kept.StatusCode, "the feature itself stays; only its preview goes");
    }

    [TestMethod]
    public async Task WhatNoFeatureOwnsAnyMoreIsSweptAway()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Kept");

        var id = (fixture.Meta.GetId(lambda.PrivateKey))!.Value;

        // what a request still writing to a deleted feature's data leaves behind
        var orphan = Path.Combine(fixture.Options.FeatureDirectory, id.ToString(), "999999", "workspace");

        Directory.CreateDirectory(orphan);
        await File.WriteAllTextAsync(Path.Combine(orphan, "late.txt"), "written after the feature went");

        var features = fixture.Application.Services.GetRequiredService<IFeatureService>();

        Assert.AreEqual(1, features.Sweep());
        Assert.IsFalse(Directory.Exists(Path.GetDirectoryName(orphan)));

        using var kept = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}");

        Assert.AreEqual(HttpStatusCode.OK, kept.StatusCode, "a feature that exists keeps its files");
    }

    #endregion

    #region Keeping things apart

    [TestMethod]
    public async Task ASaveMadeFromAnEarlierSaveIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Shared");

        using (var first = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/files",
                                                   new FeatureFilesRequest(Says("first").Files, Revision: 1)))
        {
            Assert.AreEqual(HttpStatusCode.OK, first.StatusCode, "made from the save it holds");
        }

        // somebody who read it before that save, saving over it
        using (var stale = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/files",
                                                   new FeatureFilesRequest(Says("second").Files, Revision: 1)))
        {
            Assert.AreEqual(HttpStatusCode.Conflict, stale.StatusCode);
        }

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}");

        Assert.Contains("first", (await read.GetContentAsync<FeatureContentResponse>()).Files.Single().Code, "the first save is not lost");
    }

    [TestMethod]
    public async Task AnotherLambdasKeyReachesNoneOfAFeature()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var owner = await fixture.CreateLambdaAsync();
        var other = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, owner.PrivateKey, "Mine");

        var at = $"/api/v1/lambdas/{other.PrivateKey}/features/{feature.Key}";

        foreach (var (method, path, body) in new (HttpMethod, string, object?)[]
                 {
                     (HttpMethod.Get, at, null),
                     (HttpMethod.Patch, at, new UpdateFeatureRequest(Name: "Theirs")),
                     (HttpMethod.Put, $"{at}/files", new FeatureFilesRequest(Says("taken").Files)),
                     (HttpMethod.Post, $"{at}/preview/start", null),
                     (HttpMethod.Post, $"{at}/merge", new MergeFeatureRequest()),
                     (HttpMethod.Get, $"{at}/logs", null),
                     (HttpMethod.Get, $"{at}/data", null),
                     (HttpMethod.Get, $"{at}/workspace", null),
                     (HttpMethod.Delete, at, null)
                 })
        {
            using var response = body != null ? await fixture.SendAsync(method, path, body) : await fixture.SendAsync(method, path);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, $"{method} {path}");
        }

        using var still = await fixture.GetAsync($"/api/v1/lambdas/{owner.PrivateKey}/features/{feature.Key}");

        var kept = await still.GetContentAsync<FeatureContentResponse>();

        Assert.AreEqual("Mine", kept.Feature.Name);
    }

    [TestMethod]
    public async Task TheCopyOfTheDataIsAllThatIsReached()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Walls");

        foreach (var path in (string[])["..%2Ffiles.json", "..%2Fpreview.json", "..%2F..%2F..%2Fsomething"])
        {
            using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/workspace/{path}");

            Assert.IsFalse(read.IsSuccessStatusCode, path);

            using var written = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/workspace/{path}",
                                                        new FileRequest(Convert.ToBase64String(Encoding.UTF8.GetBytes("nope"))));

            Assert.IsFalse(written.IsSuccessStatusCode, path);
        }

        using var content = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}");

        Assert.AreEqual(HttpStatusCode.OK, content.StatusCode, "its files are as they were");
    }

    [TestMethod]
    public async Task APreviewGoesOnWhileTheLambdaGoesOffline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, SaysCode("live"));

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "Counting");

        await PutFilesAsync(fixture, lambda.PrivateKey, feature.Key,
                            [new LambdaFile(LambdaSource.EntryName, "var count = 0;\nreturn Inline.Create().Get(() => (++count).ToString());")]);

        await StartAsync(fixture, lambda.PrivateKey, feature.Key);

        Assert.AreEqual("1", await ServedAsync(fixture, feature.PreviewPath));
        Assert.AreEqual("2", await ServedAsync(fixture, feature.PreviewPath));

        using (var _ = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/stop")) { }

        Assert.AreEqual("3", await ServedAsync(fixture, feature.PreviewPath), "taking the lambda offline does not start its previews over");
    }

    [TestMethod]
    public async Task TheVersionAFeatureIsBasedOnIsKept()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { MaxVersions = 2 });

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, Says("two"));

        var feature = await CreateAsync(fixture, lambda.PrivateKey, "On two");

        foreach (var text in (string[])["three", "four", "five"])
        {
            await SaveAsync(fixture, lambda.PrivateKey, Says(text));
        }

        using var versions = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions");

        var kept = (await versions.GetContentAsync<List<VersionResponse>>()).Select(v => v.Version).ToList();

        Assert.Contains(2, kept, "what the feature is compared with is not pruned");
        Assert.DoesNotContain(3, kept, "the others are, as before");

        using var based = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/{feature.Base}");

        Assert.AreEqual(HttpStatusCode.OK, based.StatusCode);
    }

    #endregion

    #region Plumbing

    private const string Keeper = """
        return Inline.Create()
                     .Get("read", () => Workspace.Exists("note.txt") ? Workspace.ReadText("note.txt") : "nothing")
                     .Post("write", () => { Workspace.WriteText("note.txt", "written by the code"); return "written"; });
        """;

    private static string SaysCode(string text) => $"return Content.From(Resource.FromString(\"{text}\"));";

    private static VersionRequest Says(string text) => new([new LambdaFile(LambdaSource.EntryName, SaysCode(text))]);

    private static async Task SaveAsync(LambdaFixture fixture, string privateKey, VersionRequest request)
    {
        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/versions", request);

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());
    }

    private static async Task<FeatureResponse> CreateAsync(LambdaFixture fixture, string privateKey, string name, string? specification = null, int? @base = null)
    {
        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/features", new CreateFeatureRequest(name, specification, @base));

        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode, await created.Content.ReadAsStringAsync());

        return await created.GetContentAsync<FeatureResponse>();
    }

    private static async Task PutFilesAsync(LambdaFixture fixture, string privateKey, string feature, IReadOnlyList<LambdaFile> files, string? change = null)
    {
        using var put = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/features/{feature}/files",
                                                new FeatureFilesRequest(files, Change: change));

        Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, await put.Content.ReadAsStringAsync());
    }

    private static async Task<FeaturePreviewResponse> StartAsync(LambdaFixture fixture, string privateKey, string feature)
    {
        using var started = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/features/{feature}/preview/start");

        Assert.AreEqual(HttpStatusCode.OK, started.StatusCode, await started.Content.ReadAsStringAsync());

        return await started.GetContentAsync<FeaturePreviewResponse>();
    }

    private static async Task PutFileAsync(LambdaFixture fixture, string path, string content)
    {
        using var response = await fixture.SendAsync(HttpMethod.Put, path, new FileRequest(Convert.ToBase64String(Encoding.UTF8.GetBytes(content))));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private static async Task<string> ServedAsync(LambdaFixture fixture, string path)
    {
        using var served = await fixture.GetAsync(path);

        return await served.GetContentAsync();
    }

    #endregion

}
