using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Git;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Data.Sqlite;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// The name and the logo whoever runs the community edition gives it, which
/// its pages show in place of GenHTTP Lambda's - and which genhttp.dev, being
/// GenHTTP Lambda, does not offer.
/// </summary>
[TestClass]
public sealed class BrandingTests
{

    /// <summary>
    /// A PNG of a single pixel, the smallest picture a browser draws.
    /// </summary>
    private static readonly byte[] Png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    private const string Svg = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10"><rect width="10" height="10" fill="#c00"/></svg>""";

    /// <summary>
    /// An index page as a bundle is built: named for GenHTTP Lambda, with the
    /// platform's icons.
    /// </summary>
    private const string Index = """
        <!doctype html>
        <html lang="en">
        <head>
        <meta property="og:title" content="GenHTTP Lambda" />
        <meta property="og:site_name" content="GenHTTP Lambda" />
        <meta property="og:image" content="/social/default.png" />
        <meta name="twitter:card" content="summary_large_image" />
        <link rel="icon" href="/favicon.ico" sizes="32x32" />
        <link rel="icon" href="/favicon.svg" type="image/svg+xml" />
        <link rel="apple-touch-icon" href="/apple-touch-icon.png" />
        <title>Front - GenHTTP Lambda</title>
        </head>
        <body><div id="root"></div></body>
        </html>
        """;

    private const string Pages = """
        {
          "/": { "image": "/social/default.png", "text": { "en": { "title": "Platform", "description": "Yours." } } },
          "/showcase": { "text": { "en": { "title": "Showcase", "description": "What people built." } } }
        }
        """;

    #region Naming it

    [TestMethod]
    public async Task TheOperatorNamesTheInstallation()
    {
        await using var fixture = await CommunityAsync();

        using var renamed = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding", Json(new { name = "  Acme Apps " }));

        Assert.AreEqual(HttpStatusCode.OK, renamed.StatusCode, await renamed.Content.ReadAsStringAsync());

        var branding = await renamed.GetContentAsync<BrandingResponse>();

        Assert.AreEqual("Acme Apps", branding.Name, "trimmed");
        Assert.AreEqual("Acme Apps", branding.Shown);
        Assert.IsNull(branding.Logo);
        Assert.IsNotNull(branding.Picture, "the first letter of the name stands in for a logo");

        var brand = await BrandAsync(fixture);

        Assert.AreEqual("Acme Apps", brand.Name, "every page asks the installation what it is called");
        Assert.AreEqual(branding.Picture, brand.Logo);
        Assert.IsTrue(brand.Changeable);
    }

    [TestMethod]
    public async Task ItsPagesAreNamedAfterIt()
    {
        await using var fixture = await CommunityAsync();

        await RenameAsync(fixture, "Acme Apps");

        var markup = await PageAsync(fixture, "/en");

        Assert.Contains("<title>Platform - Acme Apps</title>", markup);
        Assert.Contains("<meta property=\"og:site_name\" content=\"Acme Apps\" />", markup, "and so is the site of a link preview");
        Assert.Contains("<meta property=\"og:title\" content=\"Platform - Acme Apps\" />", markup);
        Assert.DoesNotContain("GenHTTP Lambda", markup, "nothing on the page names the platform any more");

        // the icon of the tab is its picture, and none of the platform's
        Assert.DoesNotContain("favicon", markup);
        Assert.Contains("<link rel=\"icon\" href=\"/api/v1/system/logo?v=", markup);
        Assert.Contains("type=\"image/svg+xml\"", markup);
        Assert.DoesNotContain("apple-touch-icon", markup, "a touch icon is a raster picture, which a letter drawn on a plate is not");

        // and the scripts draw the bar from what the page was sent with
        Assert.Contains("<script id=\"lambda-brand\" type=\"application/json\">{\"name\":\"Acme Apps\",\"logo\":\"/api/v1/system/logo?v=", markup);
    }

    [TestMethod]
    public async Task WhatAPageSaysOfItselfIsNeverRenamed()
    {
        await using var fixture = await CommunityAsync(options =>
            File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "site"), "pages.json"),
                              """{ "/": { "text": { "en": { "title": "Notes on GenHTTP Lambda", "description": "What runs here." } } } }"""));

        await RenameAsync(fixture, "Acme Apps");

        var markup = await PageAsync(fixture, "/en");

        Assert.Contains("<title>Notes on GenHTTP Lambda - Acme Apps</title>", markup, "the page's own words stay as they are, the installation is named after them");
        Assert.Contains("<meta property=\"og:title\" content=\"Notes on GenHTTP Lambda - Acme Apps\" />", markup);
    }

    [TestMethod]
    public async Task TheEditorAndThePanelAreNamedAfterItToo()
    {
        await using var fixture = await CommunityAsync(options =>
        {
            File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "editor"), "index.html"), Index.Replace("Front", "Editor"));
            File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "admin"), "index.html"), Index.Replace("Front", "Administration"));
        });

        using (var before = await fixture.GetAsync("/editor/create", accept: "text/html"))
        {
            Assert.AreEqual(Index.Replace("Front", "Editor"), await before.GetContentAsync(), "unnamed, a page is sent as it was built");
        }

        await RenameAsync(fixture, "Acme Apps");

        using var editor = await fixture.GetAsync("/editor/create", accept: "text/html");

        var markup = await editor.GetContentAsync();

        Assert.Contains("<title>Editor - Acme Apps</title>", markup);
        Assert.Contains("id=\"lambda-brand\"", markup);
        Assert.DoesNotContain("favicon", markup);

        using var panel = await fixture.GetAsync("/admin/branding", accept: "text/html");

        var page = await panel.GetContentAsync();

        Assert.Contains("<title>Administration - Acme Apps</title>", page);
        Assert.Contains("id=\"lambda-brand\"", page);
    }

    [TestMethod]
    public async Task ANameIsOneShortLine()
    {
        await using var fixture = await CommunityAsync();

        foreach (var name in (string[]) [new string('a', 41), "Acme\nApps", "Acme‮spp"])
        {
            using var response = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding", Json(new { name }));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, name);
        }

        Assert.AreEqual("GenHTTP Lambda", (await BrandAsync(fixture)).Name, "nothing was changed");
    }

    [TestMethod]
    public async Task ANameIsWrittenAsTextWhateverItSays()
    {
        await using var fixture = await CommunityAsync();

        await RenameAsync(fixture, "<script>alert(1)</script> & \"Co\"");

        var markup = await PageAsync(fixture, "/en");

        Assert.DoesNotContain("<script>alert(1)", markup);
        Assert.Contains("<title>Platform - &lt;script&gt;alert(1)&lt;/script&gt; &amp; &quot;Co&quot;</title>", markup);
        Assert.Contains("\\u003Cscript\\u003Ealert(1)\\u003C/script\\u003E", markup, "and as the scripts are handed it, nothing ends their element early");

        using (var logo = await fixture.GetAsync((await BrandAsync(fixture)).Logo!))
        {
            Assert.Contains(">S</text>", await logo.GetContentAsync(), "its first letter, past what is no letter");
        }

        await RenameAsync(fixture, "<&>");

        using var mark = await fixture.GetAsync((await BrandAsync(fixture)).Logo!);

        Assert.Contains(">&lt;</text>", await mark.GetContentAsync(), "a name of no letters is drawn by its first character, as text");
    }

    #endregion

    #region Its logo

    [TestMethod]
    public async Task ALogoIsShownBesideTheNameAndAsTheIconOfTheTab()
    {
        await using var fixture = await CommunityAsync();

        await RenameAsync(fixture, "Acme Apps");

        using var uploaded = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding/logo", Picture(Png, "application/octet-stream"));

        Assert.AreEqual(HttpStatusCode.OK, uploaded.StatusCode, await uploaded.Content.ReadAsStringAsync());

        var branding = await uploaded.GetContentAsync<BrandingResponse>();

        Assert.AreEqual("image/png", branding.Logo!.Type, "told by its content, not by what it was sent as");
        Assert.AreEqual(Png.Length, branding.Logo.Bytes);

        var brand = await BrandAsync(fixture);

        Assert.AreEqual(branding.Picture, brand.Logo);

        using var logo = await fixture.GetAsync(brand.Logo!);

        Assert.AreEqual(HttpStatusCode.OK, logo.StatusCode);
        Assert.AreEqual("image/png", logo.Content.Headers.ContentType?.MediaType);
        CollectionAssert.AreEqual(Png, await logo.Content.ReadAsByteArrayAsync());
        Assert.AreEqual("public, max-age=31536000, immutable", logo.Headers.CacheControl?.ToString(), "its address changes with it");
        Assert.AreEqual("nosniff", string.Join(",", logo.Headers.GetValues("X-Content-Type-Options")));

        using var unversioned = await fixture.GetAsync("/api/v1/system/logo");

        Assert.AreEqual(HttpStatusCode.OK, unversioned.StatusCode);
        Assert.AreEqual("no-cache", unversioned.Headers.CacheControl?.ToString(), "by another address it is asked about again");

        var markup = await PageAsync(fixture, "/en");

        Assert.Contains($"<link rel=\"icon\" href=\"{brand.Logo}\" type=\"image/png\" />", markup);
        Assert.Contains($"<link rel=\"apple-touch-icon\" href=\"{brand.Logo}\" />", markup);

        // the site's own picture has the platform drawn into it, so the
        // preview shows the logo instead, small beside the words
        Assert.Contains($"<meta property=\"og:image\" content=\"{brand.Logo}\" />", markup, "with the full address, where the installation names one");
        Assert.Contains("<meta property=\"og:image:type\" content=\"image/png\" />", markup);
        Assert.Contains("<meta name=\"twitter:card\" content=\"summary\" />", markup);
    }

    [TestMethod]
    public async Task AnSvgLogoIsDrawnAndNothingInItRuns()
    {
        await using var fixture = await CommunityAsync();

        var drawing = Svg.Replace("</svg>", "<script>alert(1)</script></svg>");

        using var uploaded = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding/logo", Picture(Encoding.UTF8.GetBytes(drawing), "image/svg+xml"));

        Assert.AreEqual(HttpStatusCode.OK, uploaded.StatusCode, await uploaded.Content.ReadAsStringAsync());

        var brand = await BrandAsync(fixture);

        Assert.AreEqual("GenHTTP Lambda", brand.Name, "a logo alone leaves the name as it was");

        using var logo = await fixture.GetAsync(brand.Logo!);

        Assert.AreEqual("image/svg+xml", logo.Content.Headers.ContentType?.MediaType);
        Assert.Contains("sandbox", string.Join(",", logo.Headers.GetValues("Content-Security-Policy")), "opened on its own, it is a document that runs nothing");

        var markup = await PageAsync(fixture, "/en");

        Assert.DoesNotContain("og:image", markup, "a social network draws no SVG, and the site's own picture is the platform's");
        Assert.Contains("<meta name=\"twitter:card\" content=\"summary\" />", markup);
    }

    [TestMethod]
    public async Task ALogoHasToBeAPictureABrowserDraws()
    {
        await using var fixture = await CommunityAsync();

        var refused = new (byte[] Content, string Why)[]
        {
            ([], "empty"),
            ("not a picture"u8.ToArray(), "text"),
            (Encoding.UTF8.GetBytes("""<svg viewBox="0 0 10 10"><rect width="10" height="10"/></svg>"""), "an SVG without its namespace, which a browser does not draw"),
            (Encoding.UTF8.GetBytes("""<svg xmlns="http://www.w3.org/2000/svg"><rect"""), "an SVG cut off"),
            ([.. Png, .. new byte[512 * 1024]], "larger than 512 KB")
        };

        foreach (var (content, why) in refused)
        {
            using var response = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding/logo", Picture(content, "image/png"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, why);
        }

        using var named = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding/logo",
                                          Picture(Encoding.UTF8.GetBytes("""<svg viewBox="0 0 1 1"/>"""), "image/svg+xml"));

        Assert.Contains("xmlns=", await named.Content.ReadAsStringAsync(), "the refusal says what an SVG is missing");

        Assert.IsNull((await BrandAsync(fixture)).Logo, "nothing was changed");
    }

    [TestMethod]
    public async Task ANameWithoutALogoIsDrawnAsItsFirstLetter()
    {
        await using var fixture = await CommunityAsync();

        await RenameAsync(fixture, "émile's apps");

        var brand = await BrandAsync(fixture);

        using var logo = await fixture.GetAsync(brand.Logo!);

        Assert.AreEqual("image/svg+xml", logo.Content.Headers.ContentType?.MediaType);
        Assert.Contains(">É</text>", await logo.GetContentAsync());

        await RenameAsync(fixture, "zebra");

        Assert.AreNotEqual(brand.Logo, (await BrandAsync(fixture)).Logo, "another letter is another address");
    }

    #endregion

    #region Going back

    [TestMethod]
    public async Task GoingBackShowsGenHTTPLambdaAgain()
    {
        await using var fixture = await CommunityAsync();

        await RenameAsync(fixture, "Acme Apps");

        using (var uploaded = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding/logo", Picture(Png, "image/png")))
        {
            Assert.AreEqual(HttpStatusCode.OK, uploaded.StatusCode);
        }

        using var reset = await SendAsync(fixture, HttpMethod.Delete, "/api/v1/admin/branding");

        Assert.AreEqual(HttpStatusCode.NoContent, reset.StatusCode);

        var brand = await BrandAsync(fixture);

        Assert.AreEqual("GenHTTP Lambda", brand.Name);
        Assert.IsNull(brand.Logo, "the pages draw the platform's mark again");

        using var logo = await fixture.GetAsync("/api/v1/system/logo");

        Assert.AreEqual(HttpStatusCode.NotFound, logo.StatusCode);

        var markup = await PageAsync(fixture, "/en");

        Assert.DoesNotContain("lambda-brand", markup);
        Assert.Contains("<title>Platform - GenHTTP Lambda</title>", markup);
        Assert.Contains("<link rel=\"icon\" href=\"/favicon.svg\" type=\"image/svg+xml\" />", markup);
    }

    [TestMethod]
    public async Task TakingTheLogoAwayKeepsTheName()
    {
        await using var fixture = await CommunityAsync();

        await RenameAsync(fixture, "Acme Apps");

        using (await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding/logo", Picture(Png, "image/png")))
        {
        }

        using var removed = await SendAsync(fixture, HttpMethod.Delete, "/api/v1/admin/branding/logo");

        Assert.AreEqual(HttpStatusCode.NoContent, removed.StatusCode);

        var brand = await BrandAsync(fixture);

        Assert.AreEqual("Acme Apps", brand.Name);

        using var logo = await fixture.GetAsync(brand.Logo!);

        Assert.AreEqual("image/svg+xml", logo.Content.Headers.ContentType?.MediaType, "its first letter again");
    }

    [TestMethod]
    public async Task AnEmptyNameGivesBackThePlatformsName()
    {
        await using var fixture = await CommunityAsync();

        await RenameAsync(fixture, "Acme Apps");
        await RenameAsync(fixture, "   ");

        var brand = await BrandAsync(fixture);

        Assert.AreEqual("GenHTTP Lambda", brand.Name);
        Assert.IsNull(brand.Logo);
    }

    #endregion

    #region Who may

    [TestMethod]
    public async Task OnlyTheOperatorBrandsTheInstallation()
    {
        await using var fixture = await CommunityAsync();

        using var anonymous = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding", Json(new { name = "Mine" }), token: null);

        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        using var wrong = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding/logo", Picture(Png, "image/png"), token: "not-the-token");

        Assert.AreEqual(HttpStatusCode.Forbidden, wrong.StatusCode);

        var brand = await BrandAsync(fixture);

        Assert.AreEqual("GenHTTP Lambda", brand.Name);
        Assert.IsNull(brand.Logo);
    }

    [TestMethod]
    public async Task WithoutAPanelThereIsNothingToBrandItWith()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync();

        using var response = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding", Json(new { name = "Mine" }));

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        Assert.IsTrue((await BrandAsync(fixture)).Changeable, "it could be branded, once the panel is switched on");
    }

    [TestMethod]
    public async Task GenHTTPDevIsGenHTTPLambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { PublicUrl = "https://genhttp.dev" });

        using var response = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding", Json(new { name = "Something else" }));

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "the cloud's host does not let its operator brand it");

        var brand = await BrandAsync(fixture);

        Assert.AreEqual("GenHTTP Lambda", brand.Name);
        Assert.IsNull(brand.Logo);
        Assert.IsFalse(brand.Changeable);

        var tools = await ToolsAsync(fixture);

        Assert.DoesNotContain("customize_installation", tools);
    }

    [TestMethod]
    public async Task WhatWasSavedSurvivesARestart()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        await using (var first = await CommunityAsync(directory: directory))
        {
            await RenameAsync(first, "Acme Apps");

            using var uploaded = await SendAsync(first, HttpMethod.Put, "/api/v1/admin/branding/logo", Picture(Png, "image/png"));

            Assert.AreEqual(HttpStatusCode.OK, uploaded.StatusCode);
        }

        SqliteConnection.ClearAllPools();

        await using var second = await CommunityAsync(directory: directory);

        var brand = await BrandAsync(second);

        Assert.AreEqual("Acme Apps", brand.Name);

        using var logo = await second.GetAsync(brand.Logo!);

        CollectionAssert.AreEqual(Png, await logo.Content.ReadAsByteArrayAsync());
    }

    [TestMethod]
    public async Task TheLogoIsAFileAndOneKeptInTheDatabaseBeforeIsMovedOut()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        var logoFile = Path.Combine(directory, "branding", "logo");

        await using (var first = await CommunityAsync(directory: directory))
        {
            using var uploaded = await SendAsync(first, HttpMethod.Put, "/api/v1/admin/branding/logo", Picture(Png, "image/png"));

            Assert.AreEqual(HttpStatusCode.OK, uploaded.StatusCode);

            CollectionAssert.AreEqual(Png, File.ReadAllBytes(logoFile));
            Assert.AreEqual(0L, LogoKept(first.Options), "no picture is kept in the database");
        }

        // as an installation from before kept it: in its row, no file
        File.Delete(logoFile);

        using (var connection = new SqliteConnection($"Data Source={new LambdaOptions { DataDirectory = directory }.DatabaseFile};Pooling=false"))
        {
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = "UPDATE branding_pictures SET content = $content";
            command.Parameters.AddWithValue("$content", Png);

            command.ExecuteNonQuery();
        }

        SqliteConnection.ClearAllPools();

        await using (var second = await CommunityAsync(directory: directory))
        {
            Assert.AreEqual(0L, LogoKept(second.Options));
            CollectionAssert.AreEqual(Png, File.ReadAllBytes(logoFile));

            using (var logo = await second.GetAsync((await BrandAsync(second)).Logo!))
            {
                CollectionAssert.AreEqual(Png, await logo.Content.ReadAsByteArrayAsync());
            }

            using var removed = await SendAsync(second, HttpMethod.Delete, "/api/v1/admin/branding/logo");

            Assert.IsTrue(removed.IsSuccessStatusCode);
        }

        Assert.IsFalse(File.Exists(logoFile), "taken away with the logo");
    }

    private static long LogoKept(LambdaOptions options)
    {
        using var connection = new SqliteConnection($"Data Source={options.DatabaseFile};Pooling=false");

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT COALESCE(SUM(length(content)), 0) FROM branding_pictures";

        return (long)command.ExecuteScalar()!;
    }

    [TestMethod]
    public async Task ACopyOfABrandedInstallationsDataBringsNothingOfItToTheCloud()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        await using (var community = await CommunityAsync(directory: directory))
        {
            await RenameAsync(community, "Acme Apps");
        }

        SqliteConnection.ClearAllPools();

        await using var cloud = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { DataDirectory = directory });

        Assert.AreEqual("GenHTTP Lambda", (await BrandAsync(cloud)).Name);
    }

    #endregion

    #region Agents

    [TestMethod]
    public async Task AnAgentWithTheTokenNamesTheInstallationAndDrawsItsLogo()
    {
        await using var fixture = await CommunityAsync();

        Assert.Contains("customize_installation", await ToolsAsync(fixture), "the operator's agents are shown the tool");

        var refused = await CallAsync(fixture, new JsonObject { ["adminToken"] = "not-the-token", ["name"] = "Mine" });

        Assert.IsTrue(refused["result"]!["isError"]!.GetValue<bool>());
        Assert.AreEqual("GenHTTP Lambda", (await BrandAsync(fixture)).Name, "nothing was changed");

        var answer = await CallAsync(fixture, new JsonObject { ["adminToken"] = LambdaFixture.OperatorToken, ["name"] = "Acme Apps", ["logo"] = Svg });

        var said = answer["result"]!["structuredContent"]!;

        Assert.AreEqual("Acme Apps", said["name"]!.GetValue<string>());
        Assert.AreEqual("image/svg+xml", said["logo"]!["type"]!.GetValue<string>(), "an SVG's markup is taken as it is");
        Assert.DoesNotContain(LambdaFixture.OperatorToken, answer.ToJsonString(), "the token is never repeated");

        var brand = await BrandAsync(fixture);

        Assert.AreEqual("Acme Apps", brand.Name);

        using var logo = await fixture.GetAsync(brand.Logo!);

        Assert.AreEqual(Svg, await logo.GetContentAsync());

        var removed = await CallAsync(fixture, new JsonObject { ["adminToken"] = LambdaFixture.OperatorToken, ["logo"] = "" });

        Assert.IsNull(removed["result"]!["structuredContent"]!["logo"], "an empty logo takes it away");

        var base64 = await CallAsync(fixture, new JsonObject { ["adminToken"] = LambdaFixture.OperatorToken, ["logo"] = Convert.ToBase64String(Png) });

        Assert.AreEqual("image/png", base64["result"]!["structuredContent"]!["logo"]!["type"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task AnAgentsRefusedLogoChangesNothing()
    {
        await using var fixture = await CommunityAsync();

        var answer = await CallAsync(fixture, new JsonObject
        {
            ["adminToken"] = LambdaFixture.OperatorToken,
            ["name"] = "Acme Apps",
            ["logo"] = "<svg viewBox=\"0 0 1 1\"/>"
        });

        Assert.IsTrue(answer["result"]!["isError"]!.GetValue<bool>());
        Assert.Contains("xmlns", answer["result"]!["content"]![0]!["text"]!.GetValue<string>(), "it says what the drawing is missing");

        Assert.AreEqual("GenHTTP Lambda", (await BrandAsync(fixture)).Name, "the name it came with was not taken either");
    }

    [TestMethod]
    public async Task WithoutATokenNoAgentIsShownTheTool()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync();

        Assert.DoesNotContain("customize_installation", await ToolsAsync(fixture));
    }

    #endregion

    #region What else names it

    [TestMethod]
    public async Task AppsBuiltHereSayTheyWereMadeWithIt()
    {
        await using var fixture = await CommunityAsync(configure: o => o with { PublicUrl = "https://apps.example.com" });

        await RenameAsync(fixture, "Acme & Co");

        var backlink = await BacklinkAsync(fixture);

        Assert.Contains("\"Made with Acme & Co\"", backlink, "the line names the installation it links to");
        Assert.Contains("<a href=\"https://apps.example.com/\">Made with Acme &amp; Co</a>", backlink,
                        "written as markup an agent puts into a page as it is");

        Assert.Contains("given this name with your next change", backlink, "a line from before the rename is brought up to date");

        using var reset = await SendAsync(fixture, HttpMethod.Delete, "/api/v1/admin/branding");

        Assert.Contains(">Made with GenHTTP Lambda</a>", await BacklinkAsync(fixture), "and the platform where nobody named it");
    }

    [TestMethod]
    public async Task ARepositoryNamesItAndARenameRefusesNoPush()
    {
        await using var fixture = await CommunityAsync();

        using var git = new GitClient();

        var lambda = await fixture.CreateLambdaAsync("named", "demo-crud");

        await git.CloneAsync(fixture.EditorUrl(lambda), "named");

        Assert.Contains("a lambda on GenHTTP Lambda", git.Read("named", "AGENTS.md"));

        await RenameAsync(fixture, "Acme Apps");

        // committed on top of what was cloned while it went by another name
        git.Change("named", "Project.cs", "var store = new TaskStore(limit: 200);", "var store = new TaskStore(limit: 300);");

        await git.CommitAsync("named", "Keeps three hundred tasks");

        var pushed = await git.RunAsync("named", "push");

        Assert.Contains("Saved version 2", pushed.Said, "the platform's files are compared with the commit the push builds on");

        using (var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                    LambdaFixture.Version("return Inline.Create().Get(() => \"renamed\");")))
        {
            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);
        }

        await git.RunAsync("named", "pull", "--ff-only");

        Assert.Contains("a lambda on Acme Apps", git.Read("named", "AGENTS.md"), "the commit made after the rename names it");
        Assert.Contains("\"Made with Acme Apps\"", git.Read("named", "AGENTS.md"));
        Assert.Contains("a lambda on Acme Apps", git.Read("named", "CLAUDE.md"));
        Assert.Contains("This app is a lambda on Acme Apps", git.Read("named", "Program.cs"));
    }

    [TestMethod]
    public async Task TheManifestNamesIt()
    {
        await using var fixture = await CommunityAsync(options =>
            File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "site"), "manifest.json"), """{ "name": "GenHTTP Lambda", "short_name": "GenHTTP λ", "display": "standalone", "icons": [] }"""));

        using (var built = await fixture.GetAsync("/manifest.json"))
        {
            Assert.Contains("GenHTTP λ", await built.GetContentAsync(), "unnamed, it is served as it was built");
        }

        await RenameAsync(fixture, "Acme Apps");

        using var named = await fixture.GetAsync("/manifest.json");

        var manifest = JsonNode.Parse(await named.GetContentAsync())!;

        Assert.AreEqual("Acme Apps", manifest["name"]!.GetValue<string>());
        Assert.AreEqual("Acme Apps", manifest["short_name"]!.GetValue<string>());
        Assert.AreEqual("standalone", manifest["display"]!.GetValue<string>(), "and says the rest as it was built");
        Assert.AreEqual((await BrandAsync(fixture)).Logo, manifest["icons"]![0]!["src"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task ASearchEngineMayFetchItsIcon()
    {
        await using var fixture = await CommunityAsync();

        using (var plain = await fixture.GetAsync("/robots.txt"))
        {
            Assert.DoesNotContain("/api/v1/system/logo", await plain.GetContentAsync());
        }

        await RenameAsync(fixture, "Acme Apps");

        using var robots = await fixture.GetAsync("/robots.txt");

        Assert.Contains("Allow: /api/v1/system/logo", await robots.GetContentAsync());

        using var llms = await fixture.GetAsync("/llms.txt");

        StringAssert.StartsWith(await llms.GetContentAsync(), "# Acme Apps");
    }

    [TestMethod]
    public async Task ThePagesOfThePublishedSourcesAreNamedAfterIt()
    {
        await using var fixture = await CommunityAsync(options =>
        {
            File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "sources"), "index.html"), Index);
            File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "sources"), "pages.json"), """
                {
                  "/source": { "prerender": false, "text": { "en": { "title": "Open source", "description": "Read the code." } } },
                  "/source/:key": { "text": { "en": { "title": "{name} - Source code", "description": "Source code of {name}, a web app built on {platform}." } } }
                }
                """);
        });

        await RenameAsync(fixture, "Acme & Co");

        var markup = await PageAsync(fixture, "/en/source");

        Assert.Contains("<title>Open source - Acme &amp; Co</title>", markup);
        Assert.Contains("id=\"lambda-brand\"", markup);

        var lambda = await fixture.CreateLambdaAsync("quiz");

        // without documentation, so the page is described in the words of its table
        using (var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", LambdaFixture.Version("return Inline.Create();")))
        {
            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);
        }

        using (var published = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/source", new SourceRequest(null, null)))
        {
            Assert.AreEqual(HttpStatusCode.OK, published.StatusCode);
        }

        var source = await PageAsync(fixture, "/en/source/quiz");

        Assert.Contains("<meta name=\"description\" content=\"Source code of quiz, a web app built on Acme &amp; Co.\" />", source,
                        "what the page says the app was built with is the installation");
    }

    [TestMethod]
    public async Task TheHostingDomainNamesItWhereItHasNoAddressToLinkTo()
    {
        await using var fixture = await CommunityAsync(options => { }, o => o with { HostingUrl = "https://apps.example.com", Addressing = LambdaAddressing.Subdomains });

        await RenameAsync(fixture, "Acme Apps");

        using var page = await fixture.GetAsync("/", accept: "text/html", host: "apps.example.com");

        var markup = await page.GetContentAsync();

        Assert.Contains("<title>Apps made with Acme Apps</title>", markup, "what the apps here were made with");
        Assert.Contains("go to Acme Apps.", markup, "and where to make one, named where there is no address to link to");
        Assert.DoesNotContain("GenHTTP Lambda", markup);
    }

    #endregion

    #region The pages as the build rendered them

    [TestMethod]
    public async Task ABrandedInstallationIsRenderedFromTheBrandedPages()
    {
        await using var fixture = await CommunityAsync(options => File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "site"), "prerender.json"), JsonSerializer.Serialize(new
        {
            pages = new Dictionary<string, string> { ["/en"] = "<header><svg></svg>GenHTTP λ</header><main>Hello</main>" },
            showcase = new Dictionary<string, Dictionary<string, string>>(),
            entry = new Dictionary<string, string>(),
            branded = new
            {
                pages = new Dictionary<string, string> { ["/en"] = "<header><img src=\"__LAMBDA_BRAND_LOGO__\"/>__LAMBDA_BRAND_NAME__</header><main>Hello</main>" },
                showcase = new Dictionary<string, Dictionary<string, string>>()
            }
        })));

        Assert.Contains("<svg></svg>GenHTTP λ", await PageAsync(fixture, "/en"), "unnamed, the platform's mark");

        await RenameAsync(fixture, "Acme & Co");

        var markup = await PageAsync(fixture, "/en");

        var brand = await BrandAsync(fixture);

        Assert.Contains($"<header><img src=\"{brand.Logo}\"/>Acme &amp; Co</header><main>Hello</main>", markup);
        Assert.DoesNotContain("GenHTTP λ", markup);
    }

    [TestMethod]
    public async Task ABuildWithoutBrandedPagesLeavesThemToTheBrowser()
    {
        await using var fixture = await CommunityAsync(options => File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "site"), "prerender.json"), JsonSerializer.Serialize(new
        {
            pages = new Dictionary<string, string> { ["/en"] = "<header>GenHTTP λ</header><main>Hello</main>" },
            showcase = new Dictionary<string, Dictionary<string, string>>(),
            entry = new Dictionary<string, string>()
        })));

        await RenameAsync(fixture, "Acme Apps");

        var markup = await PageAsync(fixture, "/en");

        Assert.Contains("<div id=\"root\"></div>", markup, "drawn by the browser rather than under the platform's mark");
        Assert.DoesNotContain("GenHTTP λ", markup);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// The community edition with its panel, and the index page and the
    /// table of pages of a site built for it.
    /// </summary>
    private static Task<LambdaFixture> CommunityAsync(Action<LambdaOptions>? bundles = null, Func<LambdaOptions, LambdaOptions>? configure = null, string? directory = null)
        => LambdaFixture.CreateCommunityAsync(options =>
        {
            options = LambdaFixture.WithPanel(configure?.Invoke(options) ?? options);

            if (directory != null)
            {
                options = options with { DataDirectory = directory };
            }

            File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "site"), "index.html"), Index);
            File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "site"), "pages.json"), Pages);

            bundles?.Invoke(options);

            return options;
        });

    private static async Task RenameAsync(LambdaFixture fixture, string name)
    {
        using var response = await SendAsync(fixture, HttpMethod.Put, "/api/v1/admin/branding", Json(new { name }));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private static async Task<BrandResponse> BrandAsync(LambdaFixture fixture)
    {
        using var system = await fixture.GetAsync("/api/v1/system");

        return (await system.GetContentAsync<PlatformResponse>()).Brand;
    }

    private static async Task<string> PageAsync(LambdaFixture fixture, string path)
    {
        using var page = await fixture.GetAsync(path, accept: "text/html");

        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode, path);

        return await page.GetContentAsync();
    }

    private static HttpContent Json(object payload)
        => new StringContent(JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json");

    private static ByteArrayContent Picture(byte[] content, string type)
    {
        var body = new ByteArrayContent(content);

        body.Headers.ContentType = new MediaTypeHeaderValue(type);

        return body;
    }

    private static async Task<HttpResponseMessage> SendAsync(LambdaFixture fixture, HttpMethod method, string path, HttpContent? content = null,
                                                             string? token = LambdaFixture.OperatorToken)
    {
        using var request = fixture.Host.GetRequest(path, method);

        if (token != null)
        {
            request.Headers.Add("X-Admin-Token", token);
        }

        request.Content = content;

        return await fixture.Host.GetResponseAsync(request);
    }

    private static async Task<string> ToolsAsync(LambdaFixture fixture)
    {
        using var response = await PostMcpAsync(fixture, new JsonObject { ["jsonrpc"] = "2.0", ["id"] = 1, ["method"] = "tools/list", ["params"] = new JsonObject() });

        return await response.Content.ReadAsStringAsync();
    }

    private static async Task<JsonObject> CallAsync(LambdaFixture fixture, JsonObject arguments)
    {
        using var response = await PostMcpAsync(fixture, new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = "customize_installation", ["arguments"] = arguments }
        });

        return (JsonObject)JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
    }

    private static async Task<string> BacklinkAsync(LambdaFixture fixture) => Guide.Section(await Guide.ReadAsync(fixture), "backlink");

    private static async Task<HttpResponseMessage> PostMcpAsync(LambdaFixture fixture, JsonObject message)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");
        request.Content = new StringContent(message.ToJsonString(), Encoding.UTF8, "application/json");

        return await fixture.Host.GetResponseAsync(request);
    }

    #endregion

}
