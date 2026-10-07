using System.Net;
using System.Text;
using System.Text.Json;

using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Git;
using GenHTTP.Lambda.Tests.Git;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// A lambda kept before the files it served were its resources and what was
/// written about it was at the top of its code: read, run, changed and
/// cloned in today's layout, with nothing on the disk rewritten.
/// </summary>
[TestClass]
public sealed class LayoutMigrationTests
{

    /// <summary>
    /// What was written about it, as a version kept it then.
    /// </summary>
    private const string Product = "# Veteran\n\nA page that was there before the resources were.\n";

    /// <summary>
    /// A version as it was stored before: a front end and a migration at the
    /// root, the documentation, a test and a build folder below .lambda/,
    /// and code that names Assets, as it was written then.
    /// </summary>
    private static string FirstLayout(string page) => JsonSerializer.Serialize(new
    {
        version = 1,
        files = new object[]
        {
            new
            {
                name = "lambda.cs",
                code = """
                       using (var connection = Database.GetConnection())
                       {
                           new Evolve(connection) { Locations = [Assets.Root + "migrations"], IsEraseDisabled = true }.Migrate();
                       }

                       return Layout.Create()
                                    .Add("count", Inline.Create().Get(() => Counter.Count()))
                                    .Add(Assets.App("web"));
                       """
            },
            new { name = "counter.cs", code = "public static class Counter\n{\n    public static long Count()\n    {\n        using var db = LambdaEnvironment.Database.GetConnection();\n        using var command = db.CreateCommand();\n        command.CommandText = \"SELECT count(*) FROM visits\";\n        return (long)command.ExecuteScalar()!;\n    }\n}\n" },
            new { name = "web/index.html", code = page },
            new { name = "migrations/V1__Create_visits.sql", code = "CREATE TABLE visits (id INTEGER PRIMARY KEY);" },
            new { name = ".lambda/docs/product.md", code = Product },
            new { name = ".lambda/tests/smoke.mjs", code = "check();" },
            new { name = ".lambda/build/web/package.json", code = "{}" }
        }
    }, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    [TestMethod]
    public async Task ALambdaKeptInTheFirstLayoutRunsAsItDid()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("veteran");

        await StoreAsync(fixture, lambda, FirstLayout("<h1>still here</h1>"));

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        var deployed = await fixture.DeployAsync(lambda.PrivateKey);

        Assert.IsTrue(deployed.Success, string.Join("; ", deployed.Diagnostics.Select(d => d.Message)));

        Assert.AreEqual("<h1>still here</h1>", await fixture.CallAsync("/lambda/veteran/"), "what it served it serves, from where it was");
        Assert.AreEqual("0", await fixture.CallAsync("/lambda/veteran/count"), "and its migration ran, where its code looks for it");

        var files = (await fixture.VersionAsync(lambda, 2)).Files.Select(f => f.Name).ToList();

        CollectionAssert.AreEqual(new[]
        {
            "lambda.cs", "counter.cs", "resources/web/index.html", "resources/migrations/V1__Create_visits.sql", "docs/product.md", "tests/smoke.mjs",
            "build/web/package.json"
        }, files, "read in today's layout");

        using var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");

        var read = await summary.GetContentAsync<LambdaSummaryResponse>();

        Assert.AreEqual(2, read.Storage.ResourceFiles);
        Assert.IsTrue(read.Storage.ServesResources, "the old name is read as what it serves");
        Assert.AreEqual("A page that was there before the resources were.", read.Documentation.About);

        // changed in a feature, which starts as a copy of what is stored
        var feature = await fixture.CreateFeatureAsync(lambda, "Brighter");

        using (var changed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/changes",
                                                     new VersionChangeRequest(null, null, [new FileEdit("resources/web/index.html", "still here", "still here, brighter")])))
        {
            Assert.AreEqual(HttpStatusCode.OK, changed.StatusCode, await changed.Content.ReadAsStringAsync());
        }

        using (var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/merge", new MergeFeatureRequest(Deploy: true)))
        {
            Assert.AreEqual(HttpStatusCode.Created, merged.StatusCode, await merged.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("<h1>still here, brighter</h1>", await fixture.CallAsync("/lambda/veteran/"));

        var stored = await File.ReadAllTextAsync(VersionFile(fixture, lambda, 3));

        StringAssert.StartsWith(stored, "{\"version\":2,", "what is saved now is saved in today's layout");
        StringAssert.StartsWith(await File.ReadAllTextAsync(VersionFile(fixture, lambda, 2)), "{\"version\":1,", "and what was saved then is left as it was");
    }

    [TestMethod]
    public async Task AFeatureKeptInTheFirstLayoutIsReadInTodays()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("drafter");

        var feature = await fixture.CreateFeatureAsync(lambda, "Old draft");

        var id = fixture.Meta.GetId(lambda.PrivateKey)!.Value;

        var directory = Directory.GetDirectories(Path.Combine(fixture.Options.FeatureDirectory, id.ToString())).Single();

        await File.WriteAllTextAsync(Path.Combine(directory, "files.json"), FirstLayout("<h1>a draft of then</h1>"));

        var files = (await fixture.FeatureAsync(lambda, feature.Key)).Files.Select(f => f.Name).ToList();

        CollectionAssert.Contains(files, "resources/web/index.html");
        CollectionAssert.Contains(files, "docs/product.md");
        CollectionAssert.DoesNotContain(files, "web/index.html");
    }

    [TestMethod]
    public async Task AHistoryReadBeforeIsLaidOutAnewOnTopOfIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("historic");

        await StoreAsync(fixture, lambda, FirstLayout("<h1>two</h1>"));

        var started = await fixture.CreateFeatureAsync(lambda, "Draft");

        await fixture.PutFeatureAsync(lambda, started.Key, "Drafts a page",
                                      [.. LambdaSource.Parse(FirstLayout("<h1>drafted</h1>"))]);

        var feature = (await fixture.FeatureAsync(lambda, started.Key)).Feature;

        // the commits a clone read before: version 1 and 2, and the feature
        // on top of 2, laid out as they were then - assets/ for what is served
        var store = new GitStore(fixture.Options);

        var id = fixture.Meta.GetId(lambda.PrivateKey)!.Value;

        var first = await OldCommitAsync(store, id, [], 1, "Version 1", ("Project.cs", "public static class Project { }\n", "lambda.cs"));

        var second = await OldCommitAsync(store, id, [first], 2, "Adds a page",
                                          ("Project.cs", "public static class Project { }\n", "lambda.cs"),
                                          ("assets/web/index.html", "<h1>two</h1>", "web/index.html"),
                                          ("docs/product.md", Product, ".lambda/docs/product.md"));

        var drafted = await OldCommitAsync(store, id, [second], null, "Draft",
                                           ("Project.cs", "public static class Project { }\n", "lambda.cs"),
                                           ("assets/web/index.html", "<h1>drafted</h1>", "web/index.html"));

        store.WriteIndex(id, new GitIndex(new Dictionary<int, string> { [1] = first.ToString(), [2] = second.ToString() },
                                          new Dictionary<string, FeatureTip> { [feature.Key] = new(drafted.ToString(), feature.Revision, 2) }, []));

        var clone = await git.CloneAsync(fixture.EditorUrl(lambda), "historic");

        Assert.AreEqual("<h1>two</h1>", git.Read("historic", "resources/web/index.html"), "main is laid out as today");
        Assert.AreEqual(Product, git.Read("historic", "docs/product.md"));
        Assert.IsFalse(Directory.Exists(Path.Combine(clone, "assets")));

        Assert.Contains("Moves assets/ to resources/", await git.ReadAsync("historic", "log", "-1", "--format=%s"), "by a commit on top of the version");
        Assert.AreEqual(second.ToString(), await git.ReadAsync("historic", "rev-parse", "HEAD~1"), "which follows the history as it was");
        Assert.AreEqual(second.ToString(), await git.ReadAsync("historic", "rev-parse", "v2^{commit}"), "and leaves the tags where they were");

        Assert.AreEqual("<h1>drafted</h1>", await git.ReadAsync("historic", "show", $"origin/{feature.Branch}:resources/web/index.html"),
                        "a feature's branch is laid out anew on top of its tip as well");

        // a branch built on the feature as a clone of before had it, in assets/
        await git.RunAsync("historic", "switch", "-c", "older", drafted.ToString());

        git.Write("historic", "assets/web/index.html", "<h1>drafted, and mine</h1>");

        await git.CommitAsync("historic", "Changes the draft");

        var built = await git.TryAsync("historic", "push", "--force", "origin", $"older:{feature.Branch}");

        Assert.AreNotEqual(0, built.ExitCode);
        Assert.Contains("made before the platform laid the lambda's files out anew", built.Said, "a push of before is told what happened");

        // a clone of before: main at the old tip, with a change of its own made in assets/
        await git.RunAsync("historic", "switch", "main");
        await git.RunAsync("historic", "update-ref", "refs/remotes/origin/main", second.ToString());
        await git.RunAsync("historic", "reset", "--hard", second.ToString());

        git.Write("historic", "assets/web/index.html", "<h1>two, and mine</h1>");

        await git.CommitAsync("historic", "Changes the page");

        var refused = await git.TryAsync("historic", "push");

        Assert.AreNotEqual(0, refused.ExitCode, "main moved on since this clone fetched it");

        await git.RunAsync("historic", "pull", "--rebase");

        Assert.AreEqual("<h1>two, and mine</h1>", git.Read("historic", "resources/web/index.html"), "git moved the change along");

        var pushed = await git.RunAsync("historic", "push");

        Assert.Contains("Saved version 3", pushed.Said);

        var files = (await fixture.VersionAsync(lambda, 3)).Files;

        Assert.AreEqual("<h1>two, and mine</h1>", files.Single(f => f.Name == "resources/web/index.html").Code);
        Assert.AreEqual(Product, files.Single(f => f.Name == "docs/product.md").Code);
    }

    #region Helpers

    /// <summary>
    /// Stores a version as it was stored before: saved through the API to be
    /// a version, then its file put back as the first layout wrote it.
    /// </summary>
    private static async Task StoreAsync(LambdaFixture fixture, LambdaResponse lambda, string stored)
    {
        await fixture.SaveAsync(lambda, "Adds a page", LambdaSource.Parse(stored).ToArray());

        await File.WriteAllTextAsync(VersionFile(fixture, lambda, 2), stored);
    }

    private static string VersionFile(LambdaFixture fixture, LambdaResponse lambda, int version)
        => Path.Combine(fixture.Options.CodeDirectory, fixture.Meta.GetId(lambda.PrivateKey)!.ToString()!, $"v{version}.cs");

    /// <summary>
    /// A commit kept as it was made before there was more than one layout:
    /// without one, its files named as the lambda named them then.
    /// </summary>
    private static async Task<GitObjectId> OldCommitAsync(GitStore store, long lambda, GitObjectId[] parents, int? version, string message,
                                                          params (string Path, string Content, string Name)[] files)
    {
        // what the platform put around a lambda then
        (string Path, string Content, string? Name)[] tree =
        [
            .. files.Select(f => (f.Path, f.Content, (string?)f.Name)),
            ("Program.cs", "// the platform's, as it was\n", null),
            ("historic.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\" />\n", null),
            ("AGENTS.md", "# as it was\n", null)
        ];

        var built = GitTree.Create();

        var entries = new List<StoredEntry>();

        var stored = new List<StoredFile>();

        foreach (var (path, content, name) in tree)
        {
            var bytes = Encoding.UTF8.GetBytes(content);

            var blob = GitObjectId.ForBlob(bytes);

            store.WriteBlob(lambda, blob, bytes);

            built.Add(path, content);

            entries.Add(new StoredEntry(path, blob.ToString()));

            if (name != null)
            {
                stored.Add(new StoredFile(name, path, blob.ToString(), false));
            }
        }

        var builder = GitCommit.Create()
                               .Tree(await built.Build().ComputeIdAsync())
                               .Author(new GitSignature("GenHTTP Lambda", "lambda@localhost", new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero)))
                               .Message(message);

        foreach (var parent in parents)
        {
            builder.Parent(parent);
        }

        var commit = builder.Build();

        entries.Sort((x, y) => string.CompareOrdinal(x.Path, y.Path));

        store.WriteCommit(lambda, commit.Id, new StoredCommit(Convert.ToBase64String(commit.Data.Span), entries, stored, version));

        return commit.Id;
    }

    #endregion

}
