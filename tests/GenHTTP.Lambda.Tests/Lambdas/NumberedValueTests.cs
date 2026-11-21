using System.Data.Common;
using System.Net;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Tests.Infrastructure;

using EvolveDb.Migration;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// The columns that hold one of a few known values keep a small number each
/// rather than a name: an installation from before reads what it saved as it
/// did, and a number no member has - a later release's, one written by hand -
/// reads as the least it could mean.
/// </summary>
[TestClass]
public sealed class NumberedValueTests
{

    #region Tests

    [TestMethod]
    public async Task AnInstallationOnSqliteReadsWhatItSavedAsNamesAsBefore()
    {
        var directory = NewDirectory();

        var options = new LambdaOptions { DataDirectory = directory };

        await SeedAsync(Sqlite(options.DatabaseFile), "GenHTTP.Lambda.Data.Migrations", "29", NamedPlatform);
        await SeedAsync(Sqlite(options.AnalyticsFile), "GenHTTP.Lambda.Services.Analytics.Migrations", "2", NamedAnalytics);

        SqliteConnection.ClearAllPools();

        await using var fixture = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o with { DataDirectory = directory }));

        await AssertReadAsBeforeAsync(fixture);
    }

    [TestMethod]
    public async Task AnInstallationOnPostgreSqlReadsWhatItSavedAsNamesAsBefore()
    {
        await using var database = await TestDatabase.CreateAsync();

        var directory = NewDirectory();

        // the schemas as the release before left them, through the
        // integration, which makes them and opens each where it belongs
        await using (var provider = database.Integration.Build(new IntegrationContext(directory, NullLoggerFactory.Instance)))
        {
            await provider.PrepareAsync();

            await SeedAsync(() => provider.OpenAsync(DatabaseStore.Platform), "GenHTTP.Lambda.Data.PostgreSql", "4", NamedPlatform);
            await SeedAsync(() => provider.OpenAsync(DatabaseStore.Analytics), "GenHTTP.Lambda.Services.Analytics.PostgreSql", "1", NamedAnalytics);
        }

        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => LambdaFixture.WithPanel(o with { DataDirectory = directory }), database.Integration);

        await AssertReadAsBeforeAsync(fixture);
    }

    [TestMethod]
    public async Task ANumberNoMemberHasIsReadAsTheLeastItCouldMean()
    {
        var directory = NewDirectory();

        var options = new LambdaOptions { DataDirectory = directory };

        // rows written by hand, or by a later release, at the schema of this one
        await SeedAsync(Sqlite(options.DatabaseFile), "GenHTTP.Lambda.Data.Migrations", null, $"""
            INSERT INTO lambdas (id, public_key, private_key, tier, created, modified, editor_view, connectivity, analytics_since) VALUES
                (1001, 'later', 'key-later', 99, '{Day}', '{Day}', 99, 99, '{Day}');

            INSERT INTO deployments (lambda_id, version, created, origin) VALUES (1001, 1, '{Day}', 99);

            INSERT INTO activations (lambda_id, version, started, origin, ended, ended_by) VALUES (1001, 1, '{Day}', 99, '{Day}', 99);

            INSERT INTO features (lambda_id, key, name, branch, base_version, origin, created, modified) VALUES
                (1001, 'feature-later', 'later', 'later', 1, 99, '{Day}', '{Day}');

            INSERT INTO events (kind, lambda_id, public_key, occurred) VALUES (99, 1001, 'later', '{Day}'), (1, 1001, 'later', '{Day}');
            """);

        await SeedAsync(Sqlite(options.AnalyticsFile), "GenHTTP.Lambda.Services.Analytics.Migrations", null, $"""
            INSERT INTO lambdas (id, created) VALUES (1001, '{Day}');

            INSERT INTO days (lambda, year, month, day, hits, views) VALUES (1001, {Today.Year}, {Today.Month}, {Today.Day}, 4, 4);

            INSERT INTO devices (lambda, year, month, day, value, hits, views) VALUES (1001, {Today.Year}, {Today.Month}, {Today.Day}, 99, 3, 3), (1001, {Today.Year}, {Today.Month}, {Today.Day}, 0, 1, 1);
            INSERT INTO browsers (lambda, year, month, day, value, hits, views) VALUES (1001, {Today.Year}, {Today.Month}, {Today.Day}, 99, 2, 2);
            INSERT INTO systems (lambda, year, month, day, value, hits, views) VALUES (1001, {Today.Year}, {Today.Month}, {Today.Day}, 99, 2, 2);
            """);

        SqliteConnection.ClearAllPools();

        await using var fixture = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o with { DataDirectory = directory }));

        var lambda = await JsonAsync(fixture, "/api/v1/lambdas/key-later");

        Assert.AreEqual("Sandbox", lambda["tier"]!.GetValue<string>(), "a tier no member has is the one that allows the least");
        Assert.AreEqual("Full", lambda["view"]!.GetValue<string>());
        Assert.IsNull(lambda["connectivity"]!["status"], "a request nobody knows is nothing asked or decided");
        Assert.IsFalse(lambda["connectivity"]!["allowed"]!.GetValue<bool>(), "so the sandbox's default holds");

        var versions = (await JsonAsync(fixture, "/api/v1/lambdas/key-later/versions")).AsArray();
        Assert.IsNull(versions[0]!["origin"], "an origin nobody knows is not recorded");

        var history = (await JsonAsync(fixture, "/api/v1/lambdas/key-later/deployment/history")).AsArray();
        Assert.IsNull(history[0]!["origin"]);
        Assert.IsNull(history[0]!["endedBy"]);

        var features = (await JsonAsync(fixture, "/api/v1/lambdas/key-later/features")).AsArray();
        Assert.IsNull(features[0]!["origin"]);

        var listing = await JsonAsync(fixture, "/api/v1/admin/lambdas?search=later", asOperator: true);
        Assert.AreEqual("Sandbox", listing["lambdas"]![0]!["tier"]!.GetValue<string>());

        using (var overview = await fixture.GetAsOperatorAsync("/api/v1/admin/overview"))
        {
            Assert.AreEqual(HttpStatusCode.OK, overview.StatusCode, await overview.Content.ReadAsStringAsync());
        }

        var events = Events(await JsonAsync(fixture, "/api/v1/telemetry", asOperator: true));
        Assert.AreEqual(1, events["created"], "an event of no kind the platform counts is left out");
        Assert.AreEqual(5, events.Count);

        var analytics = await JsonAsync(fixture, "/api/v1/lambdas/key-later/analytics");
        Assert.AreEqual(4, Views(analytics, "devices", "other"), "a device nobody knows is counted as other, beside the other of the day");
        Assert.AreEqual(2, Views(analytics, "browsers", "other"));
        Assert.AreEqual(2, Views(analytics, "systems", "other"));
    }

    #endregion

    #region Expectations

    /// <summary>
    /// What the rows of <see cref="NamedPlatform"/> and
    /// <see cref="NamedAnalytics"/> read as, on either database.
    /// </summary>
    private static async Task AssertReadAsBeforeAsync(LambdaFixture fixture)
    {
        async Task<JsonObject> Lambda(string key) => (JsonObject)await JsonAsync(fixture, $"/api/v1/lambdas/{key}");

        var shop = await Lambda("key-shop");

        Assert.AreEqual("Production", shop["tier"]!.GetValue<string>());
        Assert.AreEqual("Simple", shop["view"]!.GetValue<string>());
        Assert.AreEqual("Allowed", shop["connectivity"]!["status"]!.GetValue<string>());

        Assert.AreEqual("Sandbox", (await Lambda("key-trying"))["tier"]!.GetValue<string>());
        Assert.AreEqual("Requested", (await Lambda("key-trying"))["connectivity"]!["status"]!.GetValue<string>());
        Assert.AreEqual("System", (await Lambda("key-core"))["tier"]!.GetValue<string>());
        Assert.AreEqual("Declined", (await Lambda("key-core"))["connectivity"]!["status"]!.GetValue<string>());
        Assert.AreEqual("Demo", (await Lambda("key-demo"))["tier"]!.GetValue<string>());
        Assert.AreEqual("Production", (await Lambda("key-old"))["tier"]!.GetValue<string>(), "what production was called before V25");

        var odd = await Lambda("key-odd");

        Assert.AreEqual("Sandbox", odd["tier"]!.GetValue<string>(), "a tier nobody knows is the one that allows the least");
        Assert.AreEqual("Full", odd["view"]!.GetValue<string>());
        Assert.IsNull(odd["connectivity"]!["status"]);

        var origins = (await JsonAsync(fixture, "/api/v1/lambdas/key-shop/versions")).AsArray()
                                                                                  .OrderBy(v => v!["version"]!.GetValue<int>())
                                                                                  .Select(v => v!["origin"]?.GetValue<string>())
                                                                                  .ToList();

        CollectionAssert.AreEqual(new[] { "template", "api", "agent", "git", "admin", "system", null, null }, origins);

        var history = (await JsonAsync(fixture, "/api/v1/lambdas/key-shop/deployment/history")).AsArray()
                                                                                          .OrderBy(a => a!["version"]!.GetValue<int>())
                                                                                          .Select(a => (a!["origin"]?.GetValue<string>(), a["endedBy"]?.GetValue<string>()))
                                                                                          .ToList();

        CollectionAssert.AreEqual(new (string?, string?)[] { ("api", "replaced"), ("agent", "stopped"), ("git", "expired"), ("admin", "admin"), ("system", null), (null, null) },
                                  history);

        var features = (await JsonAsync(fixture, "/api/v1/lambdas/key-shop/features")).AsArray()
                                                                                    .ToDictionary(f => f!["name"]!.GetValue<string>(), f => f!["origin"]?.GetValue<string>());

        Assert.AreEqual("agent", features["one"]);
        Assert.AreEqual("git", features["two"]);
        Assert.IsNull(features["three"]);
        Assert.IsNull(features["four"]);

        var events = Events(await JsonAsync(fixture, "/api/v1/telemetry", asOperator: true));

        Assert.AreEqual(1, events["created"]);
        Assert.AreEqual(2, events["saved"]);
        Assert.AreEqual(3, events["deployed"]);
        Assert.AreEqual(1, events["undeployed"]);
        Assert.AreEqual(1, events["deleted"], "and an event of a kind nobody knows is counted as none of them");

        var analytics = await JsonAsync(fixture, "/api/v1/lambdas/key-shop/analytics?bots=true");

        CollectionAssert.AreEqual(new[] { "desktop", "other", "mobile", "bot", "tablet" }, Values(analytics, "devices"),
                                  "by views, a tie by name, as before; a device nobody knows is merged into the other of its day");
        Assert.AreEqual(5, Views(analytics, "devices", "other"));

        CollectionAssert.AreEqual(new[] { "Chrome", "other", "Safari", "LINE", "Samsung Internet" }, Values(analytics, "browsers"));
        CollectionAssert.AreEqual(new[] { "Windows", "other", "ChromeOS", "iOS", "macOS" }, Values(analytics, "systems"));
    }

    #endregion

    #region Rows from before

    private static readonly DateTime Today = DateTime.UtcNow.Date;

    private static string Day => Today.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ssZ");

    /// <summary>
    /// Every name each column held, and one nobody knows.
    /// </summary>
    private static string NamedPlatform => $"""
        INSERT INTO lambdas (id, public_key, private_key, tier, created, modified, editor_view, connectivity, analytics_since) VALUES
            (1001, 'shop', 'key-shop', 'Production', '{Day}', '{Day}', 'Simple', 'Allowed', '{Day}'),
            (1002, 'trying', 'key-trying', 'Sandbox', '{Day}', '{Day}', 'Full', 'Requested', NULL),
            (1003, 'core', 'key-core', 'System', '{Day}', '{Day}', 'Full', 'Declined', NULL),
            (1004, 'demo', 'key-demo', 'Demo', '{Day}', '{Day}', 'Full', NULL, NULL),
            (1005, 'old', 'key-old', 'Premium', '{Day}', '{Day}', 'Full', NULL, NULL),
            (1006, 'odd', 'key-odd', 'Enterprise', '{Day}', '{Day}', 'Wide', 'Pondering', NULL);

        INSERT INTO deployments (lambda_id, version, created, origin) VALUES
            (1001, 1, '{Day}', 'template'), (1001, 2, '{Day}', 'api'), (1001, 3, '{Day}', 'agent'), (1001, 4, '{Day}', 'git'),
            (1001, 5, '{Day}', 'admin'), (1001, 6, '{Day}', 'system'), (1001, 7, '{Day}', 'wizard'), (1001, 8, '{Day}', NULL);

        INSERT INTO activations (lambda_id, version, started, origin, ended, ended_by) VALUES
            (1001, 1, '{Day}', 'api', '{Day}', 'replaced'), (1001, 2, '{Day}', 'agent', '{Day}', 'stopped'),
            (1001, 3, '{Day}', 'git', '{Day}', 'expired'), (1001, 4, '{Day}', 'admin', '{Day}', 'admin'),
            (1001, 5, '{Day}', 'system', '{Day}', 'vanished'), (1001, 6, '{Day}', 'wizard', NULL, NULL);

        INSERT INTO features (lambda_id, key, name, branch, base_version, origin, created, modified) VALUES
            (1001, 'feature-one', 'one', 'one', 6, 'agent', '{Day}', '{Day}'), (1001, 'feature-two', 'two', 'two', 6, 'git', '{Day}', '{Day}'),
            (1001, 'feature-three', 'three', 'three', 6, 'wizard', '{Day}', '{Day}'), (1001, 'feature-four', 'four', 'four', 6, NULL, '{Day}', '{Day}');

        INSERT INTO events (kind, lambda_id, public_key, occurred) VALUES
            ('created', 1001, 'shop', '{Day}'), ('saved', 1001, 'shop', '{Day}'), ('saved', 1001, 'shop', '{Day}'),
            ('deployed', 1001, 'shop', '{Day}'), ('deployed', 1001, 'shop', '{Day}'), ('deployed', 1001, 'shop', '{Day}'),
            ('undeployed', 1001, 'shop', '{Day}'), ('deleted', NULL, 'gone', '{Day}'), ('archived', 1001, 'shop', '{Day}');
        """;

    /// <summary>
    /// What was counted of the shop today, by every name, and one nobody
    /// knows in each of the numbered dimensions.
    /// </summary>
    private static string NamedAnalytics
    {
        get
        {
            string Rows(string table, params (string Value, int Views)[] rows)
                => $"INSERT INTO {table} (lambda, year, month, day, value, hits, views, bot_views) VALUES "
                 + string.Join(", ", rows.Select(r => $"(1001, {Today.Year}, {Today.Month}, {Today.Day}, '{r.Value}', {r.Views}, {r.Views}, {(r.Value == "bot" ? r.Views : 0)})"))
                 + ";\n";

            return $"INSERT INTO lambdas (id, created) VALUES (1001, '{Day}');\n"
                 + $"INSERT INTO days (lambda, year, month, day, hits, views) VALUES (1001, {Today.Year}, {Today.Month}, {Today.Day}, 16, 16);\n"
                 + Rows("devices", ("desktop", 5), ("mobile", 3), ("tablet", 1), ("bot", 2), ("other", 1), ("phablet", 4))
                 + Rows("browsers", ("Chrome", 3), ("Safari", 2), ("Samsung Internet", 1), ("LINE", 1), ("other", 1), ("Netscape", 2))
                 + Rows("systems", ("Windows", 3), ("macOS", 1), ("iOS", 1), ("ChromeOS", 1), ("other", 1), ("BeOS", 1));
        }
    }

    #endregion

    #region Helpers

    private static string NewDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        Directory.CreateDirectory(directory);

        return directory;
    }

    private static Func<ValueTask<DbConnection>> Sqlite(string file) => async () =>
    {
        var connection = new SqliteConnection($"Data Source={file};Pooling=false");

        await connection.OpenAsync();

        return connection;
    };

    /// <summary>
    /// Migrates a store as far as the given version - all the way without
    /// one - and writes the rows given into it.
    /// </summary>
    private static async Task SeedAsync(Func<ValueTask<DbConnection>> open, string location, string? version, string rows)
    {
        await using var connection = await open();

        var evolve = new EvolveDb.Evolve(connection, _ => { })
        {
            Locations = [],
            EmbeddedResourceAssemblies = [typeof(LambdaDbContext).Assembly],
            EmbeddedResourceFilters = [location],
            IsEraseDisabled = true,
            MetadataTableName = "schema_versions"
        };

        if (version != null)
        {
            evolve.TargetVersion = new MigrationVersion(version);
        }

        evolve.Migrate();

        await using var command = connection.CreateCommand();

        command.CommandText = rows;

        await command.ExecuteNonQueryAsync();
    }

    private static async Task<JsonNode> JsonAsync(LambdaFixture fixture, string path, bool asOperator = false)
    {
        using var response = asOperator ? await fixture.GetAsOperatorAsync(path) : await fixture.GetAsync(path);

        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"{path}: {body}");

        return JsonNode.Parse(body)!;
    }

    private static Dictionary<string, int> Events(JsonNode telemetry)
        => telemetry["events"]!["series"]!.AsArray().ToDictionary(s => s!["kind"]!.GetValue<string>(), s => s!["total"]!.GetValue<int>());

    private static string[] Values(JsonNode analytics, string dimension)
        => [.. analytics[dimension]!.AsArray().Select(v => v!["value"]!.GetValue<string>())];

    private static long Views(JsonNode analytics, string dimension, string value)
        => analytics[dimension]!.AsArray().Single(v => v!["value"]!.GetValue<string>() == value)!["views"]!.GetValue<long>();

    #endregion

}
