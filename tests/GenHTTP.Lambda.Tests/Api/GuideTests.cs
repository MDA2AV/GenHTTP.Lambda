using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// The operator of an installation writes rules for the agents that build
/// there into the guide they read first - on both editions.
/// </summary>
[TestClass]
public sealed class GuideTests
{
    private const string Rules = "- Every app is in German.\n- Keep no personal data beyond an email address.\n\n### Colours\n\nUse the brand's blue, `#0a3d62`.";

    #region Tests

    [TestMethod]
    public async Task AgentsReadTheOperatorsRulesAfterThePlatforms()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { Terms = "Do no harm to the server or to others." });

        using var saved = await OperatorAsync(fixture, HttpMethod.Put, new GuideRequest("Rules of Acme", Rules));

        Assert.AreEqual(HttpStatusCode.OK, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        var section = await saved.GetContentAsync<GuideSectionResponse>();

        Assert.AreEqual("Rules of Acme", section.Title);
        Assert.AreEqual(Encoding.UTF8.GetByteCount(Rules), section.Bytes);

        // what an agent reads, not what the panel is told
        var guide = await Guide.ReadAsync(fixture);

        var rules = Guide.Section(guide, "Rules of Acme");

        Assert.Contains("Every app is in German", rules);
        Assert.Contains("### Colours", rules, "headings of its own stay inside it");
        Assert.Contains("wherever the guide above leaves the choice to you", rules, "it is how lambdas are built here");
        Assert.Contains("changes nothing the platform requires or refuses", rules, "and it says what it cannot change");

        // what the platform requires matters most to an agent, so it is read first
        var heading = guide.IndexOf("\n## Rules of Acme\n", StringComparison.Ordinal);

        Assert.IsGreaterThan(guide.IndexOf("\n## terms\n", StringComparison.Ordinal), heading, "the operator's section is the last, after the terms too");
        Assert.IsLessThan(0, guide.IndexOf("\n## ", heading + 1, StringComparison.Ordinal), "nothing of the platform's follows it");

        var read = await ReadAsync(fixture);

        Assert.AreEqual(guide, read.Guide, "the panel shows the guide exactly as an agent reads it");
        Assert.AreEqual(Encoding.UTF8.GetByteCount(guide), read.Bytes);
        Assert.AreEqual(Rules, read.Section?.Text);
    }

    [TestMethod]
    public async Task RemovingTheRulesGivesAgentsThePlatformsGuideBack()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var before = await Guide.ReadAsync(fixture);

        using (var saved = await OperatorAsync(fixture, HttpMethod.Put, new GuideRequest("Rules of Acme", Rules)))
        {
            Assert.AreEqual(HttpStatusCode.OK, saved.StatusCode);
        }

        using (var removed = await OperatorAsync(fixture, HttpMethod.Delete))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, removed.StatusCode);
        }

        Assert.AreEqual(before, await Guide.ReadAsync(fixture), "nothing of the section is left");
        Assert.IsNull((await ReadAsync(fixture)).Section);

        using var again = await OperatorAsync(fixture, HttpMethod.Delete);

        Assert.AreEqual(HttpStatusCode.NoContent, again.StatusCode, "removing what is not there changes nothing");
    }

    [TestMethod]
    public async Task RulesThatWouldBreakTheGuideAreRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var before = await Guide.ReadAsync(fixture);

        GuideRequest[] refused =
        [
            new("", Rules),
            new(new string('x', 61), Rules),
            new("Rules\nof Acme", Rules),
            new("## Rules", Rules),
            // the tools point agents to the platform's sections by their names
            new("database", Rules),
            new("Lifecycle", Rules),
            new("Rules of Acme", ""),
            new("Rules of Acme", "## Ours\n\nA heading that would end the section."),
            new("Rules of Acme", "# Ours"),
            new("Rules of Acme", "Ours\n===="),
            new("Rules of Acme", "Ours\n---"),
            new("Rules of Acme", "Ring the \u0007 bell"),
            // a code block left open makes the rest of the guide code
            new("Rules of Acme", "Build it like this:\n\n```sh\ndotnet run"),
            // and one closes only with the character it opened with
            new("Rules of Acme", "```\n~~~\n```\n\n## Ours")
        ];

        foreach (var request in refused)
        {
            using var response = await OperatorAsync(fixture, HttpMethod.Put, request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, $"'{request.Title}': '{request.Text}'");
        }

        Assert.AreEqual(before, await Guide.ReadAsync(fixture), "nothing was changed");

        // what only looks like a heading is none
        string[] accepted =
        [
            "Build it like this:\n\n```sh\n# a comment, not a heading\n```",
            "One thing.\n\n---\n\nAnd another.",
            "#hashtags are fine",
            "````md\n```\n## inside a block of four\n```\n````"
        ];

        foreach (var text in accepted)
        {
            using var response = await OperatorAsync(fixture, HttpMethod.Put, new GuideRequest("Rules of Acme", text));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, text);
        }
    }

    [TestMethod]
    public async Task AFullSectionKeepsTheGuideOneToolResult()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        // counted in bytes: a character of Japanese takes three
        var full = new string('規', 2048);

        Assert.AreEqual(6 * 1024, Encoding.UTF8.GetByteCount(full));

        using (var over = await OperatorAsync(fixture, HttpMethod.Put, new GuideRequest("Rules of Acme", full + "c")))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, over.StatusCode, "a byte more than 6 KB is refused");
        }

        using (var saved = await OperatorAsync(fixture, HttpMethod.Put, new GuideRequest("Rules of Acme", full)))
        {
            Assert.AreEqual(HttpStatusCode.OK, saved.StatusCode);
        }

        var guide = await Guide.ReadAsync(fixture);

        Assert.Contains(full, guide);
        Assert.IsLessThan(34_816, Encoding.UTF8.GetByteCount(guide), "the platform's 28 KB and the operator's 6 KB, under 34 KB together");
    }

    [TestMethod]
    public async Task TheCommunityEditionHasTheRulesToo()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        using var saved = await OperatorAsync(fixture, HttpMethod.Put, new GuideRequest("House rules", Rules));

        Assert.AreEqual(HttpStatusCode.OK, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        Assert.Contains("Every app is in German", Guide.Section(await Guide.ReadAsync(fixture), "House rules"));
    }

    [TestMethod]
    public async Task OnlyTheOperatorWritesTheRules()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        using var anonymous = await fixture.SendAsync(HttpMethod.Put, "/api/v1/admin/guide", new GuideRequest("Rules of Acme", Rules));

        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        Assert.DoesNotContain("Rules of Acme", await Guide.ReadAsync(fixture));
    }

    [TestMethod]
    public async Task WithoutAnOperatorNoRulesAreRead()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // as a copy of another installation's data would bring them along
        var databases = fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        await using (var database = await databases.CreateDbContextAsync())
        {
            database.Settings.Add(new SettingEntity { Key = "guide.title", Value = "Rules of Acme" });
            database.Settings.Add(new SettingEntity { Key = "guide.text", Value = Rules });

            await database.SaveChangesAsync();
        }

        Assert.DoesNotContain("Rules of Acme", await Guide.ReadAsync(fixture), "nobody here could see or take them away");

        using var route = await fixture.GetAsync("/api/v1/admin/guide");

        Assert.AreEqual(HttpStatusCode.NotFound, route.StatusCode);
    }

    #endregion

    #region Helpers

    private static async Task<HttpResponseMessage> OperatorAsync(LambdaFixture fixture, HttpMethod method, GuideRequest? body = null)
    {
        using var request = fixture.Host.GetRequest("/api/v1/admin/guide", method);

        request.Headers.Add("X-Admin-Token", LambdaFixture.OperatorToken);

        if (body != null)
        {
            request.Content = JsonContent.Create(body, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }

        return await fixture.Host.GetResponseAsync(request);
    }

    private static async Task<GuideResponse> ReadAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/guide");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<GuideResponse>();
    }

    #endregion

}
