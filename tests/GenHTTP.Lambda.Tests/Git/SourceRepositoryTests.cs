using System.Net;

using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Git;

/// <summary>
/// A published source cloned by anybody: the same versions the owner reads,
/// none of the owner's features, nothing written back - and never the data.
/// </summary>
[TestClass]
public sealed class SourceRepositoryTests
{

    [TestMethod]
    public async Task APublishedSourceIsTheOwnersHistoryWithoutTheFeatures()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("opened");

        await fixture.SaveAsync(lambda, "Says hello", new LambdaFile(LambdaSource.EntryName, Repository.Says("hello")));

        var feature = await fixture.CreateFeatureAsync(lambda, "Secret plans");

        await fixture.PutFeatureAsync(lambda, feature.Key, "Plans", new LambdaFile(LambdaSource.EntryName, Repository.Says("plans")));

        await git.CloneAsync(fixture.EditorUrl(lambda), "owner");

        await fixture.PublishAsync(lambda);

        await git.CloneAsync(fixture.SourceUrl("opened"), "public");

        Assert.AreEqual(await git.ReadAsync("owner", "rev-parse", "main"), await git.ReadAsync("public", "rev-parse", "main"), "the same commits");

        Assert.AreEqual("origin/HEAD -> origin/main\norigin/main", string.Join('\n', (await git.ReadAsync("public", "branch", "-r")).Split('\n').Select(l => l.Trim())));

        var plans = await git.ReadAsync("owner", "rev-parse", $"origin/{feature.Branch}");

        var fetched = await git.TryAsync("public", "fetch", "origin", plans);

        Assert.AreNotEqual(0, fetched.ExitCode, "a feature's commit is not found by its id either");
    }

    [TestMethod]
    public async Task ACommitMadeWhileTheSourceIsPublishedCarriesItsLicense()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("licensed");

        await fixture.PublishAsync(lambda, "Apache-2.0");

        await git.CloneAsync(fixture.SourceUrl("licensed"), "licensed");

        Assert.Contains("Apache License", git.Read("licensed", "LICENSE"));
    }

    [TestMethod]
    public async Task ASourceNotPublishedIsNoRepository()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        await fixture.CreateLambdaAsync("closed");

        var cloned = await git.TryAsync(null, "clone", fixture.SourceUrl("closed"), "closed");

        Assert.AreNotEqual(0, cloned.ExitCode);

        using var answer = await fixture.GetAsync("/source/closed.git/info/refs?service=git-upload-pack");

        Assert.AreEqual(HttpStatusCode.NotFound, answer.StatusCode, "the same as a key nobody has");
    }

    [TestMethod]
    public async Task APublishedSourceIsNotPushedTo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("readonly");

        await fixture.PublishAsync(lambda);

        await git.CloneAsync(fixture.SourceUrl("readonly"), "readonly");

        git.Write("readonly", "resources/mine.txt", "mine");
        await git.CommitAsync("readonly", "Mine");

        var pushed = await git.TryAsync("readonly", "push");

        Assert.AreNotEqual(0, pushed.ExitCode);

        Assert.HasCount(1, await fixture.VersionsAsync(lambda));
    }

    [TestMethod]
    public async Task TheDataIsNeverInTheRepository()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("private", "demo-crud");

        await fixture.DeployAsync(lambda.PrivateKey);

        using var added = await fixture.SendAsync(HttpMethod.Post, "http://private.localhost/tasks/", new { title = "a-record-nobody-else-sees" });

        Assert.IsTrue(added.IsSuccessStatusCode, await added.Content.ReadAsStringAsync());

        using var secrets = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/secrets");
        using var secret = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/API_KEY", new { value = "a-value-nobody-else-sees" });

        Assert.AreEqual(HttpStatusCode.OK, secret.StatusCode, await secret.Content.ReadAsStringAsync());

        var clone = await git.CloneAsync(fixture.EditorUrl(lambda), "private");

        foreach (var file in Directory.EnumerateFiles(clone, "*", SearchOption.AllDirectories))
        {
            var content = await File.ReadAllTextAsync(file);

            Assert.DoesNotContain("a-record-nobody-else-sees", content, file);
            Assert.DoesNotContain("a-value-nobody-else-sees", content, file);
        }
    }

}
