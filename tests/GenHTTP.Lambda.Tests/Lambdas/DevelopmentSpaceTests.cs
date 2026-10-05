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
/// The development space of a version: what its assets are built from, in
/// .lambda/dev/ - kept with the version wherever it goes, and never built,
/// compiled or served by the platform.
/// </summary>
[TestClass]
public sealed class DevelopmentSpaceTests
{
    private const string Snippet = "return Layout.Create().Add(Assets.Files());";

    private const string Package = """{ "name": "shop", "scripts": { "build": "vite build" }, "devDependencies": { "vite": "^5.4.0" } }""";

    private const string Readme = "# How it is built\n\n`cd dev/web && npm ci && npm run build` writes `assets/web/`.\n";

    /// <summary>
    /// A version with a front end built from a project, and the project beside it.
    /// </summary>
    private static readonly LambdaFile[] Built =
    [
        new(LambdaSource.EntryName, Snippet),
        new("index.html", "<p>what the build wrote</p>"),
        new(LambdaSource.DevelopmentReadme, Readme),
        new(".lambda/dev/web/package.json", Package),
        new(".lambda/dev/web/.gitignore", "node_modules/\ndist/\n"),
        new(".lambda/dev/web/index.html", "<p>the page before it is built</p>"),
        // a tool of the project written in C#, and nothing that compiles
        new(".lambda/dev/web/Tool.cs", "this is no C# the lambda has")
    ];

    #region Kept with the version

    [TestMethod]
    public async Task TheDevelopmentSpaceIsKeptWithTheVersionAndNeitherCompiledNorServed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("built");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        var deployed = await fixture.DeployAsync(lambda.PrivateKey);

        Assert.IsTrue(deployed.Success, "a C# file of the development space is not compiled");

        Assert.AreEqual("<p>what the build wrote</p>", await fixture.CallAsync("/lambda/built/index.html"), "what the build wrote is served");

        using (var source = await fixture.GetAsync("/lambda/built/.lambda/dev/web/index.html"))
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

        Assert.AreEqual(Package, kept.Single(f => f.Name == ".lambda/dev/web/package.json").Code);
        Assert.AreEqual("node_modules/\ndist/\n", kept.Single(f => f.Name == ".lambda/dev/web/.gitignore").Code, "dot files included");

        using var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");

        var read = await summary.GetContentAsync<LambdaSummaryResponse>();

        Assert.AreEqual(1, read.Storage.CodeFiles, "nothing of it is counted as code");
        Assert.AreEqual(1, read.Storage.Assets, "nor as an asset");
        Assert.AreEqual(5, read.Development.Files, "the overview says how much of it there is");
        Assert.AreEqual(5, read.Development.NewestFiles);

        using var alone = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/3?folder=.lambda/dev/");

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

        var development = (await summary.GetContentAsync<LambdaSummaryResponse>()).Development;

        Assert.AreEqual(0, development.Files, "the version online has none");
        Assert.AreEqual(5, development.NewestFiles, "the newest, not online yet, has one - which is what the editor shows its section for");
    }

    [TestMethod]
    public async Task AFeatureCarriesItsDevelopmentSpaceIntoTheVersionItBecomes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("drafted");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        var feature = await fixture.CreateFeatureAsync(lambda, "Dark mode");

        Assert.IsTrue((await fixture.FeatureAsync(lambda, feature.Key)).Files.Any(f => f.Name == ".lambda/dev/web/package.json"), "a feature starts with it");

        await fixture.PutFeatureAsync(lambda, feature.Key, "Adds a dark mode",
            [.. Built.Where(f => f.Name != ".lambda/dev/web/index.html"), new LambdaFile(".lambda/dev/web/dark.css", "body { background: black }")]);

        using var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/merge", new MergeFeatureRequest());

        Assert.AreEqual(HttpStatusCode.Created, merged.StatusCode, await merged.Content.ReadAsStringAsync());

        var files = (await fixture.VersionAsync(lambda, 3)).Files.Select(f => f.Name).ToList();

        CollectionAssert.Contains(files, ".lambda/dev/web/dark.css");
        CollectionAssert.DoesNotContain(files, ".lambda/dev/web/index.html");
    }

    #endregion

    #region Names and room

    [TestMethod]
    public async Task TheDevelopmentSpaceTakesTheNamesAProjectHas()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // what routers read off file names, and the dot files of a project
        await fixture.SaveAsync(lambda, "Has a project",
            new LambdaFile(LambdaSource.EntryName, Snippet),
            new LambdaFile(".lambda/dev/web/src/routes/[id]/+page.svelte", "<h1>{id}</h1>"),
            new LambdaFile(".lambda/dev/web/src/(auth)/login.tsx", "export {}"),
            new LambdaFile(".lambda/dev/web/src/routes/$slug.{lang}.tsx", "export {}"),
            new LambdaFile(".lambda/dev/web/.npmrc", "engine-strict=true"),
            new LambdaFile(".lambda/dev/.editorconfig", "root = true"),
            new LambdaFile(".lambda/dev/web/public/logo.png", Convert.ToBase64String([137, 80, 78, 71]), "base64"));

        foreach (var name in (string[])
                 [
                     ".lambda/dev/web/.git/config",
                     ".lambda/dev/my notes.txt",
                     ".lambda/dev/",
                     ".lambda/dev/web/../../secret.txt",
                     ".lambda/dev/a:b.txt",
                     ".lambda/devtools/x.js",
                 ])
        {
            using var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                        new VersionRequest([new(LambdaSource.EntryName, Snippet), new(name, "x")]));

            Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, $"'{name}' is refused");

            StringAssert.Contains(await refused.Content.ReadAsStringAsync(), name, "and the refusal says which file");
        }

        using var misplaced = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                      new VersionRequest([new(LambdaSource.EntryName, Snippet), new(".lambda/build/x.js", "x")]));

        StringAssert.Contains(await misplaced.Content.ReadAsStringAsync(), LambdaSource.DevelopmentFolder, "it says where a project goes");
    }

    [TestMethod]
    public async Task TheDevelopmentSpaceCountsTowardsWhatTheAssetsMayComeTo()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { MaxAssetBytes = 1024, PremiumMaxAssetBytes = 1024 });

        var lambda = await fixture.CreateLambdaAsync();

        using var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                    new VersionRequest([new(LambdaSource.EntryName, Snippet), new(".lambda/dev/web/package-lock.json", new string('x', 2000))]));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "every version carries its own copy, like an asset");

        var said = await refused.Content.ReadAsStringAsync();

        StringAssert.Contains(said, "the development space");
        StringAssert.Contains(said, ".gitignore", "and what is likeliest to have made it large is named, with what keeps it out");
    }

    #endregion

    #region Zip

    [TestMethod]
    public async Task AZipKeepsTheDotFilesOfTheDevelopmentSpaceAndLeavesOutWhatItIgnores()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // as a folder somebody built in is zipped, the folder included
        var archive = Zip(
            ("app/lambda.cs", Snippet),
            ("app/index.html", "<p>built</p>"),
            ("app/.DS_Store", "litter"),
            ("app/.lambda/dev/.gitignore", "*.tmp\n"),
            ("app/.lambda/dev/web/.gitignore", "node_modules/\n/dist\n*.log\n!keep.log\n!important.tmp\n"),
            ("app/.lambda/dev/web/package.json", Package),
            ("app/.lambda/dev/web/.npmrc", "engine-strict=true"),
            ("app/.lambda/dev/web/keep.log", "kept, as the rules take it back"),
            ("app/.lambda/dev/web/debug.log", "left out"),
            ("app/.lambda/dev/web/important.tmp", "kept: the deeper file has the last word"),
            ("app/.lambda/dev/web/scratch.tmp", "left out by the file above"),
            ("app/.lambda/dev/web/node_modules/vite/package.json", "{}"),
            ("app/.lambda/dev/web/node_modules/.gitignore", "!*"),
            ("app/.lambda/dev/web/dist/index.js", "left out"),
            ("app/.lambda/dev/web/src/dist/notes.md", "kept: /dist is the project's own only"),
            ("app/.lambda/dev/web/.git/HEAD", "a repository"),
            ("app/.lambda/dev/.DS_Store", "litter"));

        using var request = fixture.Host.GetRequest($"/api/v1/lambdas/{lambda.PrivateKey}/versions/zip", HttpMethod.Post);

        request.Content = new ByteArrayContent(archive);
        request.Content.Headers.ContentType = new("application/zip");

        using var saved = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        var names = (await fixture.VersionAsync(lambda, 2)).Files.Select(f => f.Name).Order(StringComparer.Ordinal).ToList();

        CollectionAssert.AreEqual(new[]
        {
            ".lambda/dev/.gitignore",
            ".lambda/dev/web/.gitignore",
            ".lambda/dev/web/.npmrc",
            ".lambda/dev/web/important.tmp",
            ".lambda/dev/web/keep.log",
            ".lambda/dev/web/package.json",
            ".lambda/dev/web/src/dist/notes.md",
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

    #endregion

    #region Agents

    [TestMethod]
    public async Task AnAgentIsHandedTheReadmeAndTheNamesOfTheDevelopmentSpace()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        var read = await ToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey });

        var development = read["development"]!.AsObject();

        Assert.AreEqual(Readme, development["readme"]!["content"]!.GetValue<string>(), "how it is built, in full");

        var listed = development["files"]!.AsArray().Select(f => f!["name"]!.GetValue<string>()).ToList();

        CollectionAssert.Contains(listed, ".lambda/dev/web/package.json");
        Assert.IsNull(development["files"]![0]!["code"], "by name and length, not by content");

        var program = read["files"]!.AsArray().Select(f => f!["name"]!.GetValue<string>()).ToList();

        CollectionAssert.AreEquivalent(new[] { LambdaSource.EntryName, "index.html" }, program, "the program's files are the program's");

        var one = await ToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["file"] = ".lambda/dev/web/package.json" });

        Assert.AreEqual(Package, one["files"]![0]!["code"]!.GetValue<string>(), "one file is read by its name");

        var guide = await ToolAsync(fixture, "platform_guide", new JsonObject());

        StringAssert.Contains(guide["development"]!["nothingIsBuiltHere"]!.GetValue<string>(), "never builds anything");
    }

    [TestMethod]
    public async Task SavingEveryFileWithoutTheDevelopmentSpaceSaysSo()
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

        Assert.IsNull(changed["development"], "nothing is said where nothing was left out");

        var written = await ToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = LambdaSource.EntryName, ["code"] = Snippet })
        });

        StringAssert.Contains(written["development"]!.GetValue<string>(), "5 files in .lambda/dev/", "an agent that did not know there was one is told what it left out");
    }

    #endregion

    #region Taken away

    [TestMethod]
    public async Task TheExportCarriesTheDevelopmentSpaceAndBuildsWithoutIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("exported");

        // a project of its own in there, and one a build would trip over
        await fixture.SaveAsync(lambda, "Builds the front end",
        [
            .. Built,
            new LambdaFile(".lambda/dev/tool/tool.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>"),
            new LambdaFile(".lambda/dev/tool/Program.cs", "System.Console.WriteLine(\"a tool\");")
        ]);

        using var answer = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export");

        var directory = Path.Combine(fixture.Options.DataDirectory, "exports", "exported");

        await ZipFile.ExtractToDirectoryAsync(new MemoryStream(await answer.Content.ReadAsByteArrayAsync()), directory);

        var project = Path.Combine(directory, "exported");

        Assert.AreEqual(Package, await File.ReadAllTextAsync(Path.Combine(project, "dev/web/package.json")), "it is where a project keeps it");
        Assert.AreEqual(Readme, await File.ReadAllTextAsync(Path.Combine(project, "dev/README.md")));
        Assert.Contains("dev/", await File.ReadAllTextAsync(Path.Combine(project, ".dockerignore")), "and stays out of the image");
        Assert.Contains("dev/ is what its assets or code are built from", await File.ReadAllTextAsync(Path.Combine(project, "Program.cs")));

        var (exit, output) = await BuildAsync(project);

        Assert.AreEqual(0, exit, $"what is in dev/ is no part of the build:\n{output}");
    }

    [TestMethod]
    public async Task AClonePushesTheDevelopmentSpaceAsDev()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("cloned");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        var clone = await git.CloneAsync(fixture.EditorUrl(lambda), "cloned");

        Assert.AreEqual(Package, git.Read("cloned", "dev/web/package.json"));
        Assert.AreEqual("node_modules/\ndist/\n", git.Read("cloned", "dev/web/.gitignore"));
        Assert.IsFalse(Directory.Exists(Path.Combine(clone, ".lambda")));

        Assert.Contains("DefaultItemExcludes", git.Read("cloned", "cloned.csproj"), "the build does not look into it");
        Assert.Contains("dev/", git.Read("cloned", ".dockerignore"));
        Assert.Contains("## What is built: dev/", git.Read("cloned", "AGENTS.md"), "the agent working here is told how");

        // what a build installed stays out, by the project's own .gitignore
        git.Write("cloned", "dev/web/node_modules/vite/package.json", "{}");
        git.Write("cloned", "dev/web/src/main.ts", "console.log('built');");
        git.Write("cloned", "assets/web/main.js", "console.log('built');");

        await git.CommitAsync("cloned", "Builds main.ts into the assets");

        var pushed = await git.RunAsync("cloned", "push");

        Assert.Contains("Saved version 3", pushed.Said);

        var files = (await fixture.VersionAsync(lambda, 3)).Files.Select(f => f.Name).ToList();

        CollectionAssert.IsSubsetOf(new[] { ".lambda/dev/web/src/main.ts", "web/main.js" }, files, "the sources and what they built, in one version");
        Assert.IsFalse(files.Any(f => f.Contains("node_modules", StringComparison.Ordinal)));

        // a project of its own in there does not trip the lambda's build
        git.Write("cloned", "dev/tool/tool.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
        git.Write("cloned", "dev/tool/Program.cs", "this is no C# the lambda has");

        var (exit, output) = await BuildAsync(clone);

        Assert.AreEqual(0, exit, $"what is in dev/ is no part of the build:\n{output}");
    }

    [TestMethod]
    public async Task APushedFileOutsideTheLambdaNamesTheDevelopmentSpace()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("strayed");

        await git.CloneAsync(fixture.EditorUrl(lambda), "strayed");

        git.Write("strayed", "web/package.json", Package);

        await git.CommitAsync("strayed", "Adds a project at the root");

        var refused = await git.TryAsync("strayed", "push");

        Assert.IsFalse(refused.Success);
        Assert.Contains("dev/", refused.Said, "it says where such a project goes");
    }

    [TestMethod]
    public async Task APublishedSourceShowsItsDevelopmentSpace()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("shared");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        await fixture.PublishAsync(lambda);

        using var tree = await fixture.GetAsync("/api/v1/sources/shared/versions/2");

        var files = (await tree.GetContentAsync<SourceTreeResponse>()).Files;

        Assert.AreEqual("dev", files.Single(f => f.Path == "dev/web/package.json").Kind, "marked as what it is");
        Assert.AreEqual("dev", files.Single(f => f.Path == "dev/README.md").Kind);
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
