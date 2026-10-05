using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Git;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// The build folder of a version: what its assets are built from, in
/// .lambda/build/ - kept with the version wherever it goes, and never built,
/// compiled or served by the platform.
/// </summary>
[TestClass]
public sealed class BuildFolderTests
{
    private const string Snippet = "return Layout.Create().Add(Assets.Files());";

    private const string Package = """{ "name": "shop", "scripts": { "build": "vite build" }, "devDependencies": { "vite": "^5.4.0" } }""";

    private const string Readme = "# How it is built\n\n`cd build/web && npm ci && npm run build` writes `assets/web/`.\n";

    /// <summary>
    /// A version with a front end built from a project, and the project beside it.
    /// </summary>
    private static readonly LambdaFile[] Built =
    [
        new(LambdaSource.EntryName, Snippet),
        new("index.html", "<p>what the build wrote</p>"),
        new(LambdaSource.BuildReadme, Readme),
        new(".lambda/build/web/package.json", Package),
        new(".lambda/build/web/.gitignore", "node_modules/\ndist/\n"),
        new(".lambda/build/web/index.html", "<p>the page before it is built</p>"),
        // a tool of the project written in C#, and nothing that compiles
        new(".lambda/build/web/Tool.cs", "this is no C# the lambda has")
    ];

    #region Kept with the version

    [TestMethod]
    public async Task TheBuildFolderIsKeptWithTheVersionAndNeitherCompiledNorServed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("built");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        var deployed = await fixture.DeployAsync(lambda.PrivateKey);

        Assert.IsTrue(deployed.Success, "a C# file of the build folder is not compiled");

        Assert.AreEqual("<p>what the build wrote</p>", await fixture.CallAsync("/lambda/built/index.html"), "what the build wrote is served");

        using (var source = await fixture.GetAsync("/lambda/built/.lambda/build/web/index.html"))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, source.StatusCode, "what it was built from is not");
        }

        // a change of the code alone keeps it, as it keeps the documentation
        using (var changed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/changes",
                                                     new VersionChangeRequest(null, null, [new FileEdit(LambdaSource.EntryName, "Assets.Files()", "Assets.Files() ")])))
        {
            Assert.AreEqual(HttpStatusCode.Created, changed.StatusCode);
        }

        var kept = (await fixture.VersionAsync(lambda, 3)).Files;

        Assert.AreEqual(Package, kept.Single(f => f.Name == ".lambda/build/web/package.json").Code);
        Assert.AreEqual("node_modules/\ndist/\n", kept.Single(f => f.Name == ".lambda/build/web/.gitignore").Code, "dot files included");

        using var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");

        var read = await summary.GetContentAsync<LambdaSummaryResponse>();

        Assert.AreEqual(1, read.Storage.CodeFiles, "nothing of it is counted as code");
        Assert.AreEqual(1, read.Storage.Assets, "nor as an asset");
        Assert.AreEqual(5, read.Build.Files, "the overview says how much of it there is");
        Assert.AreEqual(5, read.Build.NewestFiles);

        using var alone = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/3?folder=.lambda/build/");

        Assert.HasCount(5, (await alone.GetContentAsync<VersionContentResponse>()).Files, "it is read on its own");
    }

    [TestMethod]
    public async Task TheSummarySaysWhenOnlyTheNewestVersionHasOne()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("ahead");

        await fixture.DeployAsync(lambda.PrivateKey, Snippet);

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        using var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");

        var build = (await summary.GetContentAsync<LambdaSummaryResponse>()).Build;

        Assert.AreEqual(0, build.Files, "the version online has none");
        Assert.AreEqual(5, build.NewestFiles, "the newest, not online yet, has one - which is what the editor shows its section for");
    }

    [TestMethod]
    public async Task AFeatureCarriesItsBuildFolderIntoTheVersionItBecomes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("drafted");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        var feature = await fixture.CreateFeatureAsync(lambda, "Dark mode");

        Assert.IsTrue((await fixture.FeatureAsync(lambda, feature.Key)).Files.Any(f => f.Name == ".lambda/build/web/package.json"), "a feature starts with it");

        await fixture.PutFeatureAsync(lambda, feature.Key, "Adds a dark mode",
            [.. Built.Where(f => f.Name != ".lambda/build/web/index.html"), new LambdaFile(".lambda/build/web/dark.css", "body { background: black }")]);

        using var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/merge", new MergeFeatureRequest());

        Assert.AreEqual(HttpStatusCode.Created, merged.StatusCode, await merged.Content.ReadAsStringAsync());

        var files = (await fixture.VersionAsync(lambda, 3)).Files.Select(f => f.Name).ToList();

        CollectionAssert.Contains(files, ".lambda/build/web/dark.css");
        CollectionAssert.DoesNotContain(files, ".lambda/build/web/index.html");
    }

    #endregion

    #region Names and room

    [TestMethod]
    public async Task TheBuildFolderTakesTheNamesABuildToolsFilesHave()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // what routers read off file names, and the dot files of a project
        await fixture.SaveAsync(lambda, "Has a project",
            new LambdaFile(LambdaSource.EntryName, Snippet),
            new LambdaFile(".lambda/build/web/src/routes/[id]/+page.svelte", "<h1>{id}</h1>"),
            new LambdaFile(".lambda/build/web/src/(auth)/login.tsx", "export {}"),
            new LambdaFile(".lambda/build/web/src/routes/$slug.{lang}.tsx", "export {}"),
            new LambdaFile(".lambda/build/web/.npmrc", "engine-strict=true"),
            new LambdaFile(".lambda/build/.editorconfig", "root = true"),
            new LambdaFile(".lambda/build/web/public/logo.png", Convert.ToBase64String([137, 80, 78, 71]), "base64"));

        foreach (var name in (string[])
                 [
                     ".lambda/build/web/.git/config",
                     ".lambda/build/my notes.txt",
                     ".lambda/build/",
                     ".lambda/build/web/../../secret.txt",
                     ".lambda/build/a:b.txt",
                     ".lambda/builds/x.js",
                 ])
        {
            using var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                        new VersionRequest([new(LambdaSource.EntryName, Snippet), new(name, "x")]));

            Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, $"'{name}' is refused");

            StringAssert.Contains(await refused.Content.ReadAsStringAsync(), name, "and the refusal says which file");
        }

        using var misplaced = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                      new VersionRequest([new(LambdaSource.EntryName, Snippet), new(".lambda/output/x.js", "x")]));

        StringAssert.Contains(await misplaced.Content.ReadAsStringAsync(), LambdaSource.BuildFolder, "it says where a project goes");
    }

    [TestMethod]
    public async Task TheBuildFolderCountsTowardsWhatTheAssetsMayComeTo()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { MaxAssetBytes = 1024, PremiumMaxAssetBytes = 1024 });

        var lambda = await fixture.CreateLambdaAsync();

        using var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                    new VersionRequest([new(LambdaSource.EntryName, Snippet), new(".lambda/build/web/package-lock.json", new string('x', 2000))]));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "every version carries its own copy, like an asset");

        var said = await refused.Content.ReadAsStringAsync();

        StringAssert.Contains(said, "the build folder");
        StringAssert.Contains(said, ".gitignore", "and what is likeliest to have made it large is named, with what keeps it out");
    }

    #endregion

    #region Zip

    [TestMethod]
    public async Task AZipKeepsTheDotFilesOfTheBuildFolderAndLeavesOutWhatItIgnores()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // as a folder somebody built in is zipped, the folder included
        var archive = Zip(
            ("app/lambda.cs", Snippet),
            ("app/index.html", "<p>built</p>"),
            ("app/.DS_Store", "litter"),
            ("app/.lambda/build/.gitignore", "*.tmp\n"),
            ("app/.lambda/build/web/.gitignore", "node_modules/\n/dist\n*.log\n!keep.log\n!important.tmp\n"),
            ("app/.lambda/build/web/package.json", Package),
            ("app/.lambda/build/web/.npmrc", "engine-strict=true"),
            ("app/.lambda/build/web/keep.log", "kept, as the rules take it back"),
            ("app/.lambda/build/web/debug.log", "left out"),
            ("app/.lambda/build/web/important.tmp", "kept: the deeper file has the last word"),
            ("app/.lambda/build/web/scratch.tmp", "left out by the file above"),
            ("app/.lambda/build/web/node_modules/vite/package.json", "{}"),
            ("app/.lambda/build/web/node_modules/.gitignore", "!*"),
            ("app/.lambda/build/web/dist/index.js", "left out"),
            ("app/.lambda/build/web/src/dist/notes.md", "kept: /dist is the project's own only"),
            ("app/.lambda/build/web/.git/HEAD", "a repository"),
            ("app/.lambda/build/.DS_Store", "litter"));

        using var request = fixture.Host.GetRequest($"/api/v1/lambdas/{lambda.PrivateKey}/versions/zip", HttpMethod.Post);

        request.Content = new ByteArrayContent(archive);
        request.Content.Headers.ContentType = new("application/zip");

        using var saved = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        var names = (await fixture.VersionAsync(lambda, 2)).Files.Select(f => f.Name).Order(StringComparer.Ordinal).ToList();

        CollectionAssert.AreEqual(new[]
        {
            ".lambda/build/.gitignore",
            ".lambda/build/web/.gitignore",
            ".lambda/build/web/.npmrc",
            ".lambda/build/web/important.tmp",
            ".lambda/build/web/keep.log",
            ".lambda/build/web/package.json",
            ".lambda/build/web/src/dist/notes.md",
            "index.html",
            "lambda.cs"
        }, names);
    }

    [TestMethod]
    public void WhatIsLeftOutFollowsTheRulesOfGit()
    {
        var ignored = IgnoredPaths.Of(
        [
            (".gitignore", "# a comment\n\n*.log\nbuild/\n/top.txt\ndocs/**/draft.md\n**/cache\n[ab].tmp\nout/\n!out/keep.js\n\\#hash\ntrailing.txt   \n"),
            ("web/.gitignore", "!error.log\n")
        ]);

        foreach (var (path, expected) in (List<(string, bool)>)
                 [
                     ("debug.log", true),
                     ("web/debug.log", true),
                     ("web/error.log", false),            // a deeper file takes it back
                     ("build/index.js", true),
                     ("build", false),                    // a file called build is no folder
                     ("web/build/index.js", true),        // no slash before: any depth
                     ("top.txt", true),
                     ("web/top.txt", false),              // anchored to the folder of its file
                     ("docs/draft.md", true),             // ** is any number of folders, none included
                     ("docs/a/b/draft.md", true),
                     ("web/docs/draft.md", false),
                     ("cache/x", true),
                     ("web/deep/cache/x", true),
                     ("a.tmp", true),
                     ("c.tmp", false),
                     ("out/keep.js", true),               // nothing comes back from a folder left out
                     ("#hash", true),
                     ("trailing.txt", true),
                     ("src/app.ts", false),
                 ])
        {
            Assert.AreEqual(expected, ignored.Ignores(path), path);
        }
    }

    [TestMethod]
    public async Task AZipCanBeLaidOutAsACloneBothWays()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("laid");

        const string store = "public class Shelf { }\n";

        await fixture.SaveAsync(lambda, "Builds the front end", [.. Built, new LambdaFile("store.cs", store)]);

        using var downloaded = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2/zip?layout=project");

        Assert.AreEqual(HttpStatusCode.OK, downloaded.StatusCode);

        var entries = Entries(await downloaded.Content.ReadAsByteArrayAsync());

        CollectionAssert.AreEquivalent(new[]
        {
            "Project.cs", "Store.cs", "assets/index.html", "build/README.md", "build/web/package.json", "build/web/.gitignore",
            "build/web/index.html", "build/web/Tool.cs"
        }, entries.Keys.ToList(), "where a clone has them, and nothing of the platform's");

        StringAssert.Contains(entries["Project.cs"], "BuildAsync()", "the snippet in the class it is the body of, as in a clone");

        // changed where it was built, as a folder somebody built in is zipped
        var archive = Zip(
        [
            .. entries.Select(e => ($"laid/{e.Key}", e.Key == "assets/index.html" ? "<p>built again</p>" : e.Value)),
            ("laid/build/web/src/main.ts", "console.log('built');"),
            ("laid/build/web/node_modules/vite/package.json", "{}"),
            ("laid/bin/Release/laid.dll", "what dotnet run wrote"),
            ("laid/Program.cs", "the platform's"),
            ("laid/laid.csproj", "the platform's"),
            ("laid/.gitignore", "the platform's"),
            ("laid/.git/HEAD", "a repository")
        ]);

        var saved = await UploadAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/zip?layout=project", HttpMethod.Post, archive);

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        var files = (await fixture.VersionAsync(lambda, 3)).Files;

        Assert.AreEqual(Snippet, files[0].Code, "Project.cs left as it was is the snippet it was, to the byte");
        Assert.AreEqual(store, files.Single(f => f.Name == "store.cs").Code, "named as the lambda named it");
        Assert.AreEqual("<p>built again</p>", files.Single(f => f.Name == "index.html").Code);
        Assert.AreEqual("console.log('built');", files.Single(f => f.Name == ".lambda/build/web/src/main.ts").Code);

        Assert.IsFalse(files.Any(f => f.Name.Contains("node_modules", StringComparison.Ordinal) || f.Name.Contains("Release", StringComparison.Ordinal)),
                       "what the build folder and the repository ignore stays out");

        Assert.IsFalse(files.Any(f => f.Name.Contains("Program", StringComparison.Ordinal) || f.Name.EndsWith(".csproj", StringComparison.Ordinal)),
                       "and so do the platform's files");
    }

    [TestMethod]
    public async Task AZipLaidOutAsACloneRefusesWhatHasNoPlaceInIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("strict");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        using var downloaded = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2/zip?layout=project");

        var entries = Entries(await downloaded.Content.ReadAsByteArrayAsync());

        var refused = await UploadAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/zip?layout=project", HttpMethod.Post,
                                        Zip([.. entries.Select(e => (e.Key, e.Value)), ("web/package.json", Package)]));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);
        StringAssert.Contains(await refused.Content.ReadAsStringAsync(), "build/", "it says where such files go");

        using var unknown = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2/zip?layout=tree");

        Assert.AreEqual(HttpStatusCode.BadRequest, unknown.StatusCode);
    }

    [TestMethod]
    public async Task AFeatureComesAndGoesLaidOutAsAClone()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("drafty");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        var feature = await fixture.CreateFeatureAsync(lambda, "Dark mode");

        using var downloaded = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/zip?layout=project");

        var entries = Entries(await downloaded.Content.ReadAsByteArrayAsync());

        entries["build/web/index.html"] = "<p>darker</p>";

        var saved = await UploadAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/zip?layout=project", HttpMethod.Put,
                                      Zip([.. entries.Select(e => (e.Key, e.Value))]));

        Assert.AreEqual(HttpStatusCode.OK, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        var files = (await fixture.FeatureAsync(lambda, feature.Key)).Files;

        CollectionAssert.AreEqual(Built.Select(f => f.Name).ToList(), files.Select(f => f.Name).ToList(), "the same files, in the same order");
        Assert.AreEqual("<p>darker</p>", files.Single(f => f.Name == ".lambda/build/web/index.html").Code);
    }

    #endregion

    #region Agents

    [TestMethod]
    public async Task AnAgentIsHandedTheReadmeAndTheNamesOfTheBuildFolder()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        var read = await ToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey });

        var build = read["build"]!.AsObject();

        Assert.AreEqual(Readme, build["readme"]!["content"]!.GetValue<string>(), "how it is built, in full");

        var listed = build["files"]!.AsArray().Select(f => f!["name"]!.GetValue<string>()).ToList();

        CollectionAssert.Contains(listed, ".lambda/build/web/package.json");
        Assert.IsNull(build["files"]![0]!["code"], "by name and length, not by content");

        var program = read["files"]!.AsArray().Select(f => f!["name"]!.GetValue<string>()).ToList();

        CollectionAssert.AreEquivalent(new[] { LambdaSource.EntryName, "index.html" }, program, "the program's files are the program's");

        var one = await ToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["file"] = ".lambda/build/web/package.json" });

        Assert.AreEqual(Package, one["files"]![0]!["code"]!.GetValue<string>(), "one file is read by its name");

        var guide = await ToolAsync(fixture, "platform_guide", new JsonObject());

        StringAssert.Contains(guide["build"]!["nothingIsBuiltHere"]!.GetValue<string>(), "never builds anything");
    }

    [TestMethod]
    public async Task SavingEveryFileWithoutTheBuildFolderSaysSo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        // change_code keeps what it is not told about
        var changed = await ToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["edits"] = new JsonArray(new JsonObject { ["file"] = "index.html", ["find"] = "wrote", ["replace"] = "wrote again" })
        });

        Assert.IsNull(changed["build"], "nothing is said where nothing was left out");

        var written = await ToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = LambdaSource.EntryName, ["code"] = Snippet })
        });

        StringAssert.Contains(written["build"]!.GetValue<string>(), "5 files in .lambda/build/", "an agent that did not know there was one is told what it left out");
    }

    #endregion

    #region Taken away

    [TestMethod]
    public async Task TheExportCarriesTheBuildFolderAndBuildsWithoutIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("exported");

        // a project of its own in there, and one a build would trip over
        await fixture.SaveAsync(lambda, "Builds the front end",
        [
            .. Built,
            new LambdaFile(".lambda/build/tool/tool.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>"),
            new LambdaFile(".lambda/build/tool/Program.cs", "System.Console.WriteLine(\"a tool\");")
        ]);

        using var answer = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export");

        var directory = Path.Combine(fixture.Options.DataDirectory, "exports", "exported");

        await ZipFile.ExtractToDirectoryAsync(new MemoryStream(await answer.Content.ReadAsByteArrayAsync()), directory);

        var project = Path.Combine(directory, "exported");

        Assert.AreEqual(Package, await File.ReadAllTextAsync(Path.Combine(project, "build/web/package.json")), "it is where a project keeps it");
        Assert.AreEqual(Readme, await File.ReadAllTextAsync(Path.Combine(project, "build/README.md")));
        Assert.Contains("build/", await File.ReadAllTextAsync(Path.Combine(project, ".dockerignore")), "and stays out of the image");
        Assert.Contains("build/ is what its assets or code are built from", await File.ReadAllTextAsync(Path.Combine(project, "Program.cs")));

        var (exit, output) = await BuildAsync(project);

        Assert.AreEqual(0, exit, $"what is in build/ is no part of the build:\n{output}");
    }

    [TestMethod]
    public async Task AClonePushesTheBuildFolderAsBuild()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("cloned");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        var clone = await git.CloneAsync(fixture.EditorUrl(lambda), "cloned");

        Assert.AreEqual(Package, git.Read("cloned", "build/web/package.json"));
        Assert.AreEqual("node_modules/\ndist/\n", git.Read("cloned", "build/web/.gitignore"));
        Assert.IsFalse(Directory.Exists(Path.Combine(clone, ".lambda")));

        Assert.Contains("DefaultItemExcludes", git.Read("cloned", "cloned.csproj"), "the build does not look into it");
        Assert.Contains("build/", git.Read("cloned", ".dockerignore"));
        Assert.Contains("## What it is built from: build/", git.Read("cloned", "AGENTS.md"), "the agent working here is told how");
        Assert.Contains("!/build/", git.Read("cloned", ".gitignore"), "taken back in, whatever a global ignore says of a folder called build");

        // what a build installed stays out, by the project's own .gitignore
        git.Write("cloned", "build/web/node_modules/vite/package.json", "{}");
        git.Write("cloned", "build/web/src/main.ts", "console.log('built');");
        git.Write("cloned", "assets/web/main.js", "console.log('built');");

        await git.CommitAsync("cloned", "Builds main.ts into the assets");

        var pushed = await git.RunAsync("cloned", "push");

        Assert.Contains("Saved version 3", pushed.Said);

        var files = (await fixture.VersionAsync(lambda, 3)).Files.Select(f => f.Name).ToList();

        CollectionAssert.IsSubsetOf(new[] { ".lambda/build/web/src/main.ts", "web/main.js" }, files, "the sources and what they built, in one version");
        Assert.IsFalse(files.Any(f => f.Contains("node_modules", StringComparison.Ordinal)));

        // a project of its own in there does not trip the lambda's build
        git.Write("cloned", "build/tool/tool.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
        git.Write("cloned", "build/tool/Program.cs", "this is no C# the lambda has");

        var (exit, output) = await BuildAsync(clone);

        Assert.AreEqual(0, exit, $"what is in build/ is no part of the build:\n{output}");
    }

    [TestMethod]
    public async Task APushedFileOutsideTheLambdaNamesTheBuildFolder()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("strayed");

        await git.CloneAsync(fixture.EditorUrl(lambda), "strayed");

        git.Write("strayed", "web/package.json", Package);

        await git.CommitAsync("strayed", "Adds a project at the root");

        var refused = await git.TryAsync("strayed", "push");

        Assert.IsFalse(refused.Success);
        Assert.Contains("build/", refused.Said, "it says where such a project goes");
    }

    [TestMethod]
    public async Task APublishedSourceShowsItsBuildFolder()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("shared");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        await fixture.PublishAsync(lambda);

        using var tree = await fixture.GetAsync("/api/v1/sources/shared/versions/2");

        var files = (await tree.GetContentAsync<SourceTreeResponse>()).Files;

        Assert.AreEqual("build", files.Single(f => f.Path == "build/web/package.json").Kind, "marked as what it is");
        Assert.AreEqual("build", files.Single(f => f.Path == "build/README.md").Kind);
    }

    #endregion

    #region Helpers

    private static async Task<JsonObject> ToolAsync(LambdaFixture fixture, string tool, JsonObject arguments)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");

        request.Content = new StringContent(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments }
        }.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        var answer = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        return answer["result"]!["structuredContent"]!.AsObject();
    }

    private static Dictionary<string, string> Entries(byte[] zip)
    {
        using var archive = new ZipArchive(new MemoryStream(zip));

        return archive.Entries.ToDictionary(e => e.FullName, e => new StreamReader(e.Open()).ReadToEnd());
    }

    private static async Task<HttpResponseMessage> UploadAsync(LambdaFixture fixture, string path, HttpMethod method, byte[] archive)
    {
        using var request = fixture.Host.GetRequest(path, method);

        request.Content = new ByteArrayContent(archive);
        request.Content.Headers.ContentType = new("application/zip");

        return await fixture.Host.GetResponseAsync(request);
    }

    private static byte[] Zip(params (string Name, string Content)[] files)
    {
        using var buffer = new MemoryStream();

        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            foreach (var (name, content) in files)
            {
                using var target = zip.CreateEntry(name).Open();

                target.Write(Encoding.UTF8.GetBytes(content));
            }
        }

        return buffer.ToArray();
    }

    private static async Task<(int, string)> BuildAsync(string directory)
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

        return (process.ExitCode, await output + await error);
    }

    #endregion

}
