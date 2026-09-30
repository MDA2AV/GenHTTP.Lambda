using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// What a version keeps about itself beside its program: its documentation
/// and its tests, under .lambda/.
/// </summary>
[TestClass]
public sealed class DocumentationTests
{
    private const string Product = """
        # Guest book

        <!-- written by the agent -->

        A guest book people can **sign** with a name and a message, newest entries first - see [the page](web/index.html).

        ## Who it is for

        Visitors of a small website.
        """;

    private const string Decisions = "# Decisions\n\n## Entries are one JSON file\n\nA few hundred need no database.\n";

    private const string Testing = "# How it is tested\n\n`node smoke.mjs <address>` signs it once and reads it back.\n";

    private const string Snippet = "return Content.From(Resource.FromString(\"hello\"));";

    #region Kept with the version

    [TestMethod]
    public async Task DocumentationAndTestsAreKeptWithTheirVersion()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var documented = await SaveAsync(fixture, lambda.PrivateKey, Documented(Snippet));

        // a change of the code alone keeps what is written about it
        var changed = await ChangeAsync(fixture, lambda.PrivateKey, new VersionChangeRequest(null, null,
            [new FileEdit(LambdaSource.EntryName, "hello", "hello again")]));

        var read = await ReadAsync(fixture, lambda.PrivateKey, changed);

        Assert.AreEqual(Product, File(read, LambdaSource.ProductDoc), "the documentation goes on with the program");
        Assert.AreEqual("console.log('ok');", File(read, ".lambda/tests/smoke.mjs"), "and so do the tests");

        // and a change of the documentation leaves the version before it as it was
        var described = await ChangeAsync(fixture, lambda.PrivateKey, new VersionChangeRequest(null, null,
            [new FileEdit(LambdaSource.ProductDoc, "newest entries first", "newest entries first, and it keeps them for a year")]));

        StringAssert.Contains(File(await ReadAsync(fixture, lambda.PrivateKey, described), LambdaSource.ProductDoc), "for a year");

        Assert.AreEqual(Product, File(await ReadAsync(fixture, lambda.PrivateKey, documented), LambdaSource.ProductDoc),
                        "a version never changes, so rolling back brings back the documentation that was true of it");
    }

    [TestMethod]
    public async Task DocumentationAndTestsAreNeitherCompiledNorServed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, new VersionRequest(
        [
            new LambdaFile(LambdaSource.EntryName, "return Layout.Create().Add(Assets.Files());"),
            new LambdaFile("index.html", "<p>the page</p>"),
            new LambdaFile(LambdaSource.ProductDoc, Product),
            // a test written in C# - and not a program anybody could compile
            new LambdaFile(".lambda/tests/Check.cs", "this is a test script, not code of the lambda"),
        ]));

        var deployed = await fixture.DeployAsync(lambda.PrivateKey);

        Assert.IsTrue(deployed.Success, "a C# file among the tests is not compiled");

        using var page = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/index.html");

        Assert.AreEqual("<p>the page</p>", await page.GetContentAsync(), "the assets are served");

        using var documentation = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/.lambda/docs/product.md");

        Assert.AreEqual(HttpStatusCode.NotFound, documentation.StatusCode, "what is written about it is not");

        using var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");

        var storage = (await summary.GetContentAsync<LambdaSummaryResponse>()).Storage;

        Assert.AreEqual(1, storage.CodeFiles, "the test is not counted as code");
        Assert.AreEqual(1, storage.Assets, "nor is the documentation counted as an asset");
    }

    [TestMethod]
    public async Task TheContextFolderHoldsTheDocumentationAndTheTestsOnly()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        foreach (var (name, content, encoding) in (List<(string, string, string?)>)
                 [
                     (".lambda/notes.md", "# Notes", null),
                     (".lambda/docs/.hidden.md", "# Hidden", null),
                     (".Lambda/docs/product.md", "# Product", null),
                     (".lambda/docs/", "", null),
                     (LambdaSource.ProductDoc, Convert.ToBase64String("# Product"u8.ToArray()), "base64"),
                 ])
        {
            using var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                        new VersionRequest([new(LambdaSource.EntryName, Snippet), new(name, content, encoding)]));

            Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, $"'{name}' is refused");

            StringAssert.Contains(await refused.Content.ReadAsStringAsync(), name, "and the refusal says which file");
        }

        using var misplaced = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                      new VersionRequest([new(LambdaSource.EntryName, Snippet), new(".lambda/notes.md", "# Notes")]));

        StringAssert.Contains(await misplaced.Content.ReadAsStringAsync(), LambdaSource.DocsFolder, "it says where things go instead");

        // a test needs no extension - nothing infers a content type from it
        await SaveAsync(fixture, lambda.PrivateKey, new VersionRequest([new(LambdaSource.EntryName, Snippet), new(".lambda/tests/Makefile", "test:\n\tnode smoke.mjs")]));
    }

    [TestMethod]
    public async Task DocumentationCountsTowardsWhatTheAssetsMayComeTo()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { MaxAssetBytes = 1024, PremiumMaxAssetBytes = 1024 });

        var lambda = await fixture.CreateLambdaAsync();

        using var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                    new VersionRequest([new(LambdaSource.EntryName, Snippet), new(".lambda/tests/data.json", new string('x', 2000))]));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "every version carries its own copy, like an asset");
        StringAssert.Contains(await refused.Content.ReadAsStringAsync(), "the documentation and the tests");
    }

    [TestMethod]
    public async Task AVersionCanBeReadForItsDocumentationAlone()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var version = await SaveAsync(fixture, lambda.PrivateKey, Documented(Snippet, new LambdaFile("web/big.gif", Convert.ToBase64String(new byte[4096]), "base64")));

        foreach (var folder in (string[])[".lambda/", ".lambda%2F"])
        {
            using var answer = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/{version}?folder={folder}");

            var files = (await answer.GetContentAsync<VersionContentResponse>()).Files.Select(f => f.Name).ToList();

            CollectionAssert.AreEquivalent(new[] { LambdaSource.ProductDoc, LambdaSource.DecisionsDoc, LambdaSource.TestingDoc, ".lambda/tests/smoke.mjs" }, files,
                                           $"only what is below {folder}, without the program and its assets");
        }
    }

    [TestMethod]
    public async Task TheSummarySaysWhatTheAppIsFor()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, new VersionRequest([new(LambdaSource.EntryName, Snippet), new(LambdaSource.ProductDoc, Product)]));

        using var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");

        var documentation = (await summary.GetContentAsync<LambdaSummaryResponse>()).Documentation;

        Assert.AreEqual("A guest book people can sign with a name and a message, newest entries first - see the page.", documentation.About,
                        "the paragraph the page begins with, without its markup, its heading or its comment");

        Assert.IsTrue(documentation.Product);
        Assert.IsFalse(documentation.Decisions, "what is not written is said to be missing");
        Assert.IsFalse(documentation.Tests);
    }

    #endregion

    #region Features and archives

    [TestMethod]
    public async Task AFeatureCarriesItsDocumentationIntoTheVersionItBecomes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, Documented(Snippet));

        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("A year of entries"));

        var feature = await created.GetContentAsync<FeatureResponse>();

        using var copied = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}?folder=.lambda/docs/");

        var copy = await copied.GetContentAsync<FeatureContentResponse>();

        CollectionAssert.AreEquivalent(new[] { LambdaSource.ProductDoc, LambdaSource.DecisionsDoc }, copy.Files.Select(f => f.Name).ToList(),
                                       "a feature starts with the documentation of its version");

        using var changed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/changes",
            new VersionChangeRequest([new(LambdaSource.DecisionsDoc, "# Decisions\n\nEntries are kept for a year.\n")], null,
                                     [new FileEdit(LambdaSource.EntryName, "hello", "hello for a year")]));

        Assert.AreEqual(HttpStatusCode.OK, changed.StatusCode, await changed.Content.ReadAsStringAsync());

        using var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/merge", new MergeFeatureRequest());

        var version = (await merged.GetContentAsync<FeatureMergeResponse>()).Version!.Version;

        var read = await ReadAsync(fixture, lambda.PrivateKey, version);

        StringAssert.Contains(File(read, LambdaSource.DecisionsDoc), "kept for a year", "the change and what is written about it are merged together");
        Assert.AreEqual(Product, File(read, LambdaSource.ProductDoc));
    }

    [TestMethod]
    public async Task TheZipOfAVersionHoldsItsDocumentationAndTests()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // zipped as a folder, with what an operating system and a repository leave behind
        var archive = Zip(("project/lambda.cs", Snippet),
                          ("project/.lambda/docs/product.md", Product),
                          ("project/.lambda/tests/smoke.mjs", "console.log('ok');"),
                          ("project/.lambda/tests/.DS_Store", "x"),
                          ("project/.git/HEAD", "ref: refs/heads/main"),
                          (".DS_Store", "x"));

        using var request = fixture.Host.GetRequest($"/api/v1/lambdas/{lambda.PrivateKey}/versions/zip", HttpMethod.Post);

        request.Content = new ByteArrayContent(archive);
        request.Content.Headers.ContentType = new("application/zip");

        using var uploaded = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Created, uploaded.StatusCode, await uploaded.Content.ReadAsStringAsync());

        var version = (await uploaded.GetContentAsync<SavedVersionResponse>()).Version;

        var read = await ReadAsync(fixture, lambda.PrivateKey, version);

        CollectionAssert.AreEqual(new[] { LambdaSource.EntryName, LambdaSource.ProductDoc, ".lambda/tests/smoke.mjs" }, read.Files.Select(f => f.Name).ToArray(),
                                  "the context is kept, the program first, and every other hidden file is not");

        using var downloaded = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/{version}/zip");

        using var zip = new ZipArchive(new MemoryStream(await downloaded.Content.ReadAsByteArrayAsync()));

        Assert.IsNotNull(zip.GetEntry(LambdaSource.ProductDoc), "and it comes back in the zip, to be changed and uploaded again");
    }

    #endregion

    #region Secrets

    [TestMethod]
    public async Task ASecretTheDocumentationMentionsIsNotOneTheCodeReads()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, new VersionRequest(
        [
            new(LambdaSource.EntryName, Snippet),
            new(LambdaSource.DecisionsDoc, "It could read `Secret.Read(\"WEATHER_KEY\")` one day."),
            new(".lambda/tests/Check.cs", "var key = Secret.Read(\"TEST_KEY\");"),
        ]));

        using var listed = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/secrets");

        var secrets = await listed.GetContentAsync<SecretListingResponse>();

        Assert.IsEmpty(secrets.Missing, "only the program is asked what it reads");
    }

    #endregion

    #region Agents

    [TestMethod]
    public async Task AnAgentIsHandedTheDocumentationBeforeTheFiles()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await SaveAsync(fixture, lambda.PrivateKey, Documented(Snippet));

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey }));

        var pages = (JsonArray)read["documentation"]!["pages"]!;

        Assert.AreEqual(LambdaSource.ProductDoc, pages[0]!["name"]!.GetValue<string>(), "what the app is for comes first");
        Assert.AreEqual(Product, pages[0]!["content"]!.GetValue<string>());
        Assert.AreEqual(LambdaSource.DecisionsDoc, pages[1]!["name"]!.GetValue<string>());

        Assert.AreEqual(Testing, read["tests"]!["pages"]![0]!["content"]!.GetValue<string>(), "then how it is tested");
        Assert.AreEqual(".lambda/tests/smoke.mjs", read["tests"]!["files"]![0]!["name"]!.GetValue<string>(), "with its scripts by name");

        var files = ((JsonArray)read["files"]!).Select(f => f!["name"]!.GetValue<string>()).ToList();

        CollectionAssert.AreEqual(new[] { LambdaSource.EntryName }, files, "the files are the program, the context is not repeated among them");

        Assert.IsNull(read["documentationNote"], "nothing is missing, so nothing is said about it");

        var script = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["file"] = ".lambda/tests/smoke.mjs"
        }));

        Assert.AreEqual("console.log('ok');", script["files"]![0]!["code"]!.GetValue<string>(), "a test is read like any file");
    }

    [TestMethod]
    public async Task AnAgentIsToldWhatIsNotWrittenYet()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var bare = Structured(await CallToolAsync(fixture, "write_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = LambdaSource.EntryName, ["code"] = Snippet })
        }));

        StringAssert.Contains(bare["documentation"]!.GetValue<string>(), LambdaSource.ProductDoc, "a save without any says what to write");

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey }));

        StringAssert.Contains(read["documentationNote"]!.GetValue<string>(), LambdaSource.TestingDoc, "and so does reading it");

        var partly = Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = LambdaSource.ProductDoc, ["code"] = Product })
        }));

        var said = partly["documentation"]!.GetValue<string>();

        Assert.DoesNotContain(LambdaSource.ProductDoc, said, "what is there is not asked for again");
        StringAssert.Contains(said, LambdaSource.DecisionsDoc, "what is still missing is");

        var complete = Structured(await CallToolAsync(fixture, "change_code", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["files"] = new JsonArray(new JsonObject { ["name"] = LambdaSource.DecisionsDoc, ["code"] = Decisions },
                                      new JsonObject { ["name"] = LambdaSource.TestingDoc, ["code"] = Testing })
        }));

        Assert.IsNull(complete["documentation"], "and once everything is written, nothing is said");
    }

    [TestMethod]
    public async Task AnAgentIsToldToWriteAndKeepTheDocumentation()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var instructions = (await CallAsync(fixture, "initialize", new JsonObject()))["result"]!["instructions"]!.GetValue<string>();

        foreach (var page in (string[])["docs/product.md", "docs/decisions.md", "tests/README.md"])
        {
            StringAssert.Contains(instructions, page, "the instructions name every page");
        }

        StringAssert.Contains(instructions, "update", "and say to keep them up to date");

        var guide = Structured(await CallToolAsync(fixture, "platform_guide", []))["documentationAndTests"]!;

        foreach (var part in (string[])["product", "decisions", "testing", "when", "use"])
        {
            Assert.IsNotNull(guide[part], $"the guide says {part}");
        }

        var tools = (JsonArray)(await CallAsync(fixture, "tools/list", new JsonObject()))["result"]!["tools"]!;

        string Describe(string name) => tools.Single(t => t!["name"]!.GetValue<string>() == name)!["description"]!.GetValue<string>();

        StringAssert.Contains(Describe("write_code"), ".lambda/", "where a tool is picked");
        StringAssert.Contains(Describe("change_code"), ".lambda/docs/");
        StringAssert.Contains(Describe("merge_feature"), ".lambda/");
    }

    [TestMethod]
    public async Task ADocumentNamingTheLiveAddressIsNotALinkThatLeaks()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("guests");

        await SaveAsync(fixture, lambda.PrivateKey, new VersionRequest(
        [
            new(LambdaSource.EntryName, Snippet),
            new(LambdaSource.ProductDoc, "# Guests\n\nIt answers at /lambda/guests/ on the platform.\n"),
        ]));

        var feature = Structured(await CallToolAsync(fixture, "create_feature", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["name"] = "Nothing" }));

        var deployed = Structured(await CallToolAsync(fixture, "deploy", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey,
            ["feature"] = feature["feature"]!["feature"]!.GetValue<string>()
        }));

        Assert.IsTrue(deployed["ok"]!.GetValue<bool>());
        Assert.IsNull(deployed["warning"], "saying where the lambda is is not linking there from a page");
    }

    #endregion

    #region Plumbing

    private static VersionRequest Documented(string snippet, params LambdaFile[] more)
        => new(
        [
            new(LambdaSource.EntryName, snippet),
            .. more,
            new(LambdaSource.ProductDoc, Product),
            new(LambdaSource.DecisionsDoc, Decisions),
            new(LambdaSource.TestingDoc, Testing),
            new(".lambda/tests/smoke.mjs", "console.log('ok');"),
        ]);

    private static async Task<int> SaveAsync(LambdaFixture fixture, string privateKey, VersionRequest version)
    {
        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/versions", version);

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        return (await saved.GetContentAsync<SavedVersionResponse>()).Version;
    }

    private static async Task<int> ChangeAsync(LambdaFixture fixture, string privateKey, VersionChangeRequest change)
    {
        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/versions/changes", change);

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        return (await saved.GetContentAsync<SavedVersionResponse>()).Version;
    }

    private static async Task<VersionContentResponse> ReadAsync(LambdaFixture fixture, string privateKey, int version)
    {
        using var read = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/versions/{version}");

        return await read.GetContentAsync<VersionContentResponse>();
    }

    private static string? File(VersionContentResponse content, string name) => content.Files.FirstOrDefault(f => f.Name == name)?.Code;

    private static byte[] Zip(params (string Name, string Content)[] entries)
    {
        using var buffer = new MemoryStream();

        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            foreach (var (name, content) in entries)
            {
                using var stream = zip.CreateEntry(name).Open();

                stream.Write(Encoding.UTF8.GetBytes(content));
            }
        }

        return buffer.ToArray();
    }

    private static JsonObject Structured(JsonObject answer) => (JsonObject)answer["result"]!["structuredContent"]!;

    private static Task<JsonObject> CallToolAsync(LambdaFixture fixture, string tool, JsonObject arguments)
        => CallAsync(fixture, "tools/call", new JsonObject { ["name"] = tool, ["arguments"] = arguments });

    private static async Task<JsonObject> CallAsync(LambdaFixture fixture, string method, JsonObject parameters)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");

        request.Content = new StringContent(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = method,
            ["params"] = parameters
        }.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        return (JsonObject)JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
    }

    #endregion

}
