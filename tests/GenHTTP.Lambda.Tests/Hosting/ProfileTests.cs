using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// Who an installation is for - the public, somebody and the people around
/// them, or an organization - which its pages and its guide for agents speak
/// to: chosen by whoever runs the community edition, and the cloud on
/// genhttp.dev.
/// </summary>
[TestClass]
public sealed class ProfileTests
{

    private const string Index = """
        <!doctype html>
        <html lang="en">
        <head><title>GenHTTP Lambda</title></head>
        <body><div id="root"></div></body>
        </html>
        """;

    private const string Pages = """
        {
          "/": { "text": { "en": { "title": "Platform", "description": "Yours." } } },
          "/build": { "text": { "en": { "title": "Build", "description": "Describe an app." } } }
        }
        """;

    #region Choosing it

    [TestMethod]
    public async Task ACommunityInstallationIsAHomeOneUntilItsOperatorSaysOtherwise()
    {
        await using var fixture = await CommunityAsync();

        Assert.AreEqual("home", await ProfileAsync(fixture));

        var settings = await SettingsAsync(fixture);

        Assert.AreEqual("home", settings.Profile);
        CollectionAssert.AreEqual(new[] { "home", "enterprise" }, settings.Profiles.ToArray(), "what its operator may choose");
    }

    [TestMethod]
    public async Task TheOperatorSaysItIsAnOrganizations()
    {
        await using var fixture = await CommunityAsync();

        var saved = await ChangeAsync(fixture, new { profile = "enterprise" });

        Assert.AreEqual("enterprise", saved.Profile);
        Assert.AreEqual("enterprise", await ProfileAsync(fixture), "every page asks it of the installation");

        var logged = fixture.Book.Read(0, null, LogLevel.Information, 50_000).Lines.Select(l => l.Text).ToList();

        Assert.IsTrue(logged.Any(l => l.Contains("Updated settings", StringComparison.Ordinal) && l.Contains("profile enterprise", StringComparison.Ordinal)),
                      "what was done is logged");
    }

    [TestMethod]
    public async Task WhatAChangeLeavesOutStaysAsItIs()
    {
        await using var fixture = await CommunityAsync();

        await ChangeAsync(fixture, new { profile = "enterprise" });

        var switched = await ChangeAsync(fixture, new { optimizeDatabases = false });

        Assert.AreEqual("enterprise", switched.Profile, "a client that knows of one setting changes no other");
        Assert.IsFalse(switched.OptimizeDatabases);

        var chosen = await ChangeAsync(fixture, new { profile = "home" });

        Assert.AreEqual("home", chosen.Profile);
        Assert.IsFalse(chosen.OptimizeDatabases, "and the other way round");
    }

    [TestMethod]
    public async Task AProfileItsHostDoesNotOfferIsRefused()
    {
        await using var fixture = await CommunityAsync();

        foreach (var profile in (string[])["cloud", "office"])
        {
            using var response = await fixture.SendAsOperatorAsync(HttpMethod.Put, "/api/v1/admin/settings", new { profile });

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, profile);
        }

        Assert.AreEqual("home", await ProfileAsync(fixture), "nothing changed");
    }

    [TestMethod]
    public async Task GenHTTPDevIsTheCloud()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        Assert.AreEqual("cloud", await ProfileAsync(fixture));

        var settings = await SettingsAsync(fixture);

        Assert.AreEqual("cloud", settings.Profile);
        Assert.IsEmpty(settings.Profiles, "its host decided, so there is nothing to choose");

        using var refused = await fixture.SendAsOperatorAsync(HttpMethod.Put, "/api/v1/admin/settings", new { profile = "enterprise" });

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);

        using var kept = await fixture.SendAsOperatorAsync(HttpMethod.Put, "/api/v1/admin/settings", new { profile = "cloud", optimizeDatabases = false });

        Assert.AreEqual(HttpStatusCode.OK, kept.StatusCode, "what it is may be named, with the switches beside it");
        Assert.AreEqual("cloud", await ProfileAsync(fixture));
    }

    [TestMethod]
    public async Task WhatWasChosenSurvivesARestart()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        await using (var first = await CommunityAsync(directory: directory))
        {
            await ChangeAsync(first, new { profile = "enterprise" });
        }

        SqliteConnection.ClearAllPools();

        await using var second = await CommunityAsync(directory: directory);

        Assert.AreEqual("enterprise", await ProfileAsync(second));
    }

    [TestMethod]
    public async Task ACopyOfACommunityInstallationsDataBringsNothingOfItToTheCloud()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        await using (var community = await CommunityAsync(directory: directory))
        {
            await ChangeAsync(community, new { profile = "enterprise" });
        }

        SqliteConnection.ClearAllPools();

        await using var cloud = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { DataDirectory = directory });

        Assert.AreEqual("cloud", await ProfileAsync(cloud));
    }

    #endregion

    #region What agents are told

    [TestMethod]
    public async Task TheGuideSaysWhomTheAppsBuiltHereAreFor()
    {
        await using var fixture = await CommunityAsync();

        var home = await Guide.ReadAsync(fixture);

        Assert.Contains("Who it is for: whoever runs it, their household and their friends.", home);

        await ChangeAsync(fixture, new { profile = "enterprise" });

        var enterprise = await Guide.ReadAsync(fixture);

        Assert.Contains("Who it is for: an organization and its people.", enterprise, "read as the agent asks, after the operator chose");
        Assert.Contains("plain and businesslike", enterprise);
        Assert.DoesNotContain("their household", enterprise);

        Assert.AreEqual(Headings(home), Headings(enterprise), "a sentence of its introduction, not a section an operator's could collide with");
    }

    [TestMethod]
    public async Task AnOrganizationsAppsCarryNoLinkBack()
    {
        await using var fixture = await CommunityAsync();

        Assert.Contains(">Made with GenHTTP Lambda</a>", Guide.Section(await Guide.ReadAsync(fixture), "backlink"), "asked for at home, as on the cloud");

        await ChangeAsync(fixture, new { profile = "enterprise" });

        var backlink = Guide.Section(await Guide.ReadAsync(fixture), "backlink");

        Assert.Contains("asks for none", backlink);
        Assert.Contains("take out one a page you change has", backlink, "what an earlier profile asked for goes with the next change");
        Assert.DoesNotContain("<a href", backlink, "nothing to link to");
    }

    [TestMethod]
    public async Task AnOrganizationsGuideStillFitsIntoOneToolResult()
    {
        // the longest sentence, on the community edition, with terms as the cloud's budget is measured
        await using var fixture = await CommunityAsync(configure: o => o with { Terms = "Do no harm to the server or to others." });

        await ChangeAsync(fixture, new { profile = "enterprise" });

        var guide = await Guide.ReadAsync(fixture);

        Assert.Contains("Who it is for: an organization and its people.", guide);
        Assert.IsLessThan(28_000, Encoding.UTF8.GetByteCount(guide), "within the budget TheGuideFitsIntoOneToolResult holds the cloud's to");
    }

    [TestMethod]
    public async Task OnTheCloudTheGuideSaysTheAppsAreForThePublic()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        Assert.Contains("Who it is for: the public.", await Guide.ReadAsync(fixture));
    }

    #endregion

    #region The pages as the build rendered them

    [TestMethod]
    public async Task AHomeInstallationIsRenderedInTheWordsEveryProfileShares()
    {
        await using var fixture = await CommunityAsync(Rendered(enterprise: true));

        var build = await PageAsync(fixture, "/en/build");

        Assert.Contains("<main>Ideas for a club</main>", build);
        Assert.Contains("\"profile\":\"home\"", build, "the page tells the application which words it was rendered in");
    }

    [TestMethod]
    public async Task AnEnterpriseInstallationIsRenderedInItsOwnWords()
    {
        await using var fixture = await CommunityAsync(Rendered(enterprise: true));

        await ChangeAsync(fixture, new { profile = "enterprise" });

        var build = await PageAsync(fixture, "/en/build");

        Assert.Contains("<main>Ideas for work</main>", build);
        Assert.DoesNotContain("Ideas for a club", build);
        Assert.Contains("\"profile\":\"enterprise\"", build);

        Assert.Contains("<main>Shared front</main>", await PageAsync(fixture, "/en"), "a page that says nothing of its own is the shared one");
    }

    [TestMethod]
    public async Task ABrandedEnterpriseInstallationIsRenderedInItsOwnWordsUnderItsName()
    {
        await using var fixture = await CommunityAsync(Rendered(enterprise: true));

        using (var renamed = await fixture.SendAsOperatorAsync(HttpMethod.Put, "/api/v1/admin/branding", new { name = "Acme Apps" }))
        {
            Assert.AreEqual(HttpStatusCode.OK, renamed.StatusCode);
        }

        await ChangeAsync(fixture, new { profile = "enterprise" });

        Assert.Contains("<header>Acme Apps</header><main>Ideas for work</main>", await PageAsync(fixture, "/en/build"));
        Assert.Contains("<header>Acme Apps</header><main>Shared front</main>", await PageAsync(fixture, "/en"));
    }

    [TestMethod]
    public async Task AProfileTheBuildRenderedNothingForIsLeftToTheBrowser()
    {
        await using var fixture = await CommunityAsync(Rendered(enterprise: false));

        await ChangeAsync(fixture, new { profile = "enterprise" });

        var build = await PageAsync(fixture, "/en/build");

        Assert.Contains("<div id=\"root\"></div>", build, "drawn by the browser rather than in the words of another profile");
        Assert.DoesNotContain("Ideas for a club", build);
    }

    [TestMethod]
    public async Task ABuildThatNamesNoProfilesIsTheSameForEveryProfile()
    {
        await using var fixture = await CommunityAsync(options => Write(options, new
        {
            pages = new Dictionary<string, string> { ["/en"] = "<main>Shared front</main>", ["/en/build"] = "<main>Ideas for a club</main>" },
            showcase = new Dictionary<string, Dictionary<string, string>>(),
            entry = new Dictionary<string, string>()
        }));

        await ChangeAsync(fixture, new { profile = "enterprise" });

        Assert.Contains("<main>Ideas for a club</main>", await PageAsync(fixture, "/en/build"), "as genhttp.dev's site is built: in the words every profile shares");
    }

    #endregion

    #region Helpers

    /// <summary>
    /// The community edition with its panel, and the index page and the
    /// table of pages of a site built for it.
    /// </summary>
    private static Task<LambdaFixture> CommunityAsync(Action<LambdaOptions>? bundles = null, string? directory = null,
                                                      Func<LambdaOptions, LambdaOptions>? configure = null)
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

    /// <summary>
    /// The pages as a site of the community edition is built: in the words
    /// every profile shares, as the platform and as a brand, and once more for
    /// each profile - home differing nowhere, an organization on its build page.
    /// </summary>
    private static Action<LambdaOptions> Rendered(bool enterprise) => options =>
    {
        var profiles = new Dictionary<string, object>
        {
            ["home"] = new
            {
                pages = new Dictionary<string, string>(),
                showcase = new Dictionary<string, Dictionary<string, string>>(),
                branded = new { pages = new Dictionary<string, string>(), showcase = new Dictionary<string, Dictionary<string, string>>() }
            }
        };

        if (enterprise)
        {
            profiles["enterprise"] = new
            {
                pages = new Dictionary<string, string> { ["/en/build"] = "<main>Ideas for work</main>" },
                showcase = new Dictionary<string, Dictionary<string, string>>(),
                branded = new
                {
                    pages = new Dictionary<string, string> { ["/en/build"] = "<header>__LAMBDA_BRAND_NAME__</header><main>Ideas for work</main>" },
                    showcase = new Dictionary<string, Dictionary<string, string>>()
                }
            };
        }

        Write(options, new
        {
            pages = new Dictionary<string, string> { ["/en"] = "<main>Shared front</main>", ["/en/build"] = "<main>Ideas for a club</main>" },
            showcase = new Dictionary<string, Dictionary<string, string>>(),
            entry = new Dictionary<string, string>(),
            branded = new
            {
                pages = new Dictionary<string, string>
                {
                    ["/en"] = "<header>__LAMBDA_BRAND_NAME__</header><main>Shared front</main>",
                    ["/en/build"] = "<header>__LAMBDA_BRAND_NAME__</header><main>Ideas for a club</main>"
                },
                showcase = new Dictionary<string, Dictionary<string, string>>()
            },
            profiles
        });
    };

    private static void Write(LambdaOptions options, object prerendered)
        => File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "site"), "prerender.json"), JsonSerializer.Serialize(prerendered));

    private static async Task<string> ProfileAsync(LambdaFixture fixture)
    {
        using var system = await fixture.GetAsync("/api/v1/system");

        return (await system.GetContentAsync<PlatformResponse>()).Profile;
    }

    private static async Task<SettingsResponse> SettingsAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/settings");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<SettingsResponse>(new JsonSerializerOptions(JsonSerializerDefaults.Web)))!;
    }

    private static async Task<SettingsResponse> ChangeAsync(LambdaFixture fixture, object change)
    {
        using var response = await fixture.SendAsOperatorAsync(HttpMethod.Put, "/api/v1/admin/settings", change);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return (await response.Content.ReadFromJsonAsync<SettingsResponse>(new JsonSerializerOptions(JsonSerializerDefaults.Web)))!;
    }

    private static async Task<string> PageAsync(LambdaFixture fixture, string path)
    {
        using var page = await fixture.GetAsync(path, accept: "text/html");

        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode, path);

        return await page.GetContentAsync();
    }

    private static string Headings(string guide) => string.Join('\n', guide.Split('\n').Where(l => l.StartsWith('#')));

    #endregion

}
