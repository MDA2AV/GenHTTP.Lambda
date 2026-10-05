using System.Diagnostics;
using System.Net;

using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Git;

/// <summary>
/// A lambda cloned with git: the project it is exported as, with every
/// version a commit of main and every feature a branch.
/// </summary>
[TestClass]
public sealed class CloneTests
{

    #region The project

    [TestMethod]
    public async Task ALambdaClonesAsTheProjectItIsExportedAs()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("shelf", "demo-crud");

        var clone = await git.CloneAsync(fixture.EditorUrl(lambda), "shelf");

        foreach (var wanted in (string[])["Project.cs", "Program.cs", "shelf.csproj", "Store.cs", "Tasks.cs", "Platform/Usings.cs", "Platform/Database.cs",
                                          "assets/web/index.html", "assets/migrations/V1__Create_tasks.sql", "docs/product.md", "tests/README.md",
                                          "AGENTS.md", "CLAUDE.md", "Dockerfile", ".gitignore"])
        {
            Assert.IsTrue(File.Exists(Path.Combine(clone, wanted)), $"{wanted} is in the clone");
        }

        Assert.IsFalse(File.Exists(Path.Combine(clone, "lambda.cs")), "the snippet is Project.cs");
        Assert.IsFalse(Directory.Exists(Path.Combine(clone, ".lambda")), "the documentation and the tests are where a project keeps them");
        Assert.IsFalse(Directory.Exists(Path.Combine(clone, "database")), "no data is in a repository");

        Assert.Contains("private static async Task<object> BuildAsync()", git.Read("shelf", "Project.cs"), "the snippet is the body of the method the platform runs");
        Assert.Contains(".Handler(await Project.CreateAsync())", git.Read("shelf", "Program.cs"));
        Assert.Contains("@AGENTS.md", git.Read("shelf", "CLAUDE.md"));
    }

    [TestMethod]
    public async Task TheGuideForAgentsNamesTheLambdaAndNotItsKey()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("guided");

        await git.CloneAsync(fixture.EditorUrl(lambda), "guided");

        var guide = git.Read("guided", "AGENTS.md");

        Assert.Contains("# guided", guide);
        Assert.Contains("/lambda/guided/", guide, "where it runs");
        Assert.Contains("git push -o deploy", guide);
        Assert.DoesNotContain("{lambda}", guide, "every placeholder is filled in");
        Assert.DoesNotContain("{home}", guide);

        foreach (var file in Directory.EnumerateFiles(git.PathOf("guided"), "*", SearchOption.AllDirectories).Where(f => !f.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}")))
        {
            Assert.DoesNotContain(lambda.PrivateKey, await File.ReadAllTextAsync(file), $"{file} never holds the editor key - the same commits are read by anybody once the source is published");
        }
    }

    [TestMethod]
    public async Task EveryDemoClonesToAProjectThatBuilds()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var builds = new List<Task<(string Demo, int Exit, string Output)>>();

        foreach (var demo in DemoCatalog.All)
        {
            var lambda = await fixture.CreateLambdaAsync($"clone-{demo.Id}", demo.Id);

            var clone = await git.CloneAsync(fixture.EditorUrl(lambda), demo.Id);

            builds.Add(BuildAsync(demo.Id, clone));
        }

        foreach (var (demo, exit, output) in await Task.WhenAll(builds))
        {
            Assert.AreEqual(0, exit, $"The clone of {demo} does not build:\n{output}");
        }
    }

    #endregion

    #region History

    [TestMethod]
    public async Task EveryVersionIsACommitOfMainTaggedWithItsNumber()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("history");

        await fixture.SaveAsync(lambda, "Says one", new LambdaFile(LambdaSource.EntryName, Repository.Says("one")));
        await fixture.SaveAsync(lambda, "Says two", new LambdaFile(LambdaSource.EntryName, Repository.Says("two")));

        await git.CloneAsync(fixture.EditorUrl(lambda), "history");

        var log = await git.ReadAsync("history", "log", "--format=%s", "main");

        CollectionAssert.AreEqual(new[] { "Says two", "Says one", "Started empty" }, log.Split('\n'), "the change of each version, newest first");

        Assert.AreEqual("v1\nv2\nv3", await git.ReadAsync("history", "tag", "--list", "--sort=version:refname"));

        Assert.AreEqual(await git.ReadAsync("history", "rev-parse", "main"), await git.ReadAsync("history", "rev-parse", "v3^{commit}"));
        Assert.AreEqual("GenHTTP Lambda", await git.ReadAsync("history", "log", "-1", "--format=%an"));
    }

    [TestMethod]
    public async Task ACommitIsTheSameCommitForEverybodyWhoReadsItLater()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("stable");

        await git.CloneAsync(fixture.EditorUrl(lambda), "first");

        await fixture.SaveAsync(lambda, "Says more", new LambdaFile(LambdaSource.EntryName, Repository.Says("more")));

        await git.CloneAsync(fixture.EditorUrl(lambda), "second");

        Assert.AreEqual(await git.ReadAsync("first", "rev-parse", "v1"), await git.ReadAsync("second", "rev-parse", "v1"), "a commit is made once and kept");

        // the clone made before the version follows it without a forced update
        var fetched = await git.RunAsync("first", "pull", "--ff-only");

        Assert.AreEqual(await git.ReadAsync("second", "rev-parse", "main"), await git.ReadAsync("first", "rev-parse", "main"), fetched.Said);
    }

    [TestMethod]
    public async Task AVersionThatOnlyChangesAnAssetChangesOnlyThatFile()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("pictures");

        var code = new LambdaFile(LambdaSource.EntryName, "return Assets.App(\"web\");\n");

        await fixture.SaveAsync(lambda, "Has a page", code, new LambdaFile("web/index.html", "<h1>one</h1>"));
        await fixture.SaveAsync(lambda, "Changes the page", code, new LambdaFile("web/index.html", "<h1>two</h1>"));

        await git.CloneAsync(fixture.EditorUrl(lambda), "pictures");

        Assert.AreEqual("assets/web/index.html", await git.ReadAsync("pictures", "diff", "--name-only", "v2", "v3"));
    }

    #endregion

    #region Features

    [TestMethod]
    public async Task AFeatureIsABranchNamedAfterIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("drafts");

        var feature = await fixture.CreateFeatureAsync(lambda, "Dark mode for the page");

        Assert.AreEqual("dark-mode-for-the-page", feature.Branch);

        await git.CloneAsync(fixture.EditorUrl(lambda), "drafts");

        Assert.AreEqual(await git.ReadAsync("drafts", "rev-parse", "origin/main"), await git.ReadAsync("drafts", "rev-parse", "origin/dark-mode-for-the-page"),
                        "a feature that holds what its version holds is that version");

        await fixture.PutFeatureAsync(lambda, feature.Key, "Says it darkly", new LambdaFile(LambdaSource.EntryName, Repository.Says("dark")));

        await git.RunAsync("drafts", "fetch");

        Assert.AreEqual("Says it darkly", await git.ReadAsync("drafts", "log", "-1", "--format=%s", "origin/dark-mode-for-the-page"));
        Assert.AreEqual(await git.ReadAsync("drafts", "rev-parse", "origin/main"), await git.ReadAsync("drafts", "rev-parse", "origin/dark-mode-for-the-page^"),
                        "its first commit follows the version it started from");

        // renamed, it keeps its branch: a clone knows it by that
        using var renamed = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}", new { name = "Night mode" });

        Assert.AreEqual(HttpStatusCode.OK, renamed.StatusCode);

        Assert.AreEqual("dark-mode-for-the-page", (await fixture.FeaturesAsync(lambda)).Single().Branch);
    }

    [TestMethod]
    public async Task AFeatureBroughtUpToDateFollowsTheVersionItWasBroughtTo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("catchup");

        var feature = await fixture.CreateFeatureAsync(lambda, "Louder");

        await fixture.PutFeatureAsync(lambda, feature.Key, "Says it louder", new LambdaFile(LambdaSource.EntryName, Repository.Says("LOUD")));

        await git.CloneAsync(fixture.EditorUrl(lambda), "catchup");

        var newest = await fixture.SaveAsync(lambda, "Says something else", new LambdaFile(LambdaSource.EntryName, Repository.Says("else")));

        // the agent brings it in, and says so by moving the base
        using var moved = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}", new { @base = newest });

        Assert.AreEqual(HttpStatusCode.OK, moved.StatusCode);

        await git.RunAsync("catchup", "fetch");

        var parents = await git.ReadAsync("catchup", "log", "-1", "--format=%P", "origin/louder");

        Assert.Contains(await git.ReadAsync("catchup", "rev-parse", "origin/main"), parents, "the branch holds the version it was brought to");

        Assert.AreEqual(0, (await git.TryAsync("catchup", "merge-base", "--is-ancestor", "origin/main", "origin/louder")).ExitCode);
    }

    #endregion

    #region Addresses

    [TestMethod]
    public async Task AnyNameBelowTheEditorKeyIsTheSameRepository()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("named");

        await git.CloneAsync(fixture.Host.GetUrl($"/editor/{lambda.PrivateKey}/whatever.git"), "one");
        await git.CloneAsync(fixture.Host.GetUrl($"/editor/{lambda.PrivateKey}"), "two");

        Assert.AreEqual(await git.ReadAsync("one", "rev-parse", "main"), await git.ReadAsync("two", "rev-parse", "main"),
                        "the name only names the folder, so a clone keeps working once the public key changes");
    }

    [TestMethod]
    public async Task AKeyNobodyHasIsNoRepository()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var cloned = await git.TryAsync(null, "clone", fixture.Host.GetUrl("/editor/nobody-has-this-key/x.git"), "nothing");

        Assert.AreNotEqual(0, cloned.ExitCode);

        using var answer = await fixture.GetAsync("/editor/nobody-has-this-key/x.git/info/refs?service=git-upload-pack");

        Assert.AreEqual(HttpStatusCode.NotFound, answer.StatusCode, "never the page, which git would take for a broken server");
    }

    [TestMethod]
    public async Task TheAddressOfARepositoryLeadsABrowserToItsPage()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("browsed");

        using var editor = await fixture.GetAsync($"/editor/{lambda.PrivateKey}/browsed.git");

        Assert.AreEqual(HttpStatusCode.TemporaryRedirect, editor.StatusCode);
        Assert.EndsWith($"/editor/{lambda.PrivateKey}", editor.Headers.Location?.OriginalString ?? string.Empty);

        using var page = await fixture.GetAsync($"/editor/{lambda.PrivateKey}/code");

        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode, "the editor's own pages are still the editor's");
    }

    #endregion

    #region Helpers

    private static async Task<(string, int, string)> BuildAsync(string demo, string directory)
    {
        var start = new ProcessStartInfo("dotnet", "build -c Release -nologo -clp:ErrorsOnly")
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(start)!;

        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return (demo, process.ExitCode, await output + await error);
    }

    #endregion

}
