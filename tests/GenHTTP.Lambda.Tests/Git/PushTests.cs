using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Git;

/// <summary>
/// Pushing to main: every commit becomes the next version of the lambda, and
/// what is no part of the lambda is refused rather than lost.
/// </summary>
[TestClass]
public sealed class PushTests
{

    #region Versions

    [TestMethod]
    public async Task ACommitPushedToMainIsTheNextVersion()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("pushed", "demo-crud");

        await git.CloneAsync(fixture.EditorUrl(lambda), "pushed");

        git.Change("pushed", "Project.cs", "var store = new TaskStore(limit: 200);", "var store = new TaskStore(limit: 300);");

        await git.CommitAsync("pushed", "Keeps three hundred tasks\n\nThe list ran full in a week.");

        var pushed = await git.RunAsync("pushed", "push");

        Assert.Contains("Saved version 2: Keeps three hundred tasks", pushed.Said);

        var version = (await fixture.VersionsAsync(lambda)).First();

        Assert.AreEqual(2, version.Version);
        Assert.AreEqual("Keeps three hundred tasks", version.Change, "the first line of the message is the change");
        Assert.AreEqual("The list ran full in a week.", version.Specification, "the rest is what the user wanted");
        Assert.AreEqual("git", version.Origin);

        var before = (await fixture.VersionAsync(lambda, 1)).Files;
        var after = (await fixture.VersionAsync(lambda, 2)).Files;

        CollectionAssert.AreEqual(before.Select(f => f.Name).ToList(), after.Select(f => f.Name).ToList(), "the files keep their names and their order");

        var code = after.Single(f => f.Name == LambdaSource.EntryName).Code;

        Assert.AreEqual(before.Single(f => f.Name == LambdaSource.EntryName).Code.Replace("limit: 200", "limit: 300"), code,
                        "the body of BuildAsync is lambda.cs again, and only what changed changed");

        Assert.AreEqual(await git.ReadAsync("pushed", "rev-parse", "main"), (await git.ReadAsync("pushed", "ls-remote", "origin", "refs/tags/v2")).Split('\t')[0],
                        "the commit pushed is the version's commit");
    }

    [TestMethod]
    public async Task ACommitThatLeavesProjectCsAloneLeavesTheCodeAsItWas()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("untouched");

        // written in a way putting it into the class and back would not keep
        const string written = "using System.Text;\n\n   // indented oddly\nreturn Content.From(Resource.FromString(new StringBuilder(\"odd\").ToString()));\n\nrecord Unused(int X);\n";

        await fixture.SaveAsync(lambda, "Is odd", new LambdaFile(LambdaSource.EntryName, written), new LambdaFile("store.cs", "public class Shelf { }\n"));

        await git.CloneAsync(fixture.EditorUrl(lambda), "untouched");

        Assert.IsTrue(File.Exists(Path.Combine(git.PathOf("untouched"), "Store.cs")), "named the .NET way");

        git.Write("untouched", "resources/readme.txt", "hello");
        git.Change("untouched", "Store.cs", "Shelf", "Bookshelf");

        await git.CommitAsync("untouched", "Adds a file");
        await git.RunAsync("untouched", "push");

        var files = (await fixture.VersionAsync(lambda, 3)).Files;

        Assert.AreEqual(written, files.Single(f => f.Name == LambdaSource.EntryName).Code, "to the byte");
        Assert.AreEqual("public class Bookshelf { }\n", files.Single(f => f.Name == "store.cs").Code, "under the name the lambda gave it");
        Assert.AreEqual("hello", files.Single(f => f.Name == "resources/readme.txt").Code);
    }

    [TestMethod]
    public async Task EveryCommitOfAPushIsAVersion()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("several");

        await git.CloneAsync(fixture.EditorUrl(lambda), "several");

        foreach (var word in (string[])["one", "two", "three"])
        {
            git.Write("several", "resources/word.txt", word);
            await git.CommitAsync("several", $"Says {word}");
        }

        var pushed = await git.RunAsync("several", "push");

        Assert.Contains("Saved versions 2 to 4", pushed.Said);

        CollectionAssert.AreEqual(new[] { "Says three", "Says two", "Says one", "Started empty" },
                                  (await fixture.VersionsAsync(lambda)).Select(v => v.Change).ToList());

        await git.RunAsync("several", "fetch", "--tags");

        Assert.AreEqual(await git.ReadAsync("several", "rev-parse", "main~1"), await git.ReadAsync("several", "rev-parse", "v3^{commit}"));
    }

    [TestMethod]
    public async Task APushWithDeployPutsTheNewestVersionOnline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("deployed");

        await git.CloneAsync(fixture.EditorUrl(lambda), "deployed");

        git.Write("deployed", "Project.cs", ProjectOf(Repository.Says("pushed and online")));

        await git.CommitAsync("deployed", "Says it is online");

        var pushed = await git.RunAsync("deployed", "push", "-o", "deploy");

        Assert.Contains("Version 2 is online at", pushed.Said);

        Assert.AreEqual("pushed and online", await fixture.CallAsync("http://deployed.localhost/"));
    }

    [TestMethod]
    public async Task APushWithoutDeploySaysWhatIsOnline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("offline");

        await git.CloneAsync(fixture.EditorUrl(lambda), "offline");

        git.Write("offline", "resources/note.txt", "x");

        await git.CommitAsync("offline", "Adds a note");

        var pushed = await git.RunAsync("offline", "push");

        Assert.Contains("Nothing is online yet", pushed.Said);
        Assert.Contains("-o deploy", pushed.Said);
        Assert.DoesNotContain(lambda.PrivateKey, pushed.Said.Replace(fixture.EditorUrl(lambda), string.Empty), "the editor key is never said back");
    }

    #endregion

    #region Refused

    [TestMethod]
    public async Task APushThatDoesNotCompileIsRefusedWithWhereInProjectCs()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("broken", "demo-crud");

        await git.CloneAsync(fixture.EditorUrl(lambda), "broken");

        git.Change("broken", "Project.cs", "new TaskStore(limit: 200)", "new TaskStore(limit: \"200\")");

        await git.CommitAsync("broken", "Breaks it");

        var pushed = await git.TryAsync("broken", "push");

        Assert.AreNotEqual(0, pushed.ExitCode);

        var line = git.Read("broken", "Project.cs").Split('\n').ToList().FindIndex(l => l.Contains("limit: \"200\"")) + 1;

        Assert.Contains($"Project.cs({line},", pushed.Said, "where the problem is, in the file that was pushed");
        Assert.Contains("does not compile", pushed.Said);

        Assert.HasCount(1, await fixture.VersionsAsync(lambda), "no version is made of it");
    }

    [TestMethod]
    public async Task AChangeToAFileOfThePlatformIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("platform");

        await git.CloneAsync(fixture.EditorUrl(lambda), "platform");

        git.Write("platform", "Program.cs", git.Read("platform", "Program.cs") + "// mine\n");

        await git.CommitAsync("platform", "Changes the program");

        var pushed = await git.TryAsync("platform", "push");

        Assert.AreNotEqual(0, pushed.ExitCode);
        Assert.Contains("'Program.cs' is the platform's", pushed.Said);

        await git.RunAsync("platform", "reset", "--hard", "origin/main");

        git.Write("platform", "Platform/Mine.cs", "public class Mine { }");

        await git.CommitAsync("platform", "Adds to the platform");

        var foreign = await git.TryAsync("platform", "push");

        Assert.AreNotEqual(0, foreign.ExitCode);
        Assert.Contains("'Platform/Mine.cs' is no file of the lambda", foreign.Said);

        Assert.HasCount(1, await fixture.VersionsAsync(lambda));

        // anything else is the lambda's code, a README at the top among it
        await git.RunAsync("platform", "reset", "--hard", "origin/main");

        git.Write("platform", "README.md", "# Mine");

        await git.CommitAsync("platform", "Adds a readme");

        await git.RunAsync("platform", "push");

        Assert.AreEqual("# Mine", (await fixture.VersionAsync(lambda, 2)).Files.Single(f => f.Name == "README.md").Code);
    }

    [TestMethod]
    public async Task MainIsNeverRewritten()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("rewritten");

        await fixture.SaveAsync(lambda, "Says two", new LambdaFile(LambdaSource.EntryName, Repository.Says("two")));

        await git.CloneAsync(fixture.EditorUrl(lambda), "rewritten");

        await git.RunAsync("rewritten", "reset", "--hard", "HEAD~1");

        git.Write("rewritten", "resources/other.txt", "other");

        await git.CommitAsync("rewritten", "Goes another way");

        var forced = await git.TryAsync("rewritten", "push", "--force");

        Assert.AreNotEqual(0, forced.ExitCode);
        Assert.Contains("never rewritten", forced.Said);

        var deleted = await git.TryAsync("rewritten", "push", "origin", "--delete", "main");

        Assert.AreNotEqual(0, deleted.ExitCode);
    }

    [TestMethod]
    public async Task AMergeCommitHasNoPlaceOnMain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("merged");

        await git.CloneAsync(fixture.EditorUrl(lambda), "merged");

        await git.RunAsync("merged", "switch", "-c", "side");
        git.Write("merged", "resources/side.txt", "side");
        await git.CommitAsync("merged", "On the side");

        await git.RunAsync("merged", "switch", "main");
        git.Write("merged", "resources/main.txt", "main");
        await git.CommitAsync("merged", "On main");

        await git.RunAsync("merged", "merge", "--no-edit", "side");

        var pushed = await git.TryAsync("merged", "push", "origin", "main");

        Assert.AreNotEqual(0, pushed.ExitCode);
        Assert.Contains("merge commit", pushed.Said);
    }

    [TestMethod]
    public async Task APushBehindAVersionSavedElsewhereIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("behind");

        await git.CloneAsync(fixture.EditorUrl(lambda), "behind");

        await fixture.SaveAsync(lambda, "Saved in the editor", new LambdaFile(LambdaSource.EntryName, Repository.Says("editor")));

        git.Write("behind", "resources/mine.txt", "mine");
        await git.CommitAsync("behind", "Mine");

        var pushed = await git.TryAsync("behind", "push");

        Assert.AreNotEqual(0, pushed.ExitCode, "nothing saved elsewhere is lost");

        await git.RunAsync("behind", "pull", "--rebase");
        await git.RunAsync("behind", "push");

        var versions = await fixture.VersionsAsync(lambda);

        Assert.AreEqual("Mine", versions[0].Change);
        Assert.AreEqual("Saved in the editor", versions[1].Change);
    }

    [TestMethod]
    public async Task TagsAreTheVersionsAndCannotBePushed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("tagged");

        await git.CloneAsync(fixture.EditorUrl(lambda), "tagged");

        await git.RunAsync("tagged", "tag", "release");

        var pushed = await git.TryAsync("tagged", "push", "origin", "release");

        Assert.AreNotEqual(0, pushed.ExitCode);
        Assert.Contains("Tags are the versions", pushed.Said);
    }

    [TestMethod]
    public async Task ADemoIsReadOnlyThroughGitToo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        using var git = new GitClient();

        await git.CloneAsync(fixture.Host.GetUrl("/editor/demo-crud/demo-crud.git"), "demo");

        git.Write("demo", "resources/mine.txt", "mine");
        await git.CommitAsync("demo", "Mine");

        var pushed = await git.TryAsync("demo", "push");

        Assert.AreNotEqual(0, pushed.ExitCode);
        Assert.Contains("is a demo and read only", pushed.Said);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Project.cs holding the given snippet, the way the repository writes it.
    /// </summary>
    internal static string ProjectOf(string snippet) => GenHTTP.Lambda.Services.Deployment.ProjectSnippet.ForRepository(snippet);

    #endregion

}
