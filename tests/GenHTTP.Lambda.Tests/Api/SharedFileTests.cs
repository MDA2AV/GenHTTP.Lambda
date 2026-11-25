using System.Net;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// The files whoever runs an installation shares across it: for its own pages,
/// for the agents that build on it, and for the apps built there.
/// </summary>
[TestClass]
public sealed class SharedFileTests
{

    private static readonly byte[] Png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    private const string Svg = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10"><rect width="10" height="10" fill="#c00"/></svg>""";

    #region Sharing them

    [TestMethod]
    public async Task AFileIsSharedAndLoadedByAnyPage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        await fixture.ShareAsync("logo.svg", Svg);

        var listed = (await ListAsync(fixture)).Files.Single();

        Assert.AreEqual("logo.svg", listed.Name);
        Assert.AreEqual("picture", listed.Kind);
        Assert.AreEqual(Svg.Length, listed.Bytes);
        Assert.IsEmpty(listed.Uses!);
        Assert.AreEqual("/api/v1/shared/logo.svg", listed.Path);

        using var plain = await fixture.GetAsync(listed.Path);

        Assert.AreEqual(Svg, await plain.GetContentAsync());
        Assert.AreEqual("image/svg+xml", plain.Content.Headers.ContentType?.MediaType);
        Assert.AreEqual("*", Header(plain, "Access-Control-Allow-Origin"), "an app answers at another address");
        Assert.AreEqual("nosniff", Header(plain, "X-Content-Type-Options"));
        Assert.Contains("sandbox", Header(plain, "Content-Security-Policy"), "opened on its own, it is a document that runs nothing");
        Assert.AreEqual("public, max-age=60", plain.Headers.CacheControl?.ToString(), "whatever it holds by then, so not for long");

        using var versioned = await fixture.GetAsync(listed.VersionedPath);

        Assert.AreEqual("public, max-age=31536000, immutable", versioned.Headers.CacheControl?.ToString());
    }

    [TestMethod]
    public async Task AFileOfThatNameIsReplaced()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        await fixture.ShareAsync("style.css", "a { color: red }");

        var before = (await ListAsync(fixture)).Files.Single();

        await fixture.ShareAsync("style.css", "a { color: green }");

        var after = (await ListAsync(fixture)).Files.Single();

        Assert.AreNotEqual(before.VersionedPath, after.VersionedPath);

        using var replaced = await fixture.GetAsync(before.VersionedPath);

        Assert.AreEqual("a { color: green }", await replaced.GetContentAsync());
        Assert.AreEqual("public, max-age=60", replaced.Headers.CacheControl?.ToString(), "the address of what it held is not one of what it holds");

        Assert.HasCount(1, Kept(fixture), "and what it held is not kept");
    }

    [TestMethod]
    public async Task WhatAFileIsIsSaidByItsNameAndHeldToIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var font = new TestFont("Test Sans", 700, Axis: (200, 900));

        foreach (var (name, content, type) in ((string, byte[], string)[])
                 [
                     ("a.png", Png, "image/png"), ("a.svg", Encoding.UTF8.GetBytes(Svg), "image/svg+xml"), ("a.woff2", font.Woff2(), "font/woff2"),
                     ("a.woff", font.Woff(), "font/woff"), ("a.ttf", font.TrueType(), "font/ttf"), ("a.otf", font.OpenType(), "font/otf"),
                     ("a.css", "a{}"u8.ToArray(), "text/css"), ("a.md", "# A"u8.ToArray(), "text/markdown"), ("a.json", "{}"u8.ToArray(), "application/json"),
                     ("B-1_2.TXT", "x"u8.ToArray(), "text/plain")
                 ])
        {
            await fixture.ShareAsync(name, content);

            using var shared = await fixture.GetAsync($"/api/v1/shared/{name}");

            Assert.AreEqual(type, shared.Content.Headers.ContentType?.MediaType, name);
            CollectionAssert.AreEqual(content, await shared.Content.ReadAsByteArrayAsync(), name);
        }

        foreach (var file in (await ListAsync(fixture)).Files.Where(f => f.Kind == "font"))
        {
            Assert.AreEqual(("Test Sans", 200, 900), (file.Font!.Family, file.Font.From, file.Font.To), $"{file.Name} says what it draws, whatever it is wrapped in");
        }

        foreach (var (name, content, why) in ((string, byte[], string)[])
                 [
                     ("page.html", "<p>"u8.ToArray(), "no type"), ("app.js", "1"u8.ToArray(), "no type"), ("none", "x"u8.ToArray(), "no type"),
                     ("wrong.png", "x"u8.ToArray(), "PNG"), ("wrong.jpg", Png, "image/png"), ("wrong.svg", "<svg/>"u8.ToArray(), "xmlns"),
                     ("wrong.ttf", font.Woff2(), ".woff2"), ("cut.woff2", font.Woff2()[..90], "damaged"), ("cut.ttf", font.TrueType()[..60], "damaged"),
                     ("made.ttf", [0, 1, 0, 0, .. new byte[200]], "damaged"), ("several.ttf", [.. "ttcf"u8, .. new byte[40]], "collection"),
                     ("bytes.txt", [0xC3, 0x28], "UTF-8"), ("bytes.css", [0x61, 0], "UTF-8"), ("empty.md", [], "empty"),
                     ("-dash.md", "x"u8.ToArray(), "no name"), (".hidden.md", "x"u8.ToArray(), "no name"), ("a%20b.md", "x"u8.ToArray(), "no name"),
                     ("%C3%A4.md", "x"u8.ToArray(), "no name"), (new string('a', 58) + ".md", "x"u8.ToArray(), "60 characters"),
                     ("large.txt", new byte[2 * 1024 * 1024 + 1], "2 MB")
                 ])
        {
            Assert.Contains(why, await fixture.RefusedAsync(HttpMethod.Put, $"shared/{name}", content), name);
        }

        Assert.Contains("differs only in case", await fixture.RefusedAsync(HttpMethod.Put, "shared/A.PNG", Png, HttpStatusCode.Conflict));
        Assert.HasCount(10, (await ListAsync(fixture)).Files, "none of which was shared");
    }

    [TestMethod]
    public async Task ThereAreOnlySoManyOfThem()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        for (var i = 0; i < 10; i++)
        {
            await fixture.ShareAsync($"large{i}.txt", Encoding.ASCII.GetBytes(new string((char)('a' + i), 2 * 1024 * 1024)));
        }

        Assert.Contains("20 MB together", await fixture.RefusedAsync(HttpMethod.Put, "shared/more.txt", "x"u8.ToArray()));

        await fixture.ShareAsync("large0.txt", "smaller");
        await fixture.ShareAsync("more.txt", "room is made by what is replaced");

        for (var i = 0; i < 29; i++)
        {
            await fixture.ShareAsync($"small{i}.txt", "x");
        }

        Assert.Contains("as many as there may be", await fixture.RefusedAsync(HttpMethod.Put, "shared/one-more.txt", "x"u8.ToArray()));

        await fixture.ShareAsync("more.txt", "and one that is there may still be replaced");
    }

    [TestMethod]
    public async Task AFileIsDeletedForGood()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        await fixture.ShareAsync("one.txt", "the same");
        await fixture.ShareAsync("two.txt", "the same");

        using (var deleted = await fixture.OperateAsync(HttpMethod.Delete, "shared/one.txt"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode);
        }

        using (var gone = await fixture.GetAsync("/api/v1/shared/one.txt"))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, gone.StatusCode);
        }

        using (var other = await fixture.GetAsync("/api/v1/shared/two.txt"))
        {
            Assert.AreEqual("the same", await other.GetContentAsync(), "one that holds the same is another file");
        }

        await fixture.RefusedAsync(HttpMethod.Delete, "shared/one.txt", status: HttpStatusCode.NotFound);

        using (await fixture.OperateAsync(HttpMethod.Delete, "shared/two.txt"))
        {
        }

        Assert.IsEmpty(Kept(fixture));
    }

    [TestMethod]
    public async Task OnlyTheOperatorSharesFiles()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        using var anonymous = await fixture.OperateAsync(HttpMethod.Put, "shared/a.txt", new StringContent("x"), token: null);
        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        using var wrong = await fixture.OperateAsync(HttpMethod.Get, "shared", token: "not-the-token");
        Assert.AreEqual(HttpStatusCode.Forbidden, wrong.StatusCode);

        await using var without = await LambdaFixture.CreateAsync();

        using var none = await without.OperateAsync(HttpMethod.Get, "shared");
        Assert.AreEqual(HttpStatusCode.NotFound, none.StatusCode, "an installation without an operator has nobody to share any");
    }

    [TestMethod]
    public async Task WhatIsSharedSurvivesARestart()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        await using (var first = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { DataDirectory = directory }))
        {
            await first.ShareAsync("Acme.woff2", new TestFont("Acme Sans", Italic: true).Woff2());
        }

        SqliteConnection.ClearAllPools();

        await using var second = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { DataDirectory = directory });

        var file = (await ListAsync(second)).Files.Single();

        Assert.AreEqual(("Acme Sans", true), (file.Font!.Family, file.Font.Italic));

        using var content = await second.GetAsync(file.Path);

        Assert.AreEqual(HttpStatusCode.OK, content.StatusCode);
    }

    [TestMethod]
    public async Task ASearchEngineMayFetchThem()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var robots = await fixture.GetAsync("/robots.txt");

        Assert.Contains("Allow: /api/v1/shared/", await robots.GetContentAsync(), "a page it renders may be drawn with one");
    }

    #endregion

    #region The agents that build there

    [TestMethod]
    public async Task AgentsAreToldOfTheFilesThereAre()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { PublicUrl = "https://apps.example.com" });

        Assert.DoesNotContain("## sharedFiles", await Guide.ReadAsync(fixture), "nothing is said where there is nothing to say");

        await fixture.ShareAsync("house-style.md", "Teal, and square corners.");
        await fixture.ShareAsync("logo.png", Png);
        await fixture.ShareAsync("Acme.woff2", new TestFont("Acme Sans", Axis: (100, 900)).Woff2());

        var section = Guide.Section(await Guide.ReadAsync(fixture), "sharedFiles");

        Assert.Contains("https://apps.example.com/api/v1/shared/{name}", section, "with the address a page of theirs needs");
        Assert.Contains("- house-style.md: text, readable", section);
        Assert.Contains("- logo.png: picture\n", section);
        Assert.Contains("- Acme.woff2: font (Acme Sans, font-weight 100 900)", section, "what a page that uses it has to say");
        Assert.IsLessThan(section.IndexOf("- Acme.woff2", StringComparison.Ordinal), section.IndexOf("- house-style.md", StringComparison.Ordinal), "what is read ahead of what is drawn with");

        var guide = await Guide.ReadAsync(fixture);

        Assert.Contains("sharedFiles", Guide.Section(guide, "paths"), "where links are said to be relative, they are the exception");
        Assert.Contains("sharedFiles", Guide.Section(guide, "export"), "and an app that moves away takes them along");
    }

    [TestMethod]
    public async Task APageIsNeverSentToTheAddressAnAgentCalls()
    {
        // as the build agent does, by the address its container was given
        await using var paths = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        await paths.ShareAsync("logo.svg", Svg);
        await SectionAsync(paths, "Use [our logo](shared/logo.svg).");

        var guide = await Guide.ReadAsync(paths);

        Assert.Contains("links to one at /api/v1/shared/{name}", Guide.Section(guide, "sharedFiles"), "where lambdas answer below the platform, its path is theirs too");
        Assert.Contains("[our logo](/api/v1/shared/logo.svg)", Guide.Section(guide, "House rules"));
    }

    [TestMethod]
    public async Task AnAgentReadsATextThroughTheGuide()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        await fixture.ShareAsync("house-style.md", "Teal, and **square** corners.");
        await fixture.ShareAsync("logo.svg", Svg);
        await fixture.ShareAsync("logo.png", Png);
        await fixture.ShareAsync("large.css", new string('a', 33 * 1024));

        // the one door every agent has - the build agent, which makes no requests, among them
        var read = await fixture.ToolAsync("platform_guide", new JsonObject { ["file"] = "house-style.md" }, "/mcp");

        Assert.AreEqual("Teal, and **square** corners.", OperatorCalls.TextOf(read), "as it is");
        Assert.IsNull(read["structuredContent"], "sent once");

        Assert.AreEqual(Svg, OperatorCalls.TextOf(await fixture.ToolAsync("platform_guide", new JsonObject { ["file"] = "logo.svg" }, "/mcp")), "a drawing is text as well");

        foreach (var (name, why) in ((string, string)[])[("logo.png", "links to it at /api/v1/shared/logo.png"), ("large.css", "fetch it from"), ("none.md", "no shared file")])
        {
            var refused = await fixture.ToolAsync("platform_guide", new JsonObject { ["file"] = name }, "/mcp");

            Assert.IsTrue(refused["isError"]!.GetValue<bool>(), name);
            Assert.Contains(why, OperatorCalls.TextOf(refused));
        }

        Assert.StartsWith("# Platform guide", OperatorCalls.TextOf(await fixture.ToolAsync("platform_guide", path: "/mcp")), "and without one, it is the guide");
    }

    [TestMethod]
    public async Task TheGuideStaysWithinABudgetWithAllOfThem()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var before = Encoding.UTF8.GetByteCount(await Guide.ReadAsync(fixture));

        for (var i = 0; i < 40; i++)
        {
            await fixture.ShareAsync($"a-name-of-a-length-a-file-may-well-have-{i}.ttf", new TestFont("A Family Of Some Length", Axis: (100, 900)).TrueType());
        }

        var after = Encoding.UTF8.GetByteCount(await Guide.ReadAsync(fixture));

        Assert.IsLessThan(3_000, after - before, "the most there may be are not a second guide");
        Assert.Contains("more, which GET", Guide.Section(await Guide.ReadAsync(fixture), "sharedFiles"), "those that do not fit are said to be there");

        using var listed = await fixture.GetAsync("/api/v1/shared");

        var all = await listed.GetContentAsync<List<SharedFileResponse>>();

        Assert.HasCount(40, all, "and listed for whoever makes requests");
        Assert.IsNull(all[0].Uses, "without what is the operator's to know");
    }

    #endregion

    #region The operator's section of the guide

    [TestMethod]
    public async Task TheOperatorsSectionLinksToThem()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { PublicUrl = "https://apps.example.com" });

        await fixture.ShareAsync("house-style.md", "Teal.");
        await fixture.ShareAsync("logo.svg", Svg);

        const string text = "Follow the [house style](shared/house-style.md).\n\n![our logo](./shared/logo.svg) - put the front end in `resources/shared/`.\n\n[end][1]\n\n[1]: shared/house-style.md";

        await SectionAsync(fixture, text);

        var rules = Guide.Section(await Guide.ReadAsync(fixture), "House rules");

        Assert.Contains("[house style](https://apps.example.com/api/v1/shared/house-style.md)", rules);
        Assert.Contains("![our logo](https://apps.example.com/api/v1/shared/logo.svg)", rules);
        Assert.Contains("[1]: https://apps.example.com/api/v1/shared/house-style.md", rules);
        Assert.Contains("`resources/shared/`", rules, "what is no link is the operator's words");

        var kept = (await fixture.OperateAsync<GuideResponse>(HttpMethod.Get, "guide")).Section!.Text;

        Assert.AreEqual(text, kept, "kept as it was written: the address may change");

        Assert.Contains("guide", (await ListAsync(fixture)).Files[0].Uses!);
        Assert.Contains("guide", await fixture.RefusedAsync(HttpMethod.Delete, "shared/logo.svg", status: HttpStatusCode.Conflict));

        await SectionAsync(fixture, "No files any more.");

        using var deleted = await fixture.OperateAsync(HttpMethod.Delete, "shared/logo.svg");

        Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [TestMethod]
    public async Task ALinkToAFileNobodySharedIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        Assert.Contains("missing.md", await fixture.RefusedAsync(HttpMethod.Put, "guide", new { title = "House rules", text = "See [this](shared/missing.md)." }));
        Assert.Contains("is a section of the platform", await fixture.RefusedAsync(HttpMethod.Put, "guide", new { title = "sharedFiles", text = "Mine." }), "the section of the files is the platform's, whether there are any or not");
    }

    #endregion

    #region The operator's agent

    [TestMethod]
    public async Task TheOperatorsAgentManagesThem()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { PublicUrl = "https://apps.example.com" });

        var written = await fixture.AnswerAsync("write_shared_file", new JsonObject { ["name"] = "house-style.md", ["text"] = "Teal." });

        Assert.IsFalse(written["replaced"]!.GetValue<bool>());
        Assert.AreEqual("https://apps.example.com/api/v1/shared/house-style.md", written["file"]!["address"]!.GetValue<string>());

        Assert.IsTrue((await fixture.AnswerAsync("write_shared_file", new JsonObject { ["name"] = "house-style.md", ["text"] = "Green." }))["replaced"]!.GetValue<bool>());

        await fixture.AnswerAsync("write_shared_file", new JsonObject { ["name"] = "Acme.woff2", ["base64"] = Convert.ToBase64String(new TestFont("Acme Sans").Woff2()) });

        var listed = await fixture.AnswerAsync("list_shared_files");

        Assert.AreEqual("Acme Sans", listed["files"]![0]!["font"]!["family"]!.GetValue<string>());
        Assert.AreEqual(40, listed["limits"]!["files"]!.GetValue<int>());

        Assert.AreEqual("Green.", OperatorCalls.TextOf(await fixture.ToolAsync("read_shared_file", new JsonObject { ["name"] = "house-style.md" })));

        Assert.AreEqual("Acme.woff2", (await fixture.AnswerAsync("delete_shared_file", new JsonObject { ["name"] = "Acme.woff2" }))["deleted"]!.GetValue<string>());

        foreach (var arguments in (JsonObject[])
                 [
                     new() { ["name"] = "a.md" },
                     new() { ["name"] = "a.md", ["text"] = "x", ["base64"] = "eA==" },
                     new() { ["name"] = "a.md", ["base64"] = "not base64" },
                     new() { ["name"] = "a.html", ["text"] = "x" }
                 ])
        {
            Assert.IsTrue((await fixture.ToolAsync("write_shared_file", arguments))["isError"]!.GetValue<bool>(), arguments.ToJsonString());
        }

        var lines = fixture.Book.Read(0, null, LogLevel.Information, 50_000).Lines.Select(l => l.Text).ToList();

        Assert.Contains("Shared file house-style.md bytes 5 by operator", lines);
        Assert.Contains("Replaced shared file house-style.md bytes 6 by operator", lines);
        Assert.Contains(l => l.StartsWith("Deleted shared file Acme.woff2", StringComparison.Ordinal), lines);
    }

    #endregion

    #region Helpers

    private static Task<SharedFilesResponse> ListAsync(LambdaFixture fixture) => fixture.OperateAsync<SharedFilesResponse>(HttpMethod.Get, "shared");

    private static async Task SectionAsync(LambdaFixture fixture, string text)
        => await fixture.OperateAsync<GuideSectionResponse>(HttpMethod.Put, "guide", new { title = "House rules", text });

    /// <summary>
    /// The contents that are kept, where the local files integration keeps them.
    /// </summary>
    private static string[] Kept(LambdaFixture fixture) => Directory.GetFiles(Path.Combine(fixture.Options.DataDirectory, "shared"));

    private static string Header(HttpResponseMessage response, string name) => string.Join(",", response.Headers.GetValues(name));

    #endregion

}
