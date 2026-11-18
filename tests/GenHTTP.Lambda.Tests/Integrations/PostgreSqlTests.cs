using System.Net;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Analytics;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Integrations;

/// <summary>
/// The community edition with its data in PostgreSQL: the platform's records
/// and analytics in schemas of their own, every lambda's database a schema
/// reached under a role of its own - and lambdas written as everywhere else.
/// </summary>
/// <remarks>
/// Run where <c>LAMBDA_TEST_POSTGRES</c> names a server (see <see cref="TestDatabase"/>),
/// which CI starts beside the tests; inconclusive elsewhere.
/// </remarks>
[TestClass]
[TestCategory("PostgreSql")]
public sealed class PostgreSqlTests
{

    [TestMethod]
    public async Task ThePlatformKeepsItsRecordsAndItsAnalyticsInPostgreSql()
    {
        await using var database = await TestDatabase.CreateAsync();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel, database.Integration);

        var lambda = await fixture.CreateLambdaAsync("kept");

        Assert.AreEqual(1L, await database.QueryAsync("SELECT COUNT(*) FROM platform.lambdas WHERE public_key = 'kept'"));
        Assert.AreEqual(true, await database.QueryAsync("SELECT EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'analytics' AND tablename = 'days')"));

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(() => \"counted\");");

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/analytics"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode, await on.Content.ReadAsStringAsync());
        }

        using (var visit = await fixture.GetAsync("/lambda/kept/"))
        {
            Assert.AreEqual(HttpStatusCode.OK, visit.StatusCode);
        }

        fixture.Platform.Services.GetRequiredService<IAnalyticsService>().Flush();

        Assert.AreEqual(1L, await database.QueryAsync("SELECT CAST(SUM(hits) AS bigint) FROM analytics.days"));

        using var integrations = await fixture.GetAsOperatorAsync("/api/v1/admin/integrations");

        var listed = await integrations.GetContentAsync<List<IntegrationResponse>>();

        Assert.AreEqual("PostgreSQL", listed[0].Name);
        Assert.IsFalse(listed[0].Default, "chosen by the host");
        Assert.IsFalse(listed.SelectMany(i => i.Details).Any(d => d.Value.Contains("Password", StringComparison.OrdinalIgnoreCase)), "never what grants access");
    }

    [TestMethod]
    public async Task ALambdaKeepsItsRecordsInASchemaOfItsOwnWithTheSameCode()
    {
        await using var database = await TestDatabase.CreateAsync();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(database: database.Integration);

        var lambda = await fixture.CreateLambdaAsync("notes");

        await NeutralNotes.SaveAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual("1", await NeutralNotes.ServedAsync(fixture, "/lambda/notes/add?text=milk"));
        Assert.AreEqual("1", await NeutralNotes.ServedAsync(fixture, "/lambda/notes/add?text=bread"));

        Assert.AreEqual("milk,bread", await NeutralNotes.ServedAsync(fixture, "/lambda/notes/notes"));
        Assert.AreEqual("bread", await NeutralNotes.ServedAsync(fixture, "/lambda/notes/done"), "names folded to lower case, flags as booleans");

        var id = fixture.Meta.GetId(lambda.PrivateKey);

        Assert.AreEqual(2L, await database.QueryAsync($"SELECT COUNT(*) FROM lambda_{id}.notes"));

        using var tables = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/database");

        var described = await tables.GetContentAsync<DatabaseResponse>();

        Assert.IsTrue(described.Tables.Any(t => t is { Name: "notes", Rows: 2 }), "the owner reads it as on any other database");
        Assert.IsTrue(described.Tables.Any(t => t.Migrations), "the table Evolve keeps of what it applied");
        Assert.IsTrue(described.UsedBytes > 0);

        using var rows = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/database/tables/notes?order=created&descending=false");

        var page = (JsonObject)JsonNode.Parse(await rows.Content.ReadAsStringAsync())!;

        Assert.AreEqual("milk", page["rows"]![0]![1]!.GetValue<string>());
        Assert.IsTrue(page["rows"]![0]![3]!.GetValue<bool>(), "a boolean travels as one");
    }

    [TestMethod]
    public async Task ALambdaReachesNothingButItsOwnSchema()
    {
        await using var database = await TestDatabase.CreateAsync();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(database: database.Integration);

        var other = await fixture.CreateLambdaAsync("other");

        await NeutralNotes.SaveAsync(fixture, other.PrivateKey);

        await NeutralNotes.ServedAsync(fixture, "/lambda/other/add?text=private");

        var id = fixture.Meta.GetId(other.PrivateKey);

        var prying = await fixture.CreateLambdaAsync("prying");

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{prying.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        await fixture.DeployAsync(prying.PrivateKey, $$"""
            string Run(string sql)
            {
                try
                {
                    using var connection = Database.GetConnection();
                    using var command = connection.CreateCommand();

                    command.CommandText = sql;

                    return Convert.ToString(command.ExecuteScalar());
                }
                catch (DbException e)
                {
                    return "refused: " + e.Message;
                }
            }

            return Inline.Create()
                         .Get("platform", () => Run("SELECT private_key FROM platform.lambdas LIMIT 1"))
                         .Get("other", () => Run("SELECT text FROM lambda_{{id}}.notes LIMIT 1"))
                         .Get("role", () => Run("SET ROLE postgres; SELECT 1"))
                         .Get("schema", () => Run("CREATE SCHEMA mine"));
            """);

        foreach (var path in new[] { "platform", "other", "role", "schema" })
        {
            var answer = await NeutralNotes.ServedAsync(fixture, $"/lambda/prying/{path}");

            Assert.StartsWith("refused:", answer, path);
        }
    }

    [TestMethod]
    public async Task ADraftWorksOnACopyOfTheRecords()
    {
        await using var database = await TestDatabase.CreateAsync();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(database: database.Integration);

        var lambda = await fixture.CreateLambdaAsync("drafted");

        await NeutralNotes.SaveAsync(fixture, lambda.PrivateKey);

        await NeutralNotes.ServedAsync(fixture, "/lambda/drafted/add?text=milk");

        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("More notes"));

        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode, await created.Content.ReadAsStringAsync());

        var feature = await created.GetContentAsync<FeatureResponse>();

        using (var started = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/preview/start"))
        {
            Assert.AreEqual(HttpStatusCode.OK, started.StatusCode, await started.Content.ReadAsStringAsync());
        }

        await NeutralNotes.ServedAsync(fixture, $"/features/{feature.Key}/add?text=tried");

        Assert.AreEqual("milk,tried", await NeutralNotes.ServedAsync(fixture, $"/features/{feature.Key}/notes"));
        Assert.AreEqual("milk", await NeutralNotes.ServedAsync(fixture, "/lambda/drafted/notes"), "the lambda's own records are as they were");

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}"))
        {
            Assert.IsTrue(deleted.IsSuccessStatusCode, await deleted.Content.ReadAsStringAsync());
        }

        var id = fixture.Meta.GetId(lambda.PrivateKey);

        Assert.AreEqual(0L, await database.QueryAsync($"SELECT COUNT(*) FROM pg_namespace WHERE nspname LIKE 'lambda\\_{id}\\_feature\\_%'"), "the copy goes with its draft");
    }

    /// <summary>
    /// What PostgreSQL has and a lambda may use beyond plain SQL - a serial,
    /// an identity, a key between tables, a view - comes along into a draft's
    /// copy, its sequences where they were.
    /// </summary>
    [TestMethod]
    public async Task ADraftsCopyKeepsSequencesKeysAndViews()
    {
        await using var database = await TestDatabase.CreateAsync();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(database: database.Integration);

        var lambda = await fixture.CreateLambdaAsync("shapes");

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        const string code = """
            using (var connection = Database.GetConnection())
            {
                new Evolve(connection) { Locations = [Resources.Root + "migrations"], IsEraseDisabled = true }.Migrate();
            }

            string Run(string sql)
            {
                using var connection = Database.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = sql;

                return Convert.ToString(command.ExecuteScalar());
            }

            return Inline.Create()
                         .Get("add", () => Run("INSERT INTO groups (label) VALUES ('g') RETURNING id"))
                         .Get("item", (int group) => Run($"INSERT INTO items (grp) VALUES ({group}) RETURNING id"))
                         .Get("labels", () => Run("SELECT COUNT(*) FROM labels"));
            """;

        const string migration = """
            CREATE TABLE groups (id SERIAL PRIMARY KEY, label TEXT NOT NULL);
            CREATE TABLE items (id INTEGER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, grp INTEGER NOT NULL REFERENCES groups (id) ON DELETE CASCADE);
            CREATE VIEW labels AS SELECT label FROM groups;
            """;

        using (var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions?deploy=true", new VersionRequest(
               [
                   new LambdaFile(LambdaSource.EntryName, code),
                   new LambdaFile("resources/migrations/V1__Shapes.sql", migration)
               ])))
        {
            var version = await saved.GetContentAsync<SavedVersionResponse>();

            Assert.IsTrue(version.Deployment!.Success, string.Join(" | ", version.Deployment.Diagnostics.Select(d => d.Message)));
        }

        Assert.AreEqual("1", await NeutralNotes.ServedAsync(fixture, "/lambda/shapes/add"));
        Assert.AreEqual("1", await NeutralNotes.ServedAsync(fixture, "/lambda/shapes/item?group=1"));

        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Shapes"));

        var feature = await created.GetContentAsync<FeatureResponse>();

        using (var started = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/preview/start"))
        {
            Assert.AreEqual(HttpStatusCode.OK, started.StatusCode, await started.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("2", await NeutralNotes.ServedAsync(fixture, $"/features/{feature.Key}/add"), "the serial goes on where it was");
        Assert.AreEqual("2", await NeutralNotes.ServedAsync(fixture, $"/features/{feature.Key}/item?group=2"), "and the identity");
        Assert.AreEqual("2", await NeutralNotes.ServedAsync(fixture, $"/features/{feature.Key}/labels"), "the view reads the copy");

        using var orphan = await fixture.GetAsync($"/features/{feature.Key}/item?group=99");

        Assert.AreEqual(HttpStatusCode.InternalServerError, orphan.StatusCode, "the key between the tables came along");

        Assert.AreEqual("1", await NeutralNotes.ServedAsync(fixture, "/lambda/shapes/labels"));
    }

    [TestMethod]
    public async Task SwitchingTheDatabaseOffOrDeletingTheLambdaDropsItsSchemaAndRole()
    {
        await using var database = await TestDatabase.CreateAsync();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(database: database.Integration);

        var lambda = await fixture.CreateLambdaAsync("dropped");

        await NeutralNotes.SaveAsync(fixture, lambda.PrivateKey);

        var id = fixture.Meta.GetId(lambda.PrivateKey);

        Assert.AreEqual(1L, await database.QueryAsync($"SELECT COUNT(*) FROM pg_namespace WHERE nspname = 'lambda_{id}'"));

        using (var off = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, off.StatusCode, await off.Content.ReadAsStringAsync());
        }

        Assert.AreEqual(0L, await database.QueryAsync($"SELECT COUNT(*) FROM pg_namespace WHERE nspname = 'lambda_{id}'"));

        await NeutralNotes.SaveAsync(fixture, lambda.PrivateKey);

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}"))
        {
            Assert.IsTrue(deleted.IsSuccessStatusCode, await deleted.Content.ReadAsStringAsync());
        }

        Assert.AreEqual(0L, await database.QueryAsync($"SELECT COUNT(*) FROM pg_namespace WHERE nspname = 'lambda_{id}'"));
        Assert.AreEqual(0L, await database.QueryAsync($"SELECT COUNT(*) FROM pg_roles WHERE rolname = '{database.RolePrefix}_{id}'"));
    }

    /// <summary>
    /// The demos teach what an agent writes everywhere, so they run here as
    /// they are, and so does a copy of one.
    /// </summary>
    [TestMethod]
    public async Task EveryDemoRunsOnPostgreSql()
    {
        await using var database = await TestDatabase.CreateAsync();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(database: database.Integration);

        await fixture.SeedDemosAsync();

        foreach (var demo in DemoCatalog.All)
        {
            var lambda = fixture.Meta.Get(demo.Key);

            Assert.IsNotNull(lambda?.ActiveVersion, $"{demo.Id} went online");

            using var home = await fixture.GetAsync($"/lambda/{demo.Key}/");

            Assert.AreEqual(HttpStatusCode.OK, home.StatusCode, demo.Id);
        }

        using var listed = await fixture.GetAsync("/lambda/demo-crud/tasks/");

        Assert.AreEqual(HttpStatusCode.OK, listed.StatusCode, await listed.Content.ReadAsStringAsync());

        var copy = await fixture.CreateLambdaAsync("tasks", "demo-crud");

        await fixture.DeployAsync(copy.PrivateKey);

        using var tasks = await fixture.GetAsync("/lambda/tasks/tasks/?search=MILK");

        Assert.AreEqual(HttpStatusCode.OK, tasks.StatusCode, await tasks.Content.ReadAsStringAsync());
    }

}
