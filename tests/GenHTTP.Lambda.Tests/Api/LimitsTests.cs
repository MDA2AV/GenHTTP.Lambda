using System.Net;
using System.Text.Json;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Data.Sqlite;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// The limits of the tiers, which the operator sets in the panel while the
/// server runs rather than by restarting it with other variables.
/// </summary>
[TestClass]
public sealed class LimitsTests
{
    private const string Token = LambdaFixture.OperatorToken;

    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [TestMethod]
    public async Task TheLimitsAreTheConfiguredDefaultsUntilTheOperatorSavesThem()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o).WithSizes(build: 1234 * 1024) with { DeploymentLifetime = TimeSpan.FromHours(36) });

        var limits = await ReadAsync(fixture);

        Assert.AreEqual(1234 * 1024, limits.Sandbox.BuildBytes, "what the host said is the default");
        Assert.AreEqual(fixture.Options.Production.BuildBytes, limits.Production.BuildBytes);
        Assert.AreEqual(fixture.Options.Production.DataBytes, limits.Production.DataBytes);
        Assert.AreEqual(fixture.Options.System.DataBytes, limits.System.DataBytes);
        Assert.AreEqual("Sandbox", limits.DefaultTier);
        Assert.IsTrue(limits.Production.Connectivity, "production connects by default, as the premium tier did");
        Assert.IsFalse(limits.Sandbox.Connectivity);
        Assert.AreEqual(36, limits.OfflineAfterHours);
        Assert.AreEqual(fixture.Options.RateLimit, limits.RequestsPerSecond);
    }

    [TestMethod]
    public async Task AnOperatorLowersTheSandboxBuildAllowanceAndALargerSaveIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var sandbox = await fixture.CreateLambdaAsync();
        var production = await fixture.CreateLambdaAsync();

        fixture.ChangeTier(production.PrivateKey, LambdaTier.Production);

        using (var before = await SaveAsync(fixture, sandbox.PrivateKey, Coding(2000)))
        {
            Assert.AreEqual(HttpStatusCode.Created, before.StatusCode, "two thousand bytes fit before");
        }

        var current = await ReadAsync(fixture);

        await ChangeAsync(fixture, current with { Sandbox = current.Sandbox with { BuildBytes = 1024 } });

        using (var refused = await SaveAsync(fixture, sandbox.PrivateKey, Coding(2000)))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "and not once the operator lowered it, without a restart");
            Assert.Contains("1 KB", await refused.Content.ReadAsStringAsync());
        }

        using var unaffected = await SaveAsync(fixture, production.PrivateKey, Coding(2000));

        Assert.AreEqual(HttpStatusCode.Created, unaffected.StatusCode, "a lambda in production is held to its own tier's");
    }

    [TestMethod]
    public async Task TheFourAllowancesSavedBeforeAreTheTwoTheyBecame()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        await using (var first = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { DataDirectory = directory }))
        {
            // what a panel saved while code and assets, the workspace and the
            // database had an allowance each - under the names the migration to
            // the purpose tiers gave them; the sandbox database's left as it was
            await using var connection = new SqliteConnection($"Data Source={first.Options.DatabaseFile};Pooling=false");

            await connection.OpenAsync();

            await using var command = connection.CreateCommand();

            command.CommandText = "INSERT INTO settings (key, value) VALUES ('limits.sandbox.code-characters', '1000'), ('limits.sandbox.asset-bytes', '2048'), "
                                + "('limits.sandbox.workspace-bytes', '8192'), ('limits.production.asset-bytes', '4096')";

            await command.ExecuteNonQueryAsync();
        }

        SqliteConnection.ClearAllPools();

        await using var second = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { DataDirectory = directory });

        var limits = await ReadAsync(second);

        Assert.AreEqual(3048, limits.Sandbox.BuildBytes, "code and assets together, as a version may now come to");
        Assert.AreEqual(8192 + LambdaOptions.RetiredDefaults["database-bytes"].Sandbox, limits.Sandbox.DataBytes, "the workspace and the database's default together");
        Assert.AreEqual(4096 + LambdaOptions.RetiredDefaults["code-characters"].Production, limits.Production.BuildBytes, "and an allowance saved alone with the other's default");
        Assert.AreEqual(second.Options.Production.DataBytes, limits.Production.DataBytes, "what was never saved is the default");

        await ChangeAsync(second, limits);

        await using var check = new SqliteConnection($"Data Source={second.Options.DatabaseFile};Pooling=false");

        await check.OpenAsync();

        await using var query = check.CreateCommand();

        query.CommandText = "SELECT count(*) FROM settings WHERE key LIKE '%code-characters' OR key LIKE '%asset-bytes' OR key LIKE '%workspace-bytes' OR key LIKE '%database-bytes'";

        Assert.AreEqual(0L, (long)(await query.ExecuteScalarAsync())!, "saved once, the four are gone");
    }

    [TestMethod]
    public async Task TheLimitsSurviveARestart()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        await using (var first = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { DataDirectory = directory }))
        {
            var current = await ReadAsync(first);

            await ChangeAsync(first, current with { Sandbox = current.Sandbox with { BuildBytes = 4096, Label = "Free" }, BuildsPerDay = 3, Versions = 7 });
        }

        SqliteConnection.ClearAllPools();

        await using var second = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { DataDirectory = directory });

        var kept = await ReadAsync(second);

        Assert.AreEqual(4096, kept.Sandbox.BuildBytes);
        Assert.AreEqual("Free", kept.Sandbox.Label);
        Assert.AreEqual(3, kept.BuildsPerDay);
        Assert.AreEqual(7, kept.Versions);
    }

    [TestMethod]
    public async Task ASavedLimitWinsOverTheEnvironment()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        await using (var first = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { DataDirectory = directory }))
        {
            var current = await ReadAsync(first);

            await ChangeAsync(first, current with { Sandbox = current.Sandbox with { BuildBytes = 4096 } });
        }

        SqliteConnection.ClearAllPools();

        // what a host restarted with another default looks like
        await using var second = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o).WithSizes(build: 9000) with { DataDirectory = directory });

        Assert.AreEqual(4096, (await ReadAsync(second)).Sandbox.BuildBytes);
    }

    [TestMethod]
    public async Task ProductionMayNotAllowLessThanTheSandbox()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var current = await ReadAsync(fixture);

        using var refused = await SendAsync(fixture, HttpMethod.Put, current with { Production = current.Production with { BuildBytes = current.Sandbox.BuildBytes - 1 } });

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Contains("build bytes", await refused.Content.ReadAsStringAsync());

        Assert.AreEqual(current, await ReadAsync(fixture), "and nothing of it was saved");
    }

    [TestMethod]
    public async Task AnalyticsAreShownForAWeekOrAYearAndNeverLongerThanTheyAreKept()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var current = await ReadAsync(fixture);

        Assert.AreEqual(7, current.Sandbox.AnalyticsDays);
        Assert.AreEqual(365, current.Production.AnalyticsDays);
        Assert.AreEqual(365, current.System.AnalyticsDays);

        using var refused = await SendAsync(fixture, HttpMethod.Put, current with { Production = current.Production with { AnalyticsDays = 400 } });

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "a year is all that is kept");

        await ChangeAsync(fixture, current with { Sandbox = current.Sandbox with { AnalyticsDays = 30 } });

        Assert.AreEqual(30, (await ReadAsync(fixture)).Sandbox.AnalyticsDays);
    }

    [TestMethod]
    public async Task ALimitOfZeroIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var current = await ReadAsync(fixture);

        using var refused = await SendAsync(fixture, HttpMethod.Put, current with { RequestsPerSecond = 0 });

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);
    }

    [TestMethod]
    public async Task TheLimitsNeedTheToken()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var current = await ReadAsync(fixture);

        using var request = fixture.Host.GetRequest("/api/v1/admin/limits", HttpMethod.Put);

        request.Headers.Add("X-Admin-Token", "not-the-token");
        request.Content = System.Net.Http.Json.JsonContent.Create(current with { BuildsPerDay = 1 }, options: Web);

        using var refused = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode);
        Assert.AreEqual(current, await ReadAsync(fixture));
    }

    [TestMethod]
    public async Task TheSiteSaysWhatTheOperatorSet()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var current = await ReadAsync(fixture);

        await ChangeAsync(fixture, current with { Sandbox = current.Sandbox with { BuildBytes = 4321 * 1024 }, OfflineAfterHours = 48, RemovedAfterHours = 24 * 10 });

        using var response = await fixture.GetAsync("/api/v1/system");

        var platform = await response.GetContentAsync<PlatformResponse>();

        Assert.AreEqual(4321 * 1024, platform.BuildBytes);
        Assert.AreEqual(48, platform.DeploymentLifetimeHours);
        Assert.AreEqual(10, platform.RetentionDays);
    }

    [TestMethod]
    public async Task ALoweredDataLimitHoldsFromTheNextConnection()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync();

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create().Get(() =>
            {
                using var db = Database.GetConnection();
                using var command = db.CreateCommand();

                command.CommandText = "CREATE TABLE IF NOT EXISTS blobs (content BLOB); INSERT INTO blobs SELECT randomblob(100000) FROM (SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4)";

                try
                {
                    command.ExecuteNonQuery();
                    return "grew";
                }
                catch (SqliteException e)
                {
                    return e.Message;
                }
            });
            """);

        Assert.AreEqual("grew", await ServedAsync(fixture, $"http://{lambda.PublicKey}.localhost/"));

        var current = await ReadAsync(fixture);

        await ChangeAsync(fixture, current with { Sandbox = current.Sandbox with { DataBytes = 256 * 1024 } });

        StringAssert.Contains(await ServedAsync(fixture, $"http://{lambda.PublicKey}.localhost/"), "full", "the records it has are kept; it grows no further");
    }

    [TestMethod]
    public async Task NewLambdasStartInTheTierTheOperatorChose()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        Assert.AreEqual(nameof(LambdaTier.Sandbox), (await fixture.CreateLambdaAsync()).Tier, "the sandbox, by default");

        var current = await ReadAsync(fixture);

        await ChangeAsync(fixture, current with { DefaultTier = "Production" });

        var created = await fixture.CreateLambdaAsync();

        Assert.AreEqual(nameof(LambdaTier.Production), created.Tier);
        Assert.IsNull(created.KeptUntil, "and is kept, as production is");

        using var refused = await SendAsync(fixture, HttpMethod.Put, current with { DefaultTier = "System" });

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, "a system lambda is the installation's own, which nobody starts by creating one");
    }

    [TestMethod]
    public async Task ALabelIsWhatThePagesCallATierAndTheApiStillNamesItByPurpose()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync();

        Assert.AreEqual("Free", lambda.TierPolicy.Label, "what genhttp.dev calls the sandbox");
        Assert.AreEqual(nameof(LambdaTier.Production), lambda.TierPolicy.Above);

        var current = await ReadAsync(fixture);

        using (var saved = await SendAsync(fixture, HttpMethod.Put, current with { Sandbox = current.Sandbox with { Label = "  Trying " }, Production = current.Production with { Label = "Business" } }))
        {
            Assert.AreEqual(HttpStatusCode.OK, saved.StatusCode, await saved.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("Trying", (await ReadAsync(fixture)).Sandbox.Label, "trimmed");

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}");

        var labelled = await read.GetContentAsync<LambdaResponse>();

        Assert.AreEqual(nameof(LambdaTier.Sandbox), labelled.Tier, "the identifier, never the label");
        Assert.AreEqual("Trying", labelled.TierPolicy.Label);
        Assert.AreEqual("Business", labelled.TierPolicy.AboveLabel);

        using (var cleared = await SendAsync(fixture, HttpMethod.Put, (await ReadAsync(fixture)) with { Sandbox = current.Sandbox with { Label = "" } }))
        {
            Assert.AreEqual(HttpStatusCode.OK, cleared.StatusCode);
        }

        Assert.IsNull((await ReadAsync(fixture)).Sandbox.Label, "an empty label is the tier's own name again");

        using var tooLong = await SendAsync(fixture, HttpMethod.Put, current with { Sandbox = current.Sandbox with { Label = new string('x', 25) } });

        Assert.AreEqual(HttpStatusCode.BadRequest, tooLong.StatusCode);
    }

    [TestMethod]
    public async Task VersionsAndFeaturesAreTheSameForEveryTier()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var current = await ReadAsync(fixture);

        Assert.AreEqual(fixture.Options.MaxVersions, current.Versions);
        Assert.AreEqual(fixture.Options.MaxFeatures, current.Features);

        await ChangeAsync(fixture, current with { Features = 1 });

        var production = await fixture.CreateLambdaAsync();

        fixture.ChangeTier(production.PrivateKey, LambdaTier.Production);

        await fixture.DeployAsync(production.PrivateKey, "return Content.From(Resource.FromString(\"x\"));");

        using (var first = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{production.PrivateKey}/features", new CreateFeatureRequest("one")))
        {
            Assert.AreEqual(HttpStatusCode.Created, first.StatusCode, await first.Content.ReadAsStringAsync());
        }

        using var second = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{production.PrivateKey}/features", new CreateFeatureRequest("two"));

        Assert.AreEqual(HttpStatusCode.Conflict, second.StatusCode, "production has no more of them than the sandbox");
    }

    #region Helpers

    private static async Task<LimitsModel> ReadAsync(LambdaFixture fixture)
    {
        using var response = await SendAsync(fixture, HttpMethod.Get);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<LimitsModel>();
    }

    private static async Task ChangeAsync(LambdaFixture fixture, LimitsModel limits)
    {
        using var response = await SendAsync(fixture, HttpMethod.Put, limits);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        Assert.AreEqual(limits, await response.GetContentAsync<LimitsModel>());
    }

    private static async Task<HttpResponseMessage> SendAsync(LambdaFixture fixture, HttpMethod method, LimitsModel? limits = null)
    {
        using var request = fixture.Host.GetRequest("/api/v1/admin/limits", method);

        request.Headers.Add("X-Admin-Token", Token);

        if (limits != null)
        {
            request.Content = System.Net.Http.Json.JsonContent.Create(limits, options: Web);
        }

        return await fixture.Host.GetResponseAsync(request);
    }

    private static async Task<string> ServedAsync(LambdaFixture fixture, string path)
    {
        using var served = await fixture.GetAsync(path);

        var content = await served.GetContentAsync();

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode, content);

        return content;
    }

    private static LambdaFile[] Coding(int bytes)
    {
        const string code = "return Content.From(Resource.FromString(\"x\"));\n// ";

        return [new(LambdaSource.EntryName, code + new string('a', bytes - code.Length))];
    }

    private static Task<HttpResponseMessage> SaveAsync(LambdaFixture fixture, string privateKey, LambdaFile[] files)
        => fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/versions", new VersionRequest(files));

    #endregion

}
