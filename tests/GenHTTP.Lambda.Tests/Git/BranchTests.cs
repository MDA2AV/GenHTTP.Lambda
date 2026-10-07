using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Git;

/// <summary>
/// Pushing a branch: a feature, with its own copy of the data and its preview
/// online, and merged once it is right - the way a change to a lambda in use
/// is made.
/// </summary>
[TestClass]
public sealed class BranchTests
{

    [TestMethod]
    public async Task ABranchPushedIsAFeatureWithItsPreviewOnline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("branched");

        await fixture.SaveAsync(lambda, "Says live", new LambdaFile(LambdaSource.EntryName, Repository.Says("live")));
        await fixture.DeployAsync(lambda.PrivateKey);

        await git.CloneAsync(fixture.EditorUrl(lambda), "branched");

        await git.RunAsync("branched", "switch", "-c", "louder");
        git.Write("branched", "Project.cs", PushTests.ProjectOf(Repository.Says("LIVE")));
        await git.CommitAsync("branched", "Says it louder");

        var pushed = await git.RunAsync("branched", "push", "-u", "origin", "louder");

        Assert.Contains("Started the feature 'louder' from version 2", pushed.Said);

        var feature = (await fixture.FeaturesAsync(lambda)).Single();

        Assert.AreEqual("louder", feature.Branch);
        Assert.AreEqual("git", feature.Origin);
        Assert.AreEqual(2, feature.Base);
        Assert.AreEqual("Says it louder", feature.Change, "what its first commit says it changes");
        Assert.IsTrue(feature.Online);

        Assert.Contains($"Its preview is online at {fixture.Host.GetUrl(feature.PreviewPath)}", pushed.Said);

        Assert.AreEqual("LIVE", await fixture.CallAsync(feature.PreviewPath));
        Assert.AreEqual("live", await fixture.CallAsync("/lambda/branched/"), "the lambda goes on as it was");

        // pushed again, its files are replaced - a rewrite included
        git.Write("branched", "Project.cs", PushTests.ProjectOf(Repository.Says("LIVE!")));
        await git.RunAsync("branched", "add", "-A");
        await git.RunAsync("branched", "commit", "-q", "--amend", "--no-edit");
        await git.RunAsync("branched", "push", "--force");

        Assert.AreEqual("LIVE!", await fixture.CallAsync(feature.PreviewPath));
        Assert.Contains("LIVE!", (await fixture.FeatureAsync(lambda, feature.Key)).Files.Single().Code);
    }

    [TestMethod]
    public async Task ABranchPushedWithMergeIsTheNextVersion()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("squashed");

        await git.CloneAsync(fixture.EditorUrl(lambda), "squashed");

        await git.RunAsync("squashed", "switch", "-c", "greeting");

        git.Write("squashed", "resources/one.txt", "one");
        await git.CommitAsync("squashed", "Adds one");
        await git.RunAsync("squashed", "push", "-u", "origin", "greeting");

        git.Write("squashed", "Project.cs", PushTests.ProjectOf(Repository.Says("merged")));
        await git.CommitAsync("squashed", "Says merged");

        var pushed = await git.RunAsync("squashed", "push", "-o", "merge", "-o", "deploy");

        Assert.Contains("Merged it as version 2", pushed.Said);
        Assert.Contains("Version 2 is online", pushed.Said);

        Assert.IsEmpty(await fixture.FeaturesAsync(lambda), "the feature is gone, with its preview and its copy of the data");
        Assert.AreEqual("merged", await fixture.CallAsync("/lambda/squashed/"));

        Assert.HasCount(2, await fixture.VersionsAsync(lambda), "a feature merged is one version, however many commits it had");

        await git.RunAsync("squashed", "switch", "main");
        await git.RunAsync("squashed", "pull");
        await git.RunAsync("squashed", "fetch", "--prune");

        Assert.AreEqual(string.Empty, await git.ReadAsync("squashed", "branch", "-r", "--list", "origin/greeting"));
        Assert.Contains("one", git.Read("squashed", "resources/one.txt"));
    }

    [TestMethod]
    public async Task ABranchPushedToMainIsMergedAVersionForEachCommit()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("forwarded");

        await git.CloneAsync(fixture.EditorUrl(lambda), "forwarded");

        await git.RunAsync("forwarded", "switch", "-c", "two-steps");

        git.Write("forwarded", "resources/one.txt", "one");
        await git.CommitAsync("forwarded", "Adds one");

        git.Write("forwarded", "resources/two.txt", "two");
        await git.CommitAsync("forwarded", "Adds two");

        await git.RunAsync("forwarded", "push", "-u", "origin", "two-steps");

        var pushed = await git.RunAsync("forwarded", "push", "origin", "two-steps:main");

        Assert.Contains("Saved versions 2 to 3", pushed.Said);
        Assert.Contains("The feature 'two-steps' is in main now, so it is merged", pushed.Said);

        Assert.IsEmpty(await fixture.FeaturesAsync(lambda));

        CollectionAssert.AreEqual(new[] { "Adds two", "Adds one", "Started empty" }, (await fixture.VersionsAsync(lambda)).Select(v => v.Change).ToList());
    }

    [TestMethod]
    public async Task ABranchDeletedTakesItsFeatureWithIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("pruned");

        var feature = await fixture.CreateFeatureAsync(lambda, "Not wanted");

        await git.CloneAsync(fixture.EditorUrl(lambda), "pruned");

        var deleted = await git.RunAsync("pruned", "push", "origin", "--delete", feature.Branch);

        Assert.Contains("Deleted the feature 'Not wanted'", deleted.Said);

        Assert.IsEmpty(await fixture.FeaturesAsync(lambda));
    }

    [TestMethod]
    public async Task ABranchThatStartsFromNoVersionIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("orphaned");

        await git.CloneAsync(fixture.EditorUrl(lambda), "orphaned");

        await git.RunAsync("orphaned", "switch", "--orphan", "elsewhere");
        await git.RunAsync("orphaned", "checkout", "main", "--", ".");
        await git.CommitAsync("orphaned", "From nowhere");

        var pushed = await git.TryAsync("orphaned", "push", "origin", "elsewhere");

        Assert.AreNotEqual(0, pushed.ExitCode);
        Assert.Contains("A branch starts from main or one of its versions", pushed.Said);

        Assert.IsEmpty(await fixture.FeaturesAsync(lambda));
    }

    [TestMethod]
    public async Task ABranchRefusedLeavesNoFeatureBehind()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var limits = fixture.Limits.Get();

        fixture.Limits.Save(limits with { Free = limits.Free with { BuildBytes = 1024 } });

        var lambda = await fixture.CreateLambdaAsync("toolarge");

        await git.CloneAsync(fixture.EditorUrl(lambda), "toolarge");

        await git.RunAsync("toolarge", "switch", "-c", "pictures");
        git.Write("toolarge", "resources/large.txt", new string('x', 4096));
        await git.CommitAsync("toolarge", "Adds something large");

        var pushed = await git.TryAsync("toolarge", "push", "-u", "origin", "pictures");

        Assert.AreNotEqual(0, pushed.ExitCode);
        Assert.Contains("must not exceed", pushed.Said, "the tier's refusal, in its words");

        Assert.IsEmpty(await fixture.FeaturesAsync(lambda), "the feature started for the branch went with the refusal");
    }

    [TestMethod]
    public async Task AFeatureChangedElsewhereIsNotOverwrittenByAPush()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("raced");

        var feature = await fixture.CreateFeatureAsync(lambda, "Shared");

        await git.CloneAsync(fixture.EditorUrl(lambda), "raced");

        await git.RunAsync("raced", "switch", feature.Branch);

        // the agent saves it meanwhile
        await fixture.PutFeatureAsync(lambda, feature.Key, "The agent's change", new LambdaFile(LambdaSource.EntryName, Repository.Says("agent")));

        git.Write("raced", "resources/mine.txt", "mine");
        await git.CommitAsync("raced", "Mine");

        var pushed = await git.TryAsync("raced", "push");

        Assert.AreNotEqual(0, pushed.ExitCode);

        Assert.Contains("agent", (await fixture.FeatureAsync(lambda, feature.Key)).Files.Single(f => f.Name == LambdaSource.EntryName).Code);
    }

}
