using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Git;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// The code of a version: every file that is not a resource, in whatever
/// folders - its C# compiled in any of them, as a C# project compiles its
/// own, everything else kept with the version wherever it goes, and never
/// built, compiled or served by the platform.
/// </summary>
[TestClass]
public sealed class CodeTests
{
    private const string Snippet = "return Layout.Create().Add(Resources.Files());";

    /// <summary>A snippet that needs a type of a folder of the code.</summary>
    private const string Greeting = "return Layout.Create().Add(Resources.Files()).Add(\"greeting\", Content.From(Resource.FromString(Words.Greeting)));";

    private const string Package = """{ "name": "shop", "scripts": { "build": "vite build" }, "devDependencies": { "vite": "^5.4.0" } }""";

    private const string Readme = "# How it is built\n\n`cd frontend && npm ci && npm run build` writes `resources/`.\n";

    /// <summary>
    /// A version with a front end built from a project, and the project beside it.
    /// </summary>
    private static readonly LambdaFile[] Built =
    [
        new(LambdaSource.EntryName, Snippet),
        new("resources/index.html", "<p>what the build wrote</p>"),
        new("frontend/README.md", Readme),
        new("frontend/package.json", Package),
        new("frontend/.gitignore", "node_modules/\ndist/\n"),
        new("frontend/index.html", "<p>the page before it is built</p>"),
        // C# in a folder, compiled with the rest
        new("models/Words.cs", "public static class Words { public const string Greeting = \"hello from a folder\"; }")
    ];

    #region Kept with the version

    [TestMethod]
    public async Task TheCodeIsKeptWithTheVersionAndItsCSharpIsCompiledInAnyFolder()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("built");

        await fixture.SaveAsync(lambda, "Builds the front end", [new LambdaFile(LambdaSource.EntryName, Greeting), .. Built.Skip(1)]);

        var deployed = await fixture.DeployAsync(lambda.PrivateKey);

        Assert.IsTrue(deployed.Success, string.Join("\n", deployed.Diagnostics.Select(d => d.Message)));

        Assert.AreEqual("hello from a folder", await fixture.CallAsync("/lambda/built/greeting"), "a C# file in a folder is compiled with the snippet, as in a C# project");
        Assert.AreEqual("<p>what the build wrote</p>", await fixture.CallAsync("/lambda/built/index.html"), "a resource is served by its name below resources/");

        foreach (var path in (string[]) ["/lambda/built/frontend/index.html", "/lambda/built/resources/index.html", "/lambda/built/lambda.cs"])
        {
            using var code = await fixture.GetAsync(path);

            Assert.AreEqual(HttpStatusCode.NotFound, code.StatusCode, $"{path}: the code is not served");
        }

        // a change of the snippet alone keeps the rest
        using (var changed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/changes",
                                                     new VersionChangeRequest(null, null, [new FileEdit(LambdaSource.EntryName, "Resources.Files()", "Resources.Files() ")])))
        {
            Assert.AreEqual(HttpStatusCode.Created, changed.StatusCode);
        }

        var kept = (await fixture.VersionAsync(lambda, 3)).Files;

        Assert.AreEqual(Package, kept.Single(f => f.Name == "frontend/package.json").Code);
        Assert.AreEqual("node_modules/\ndist/\n", kept.Single(f => f.Name == "frontend/.gitignore").Code, "dot files included");

        using var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");

        var read = await summary.GetContentAsync<LambdaSummaryResponse>();

        Assert.AreEqual(6, read.Storage.CodeFiles, "every file that is not a resource is code");
        Assert.AreEqual(1, read.Storage.ResourceFiles);

        using var alone = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/3?folder=frontend/");

        Assert.HasCount(4, (await alone.GetContentAsync<VersionContentResponse>()).Files, "a folder is read on its own");

        // and what is no C# the lambda has does not compile, whichever folder it is in
        await fixture.SaveAsync(lambda, "A tool of its own", [.. Built, new LambdaFile("tools/Tool.cs", "this is no C# the lambda has")]);

        using var deployment = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/start", new DeploymentRequest(null));

        var refused = await deployment.GetContentAsync<DeploymentOutcomeResponse>();

        Assert.IsFalse(refused.Success);
        Assert.IsTrue(refused.Diagnostics.Any(d => d.File == "tools/Tool.cs"), "the compiler names the file, folder and all");
    }

    [TestMethod]
    public async Task AFeatureCarriesItsFoldersIntoTheVersionItBecomes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("drafted");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        var feature = await fixture.CreateFeatureAsync(lambda, "Dark mode");

        Assert.IsTrue((await fixture.FeatureAsync(lambda, feature.Key)).Files.Any(f => f.Name == "frontend/package.json"), "a feature starts with it");

        await fixture.PutFeatureAsync(lambda, feature.Key, "Adds a dark mode",
            [.. Built.Where(f => f.Name != "frontend/index.html"), new LambdaFile("frontend/dark.css", "body { background: black }")]);

        using var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/merge", new MergeFeatureRequest());

        Assert.AreEqual(HttpStatusCode.Created, merged.StatusCode, await merged.Content.ReadAsStringAsync());

        var files = (await fixture.VersionAsync(lambda, 3)).Files.Select(f => f.Name).ToList();

        CollectionAssert.Contains(files, "frontend/dark.css");
        CollectionAssert.DoesNotContain(files, "frontend/index.html");
    }

    #endregion

    #region Names and room

    [TestMethod]
    public async Task TheCodeTakesTheNamesAToolsFilesHave()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // what routers read off file names, the dot files of a project, and a README at the top
        await fixture.SaveAsync(lambda, "Has a project",
            new LambdaFile(LambdaSource.EntryName, Snippet),
            new LambdaFile("README.md", "# The lambda"),
            new LambdaFile(".editorconfig", "root = true"),
            new LambdaFile("frontend/src/routes/[id]/+page.svelte", "<h1>{id}</h1>"),
            new LambdaFile("frontend/src/(auth)/login.tsx", "export {}"),
            new LambdaFile("frontend/src/routes/$slug.{lang}.tsx", "export {}"),
            new LambdaFile("frontend/src/console.ts", "export {}"),
            new LambdaFile("frontend/.npmrc", "engine-strict=true"),
            new LambdaFile("frontend/public/logo.png", Convert.ToBase64String([137, 80, 78, 71]), "base64"),
            new LambdaFile("tools/bin/run.sh", "echo"));

        foreach (var name in (string[])
                 [
                     "frontend/.git/config",
                     "frontend/.GIT/config",          // what a clone refuses in any case
                     "git~1/config",                  // and by its short name on Windows
                     "aux.js",                        // what Windows reserves, with an extension too
                     "src/con.ts",
                     "COM1",
                     "notes.",
                     "my notes.txt",
                     "frontend/",
                     "frontend/../../secret.txt",
                     "a:b.txt",
                     "Dockerfile",                    // what the project of a lambda has for its own at the top
                     "agents.md",
                     ".gitignore",
                     "lambda.csproj",
                     "Platform/Extra.cs",
                     "bin/app.js",
                     "Resources/web/index.html",      // the resources are in lowercase
                     "assets/web/index.html",         // and no longer in assets/
                     "resources/.hidden.css",         // and none of them hidden
                     "resources/web/noextension",
                     "Too Long Name.cs",
                 ])
        {
            using var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                        new VersionRequest([new(LambdaSource.EntryName, Snippet), new(name, "x")]));

            Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, $"'{name}' is refused");

            StringAssert.Contains(await refused.Content.ReadAsStringAsync(), name, "and the refusal says which file");
        }

        using var old = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                new VersionRequest([new(LambdaSource.EntryName, Snippet), new(".lambda/docs/product.md", "# What it is")]));

        Assert.AreEqual(HttpStatusCode.BadRequest, old.StatusCode, "the folder a lambda kept its documentation in once is no more");

        StringAssert.Contains(await old.Content.ReadAsStringAsync(), "docs/ and the tests are in tests/", "it says where it goes now");

        using var both = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                 new VersionRequest([new(LambdaSource.EntryName, Snippet), new("frontend", "x"), new("frontend/x.js", "x")]));

        Assert.AreEqual(HttpStatusCode.BadRequest, both.StatusCode, "a file and a folder of the same name cannot both be on a disk");
    }

    [TestMethod]
    public async Task TheCodeCountsTowardsWhatAVersionMayComeTo()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { BuildBytes = 1024, PremiumBuildBytes = 1024 });

        var lambda = await fixture.CreateLambdaAsync();

        using var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                    new VersionRequest([new(LambdaSource.EntryName, Snippet), new("frontend/package-lock.json", new string('x', 2000))]));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "every version carries its own copy of all of it");

        var said = await refused.Content.ReadAsStringAsync();

        StringAssert.Contains(said, "frontend/ comes to");
        StringAssert.Contains(said, ".gitignore", "and what is likeliest to have made it large is named, with what keeps it out");
    }

    #endregion

    #region Zip

    [TestMethod]
    public async Task AZipKeepsTheDotFilesOfTheCodeAndLeavesOutWhatItIgnores()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // as a folder somebody built in is zipped, the folder included
        var archive = Zip(
            ("app/lambda.cs", Snippet),
            ("app/resources/index.html", "<p>built</p>"),
            ("app/resources/.hidden.css", "nothing serves it"),
            ("app/.DS_Store", "litter"),
            ("app/.editorconfig", "root = true"),
            ("app/bin/Debug/app.dll", "what dotnet wrote"),
            ("app/frontend/.gitignore", "node_modules/\n/dist\n*.log\n!keep.log\n!important.tmp\n*.tmp\n"),
            ("app/frontend/package.json", Package),
            ("app/frontend/.npmrc", "engine-strict=true"),
            ("app/frontend/keep.log", "kept, as the rules take it back"),
            ("app/frontend/debug.log", "left out"),
            ("app/frontend/src/.gitignore", "!important.tmp\n"),
            ("app/frontend/src/important.tmp", "kept: the deeper file has the last word"),
            ("app/frontend/scratch.tmp", "left out by the file above"),
            ("app/frontend/node_modules/vite/package.json", "{}"),
            ("app/frontend/node_modules/.gitignore", "!*"),
            ("app/frontend/dist/index.js", "left out"),
            ("app/frontend/src/dist/notes.md", "kept: /dist is the project's own only"),
            ("app/frontend/.git/HEAD", "a repository"),
            ("app/.git/HEAD", "a repository"),
            ("app/frontend/.DS_Store", "litter"));

        var saved = await UploadAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/zip", HttpMethod.Post, archive);

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        var names = (await fixture.VersionAsync(lambda, 2)).Files.Select(f => f.Name).Order(StringComparer.Ordinal).ToList();

        CollectionAssert.AreEqual(new[]
        {
            ".editorconfig",
            "frontend/.gitignore",
            "frontend/.npmrc",
            "frontend/keep.log",
            "frontend/package.json",
            "frontend/src/.gitignore",
            "frontend/src/dist/notes.md",
            "frontend/src/important.tmp",
            "lambda.cs",
            "resources/index.html"
        }, names);
    }

    [TestMethod]
    public void WhatIsLeftOutFollowsTheRulesOfGit()
    {
        var ignored = IgnoredPaths.Of(
        [
            (".gitignore", "# a comment\n\n*.log\nbuild/\n/top.txt\ndocs/**/draft.md\n**/cache\n[ab].tmp\nout/\n!out/keep.js\n\\#hash\ntrailing.txt   \n"),
            ("web/.gitignore", "!error.log\n"),
            // a set that is never closed, which git matches nothing with
            ("odd/.gitignore", "[\nbroken[\n")
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
                     ("odd/broken[", false),
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

        const string helper = "public static class Helper { }\n";

        await fixture.SaveAsync(lambda, "Builds the front end", [.. Built, new LambdaFile("store.cs", store), new LambdaFile("models/helper.cs", helper)]);

        using var downloaded = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2/zip?layout=project");

        Assert.AreEqual(HttpStatusCode.OK, downloaded.StatusCode);

        var entries = Entries(await downloaded.Content.ReadAsByteArrayAsync());

        CollectionAssert.AreEquivalent(new[]
        {
            "Project.cs", "Store.cs", "resources/index.html", "frontend/README.md", "frontend/package.json", "frontend/.gitignore",
            "frontend/index.html", "models/Words.cs", "models/Helper.cs"
        }, entries.Keys.ToList(), "where a clone has them, and nothing of the platform's");

        StringAssert.Contains(entries["Project.cs"], "BuildAsync()", "the snippet in the class it is the body of, as in a clone");

        // changed where it was built, as a folder somebody built in is zipped
        var archive = Zip(
        [
            .. entries.Select(e => ($"laid/{e.Key}", e.Key == "resources/index.html" ? "<p>built again</p>" : e.Value)),
            ("laid/frontend/src/main.ts", "console.log('built');"),
            ("laid/frontend/node_modules/vite/package.json", "{}"),
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
        Assert.AreEqual(helper, files.Single(f => f.Name == "models/helper.cs").Code, "in a folder as well");
        Assert.AreEqual("<p>built again</p>", files.Single(f => f.Name == "resources/index.html").Code);
        Assert.AreEqual("console.log('built');", files.Single(f => f.Name == "frontend/src/main.ts").Code);

        Assert.IsFalse(files.Any(f => f.Name.Contains("node_modules", StringComparison.Ordinal) || f.Name.Contains("Release", StringComparison.Ordinal)),
                       "what the folder's .gitignore and the repository's ignore stays out");

        Assert.IsFalse(files.Any(f => f.Name.Contains("Program", StringComparison.Ordinal) || f.Name.EndsWith(".csproj", StringComparison.Ordinal)),
                       "and so do the platform's files");
    }

    [TestMethod]
    public async Task AZipLaidOutAsACloneRefusesWhatALambdaCannotHold()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("strict");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        using var downloaded = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2/zip?layout=project");

        var entries = Entries(await downloaded.Content.ReadAsByteArrayAsync());

        var refused = await UploadAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/zip?layout=project", HttpMethod.Post,
                                        Zip([.. entries.Select(e => (e.Key, e.Value)), ("assets/index.html", "<p>where it was once</p>")]));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "where a clone kept what it served, before it was called resources");
        StringAssert.Contains(await refused.Content.ReadAsStringAsync(), "they are in resources/ now", "it says where such files go");

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

        entries["frontend/index.html"] = "<p>darker</p>";

        var saved = await UploadAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/zip?layout=project", HttpMethod.Put,
                                      Zip([.. entries.Select(e => (e.Key, e.Value))]));

        Assert.AreEqual(HttpStatusCode.OK, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        var files = (await fixture.FeatureAsync(lambda, feature.Key)).Files;

        CollectionAssert.AreEqual(Built.Select(f => f.Name).ToList(), files.Select(f => f.Name).ToList(), "the same files, in the same order");
        Assert.AreEqual("<p>darker</p>", files.Single(f => f.Name == "frontend/index.html").Code);
    }

    [TestMethod]
    public async Task AZipPutBackKeepsWhatTheLambdaHasWhateverAGitignoreSays()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("kept");

        // a file saved on purpose where the .gitignore would leave it out, and
        // folders named as what dotnet run writes, below the top
        await fixture.SaveAsync(lambda, "Keeps a vendored library",
            new LambdaFile(LambdaSource.EntryName, Snippet),
            new LambdaFile("resources/bin/app.js", "a resource"),
            new LambdaFile("frontend/.gitignore", "vendor/\n"),
            new LambdaFile("frontend/vendor/lib.js", "vendored on purpose"),
            new LambdaFile("frontend/src/bin/main.rs", "fn main() {}"));

        var expected = new[] { LambdaSource.EntryName, "resources/bin/app.js", "frontend/.gitignore", "frontend/vendor/lib.js", "frontend/src/bin/main.rs" };

        foreach (var (layout, version) in (List<(string, int)>) [("project", 3), ("lambda", 4)])
        {
            using var downloaded = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/{version - 1}/zip?layout={layout}");

            var entries = Entries(await downloaded.Content.ReadAsByteArrayAsync());

            // put back as it came, with a file of the build's own beside the vendored one
            var saved = await UploadAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/zip?layout={layout}", HttpMethod.Post,
                                          Zip([.. entries.Select(e => (e.Key, e.Value)), ("frontend/vendor/installed.js", "what a build installed")]));

            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());

            var names = (await fixture.VersionAsync(lambda, version)).Files.Select(f => f.Name).ToList();

            CollectionAssert.AreEquivalent(expected, names, $"laid out as {layout}: what it had stays, as git keeps what it tracks, and only that");
        }
    }

    [TestMethod]
    public async Task AZipWithWhatABuildInstalledIsRefusedAsItIsSent()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { BuildBytes = 4096, PremiumBuildBytes = 4096 });

        var lambda = await fixture.CreateLambdaAsync();

        // what an install leaves behind does not compress away
        var installed = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16 * 1024));

        var archive = Zip(
            ("lambda.cs", Snippet),
            ("frontend/.gitignore", "node_modules/\n"),
            ("frontend/node_modules/vite/dist/index.js", installed));

        var refused = await UploadAsync(fixture, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/zip", HttpMethod.Post, archive);

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "left out by its .gitignore or not, it was sent");

        StringAssert.Contains(await refused.Content.ReadAsStringAsync(), "never what a build installed", "and it says what to leave out of the next one");
    }

    #endregion

    #region Agents

    [TestMethod]
    public async Task AnAgentIsHandedTheProgramFirstAndTheRestByNameWhereItRunsLong()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // a lock file larger than an answer carries
        await fixture.SaveAsync(lambda, "Builds the front end", [.. Built, new LambdaFile("frontend/package-lock.json", new string('x', 40_000))]);

        var read = await ToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey });

        var files = read["files"]!.AsArray().Select(f => f!.AsObject()).ToList();

        Assert.AreEqual(LambdaSource.EntryName, files[0]["name"]!.GetValue<string>(), "the C# first");
        Assert.AreEqual(Snippet, files[0]["code"]!.GetValue<string>());
        Assert.AreEqual("models/Words.cs", files[1]["name"]!.GetValue<string>(), "C# in a folder among it");
        Assert.AreEqual("resources/index.html", files[2]["name"]!.GetValue<string>(), "then the resources");
        Assert.IsNotNull(files[2]["code"]);

        var lockFile = files.Single(f => f["name"]!.GetValue<string>() == "frontend/package-lock.json");

        Assert.IsNull(lockFile["code"], "what does not fit is named");
        Assert.AreEqual(40_000, lockFile["length"]!.GetValue<int>(), "with its length");
        Assert.IsNotNull(files.Single(f => f["name"]!.GetValue<string>() == "frontend/package.json")["code"], "and the rest that fits is sent");
        Assert.IsTrue(read["filesOmitted"]!.GetValue<bool>());

        var one = await ToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["file"] = "frontend/package.json" });

        Assert.AreEqual(Package, one["files"]![0]!["code"]!.GetValue<string>(), "one file is read by its name");

        var guide = await ToolAsync(fixture, "platform_guide", new JsonObject());

        StringAssert.Contains(guide["code"]!["builtWithATool"]!["nothingIsBuiltHere"]!.GetValue<string>(), "never builds anything");
    }

    [TestMethod]
    public async Task SavingEveryFileWithoutAFolderSaysSo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        // change_code keeps what it is not told about
        var changed = await ToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["edits"] = new JsonArray(new JsonObject { ["file"] = "resources/index.html", ["find"] = "wrote", ["replace"] = "wrote again" })
        });

        Assert.IsNull(changed["dropped"], "nothing is said where nothing was left out");

        var written = await ToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = LambdaSource.EntryName, ["code"] = Snippet },
                                      new JsonObject { ["name"] = "resources/index.html", ["code"] = "<p>again</p>" })
        });

        StringAssert.Contains(written["dropped"]!.GetValue<string>(), "frontend/, models/ - 5 files", "an agent that did not know the folders were there is told what it left out");
    }

    [TestMethod]
    public async Task MigrationsAtTheTopOfTheCodeAreToldToMove()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // where a lambda kept them before its resources were a folder of their own
        var written = await ToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = LambdaSource.EntryName, ["code"] = Snippet },
                                      new JsonObject { ["name"] = "migrations/V1__Create_items.sql", ["code"] = "CREATE TABLE items (id INTEGER PRIMARY KEY);" })
        });

        StringAssert.Contains(written["misplaced"]!.GetValue<string>(), "resources/migrations/", "an agent used to the old layout is told where Evolve looks");

        var moved = await ToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = "resources/migrations/V1__Create_items.sql", ["code"] = "CREATE TABLE items (id INTEGER PRIMARY KEY);" }),
            ["remove"] = new JsonArray("migrations/V1__Create_items.sql")
        });

        Assert.IsNull(moved["misplaced"], "nothing is said once they are where they are read");
    }

    #endregion

    #region Taken away

    [TestMethod]
    public async Task TheExportCarriesTheCodeAndCompilesItsCSharpInAnyFolder()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("exported");

        // the snippet needs the C# of a folder, and a resource called .cs is no C#
        await fixture.SaveAsync(lambda, "Builds the front end",
        [
            new LambdaFile(LambdaSource.EntryName, Greeting),
            .. Built.Skip(1),
            new LambdaFile("resources/samples/Example.cs", "shown, never compiled")
        ]);

        using var answer = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export");

        var directory = Path.Combine(fixture.Options.DataDirectory, "exports", "exported");

        await ZipFile.ExtractToDirectoryAsync(new MemoryStream(await answer.Content.ReadAsByteArrayAsync()), directory);

        var project = Path.Combine(directory, "exported");

        Assert.AreEqual(Package, await File.ReadAllTextAsync(Path.Combine(project, "frontend/package.json")), "laid out as the lambda is");
        Assert.AreEqual("<p>what the build wrote</p>", await File.ReadAllTextAsync(Path.Combine(project, "resources/index.html")));
        Assert.StartsWith("*\n", await File.ReadAllTextAsync(Path.Combine(project, ".dockerignore")), "only what is compiled and copied goes into the image");

        var (exit, output) = await BuildAsync(project);

        Assert.AreEqual(0, exit, $"the C# of every folder of the code is compiled, the resources are not:\n{output}");

        Assert.IsTrue(File.Exists(Path.Combine(project, "models", "Words.cs")), "named as the lambda has it");

        Assert.IsTrue(File.Exists(Path.Combine(project, "bin", "Release", "net10.0", "resources", "index.html")), "and the resources are copied beside the program");
    }

    [TestMethod]
    public async Task AClonePushesTheFoldersOfTheCode()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("cloned");

        await fixture.SaveAsync(lambda, "Builds the front end", Built);

        var clone = await git.CloneAsync(fixture.EditorUrl(lambda), "cloned");

        Assert.AreEqual(Package, git.Read("cloned", "frontend/package.json"));
        Assert.AreEqual("node_modules/\ndist/\n", git.Read("cloned", "frontend/.gitignore"));
        Assert.IsFalse(Directory.Exists(Path.Combine(clone, ".lambda")));

        Assert.Contains("<EnableDefaultItems>false</EnableDefaultItems>", git.Read("cloned", "cloned.csproj"), "the build compiles what the platform compiles, and nothing it gathers besides");
        Assert.Contains("## What a tool builds", git.Read("cloned", "AGENTS.md"), "the agent working here is told how");
        Assert.Contains("!/build/", git.Read("cloned", ".gitignore"), "taken back in, whatever a global ignore says of a folder called build");
        Assert.StartsWith("/bin/\n/obj/\n", git.Read("cloned", ".gitignore"), "what dotnet run writes, at the root only");

        // what a build installed stays out, by the folder's own .gitignore
        git.Write("cloned", "frontend/node_modules/vite/package.json", "{}");
        git.Write("cloned", "frontend/src/main.ts", "console.log('built');");
        git.Write("cloned", "resources/web/main.js", "console.log('built');");
        git.Write("cloned", "notes/plan.md", "# What comes next");

        await git.CommitAsync("cloned", "Builds main.ts into the resources");

        var pushed = await git.RunAsync("cloned", "push");

        Assert.Contains("Saved version 3", pushed.Said);

        var files = (await fixture.VersionAsync(lambda, 3)).Files.Select(f => f.Name).ToList();

        CollectionAssert.IsSubsetOf(new[] { "frontend/src/main.ts", "resources/web/main.js", "notes/plan.md" }, files,
                                    "the sources, what they built and a folder of its own, in one version");
        Assert.IsFalse(files.Any(f => f.Contains("node_modules", StringComparison.Ordinal)));

        // C# in a folder is built here as on the platform, and a resource called .cs is not
        git.Write("cloned", "models/Extra.cs", "public static class Extra { public const int Answer = 42; }");
        git.Write("cloned", "resources/samples/Example.cs", "shown, never compiled");

        var (exit, output) = await BuildAsync(clone);

        Assert.AreEqual(0, exit, $"the C# of the code is built, in any folder, and nothing among the resources:\n{output}");
    }

    [TestMethod]
    public async Task APushedFileOfTheProjectsOwnIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("strayed");

        await git.CloneAsync(fixture.EditorUrl(lambda), "strayed");

        git.Write("strayed", "Platform/Extra.cs", "public class Extra { }");

        await git.CommitAsync("strayed", "Adds to what stands in for the platform");

        var refused = await git.TryAsync("strayed", "push");

        Assert.IsFalse(refused.Success);
        Assert.Contains("Platform/Extra.cs", refused.Said, "it says which file");
    }

    [TestMethod]
    public async Task APublishedSourceSaysWhatEachFileIs()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("shared");

        await fixture.SaveAsync(lambda, "Builds the front end", [.. Built, new LambdaFile(LambdaSource.ProductDoc, "# Shop")]);

        await fixture.PublishAsync(lambda);

        using var tree = await fixture.GetAsync("/api/v1/sources/shared/versions/2");

        var files = (await tree.GetContentAsync<SourceTreeResponse>()).Files;

        Assert.AreEqual("code", files.Single(f => f.Path == "frontend/package.json").Kind, "marked as what it is");
        Assert.AreEqual("resource", files.Single(f => f.Path == "resources/index.html").Kind);
        Assert.AreEqual("docs", files.Single(f => f.Path == "docs/product.md").Kind);
        Assert.AreEqual("project", files.Single(f => f.Path == "Program.cs").Kind);
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
