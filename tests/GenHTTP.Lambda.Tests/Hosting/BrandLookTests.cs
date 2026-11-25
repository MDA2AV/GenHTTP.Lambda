using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// The colours and the fonts whoever runs the community edition has its pages
/// drawn with - which genhttp.dev, being GenHTTP Lambda, does not offer.
/// </summary>
[TestClass]
public sealed class BrandLookTests
{

    private const string Teal = "#0f766e";

    /// <summary>
    /// An index page as a bundle is built, its style sheet linked in its head.
    /// </summary>
    private const string Index = """
        <!doctype html>
        <html lang="en">
        <head>
        <meta property="og:title" content="GenHTTP Lambda" />
        <meta property="og:site_name" content="GenHTTP Lambda" />
        <meta property="og:image" content="/social/default.png" />
        <link rel="icon" href="/favicon.svg" type="image/svg+xml" />
        <link rel="stylesheet" href="/assets/index.css">
        <title>Front - GenHTTP Lambda</title>
        </head>
        <body><div id="root"></div></body>
        </html>
        """;

    private const string Pages = """{ "/": { "text": { "en": { "title": "Platform", "description": "What runs here." } } } }""";

    #region Its colours

    [TestMethod]
    public async Task ThePagesAreDrawnInTheOperatorsAccent()
    {
        await using var fixture = await CommunityAsync();

        var branding = await ColorAsync(fixture, Teal);

        Assert.AreEqual(Teal, branding.Colors.Accent);
        Assert.IsNull(branding.Colors.Second);

        var markup = await PageAsync(fixture, "/en");

        Assert.Contains("<style id=\"lambda-look\">", markup);
        Assert.IsGreaterThan(markup.IndexOf("index.css", StringComparison.Ordinal), markup.IndexOf("lambda-look", StringComparison.Ordinal),
                             "after the bundle's style sheet, whose values it takes the place of");

        Assert.AreEqual("15 118 110", Value(markup, "accent-fill"), "a button is the accent as it was entered");
        Assert.AreEqual("255 255 255", Value(markup, "accent-ink"), "with what can be read written on it");

        Assert.AreEqual(Teal, (await BrandAsync(fixture)).Accent, "and the installation says which it is");
    }

    [TestMethod]
    public async Task AColorMakesNobodyAnotherInstallation()
    {
        await using var fixture = await CommunityAsync(options => File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "site"), "prerender.json"), JsonSerializer.Serialize(new
        {
            pages = new Dictionary<string, string> { ["/en"] = "<header><svg></svg>GenHTTP λ</header>" },
            showcase = new Dictionary<string, Dictionary<string, string>>(),
            entry = new Dictionary<string, string>(),
            branded = new { pages = new Dictionary<string, string> { ["/en"] = "<header>__LAMBDA_BRAND_NAME__</header>" }, showcase = new Dictionary<string, Dictionary<string, string>>() }
        })));

        await ColorAsync(fixture, Teal);

        var markup = await PageAsync(fixture, "/en");

        Assert.Contains("<title>Platform - GenHTTP Lambda</title>", markup);
        Assert.Contains("<svg></svg>GenHTTP λ", markup, "under the platform's mark");
        Assert.Contains("/favicon.svg", markup, "which is also the icon of the tab");
        Assert.Contains("og:image", markup, "and is drawn into the picture of a link preview");
        Assert.DoesNotContain("lambda-brand", markup);

        using var logo = await fixture.GetAsync("/api/v1/system/logo");

        Assert.AreEqual(HttpStatusCode.NotFound, logo.StatusCode);
    }

    [TestMethod]
    public async Task TheEditorAndThePanelAreDrawnInItToo()
    {
        await using var fixture = await CommunityAsync(options =>
        {
            File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "editor"), "index.html"), Index);
            File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "admin"), "index.html"), Index);
        });

        await ColorAsync(fixture, Teal);

        foreach (var path in (string[])["/editor/create", "/admin/branding"])
        {
            Assert.AreEqual("15 118 110", Value(await PageAsync(fixture, path), "accent-fill"), path);
        }
    }

    /// <summary>
    /// Colours that are somebody's house, and the ones that make it hard.
    /// </summary>
    [TestMethod]
    [DataRow("#0f766e")]
    [DataRow("#ffd600")]
    [DataRow("#e2001a")]
    [DataRow("#0b1f3a")]
    [DataRow("#f57c00")]
    [DataRow("#c6ff00")]
    [DataRow("#808080")]
    [DataRow("#ffffff")]
    [DataRow("#000000")]
    public async Task WhateverTheColorWhatIsWrittenInItCanBeRead(string accent)
    {
        await using var fixture = await CommunityAsync();

        var colors = (await PreviewAsync(fixture, accent)).Colors;

        var (light, dark) = (colors.Light.Accent!, colors.Dark.Accent!);

        foreach (var text in (string[])[light.Line, light.Text, light.Deep])
        {
            Assert.IsGreaterThanOrEqualTo(4.5, Contrast(text, "#ffffff"), $"{accent}: {text} on a light page");
        }

        foreach (var text in (string[])[dark.Tint, dark.Line, dark.Text, dark.Deep])
        {
            Assert.IsGreaterThanOrEqualTo(4.5, Contrast(text, "#202124"), $"{accent}: {text} on a dark page");
        }

        foreach (var (ink, surface) in ((string, string)[])[(light.Ink, light.Fill), (light.Ink, light.Hover), (dark.Ink, dark.Fill), (dark.Ink, dark.Hover)])
        {
            Assert.IsGreaterThanOrEqualTo(4.5, Contrast(ink, surface), $"{accent}: {ink} on a button of {surface}");
        }

        Assert.IsGreaterThanOrEqualTo(3.0, Contrast(dark.Fill, "#202124"), $"{accent}: the button on a dark page");
    }

    [TestMethod]
    [DataRow("#f59e0b")]
    [DataRow("#0b1f3a")]
    [DataRow("#c6ff00")]
    [DataRow("#7b2fd1")]
    public async Task WhatIsWrittenOnTheSecondAccentCanBeReadToo(string second)
    {
        await using var fixture = await CommunityAsync();

        var sheet = (await ColorAsync(fixture, null, second)).StyleSheet!;

        var themes = sheet.Split("html.dark");

        Assert.HasCount(2, themes);

        foreach (var theme in themes)
        {
            Assert.IsGreaterThanOrEqualTo(4.5, Contrast(Hex(Value(theme, "second-ink")!), Hex(Value(theme, "second-500")!)), $"{second}: {theme}");
        }

        Assert.IsGreaterThanOrEqualTo(4.5, Contrast(Hex(Value(themes[0], "second-700")!), "#ffffff"), "nor is a word in it on a light page harder to read");
        Assert.IsGreaterThanOrEqualTo(4.5, Contrast(Hex(Value(themes[1], "second-400")!), "#202124"), "or on a dark one");
    }

    [TestMethod]
    public async Task AColorIsDrawnAsItIsWhereverItCanBe()
    {
        await using var fixture = await CommunityAsync();

        var yellow = (await PreviewAsync(fixture, "#ffd600")).Colors;

        Assert.AreEqual("#ffd600", yellow.Light.Accent!.Fill, "a button of it has dark words on it rather than another colour");
        Assert.AreEqual("#202124", yellow.Light.Accent.Ink);
        Assert.AreEqual("#ffd600", yellow.Dark.Accent!.Tint, "and it is read on the dark page as it is");

        var red = (await PreviewAsync(fixture, "#e2001a")).Colors;

        Assert.AreEqual("#e2001a", red.Light.Accent!.Line);
        Assert.AreEqual("#e2001a", red.Dark.Accent!.Fill);
    }

    [TestMethod]
    public async Task TheMovingBackgroundIsInTheAccent()
    {
        await using var fixture = await CommunityAsync();

        await ColorAsync(fixture, Teal);

        var markup = await PageAsync(fixture, "/en");

        Assert.AreEqual("15 118 110", Value(markup, "aurora-a"), "its first light is the accent");
        Assert.AreNotEqual("15 118 110", Value(markup, "aurora-b"), "its second one is told apart from it");
        Assert.DoesNotContain("--second-500", markup, "and what the platform draws in purple stays in it");

        await ColorAsync(fixture, Teal, "#f59e0b");

        markup = await PageAsync(fixture, "/en");

        Assert.AreEqual("245 158 11", Value(markup, "aurora-b"), "unless there is a second accent, which is what it is then");
        Assert.Contains("--second-500", markup);

        await ColorAsync(fixture, null, "#f59e0b");

        markup = await PageAsync(fixture, "/en");

        Assert.DoesNotContain("--aurora-a", markup, "the platform's blue, where only the second is the operator's");
        Assert.AreEqual("245 158 11", Value(markup, "aurora-b"));
    }

    [TestMethod]
    public async Task TheLetterInPlaceOfALogoIsOnAPlateOfTheAccent()
    {
        await using var fixture = await CommunityAsync();

        await fixture.OperateAsync<BrandingResponse>(HttpMethod.Put, "branding", new { name = "Acme Apps" });

        var before = (await BrandAsync(fixture)).Logo;

        await ColorAsync(fixture, "#ffd600");

        var after = (await BrandAsync(fixture)).Logo;

        Assert.AreNotEqual(before, after, "another picture is another address");

        using var logo = await fixture.GetAsync(after!);

        var drawing = await logo.GetContentAsync();

        Assert.Contains("fill=\"#ffd600\"", drawing);
        Assert.Contains("fill=\"#202124\">A</text>", drawing, "with what can be read on it");
    }

    [TestMethod]
    public async Task AColorIsEnteredAsHexadecimal()
    {
        await using var fixture = await CommunityAsync();

        Assert.AreEqual("#11aa88", (await ColorAsync(fixture, " #1A8 ")).Colors.Accent, "in short as well");
        Assert.AreEqual(Teal, (await ColorAsync(fixture, "0F766E")).Colors.Accent, "and without the sign");

        foreach (var color in (string[])["teal", "#12345", "rgb(1,2,3)", "#0f766e;} body{display:none", "</style><script>"])
        {
            Assert.Contains("#rrggbb", await fixture.RefusedAsync(HttpMethod.Put, "branding/colors", new { accent = color }), color);
            Assert.Contains("second accent", await fixture.RefusedAsync(HttpMethod.Put, "branding/colors", new { second = color }), color);
        }

        Assert.AreEqual(Teal, (await BrandAsync(fixture)).Accent, "nothing was changed");
    }

    [TestMethod]
    public async Task WorkingOutALookChangesNothing()
    {
        await using var fixture = await CommunityAsync();

        await fixture.ShareAsync("Acme.woff2", new TestFont().Woff2());

        var preview = await PreviewAsync(fixture, "#e2001a", "#123456", "Acme.woff2");

        Assert.AreEqual("#e2001a", preview.Colors.Accent);
        Assert.AreEqual("Acme.woff2", preview.Fonts.Text.Single().Name);
        Assert.Contains("@font-face", preview.StyleSheet!);

        Assert.IsNull((await BrandAsync(fixture)).Accent);
        Assert.DoesNotContain("lambda-look", await PageAsync(fixture, "/en"));

        Assert.Contains("#rrggbb", await fixture.RefusedAsync(HttpMethod.Get, "branding/preview?accent=red"));
        Assert.Contains("none.woff2", await fixture.RefusedAsync(HttpMethod.Get, "branding/preview?text=none.woff2"));
    }

    [TestMethod]
    public async Task NamingItLeavesItsLookAsItIs()
    {
        await using var fixture = await CommunityAsync();

        await fixture.ShareAsync("Acme.woff2", new TestFont().Woff2());
        await FontAsync(fixture, ["Acme.woff2"]);
        await ColorAsync(fixture, Teal);

        var named = await fixture.OperateAsync<BrandingResponse>(HttpMethod.Put, "branding", new { name = "Acme Apps" });

        Assert.AreEqual(Teal, named.Colors.Accent);
        Assert.AreEqual("Acme.woff2", named.Fonts.Text.Single().Name);

        var colored = await ColorAsync(fixture, "#123456");

        Assert.AreEqual("Acme Apps", colored.Name, "and the other way round");
        Assert.AreEqual("Acme.woff2", colored.Fonts.Text.Single().Name);
    }

    #endregion

    #region Its fonts

    [TestMethod]
    public async Task ThePagesAreSetInTheOperatorsFonts()
    {
        await using var fixture = await CommunityAsync();

        await fixture.ShareAsync("Acme-Variable.woff2", new TestFont("Acme Sans", Axis: (100, 900)).Woff2());
        await fixture.ShareAsync("Acme-Italic.woff", new TestFont("Acme Sans", Italic: true).Woff());
        await fixture.ShareAsync("Display.ttf", new TestFont("Acme Display", 700).TrueType());

        var branding = await FontAsync(fixture, ["Acme-Variable.woff2", "Acme-Italic.woff"], ["Display.ttf"]);

        var variable = branding.Fonts.Text[0].Font!;

        Assert.AreEqual("Acme Sans", variable.Family);
        Assert.AreEqual((100, 900), (variable.From, variable.To), "the weights it draws are read from the file");
        Assert.IsTrue(branding.Fonts.Text[1].Font!.Italic);
        Assert.AreEqual(700, branding.Fonts.Headings[0].Font!.From);

        var markup = await PageAsync(fixture, "/en");

        var address = branding.Fonts.Text[0].VersionedPath;

        Assert.Contains($"@font-face{{font-family:\"Lambda Brand Text\";src:url(\"{address}\");font-weight:100 900;font-style:normal;font-display:swap}}", markup,
                        "shown once it is there, the words before");
        Assert.Contains("font-weight:400;font-style:italic", markup);
        Assert.Contains("--font-heading:\"Lambda Brand Headings\",\"Lambda Brand Text\",var(--font-system)", markup, "over what draws the letters it has none for");

        Assert.Contains($"<link rel=\"preload\" href=\"{address}\" as=\"font\" type=\"font/woff2\" crossorigin />", markup, "what the page needs first is named first");
        Assert.Contains($"<link rel=\"preload\" href=\"{branding.Fonts.Headings[0].VersionedPath}\" as=\"font\" type=\"font/ttf\" crossorigin />", markup);
        Assert.HasCount(2, Regex.Matches(markup, "rel=\"preload\""), "and nothing it may not need");

        using var font = await fixture.GetAsync(address);

        Assert.AreEqual(HttpStatusCode.OK, font.StatusCode);
        Assert.AreEqual("font/woff2", font.Content.Headers.ContentType?.MediaType);
        Assert.AreEqual("public, max-age=31536000, immutable", font.Headers.CacheControl?.ToString());
    }

    [TestMethod]
    public async Task AFontSaysForWhichLanguagesItHasTooFewLetters()
    {
        await using var fixture = await CommunityAsync();

        await fixture.ShareAsync("Acme-latin.woff2", new TestFont(Letters: TestFont.Latin).Woff2());
        await fixture.ShareAsync("Acme-latin-ext.woff2", new TestFont(Letters: TestFont.LatinExtended).Woff2());
        await fixture.ShareAsync("Icons.ttf", new TestFont(Letters: "AB").TrueType());

        var latin = await FontAsync(fixture, ["Acme-latin.woff2"], ["Icons.ttf"]);

        CollectionAssert.AreEquivalent((string[])["pl", "tr"], latin.Fonts.TextLacks.ToArray(), "cut down to western Europe");
        Assert.Contains("en", latin.Fonts.HeadingLacks, "no font for words at all");

        var whole = await FontAsync(fixture, ["Acme-latin.woff2", "Acme-latin-ext.woff2"]);

        Assert.IsEmpty(whole.Fonts.TextLacks, "the files of a font are taken together");
        Assert.IsEmpty(whole.Fonts.HeadingLacks, "and one that is not there lacks nothing");
    }

    [TestMethod]
    public async Task AFontIsMadeOfSharedFonts()
    {
        await using var fixture = await CommunityAsync();

        await fixture.ShareAsync("notes.md", "Not a font.");

        for (var i = 0; i < 13; i++)
        {
            await fixture.ShareAsync($"w{i}.ttf", new TestFont(Weight: 100 + i).TrueType());
        }

        Assert.Contains("missing.woff2", await fixture.RefusedAsync(HttpMethod.Put, "branding/fonts", new { text = (string[])["missing.woff2"] }));
        Assert.Contains("no font", await fixture.RefusedAsync(HttpMethod.Put, "branding/fonts", new { headings = (string[])["notes.md"] }));
        Assert.Contains("12 files", await fixture.RefusedAsync(HttpMethod.Put, "branding/fonts", new { text = Enumerable.Range(0, 13).Select(i => $"w{i}.ttf") }));

        Assert.DoesNotContain("lambda-look", await PageAsync(fixture, "/en"), "nothing was changed");
    }

    [TestMethod]
    public async Task AFileTheFontIsMadeOfIsReplacedAndNotDeleted()
    {
        await using var fixture = await CommunityAsync();

        await fixture.ShareAsync("Acme.woff2", new TestFont(Weight: 400).Woff2());

        var before = (await FontAsync(fixture, ["Acme.woff2"], ["Acme.woff2"])).Fonts.Text[0].VersionedPath;

        var refused = await fixture.RefusedAsync(HttpMethod.Delete, "shared/Acme.woff2", status: HttpStatusCode.Conflict);

        Assert.Contains("text font", refused);
        Assert.Contains("heading font", refused, "by all that uses it");

        await fixture.ShareAsync("Acme.woff2", new TestFont(Axis: (300, 800)).Woff2());

        var markup = await PageAsync(fixture, "/en");

        Assert.DoesNotContain(before, markup, "the pages are set in the new one at once");
        Assert.Contains("font-weight:300 800", markup, "in the weights it has");

        await FontAsync(fixture, []);

        using var deleted = await fixture.OperateAsync(HttpMethod.Delete, "shared/Acme.woff2");

        Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode, "let go of, it may be deleted");
    }

    #endregion

    #region Going back

    [TestMethod]
    public async Task WithoutAnyOfItAPageIsSentAsItWasBuilt()
    {
        await using var fixture = await CommunityAsync(options => File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "editor"), "index.html"), Index));

        Assert.AreEqual(Index, await PageAsync(fixture, "/editor/create"));

        await fixture.ShareAsync("Acme.woff2", new TestFont().Woff2());
        await FontAsync(fixture, ["Acme.woff2"]);
        await ColorAsync(fixture, Teal);

        Assert.AreNotEqual(Index, await PageAsync(fixture, "/editor/create"));

        await FontAsync(fixture, []);
        await ColorAsync(fixture, "", null);

        Assert.AreEqual(Index, await PageAsync(fixture, "/editor/create"));
    }

    [TestMethod]
    public async Task GoingBackTakesTheColorsAndTheFontsButNotTheirFiles()
    {
        await using var fixture = await CommunityAsync();

        await fixture.ShareAsync("Acme.woff2", new TestFont().Woff2());
        await FontAsync(fixture, ["Acme.woff2"]);
        await ColorAsync(fixture, Teal, "#123456");

        using var reset = await fixture.OperateAsync(HttpMethod.Delete, "branding");

        Assert.AreEqual(HttpStatusCode.NoContent, reset.StatusCode);
        Assert.DoesNotContain("lambda-look", await PageAsync(fixture, "/en"));
        Assert.IsNull((await BrandAsync(fixture)).Accent);

        using var font = await fixture.GetAsync("/api/v1/shared/Acme.woff2");

        Assert.AreEqual(HttpStatusCode.OK, font.StatusCode, "a shared file is the installation's, not the branding's");
    }

    [TestMethod]
    public async Task WhatWasChosenSurvivesARestart()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        string expected;

        await using (var first = await CommunityAsync(directory: directory))
        {
            await first.ShareAsync("Acme.woff2", new TestFont(Axis: (200, 700)).Woff2());
            await FontAsync(first, ["Acme.woff2"]);

            expected = (await ColorAsync(first, Teal, "#123456")).StyleSheet!;
        }

        SqliteConnection.ClearAllPools();

        await using var second = await CommunityAsync(directory: directory);

        Assert.Contains(expected, await PageAsync(second, "/en"));
    }

    #endregion

    #region Who may

    [TestMethod]
    public async Task OnlyTheOperatorChooses()
    {
        await using var fixture = await CommunityAsync();

        foreach (var (token, status) in ((string?, HttpStatusCode)[])[(null, HttpStatusCode.Unauthorized), ("not-the-token", HttpStatusCode.Forbidden)])
        {
            using var colors = await fixture.OperateAsync(HttpMethod.Put, "branding/colors", new StringContent("{\"accent\":\"#000000\"}"), token);
            Assert.AreEqual(status, colors.StatusCode);
        }

        Assert.IsNull((await BrandAsync(fixture)).Accent);
    }

    [TestMethod]
    public async Task GenHTTPDevIsDrawnAsItIs()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        await using (var community = await CommunityAsync(directory: directory))
        {
            await ColorAsync(community, Teal);
        }

        SqliteConnection.ClearAllPools();

        await using var cloud = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { DataDirectory = directory });

        Assert.IsNull((await BrandAsync(cloud)).Accent, "a copy of a branded installation's data brings nothing of it along");

        using var refused = await cloud.OperateAsync(HttpMethod.Put, "branding/colors", new StringContent("{\"accent\":\"#000000\"}"));

        Assert.AreEqual(HttpStatusCode.NotFound, refused.StatusCode);
    }

    #endregion

    #region Agents

    [TestMethod]
    public async Task TheOperatorsAgentChoosesTheLook()
    {
        await using var fixture = await CommunityAsync();

        await fixture.ShareAsync("Acme.woff2", new TestFont("Acme Sans", Axis: (100, 900)).Woff2());
        await fixture.ShareAsync("logo.svg", "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 1 1\"/>");

        var answer = await fixture.AnswerAsync("customize_installation", new JsonObject
        {
            ["name"] = "Acme Apps",
            ["logo"] = "logo.svg",
            ["accent"] = "#ffd600",
            ["textFont"] = new JsonArray("Acme.woff2")
        });

        Assert.AreEqual("#ffd600", answer["accent"]!.GetValue<string>());
        Assert.AreEqual("#202124", answer["drawnAs"]!["light"]!["writtenOnIt"]!.GetValue<string>(), "it is told what became of it");
        Assert.AreEqual("Acme Sans", answer["textFont"]![0]!["family"]!.GetValue<string>());
        Assert.Contains("pl", answer["textFontLacks"]!.AsArray().Select(l => l!.GetValue<string>()));
        Assert.AreEqual("image/svg+xml", answer["logo"]!["type"]!.GetValue<string>(), "a shared picture is a logo by its name");

        var markup = await PageAsync(fixture, "/en");

        Assert.AreEqual("255 214 0", Value(markup, "accent-fill"));
        Assert.Contains("Lambda Brand Text", markup);

        // what is left out stays, what is empty is given back
        var kept = await fixture.AnswerAsync("customize_installation", new JsonObject { ["secondAccent"] = "#123456" });

        Assert.AreEqual("#ffd600", kept["accent"]!.GetValue<string>());
        Assert.HasCount(1, kept["textFont"]!.AsArray());

        var emptied = await fixture.AnswerAsync("customize_installation", new JsonObject { ["accent"] = "", ["textFont"] = new JsonArray() });

        Assert.IsNull(emptied["accent"]);
        Assert.AreEqual("#123456", emptied["secondAccent"]!.GetValue<string>());
        Assert.IsEmpty(emptied["textFont"]!.AsArray());
        Assert.AreEqual("Acme Apps", emptied["name"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task AnAgentIsShownWhatItWouldBeAndARefusalChangesNothing()
    {
        await using var fixture = await CommunityAsync();

        var shown = await fixture.AnswerAsync("customize_installation", new JsonObject { ["name"] = "Acme Apps", ["accent"] = "#e2001a", ["preview"] = true });

        Assert.AreEqual("Acme Apps", shown["name"]!.GetValue<string>());
        Assert.AreEqual("#e2001a", shown["drawnAs"]!["light"]!["button"]!.GetValue<string>());
        Assert.Contains("Nothing was changed", shown["shown"]!.GetValue<string>());

        var refused = await fixture.ToolAsync("customize_installation", new JsonObject { ["name"] = "Acme Apps", ["accent"] = "#e2001a", ["textFont"] = new JsonArray("none.woff2") });

        Assert.IsTrue(refused["isError"]!.GetValue<bool>());
        Assert.Contains("none.woff2", OperatorCalls.TextOf(refused));

        var brand = await BrandAsync(fixture);

        Assert.AreEqual("GenHTTP Lambda", brand.Name, "neither of them, and nothing of the refused one");
        Assert.IsNull(brand.Accent);
    }

    #endregion

    #region Elsewhere

    [TestMethod]
    public async Task ThePagesOfTheHostingDomainAreInItsColorsAndFonts()
    {
        await using var fixture = await CommunityAsync(configure: o => o with
        {
            PublicUrl = "https://apps.example.com",
            HostingUrl = "https://run.example.com",
            Addressing = LambdaAddressing.Subdomains
        });

        async Task<string> HomeAsync()
        {
            using var page = await fixture.GetAsync("/", accept: "text/html", host: "run.example.com");

            return await page.GetContentAsync();
        }

        Assert.Contains("a { color: #1765cc; }", await HomeAsync());

        await fixture.ShareAsync("Acme.woff2", new TestFont().Woff2());
        await FontAsync(fixture, ["Acme.woff2"]);

        var branding = await ColorAsync(fixture, Teal);

        var markup = await HomeAsync();

        Assert.Contains($"a {{ color: {branding.Colors.Light.Accent!.Text}; }}", markup);
        Assert.Contains($"url(\"https://apps.example.com{branding.Fonts.Text[0].VersionedPath}\")", markup, "the font is the platform's, which answers elsewhere");
    }

    [TestMethod]
    public async Task WhatTheOperatorChoseIsInTheLog()
    {
        await using var fixture = await CommunityAsync();

        await fixture.ShareAsync("Acme.woff2", new TestFont().Woff2());
        await FontAsync(fixture, ["Acme.woff2"]);
        await ColorAsync(fixture, Teal);
        await ColorAsync(fixture, Teal);

        var lines = fixture.Book.Read(0, null, LogLevel.Information, 50_000).Lines.Select(l => l.Text).ToList();

        Assert.Contains("Changed branding fonts text Acme.woff2 headings none by operator", lines);
        Assert.HasCount(1, lines.Where(l => l == $"Changed branding colors accent {Teal} second none by operator"), "once, though it was saved twice");
    }

    #endregion

    #region Helpers

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

    private static Task<BrandingResponse> ColorAsync(LambdaFixture fixture, string? accent, string? second = null)
        => fixture.OperateAsync<BrandingResponse>(HttpMethod.Put, "branding/colors", new { accent, second });

    private static Task<BrandingResponse> FontAsync(LambdaFixture fixture, string[] text, string[]? headings = null)
        => fixture.OperateAsync<BrandingResponse>(HttpMethod.Put, "branding/fonts", new { text, headings });

    private static Task<BrandingPreviewResponse> PreviewAsync(LambdaFixture fixture, string accent, string? second = null, string? text = null)
        => fixture.OperateAsync<BrandingPreviewResponse>(HttpMethod.Get,
            $"branding/preview?accent={Uri.EscapeDataString(accent)}&second={Uri.EscapeDataString(second ?? "")}&text={text}");

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

    /// <summary>
    /// What a page sets one of the kit's values to in the light theme, which
    /// is said first.
    /// </summary>
    private static string? Value(string markup, string name)
        => Regex.Match(markup, $"--{Regex.Escape(name)}:([^;]+);") is { Success: true } match ? match.Groups[1].Value : null;

    /// <summary>
    /// A colour as the style sheet keeps it, by its channels, as hexadecimal.
    /// </summary>
    private static string Hex(string channels)
        => "#" + string.Concat(channels.Split(' ').Select(c => int.Parse(c, CultureInfo.InvariantCulture).ToString("x2", CultureInfo.InvariantCulture)));

    /// <summary>
    /// How well one colour is told from the other, as the WCAG counts it.
    /// </summary>
    private static double Contrast(string one, string other)
    {
        var (a, b) = (Luminance(one), Luminance(other));

        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    private static double Luminance(string hex)
    {
        double Linear(int at)
        {
            var value = int.Parse(hex.AsSpan(at, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;

            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Linear(1) + 0.7152 * Linear(3) + 0.0722 * Linear(5);
    }

    #endregion

}
