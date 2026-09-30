using System.IO.Compression;
using System.Net;
using System.Text;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Sources;

/// <summary>
/// A lambda whose owner published its source: read, starred and downloaded by
/// anybody, every version of it - and never with the data it keeps.
/// </summary>
[TestClass]
public sealed class SourceTests
{

    private const string Snippet = "return Inline.Create().Get(() => \"Hello from the quiz\");";

    private const string Product = "# Pub quiz\n\nScores for the Tuesday pub quiz, kept round by round.\n\nMore about it.\n";

    #region Publishing

    [TestMethod]
    public async Task NothingIsPublishedUntilTheOwnerAsks()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("quiz");

        await fixture.DeployAsync(lambda.PrivateKey, Snippet);

        using var project = await fixture.GetAsync("/api/v1/sources/quiz");
        using var tree = await fixture.GetAsync("/api/v1/sources/quiz/versions/2");
        using var zip = await fixture.GetAsync("/api/v1/sources/quiz/versions/2/zip");

        Assert.AreEqual(HttpStatusCode.NotFound, project.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, tree.StatusCode, "an unpublished source is not there, however it is asked for");
        Assert.AreEqual(HttpStatusCode.NotFound, zip.StatusCode);

        var own = await (await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/source")).GetContentAsync<OwnSourceResponse>();

        Assert.IsNull(own.Source);
        Assert.AreEqual("MIT", own.Default);
        Assert.AreEqual("MIT", own.Licenses[0].Id, "the default is offered first");
    }

    [TestMethod]
    public async Task APublishedSourceIsReadByAnybodyUnderItsLicense()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("quiz");

        await SaveAsync(fixture, lambda.PrivateKey, "Adds the scoreboard", ("index.html", "<h1>Quiz</h1>"), (LambdaSource.ProductDoc, Product));

        var published = await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest("Apache-2.0", null));

        Assert.IsTrue(published.Published);
        Assert.AreEqual("Apache-2.0", published.License.Id);
        Assert.AreEqual("/source/quiz", published.Path);
        Assert.AreEqual("The authors of quiz", published.Holder, "nobody named, the license names the lambda's authors");

        var project = await ProjectAsync(fixture, "quiz");

        Assert.AreEqual("Apache-2.0", project.Source.License.Id);
        Assert.AreEqual("Scores for the Tuesday pub quiz, kept round by round.", project.Source.About, "what it is, as its documentation says in its first paragraph");
        Assert.AreEqual(2, project.Source.LatestVersion);
        Assert.AreEqual(2, project.Versions.Count);
        Assert.AreEqual("Adds the scoreboard", project.Versions[0].Change);
        Assert.IsFalse(string.IsNullOrEmpty(project.StarTicket));

        var tree = await TreeAsync(fixture, "quiz", 2);

        var paths = tree.Files.Select(f => f.Path).ToList();

        CollectionAssert.IsSubsetOf(new[] { "Program.cs", "Project.cs", "quiz.csproj", "Dockerfile", "LICENSE", "assets/index.html", "docs/product.md" }, paths,
                                    "the project the export makes, with the license beside it");

        Assert.AreEqual("code", tree.Files.Single(f => f.Path == "Project.cs").Kind);
        Assert.AreEqual("asset", tree.Files.Single(f => f.Path == "assets/index.html").Kind);
        Assert.AreEqual("docs", tree.Files.Single(f => f.Path == "docs/product.md").Kind);
        Assert.AreEqual("platform", tree.Files.Single(f => f.Path == "Platform/Handlers.cs").Kind);
        Assert.AreEqual("project", tree.Files.Single(f => f.Path == "LICENSE").Kind);

        var code = await FileAsync(fixture, "quiz", 2, "Project.cs");

        Assert.IsTrue(code.Text);
        StringAssert.Contains(code.Content, "Hello from the quiz");

        var license = await FileAsync(fixture, "quiz", 2, "LICENSE");

        StringAssert.StartsWith(license.Content!.TrimStart(), "Apache License");

        var program = await FileAsync(fixture, "quiz", 2, "Program.cs");

        StringAssert.Contains(program.Content, "Apache-2.0, see LICENSE", "the program says what it is under");
        Assert.DoesNotContain("Exported", program.Content!, "a published source is the same whenever it is read");
    }

    [TestMethod]
    public async Task TheSourceNeverCarriesTheDataTheLambdaKeeps()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("notes");

        const string record = "a record nobody should read";
        const string value = "sk-the-value-of-a-secret";
        const string saved = "what-the-app-saved.txt";

        // records in the database, written by the lambda itself
        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        await SaveAsync(fixture, lambda.PrivateKey, "Keeps notes",
                        ("migrations/V1__Notes.sql", "CREATE TABLE notes (id INTEGER PRIMARY KEY, text TEXT NOT NULL);"),
                        (LambdaSource.EntryName, Notes));

        await fixture.DeployAsync(lambda.PrivateKey);

        using (var added = await fixture.GetAsync($"/lambda/notes/add?text={Uri.EscapeDataString(record)}"))
        {
            Assert.AreEqual(HttpStatusCode.OK, added.StatusCode);
        }

        // a file in the workspace, and a secret with a value
        using (var file = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/files/{saved}",
                                                   new FileRequest(Convert.ToBase64String(Encoding.UTF8.GetBytes(record)))))
        {
            Assert.AreEqual(HttpStatusCode.OK, file.StatusCode);
        }

        using (var secrets = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/secrets"))
        {
            Assert.AreEqual(HttpStatusCode.OK, secrets.StatusCode);
        }

        using (var secret = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/API_KEY", new SecretRequest(value)))
        {
            Assert.AreEqual(HttpStatusCode.OK, secret.StatusCode);
        }

        // the owner's export takes the records along - which is what makes it
        // worth checking that the published source does not
        using (var export = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export"))
        {
            CollectionAssert.Contains(Entries(await export.Content.ReadAsByteArrayAsync()).Keys.ToList(), "notes/database/database.db");
        }

        await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest(null, null));

        using var download = await fixture.GetAsync("/api/v1/sources/notes/versions/2/zip");

        Assert.AreEqual(HttpStatusCode.OK, download.StatusCode);
        Assert.AreEqual("application/zip", download.Content.Headers.ContentType?.MediaType);

        var entries = Entries(await download.Content.ReadAsByteArrayAsync());

        Assert.IsFalse(entries.Keys.Any(e => e.Contains("/database/", StringComparison.Ordinal)), "no database, not even an empty one");
        Assert.IsFalse(entries.Keys.Any(e => e.EndsWith(saved, StringComparison.Ordinal)), "nothing from the workspace");

        foreach (var (name, content) in entries)
        {
            var text = Encoding.UTF8.GetString(content);

            Assert.DoesNotContain(record, text, $"{name} holds a record or a file the app kept");
            Assert.DoesNotContain(value, text, $"{name} holds the value of a secret");
        }

        var tree = await TreeAsync(fixture, "notes", 2);

        Assert.IsFalse(tree.Files.Any(f => f.Path.StartsWith("database/", StringComparison.Ordinal)));
        Assert.IsTrue(tree.Files.Any(f => f.Path == "Platform/Database.cs"), "the code that opens a database is code, and comes along");
    }

    [TestMethod]
    public async Task TheLicenseNamesWhomTheOwnerNamed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("named");

        var published = await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest(null, "  Jane Doe  "));

        Assert.AreEqual("MIT", published.License.Id, "MIT unless another is asked for");
        Assert.AreEqual("Jane Doe", published.Author);

        var license = await FileAsync(fixture, "named", 1, "LICENSE");

        StringAssert.Contains(license.Content, $"Copyright (c) {DateTime.UtcNow.Year} Jane Doe");

        // left out, it stays; empty, it names nobody again
        var kept = await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest("BSD-3-Clause", null));

        Assert.AreEqual("Jane Doe", kept.Author);

        var cleared = await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest(null, ""));

        Assert.IsNull(cleared.Author);
        Assert.AreEqual("BSD-3-Clause", cleared.License.Id);

        var repacked = await FileAsync(fixture, "named", 1, "LICENSE");

        StringAssert.Contains(repacked.Content, "BSD 3-Clause License", "a new license is a new project");
        StringAssert.Contains(repacked.Content, "The authors of named");
    }

    [TestMethod]
    public async Task OnlyALicenseOnOfferIsAccepted()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var refused = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/source", new SourceRequest("WTFPL", null));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);

        using var tooLong = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/source", new SourceRequest(null, new string('a', 101)));

        Assert.AreEqual(HttpStatusCode.BadRequest, tooLong.StatusCode);

        using var twoLines = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/source", new SourceRequest(null, "Jane\nDoe"));

        Assert.AreEqual(HttpStatusCode.BadRequest, twoLines.StatusCode);

        var own = await (await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/source")).GetContentAsync<OwnSourceResponse>();

        Assert.IsNull(own.Source, "a request that is wrong publishes nothing");
    }

    [TestMethod]
    public async Task EveryLicenseOnOfferHasItsText()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("licensed");

        var own = await (await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/source")).GetContentAsync<OwnSourceResponse>();

        foreach (var license in own.Licenses)
        {
            await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest(license.Id, "Jane Doe"));

            var text = await FileAsync(fixture, "licensed", 1, "LICENSE");

            Assert.IsGreaterThan(500, text.Content!.Length, $"{license.Id} is written out in full");
            Assert.DoesNotContain("{year}", text.Content!);
            Assert.DoesNotContain("{holder}", text.Content!);
        }
    }

    [TestMethod]
    public async Task TakingItDownKeepsItsStarsForLater()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("starred");

        await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest(null, null));

        await TreeAsync(fixture, "starred", 1);

        var packed = Path.Combine(fixture.Options.SourceDirectory, (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value.ToString());

        Assert.AreEqual(1, Directory.GetFiles(packed, "*.zip").Length, "packed once, on the first read");

        await StarAsync(fixture, "starred", true);

        using (var down = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/source"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, down.StatusCode);
        }

        using (var gone = await fixture.GetAsync("/api/v1/sources/starred"))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, gone.StatusCode);
        }

        Assert.IsFalse(Directory.Exists(packed), "nothing is kept packed for a source nobody may read");

        var again = await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest(null, null));

        Assert.AreEqual(1, again.Stars);
    }

    [TestMethod]
    public async Task ADemoIsPublishedByTheInstallationAndByNobodyElse()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        var listing = await (await fixture.GetAsync("/api/v1/sources/?take=48")).GetContentAsync<SourceListingResponse>();

        CollectionAssert.IsSubsetOf(new[] { "demo-crud", "demo-registration", "demo-game", "demo-files", "demo-live" },
                                    listing.Entries.Select(e => e.PublicKey).ToList(), "the demos are there to be read and built on");

        using var refused = await fixture.SendAsync(HttpMethod.Put, "/api/v1/lambdas/demo-crud/source", new SourceRequest("GPL-3.0-or-later", null));

        Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode, "a demo's key is announced, and says nothing about who decides its license");

        using var taken = await fixture.SendAsync(HttpMethod.Delete, "/api/v1/lambdas/demo-crud/source");

        Assert.AreEqual(HttpStatusCode.Forbidden, taken.StatusCode);
    }

    #endregion

    #region Reading

    [TestMethod]
    public async Task EveryVersionIsReadAndDownloadedAsItWas()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("history");

        await SaveAsync(fixture, lambda.PrivateKey, "First", (LambdaSource.EntryName, "return Inline.Create().Get(() => \"one\");"));
        await SaveAsync(fixture, lambda.PrivateKey, "Second", (LambdaSource.EntryName, "return Inline.Create().Get(() => \"two\");"));

        await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest(null, null));

        StringAssert.Contains((await FileAsync(fixture, "history", 2, "Project.cs")).Content, "\"one\"");
        StringAssert.Contains((await FileAsync(fixture, "history", 3, "Project.cs")).Content, "\"two\"");

        using var zip = await fixture.GetAsync("/api/v1/sources/history/versions/2/zip");

        Assert.AreEqual(HttpStatusCode.OK, zip.StatusCode);
        StringAssert.Contains(zip.Content.Headers.GetValues("Content-Disposition").Single(), "history-v2.zip");

        var entries = Entries(await zip.Content.ReadAsByteArrayAsync());

        StringAssert.Contains(Encoding.UTF8.GetString(entries["history/Project.cs"]), "\"one\"");

        using var missing = await fixture.GetAsync("/api/v1/sources/history/versions/9");

        Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [TestMethod]
    public async Task TheHistorySaysWhatChangedAndNotWhatWasAsked()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("asked");

        using (var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                    new VersionRequest([new LambdaFile(LambdaSource.EntryName, Snippet)],
                                                                       "My wedding on 12 June, for Anna and Tom", "Adds a guestbook")))
        {
            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);
        }

        await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest(null, null));

        using var answer = await fixture.GetAsync("/api/v1/sources/asked");

        var body = await answer.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "Adds a guestbook");
        Assert.DoesNotContain("Anna and Tom", body, "what the owner asked for, in their words, stays with the owner");
        Assert.DoesNotContain(lambda.PrivateKey, body, "and so does the editor key");
    }

    [TestMethod]
    public async Task AFileIsNeverServedAsSomethingThatRuns()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("pages");

        await SaveAsync(fixture, lambda.PrivateKey, "Pages", ("index.html", "<script>alert(1)</script>"));

        await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest(null, null));

        using var page = await fixture.GetAsync($"/api/v1/sources/pages/versions/2/raw/{Uri.EscapeDataString("assets/index.html")}");

        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode, await page.Content.ReadAsStringAsync());
        Assert.AreEqual("text/plain", page.Content.Headers.ContentType?.MediaType, "a page a lambda ships is text here");
        Assert.AreEqual("nosniff", page.Headers.GetValues("X-Content-Type-Options").Single());
        StringAssert.Contains(page.Headers.GetValues("Content-Security-Policy").Single(), "sandbox");
        Assert.AreEqual("<script>alert(1)</script>", await page.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task TheListingFindsSourcesByWhatTheyAre()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var quiz = await fixture.CreateLambdaAsync("quiz");
        var poll = await fixture.CreateLambdaAsync("poll");

        await SaveAsync(fixture, quiz.PrivateKey, "Quiz", (LambdaSource.ProductDoc, Product));
        await SaveAsync(fixture, poll.PrivateKey, "Poll", (LambdaSource.ProductDoc, "# Lunch poll\n\nThe office votes on where to have lunch.\n"));

        await PublishAsync(fixture, quiz.PrivateKey, new SourceRequest(null, null));
        await PublishAsync(fixture, poll.PrivateKey, new SourceRequest("GPL-3.0-or-later", null));

        await StarAsync(fixture, "poll", true);

        var all = await (await fixture.GetAsync("/api/v1/sources/")).GetContentAsync<SourceListingResponse>();

        Assert.AreEqual(2, all.Total);
        Assert.AreEqual("poll", all.Entries[0].PublicKey, "the most starred first");
        Assert.AreEqual("/source/poll", all.Entries[0].Path);
        Assert.AreEqual("GPL-3.0-or-later", all.Entries[0].License.Id);

        var found = await (await fixture.GetAsync("/api/v1/sources/?search=tuesday%20quiz")).GetContentAsync<SourceListingResponse>();

        Assert.AreEqual(1, found.Total, "by what its documentation says, every word of it");
        Assert.AreEqual("quiz", found.Entries[0].PublicKey);

        var none = await (await fixture.GetAsync("/api/v1/sources/?search=spreadsheet")).GetContentAsync<SourceListingResponse>();

        Assert.AreEqual(0, none.Total);
    }

    #endregion

    #region Stars

    [TestMethod]
    public async Task AStarIsGivenOnceWithATicketAndTakenBack()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("stars");

        await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest(null, null));

        // a link, which anything that follows links would follow
        using (var link = await fixture.GetAsync("/api/v1/sources/stars/star"))
        {
            Assert.AreNotEqual(HttpStatusCode.OK, link.StatusCode);
        }

        using (var blind = await fixture.SendAsync(HttpMethod.Post, "/api/v1/sources/stars/star", new StarRequest(null, true)))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, blind.StatusCode, "no ticket, no star");
        }

        var ticket = (await ProjectAsync(fixture, "stars")).StarTicket;

        using (var hasty = await fixture.SendAsync(HttpMethod.Post, "/api/v1/sources/stars/star", new StarRequest(ticket, true)))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, hasty.StatusCode, "nobody finds the star in the moment the page arrives");
        }

        using (var forged = await fixture.SendAsync(HttpMethod.Post, "/api/v1/sources/stars/star", new StarRequest("1.forged", true)))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, forged.StatusCode);
        }

        await Task.Delay(TimeSpan.FromSeconds(1.2));

        var first = await PostStarAsync(fixture, "stars", ticket, true);

        Assert.AreEqual(1, first.Stars);
        Assert.IsTrue(first.Counted);

        var twice = await PostStarAsync(fixture, "stars", ticket, true);

        Assert.AreEqual(1, twice.Stars, "one visitor, one star");
        Assert.IsFalse(twice.Counted);

        var back = await PostStarAsync(fixture, "stars", ticket, false);

        Assert.AreEqual(0, back.Stars);

        var again = await PostStarAsync(fixture, "stars", ticket, false);

        Assert.AreEqual(0, again.Stars, "a star taken back once is not taken back twice");

        Assert.AreEqual(0, (await ProjectAsync(fixture, "stars")).Source.Stars, "kept in the database, not in the page");
    }

    [TestMethod]
    public async Task AStarForAnotherSourceNeedsItsOwnTicket()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var one = await fixture.CreateLambdaAsync("one");
        var two = await fixture.CreateLambdaAsync("two");

        await PublishAsync(fixture, one.PrivateKey, new SourceRequest(null, null));
        await PublishAsync(fixture, two.PrivateKey, new SourceRequest(null, null));

        var ticket = (await ProjectAsync(fixture, "one")).StarTicket;

        await Task.Delay(TimeSpan.FromSeconds(1.2));

        using var refused = await fixture.SendAsync(HttpMethod.Post, "/api/v1/sources/two/star", new StarRequest(ticket, true));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);
    }

    #endregion

    #region Owner

    [TestMethod]
    public async Task TheExportOfAPublishedLambdaCarriesItsLicense()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("carried");

        using (var before = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export"))
        {
            Assert.IsFalse(Entries(await before.Content.ReadAsByteArrayAsync()).ContainsKey("carried/LICENSE"), "no license until it is published under one");
        }

        await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest("MPL-2.0", null));

        using var after = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export");

        var entries = Entries(await after.Content.ReadAsByteArrayAsync());

        StringAssert.Contains(Encoding.UTF8.GetString(entries["carried/LICENSE"]), "Mozilla Public License");
    }

    [TestMethod]
    public async Task DeletingALambdaDeletesWhatItsSourceWasPackedInto()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("temporary");

        await PublishAsync(fixture, lambda.PrivateKey, new SourceRequest(null, null));

        await TreeAsync(fixture, "temporary", 1);

        var packed = Path.Combine(fixture.Options.SourceDirectory, (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value.ToString());

        Assert.IsTrue(Directory.Exists(packed));

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode);
        }

        Assert.IsFalse(Directory.Exists(packed));

        using var gone = await fixture.GetAsync("/api/v1/sources/temporary");

        Assert.AreEqual(HttpStatusCode.NotFound, gone.StatusCode);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Notes kept in the lambda's database, the way an agent is steered to.
    /// </summary>
    private const string Notes = """
        using (var connection = Database.GetConnection())
        {
            new Evolve(connection) { Locations = [Assets.Root + "migrations"], IsEraseDisabled = true }.Migrate();
        }

        return Inline.Create()
                     .Get("add", (string text) =>
                     {
                         using var db = Database.GetConnection();
                         using var command = db.CreateCommand();

                         command.CommandText = "INSERT INTO notes (text) VALUES ($text)";
                         command.Parameters.AddWithValue("$text", text);
                         command.ExecuteNonQuery();

                         return "added";
                     })
                     .Get("key", () => Secret.Exists("API_KEY") ? "set" : "unset");
        """;

    /// <summary>
    /// Saves the snippet with the given files beside it as the next version.
    /// </summary>
    private static async Task SaveAsync(LambdaFixture fixture, string privateKey, string change, params (string Name, string Content)[] files)
    {
        var all = files.Any(f => f.Name == LambdaSource.EntryName)
                ? files.OrderBy(f => f.Name == LambdaSource.EntryName ? 0 : 1).Select(f => new LambdaFile(f.Name, f.Content)).ToList()
                : [new LambdaFile(LambdaSource.EntryName, Snippet), .. files.Select(f => new LambdaFile(f.Name, f.Content))];

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/versions", new VersionRequest(all, null, change));

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());
    }

    private static async Task<SourceSettingsResponse> PublishAsync(LambdaFixture fixture, string privateKey, SourceRequest request)
    {
        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/source", request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<SourceSettingsResponse>();
    }

    private static async Task<SourceProjectResponse> ProjectAsync(LambdaFixture fixture, string publicKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/sources/{publicKey}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<SourceProjectResponse>();
    }

    private static async Task<SourceTreeResponse> TreeAsync(LambdaFixture fixture, string publicKey, int version)
    {
        using var response = await fixture.GetAsync($"/api/v1/sources/{publicKey}/versions/{version}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<SourceTreeResponse>();
    }

    private static async Task<SourceFileContentResponse> FileAsync(LambdaFixture fixture, string publicKey, int version, string path)
    {
        using var response = await fixture.GetAsync($"/api/v1/sources/{publicKey}/versions/{version}/files/{Uri.EscapeDataString(path)}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<SourceFileContentResponse>();
    }

    /// <summary>
    /// Stars a source the way a visitor does: reading it, and a moment later
    /// pressing the star.
    /// </summary>
    private static async Task<StarResponse> StarAsync(LambdaFixture fixture, string publicKey, bool starred)
    {
        var ticket = (await ProjectAsync(fixture, publicKey)).StarTicket;

        await Task.Delay(TimeSpan.FromSeconds(1.2));

        return await PostStarAsync(fixture, publicKey, ticket, starred);
    }

    private static async Task<StarResponse> PostStarAsync(LambdaFixture fixture, string publicKey, string ticket, bool starred)
    {
        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/sources/{publicKey}/star", new StarRequest(ticket, starred));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<StarResponse>();
    }

    private static Dictionary<string, byte[]> Entries(byte[] zip)
    {
        using var archive = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);

        return archive.Entries.Where(e => !e.FullName.EndsWith('/')).ToDictionary(e => e.FullName, e =>
        {
            using var stream = e.Open();
            using var buffer = new MemoryStream();

            stream.CopyTo(buffer);

            return buffer.ToArray();
        });
    }

    #endregion

}
