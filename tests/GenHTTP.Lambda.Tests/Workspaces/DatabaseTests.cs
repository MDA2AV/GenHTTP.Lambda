using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Data.Sqlite;

namespace GenHTTP.Lambda.Tests.Workspaces;

/// <summary>
/// The database of a lambda: a SQLite file of its own the code reaches with
/// <c>Database.GetConnection()</c>, migrates with Evolve, and nothing else
/// can open.
/// </summary>
[TestClass]
public sealed class DatabaseTests
{

    /// <summary>
    /// A list of notes, kept in a table its migration makes - the way an agent
    /// is steered to write it.
    /// </summary>
    private const string Notes = """
        using (var connection = Database.GetConnection())
        {
            new Evolve(connection) { Locations = [Resources.Root + "migrations"], IsEraseDisabled = true }.Migrate();
        }

        return Inline.Create()
                     .Get("notes", () =>
                     {
                         using var db = Database.GetConnection();
                         using var command = db.CreateCommand();

                         command.CommandText = "SELECT text FROM notes ORDER BY id";

                         using var reader = command.ExecuteReader();

                         var notes = new List<string>();

                         while (reader.Read())
                         {
                             notes.Add(reader.GetString(0));
                         }

                         return string.Join(",", notes);
                     })
                     .Get("add", (string text) =>
                     {
                         using var db = Database.GetConnection();
                         using var command = db.CreateCommand();

                         command.CommandText = "INSERT INTO notes (text) VALUES ($text)";
                         command.Parameters.AddWithValue("$text", text);
                         command.ExecuteNonQuery();

                         return "added";
                     });
        """;

    private const string Migration = "CREATE TABLE notes (id INTEGER PRIMARY KEY, text TEXT NOT NULL);";

    /// <summary>
    /// The same notes, read and written through a context of Entity Framework
    /// Core that maps the table the migration makes.
    /// </summary>
    private const string ContextNotes = """
        using (var connection = Database.GetConnection())
        {
            new Evolve(connection) { Locations = [Resources.Root + "migrations"], IsEraseDisabled = true }.Migrate();
        }

        return Inline.Create()
                     .Get("notes", () =>
                     {
                         using var db = new Notes(Database.GetConnection());

                         return string.Join(",", db.Entries.AsNoTracking().OrderBy(n => n.Id).Select(n => n.Text).ToList());
                     })
                     .Get("add", (string text) =>
                     {
                         using var db = new Notes(Database.GetConnection());

                         db.Entries.Add(new Note { Text = text });

                         return db.SaveChanges().ToString();
                     });

        public class Note
        {
            public long Id { get; set; }

            public string Text { get; set; }
        }

        public class Notes(SqliteConnection connection) : DbContext
        {
            public DbSet<Note> Entries => Set<Note>();

            protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(connection, contextOwnsConnection: true);

            protected override void OnModelCreating(ModelBuilder model) => model.Entity<Note>().ToTable("notes");
        }
        """;

    [TestMethod]
    public async Task TheDatabaseIsOffUntilSomebodySwitchesItOn()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var store = await StoreAsync(fixture, lambda.PrivateKey);

        Assert.IsFalse(store.Enabled);
        Assert.IsFalse(store.Default);
        Assert.AreEqual(fixture.Options.DataBytes, store.QuotaBytes);

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(() => { using var connection = Database.GetConnection(); return \"connected\"; });");

        using var refused = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual(HttpStatusCode.InternalServerError, refused.StatusCode);

        Assert.IsFalse(File.Exists(FileOf(fixture, lambda.PrivateKey)), "nothing is made while it is off");
    }

    [TestMethod]
    public async Task SwitchingItOnMakesAnEmptyDatabase()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        var file = FileOf(fixture, lambda.PrivateKey);

        Assert.IsTrue(File.Exists(file), "made when it is switched on, before anything uses it");

        var store = await StoreAsync(fixture, lambda.PrivateKey);

        Assert.IsTrue(store.Enabled);
        Assert.AreEqual(0, store.Items, "no tables yet");
        Assert.IsGreaterThan(0L, store.UsedBytes);

        var header = new byte[16];

        await using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            await stream.ReadExactlyAsync(header);
        }

        Assert.AreEqual("SQLite format 3", Encoding.ASCII.GetString(header, 0, 15), "an ordinary SQLite file, which any tool opens");
    }

    [TestMethod]
    public async Task ALambdaMigratesItsDatabaseAndKeepsRecordsAcrossVersions()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await SaveAsync(fixture, lambda.PrivateKey, Notes, ("resources/migrations/V1__Notes.sql", Migration));

        Assert.AreEqual("added", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/add?text=milk"));
        Assert.AreEqual("added", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/add?text=eggs"));

        // a second version adds a column with a migration of its own; the rows
        // written before are still there, and rolling back reads them too
        await SaveAsync(fixture, lambda.PrivateKey, Notes.Replace("SELECT text", "SELECT text || ':' || done"),
                        ("resources/migrations/V1__Notes.sql", Migration), ("resources/migrations/V2__Done.sql", "ALTER TABLE notes ADD COLUMN done INTEGER NOT NULL DEFAULT 0;"));

        Assert.AreEqual("milk:0,eggs:0", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/notes"));

        using (var back = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/start", new DeploymentRequest(2)))
        {
            Assert.AreEqual(HttpStatusCode.OK, back.StatusCode, await back.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("milk,eggs", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/notes"), "an older version reads what a newer one left");

        var store = await StoreAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(2, store.Items, "the notes, and the history Evolve keeps");
    }

    [TestMethod]
    public async Task SwitchingItOffDeletesTheDatabase()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await SaveAsync(fixture, lambda.PrivateKey, Notes, ("resources/migrations/V1__Notes.sql", Migration));

        await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/add?text=milk");

        using (var off = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, off.StatusCode);
        }

        Assert.IsFalse(File.Exists(FileOf(fixture, lambda.PrivateKey)));

        using (var refused = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/notes"))
        {
            Assert.AreEqual(HttpStatusCode.InternalServerError, refused.StatusCode);
        }

        // and on again, it starts empty: the lambda starts anew, and migrates it
        await EnableAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(string.Empty, await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/notes"));
    }

    [TestMethod]
    public async Task ALambdaCannotOpenAConnectionOfItsOwn()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        string[] attempts =
        [
            "var c = new SqliteConnection(\"Data Source=other.db\");\nreturn Inline.Create();",
            "SqliteConnection c = new(\"Data Source=other.db\");\nreturn Inline.Create();",
            "var c = new Microsoft.Data.Sqlite.SqliteConnection();\nreturn Inline.Create();",
            "var c = new Evil();\nreturn Inline.Create();\n\nclass Evil : SqliteConnection { public Evil() : base(\"Data Source=other.db\") { } }",
            "var c = Database.GetConnection();\nc.Close();\nc.ConnectionString = \"Data Source=other.db\";\nreturn Inline.Create();",
            "var c = Database.GetConnection();\nc.LoadExtension(\"evil\");\nreturn Inline.Create();",
            "var f = System.Data.Common.DbProviderFactories.GetFactory(Database.GetConnection());\nreturn Inline.Create();",
            "var e = System.Linq.Expressions.Expression.Constant(1);\nreturn Inline.Create();",
            // Entity Framework, on anything but the connection it is handed
            "var o = new DbContextOptionsBuilder().UseSqlite(\"Data Source=other.db\");\nreturn Inline.Create();",
            "var o = new DbContextOptionsBuilder().UseSqlite();\nreturn Inline.Create();",
            "using var db = new DbContext(new DbContextOptionsBuilder().UseSqlite(Database.GetConnection(), true).Options);\ndb.Database.SetConnectionString(\"Data Source=other.db\");\nreturn Inline.Create();",
            "return Inline.Create();\n\nclass Records : DbContext { protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlite(\"Data Source=other.db\"); }",
            // and its internals, where a context's services and options are
            "using Microsoft.EntityFrameworkCore.Infrastructure;\nreturn Inline.Create();",
            "using Internals = Microsoft.EntityFrameworkCore.Infrastructure;\nreturn Inline.Create();",
            "using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;\nreturn Inline.Create();",
            "var s = Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions.GetService<object>(null);\nreturn Inline.Create();",
            "global::Microsoft.EntityFrameworkCore.Storage.IRelationalConnection c = null;\nreturn Inline.Create();",
            // and what binds at runtime, around all of the above
            "dynamic o = new DbContextOptionsBuilder();\nreturn Inline.Create();"
        ];

        foreach (var attempt in attempts)
        {
            using var check = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/code/check", LambdaFixture.Code(attempt));

            var outcome = await check.GetContentAsync<CompilationResponse>();

            Assert.IsFalse(outcome.Success, attempt);
        }
    }

    [TestMethod]
    public async Task ALambdaKeepsItsRecordsWithEntityFramework()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("notes-in-context");

        await EnableAsync(fixture, lambda.PrivateKey);

        await SaveAsync(fixture, lambda.PrivateKey, ContextNotes, ("resources/migrations/V1__Notes.sql", Migration));

        Assert.AreEqual("1", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/add?text=milk"));
        Assert.AreEqual("1", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/add?text=eggs"));

        Assert.AreEqual("milk,eggs", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/notes"));

        var store = await StoreAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(2, store.Items, "the table the migration made, and Evolve's history - nothing Entity Framework made of its own");

        // it leaves with its code, and the project references what it uses
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export");

        using var archive = new ZipArchive(await response.Content.ReadAsStreamAsync());

        string Read(string name)
        {
            using var reader = new StreamReader(archive.GetEntry($"notes-in-context/{name}")!.Open());

            return reader.ReadToEnd();
        }

        Assert.Contains("<PackageReference Include=\"Microsoft.EntityFrameworkCore.Sqlite\"", Read("notes-in-context.csproj"));
        Assert.Contains("global using Microsoft.EntityFrameworkCore;", Read("Platform/Usings.cs"));
    }

    [TestMethod]
    public async Task WhatDescribingAModelTakesIsThere()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync("""
            using Microsoft.EntityFrameworkCore.ChangeTracking;
            using Microsoft.EntityFrameworkCore.Metadata.Builders;
            using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

            return Inline.Create().Get(() =>
            {
                using var db = new Records(Database.GetConnection());

                EntityEntry<Note> entry = db.Entry(new Note());

                return Microsoft.EntityFrameworkCore.EF.IsDesignTime ? "design" : entry.State.ToString();
            });

            public class Note
            {
                public long Id { get; set; }

                public DateTime Written { get; set; }
            }

            public class NoteMapping : IEntityTypeConfiguration<Note>
            {
                public void Configure(EntityTypeBuilder<Note> note) => note.ToTable("notes").Property(n => n.Written).HasConversion(new DateTimeToStringConverter());
            }

            public class Records(SqliteConnection connection) : DbContext
            {
                protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(connection, contextOwnsConnection: true);

                protected override void OnModelCreating(ModelBuilder model) => model.ApplyConfiguration(new NoteMapping());
            }
            """);

        Assert.IsTrue(outcome.Success, string.Join("; ", outcome.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    public async Task EntityFrameworkLeavesTheSchemaToEvolve()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        string[] attempts = ["EnsureCreated()", "EnsureDeleted()", "Migrate()"];

        foreach (var attempt in attempts)
        {
            var outcome = await fixture.Deployments.ValidateAsync($$"""
                using (var db = new DbContext(new DbContextOptionsBuilder().UseSqlite(Database.GetConnection(), true).Options))
                {
                    db.Database.{{attempt}};
                }

                return Inline.Create();
                """);

            Assert.IsFalse(outcome.Success, attempt);
            Assert.IsTrue(outcome.Diagnostics.Any(d => d.Message.Contains("Evolve", StringComparison.Ordinal)), $"{attempt}: refused, pointing at Evolve");
        }
    }

    [TestMethod]
    public async Task InsideAContextTheLambdasDatabaseIsNamedForWhatItIs()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // inside a DbContext, Database is the context's own - the message says so
        var outcome = await fixture.Deployments.ValidateAsync("""
            return Inline.Create();

            class Records : DbContext
            {
                protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(Database.GetConnection(), true);
            }
            """);

        Assert.IsFalse(outcome.Success);
        Assert.IsTrue(outcome.Diagnostics.Any(d => d.Message.Contains("Inside a DbContext", StringComparison.Ordinal)), string.Join("; ", outcome.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    public async Task SqlCannotReachBeyondTheDatabase()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        var target = Path.Combine(fixture.Options.DataDirectory, "escaped.db");

        await fixture.DeployAsync(lambda.PrivateKey, $$"""
            string Run(string sql)
            {
                try
                {
                    using var db = Database.GetConnection();
                    using var command = db.CreateCommand();

                    command.CommandText = sql;
                    command.ExecuteNonQuery();

                    return "ran";
                }
                catch (SqliteException e)
                {
                    return "refused: " + e.Message;
                }
            }

            // verbatim, so the backslashes of a path on Windows stay backslashes
            return Inline.Create()
                         .Get("attach", () => Run(@"ATTACH DATABASE '{{fixture.Options.DatabaseFile}}' AS platform"))
                         .Get("into", () => Run(@"VACUUM INTO '{{target}}'"))
                         .Get("elsewhere", () => Run(@"PRAGMA temp_store_directory = '{{fixture.Options.DataDirectory}}'"))
                         .Get("grow", () => Run("PRAGMA max_page_count = 999999999"))
                         .Get("vacuum", () => Run("VACUUM"));
            """);

        foreach (var refused in new[] { "attach", "into", "elsewhere", "grow" })
        {
            StringAssert.StartsWith(await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/{refused}"), "refused", refused);
        }

        Assert.AreEqual("ran", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/vacuum"), "what stays inside the file is fine");

        Assert.IsFalse(File.Exists(target));
    }

    [TestMethod]
    public async Task ADatabaseStopsGrowingAtItsQuota()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { DataBytes = 256 * 1024 });

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

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

        StringAssert.Contains(await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/"), "full");
    }

    [TestMethod]
    public async Task AFeatureWorksOnACopyOfTheDatabase()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await SaveAsync(fixture, lambda.PrivateKey, Notes, ("resources/migrations/V1__Notes.sql", Migration));

        await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/add?text=live");

        FeatureResponse feature;

        using (var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Notes")))
        {
            Assert.AreEqual(HttpStatusCode.Created, created.StatusCode, await created.Content.ReadAsStringAsync());

            feature = await created.GetContentAsync<FeatureResponse>();
        }

        using (var started = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/preview/start"))
        {
            Assert.AreEqual(HttpStatusCode.OK, started.StatusCode, await started.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("live", await ServedAsync(fixture, $"{feature.PreviewPath}notes"), "a copy of what the lambda had");

        await ServedAsync(fixture, $"{feature.PreviewPath}add?text=tried");

        Assert.AreEqual("live,tried", await ServedAsync(fixture, $"{feature.PreviewPath}notes"));
        Assert.AreEqual("live", await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/notes"), "the lambda's own is not touched");

        // the owner reads the copy as the preview left it
        using (var copy = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/database/tables/notes"))
        {
            var rows = await copy.GetContentAsync<DatabaseRowsResponse>();

            Assert.AreEqual(2, rows.Total);
        }

        using (var refreshed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/data/refresh"))
        {
            Assert.AreEqual(HttpStatusCode.OK, refreshed.StatusCode);
        }

        Assert.AreEqual("live", await ServedAsync(fixture, $"{feature.PreviewPath}notes"), "a fresh copy, and the preview started again with it");

        var id = fixture.Meta.GetId(lambda.PrivateKey);

        var copyFile = Path.Combine(fixture.Options.FeatureDirectory, id.ToString()!, "database.db");

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode);
        }

        Assert.IsFalse(Directory.EnumerateFiles(Path.GetDirectoryName(Path.GetDirectoryName(copyFile)!)!, "database.db*", SearchOption.AllDirectories)
                                .Any(f => !f.StartsWith(fixture.Options.DatabaseDirectory, StringComparison.Ordinal)),
                       "the copy goes with the feature");
    }

    [TestMethod]
    public async Task TheOwnerReadsTheTablesAndTheirRows()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await SaveAsync(fixture, lambda.PrivateKey, Notes, ("resources/migrations/V1__Notes.sql", Migration),
                        ("resources/migrations/V2__More.sql", "CREATE TABLE \"odd name\" (id INTEGER PRIMARY KEY, content BLOB, note TEXT); INSERT INTO \"odd name\" (content, note) VALUES (randomblob(12), printf('%.3000c', 'x'));"));

        foreach (var text in new[] { "one", "two", "three" })
        {
            await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/add?text={text}");
        }

        DatabaseResponse database;

        using (var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

            database = await response.GetContentAsync<DatabaseResponse>();
        }

        Assert.IsTrue(database.Enabled);
        Assert.IsTrue(database.Used, "the code connects to it");
        Assert.AreEqual(fixture.Options.DataBytes, database.QuotaBytes);

        var notes = database.Tables.Single(t => t.Name == "notes");

        Assert.AreEqual(3L, notes.Rows);
        Assert.AreEqual("table", notes.Kind);
        Assert.IsTrue(notes.Columns.Single(c => c.Name == "id").PrimaryKey);
        Assert.IsTrue(notes.Columns.Single(c => c.Name == "text").NotNull);

        Assert.IsTrue(database.Tables.Single(t => t.Name == "changelog").Migrations, "Evolve's history is told apart from the app's records");
        Assert.IsFalse(database.Tables.Any(t => t.Name.StartsWith("sqlite_", StringComparison.Ordinal)), "nor what SQLite keeps for itself");

        // newest first, a page at a time
        var page = await RowsAsync(fixture, lambda.PrivateKey, "notes", "limit=2");

        Assert.AreEqual(3, page.Total);
        Assert.HasCount(2, page.Rows);
        Assert.AreEqual("three", page.Rows[0][1]!.ToString());

        var sorted = await RowsAsync(fixture, lambda.PrivateKey, "notes", "order=text&descending=false");

        Assert.AreEqual("one", sorted.Rows[0][1]!.ToString());
        Assert.AreEqual("text", sorted.Order);

        // bytes by their length, and long text cut short with its length
        var odd = await RowsAsync(fixture, lambda.PrivateKey, Uri.EscapeDataString("odd name"));

        var cells = JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(odd.Rows[0]))!.AsArray();

        Assert.AreEqual(12, cells[1]!["blob"]!.GetValue<long>());
        Assert.AreEqual(3000, cells[2]!["length"]!.GetValue<int>());

        using (var missing = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/database/tables/nothing"))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode);
        }

        using (var injected = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/database/tables/notes?order={Uri.EscapeDataString("id; DROP TABLE notes")}"))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, injected.StatusCode, "only a column the table has");
        }
    }

    [TestMethod]
    public async Task ACopyOfADemoStartsWithADatabaseOfItsOwn()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        var lambda = await fixture.CreateLambdaAsync(template: "demo-crud");

        Assert.IsTrue((await StoreAsync(fixture, lambda.PrivateKey)).Enabled);

        await fixture.DeployAsync(lambda.PrivateKey);

        using (var tasks = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/tasks/", "application/json"))
        {
            var listed = await tasks.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, tasks.StatusCode, listed);
            Assert.Contains("welcome1", listed, "migrated and seeded as it started");
        }

        using (var demo = await fixture.GetAsync("/api/v1/lambdas/demo-crud/database"))
        {
            var tables = (await demo.GetContentAsync<DatabaseResponse>()).Tables;

            Assert.IsTrue(tables.Any(t => t.Name == "tasks"), "the demo has one too, and it is read like any other");
        }

        using var refused = await fixture.SendAsync(HttpMethod.Delete, "/api/v1/lambdas/demo-crud/data/database");

        Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode);
    }

    [TestMethod]
    public async Task TheExportCarriesTheDatabase()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("notes-to-go");

        await EnableAsync(fixture, lambda.PrivateKey);

        await SaveAsync(fixture, lambda.PrivateKey, Notes, ("resources/migrations/V1__Notes.sql", Migration));

        await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/add?text=kept");

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        using var archive = new ZipArchive(await response.Content.ReadAsStreamAsync());

        var entry = archive.GetEntry("notes-to-go/database/database.db");

        Assert.IsNotNull(entry, string.Join(", ", archive.Entries.Select(e => e.FullName)));

        var file = Path.Combine(fixture.Options.DataDirectory, "exported.db");

        entry.ExtractToFile(file);

        Assert.AreEqual("SQLite format 3", Encoding.ASCII.GetString(File.ReadAllBytes(file), 0, 15), "plain SQLite, which any tool opens");

        using (var plain = new SqliteConnection($"Data Source={file};Pooling=False"))
        {
            plain.Open();

            using var command = plain.CreateCommand();

            command.CommandText = "SELECT text FROM notes";

            Assert.AreEqual("kept", command.ExecuteScalar());
        }

        string Read(string name)
        {
            using var reader = new StreamReader(archive.GetEntry($"notes-to-go/{name}")!.Open());

            return reader.ReadToEnd();
        }

        Assert.Contains("<PackageReference Include=\"Microsoft.Data.Sqlite\"", Read("notes-to-go.csproj"));
        Assert.Contains("<PackageReference Include=\"Evolve\"", Read("notes-to-go.csproj"), "it migrates with Evolve");
        Assert.DoesNotContain("EntityFrameworkCore", Read("notes-to-go.csproj"), "plain SQL needs no Entity Framework");
        Assert.DoesNotContain("EntityFrameworkCore", Read("Platform/Usings.cs"), "nor its import, which would not compile without it");
        Assert.Contains("public SqliteConnection GetConnection()", Read("Platform/Database.cs"));
        Assert.Contains("global using EvolveDb;", Read("Platform/Usings.cs"));
        Assert.Contains("database/", Read(".gitignore"), "what the app keeps is nobody's source");
        Assert.Contains("database/database.db", Read("Program.cs"));

        Assert.IsFalse(Directory.EnumerateFiles(Path.GetTempPath(), "genhttp-lambda-export-*").Any(f => File.GetCreationTimeUtc(f) > DateTime.UtcNow.AddMinutes(-1) && f.EndsWith(".db", StringComparison.Ordinal)),
                       "the copy is not left lying about");
    }

    [TestMethod]
    public async Task AnAgentSwitchesTheDatabaseOnAndReadsIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var enabled = Structured(await CallToolAsync(fixture, "enable_data", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["kind"] = "database" }));

        Assert.IsTrue(enabled["enabled"]!.GetValue<bool>());
        Assert.Contains("Evolve", enabled["next"]!.GetValue<string>(), "and it is told how to use it");

        await SaveAsync(fixture, lambda.PrivateKey, Notes, ("resources/migrations/V1__Notes.sql", Migration));

        await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/add?text=hello");

        var tables = Structured(await CallToolAsync(fixture, "read_database", new JsonObject { ["privateKey"] = lambda.PrivateKey }));

        var notes = tables["tables"]!.AsArray().Single(t => t!["name"]!.GetValue<string>() == "notes")!;

        Assert.AreEqual(1, notes["rows"]!.GetValue<long>());
        Assert.Contains("text TEXT NOT NULL", notes["columns"]!.AsArray().Select(c => c!.GetValue<string>()));

        var rows = Structured(await CallToolAsync(fixture, "read_database", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["table"] = "notes" }));

        Assert.AreEqual("hello", rows["rows"]![0]!["text"]!.GetValue<string>(), "a row reads as an object, by column");

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey }));

        Assert.IsTrue(read["data"]!["database"]!["enabled"]!.GetValue<bool>());
        Assert.AreEqual(2, read["data"]!["database"]!["tables"]!.GetValue<int>());
    }

    [TestMethod]
    public async Task DeletingALambdaDeletesItsDatabase()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await SaveAsync(fixture, lambda.PrivateKey, Notes, ("resources/migrations/V1__Notes.sql", Migration));

        await ServedAsync(fixture, $"/lambda/{lambda.PublicKey}/add?text=gone");

        var file = FileOf(fixture, lambda.PrivateKey);

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode);
        }

        Assert.IsFalse(Directory.Exists(Path.GetDirectoryName(file)));
    }

    #region Helpers

    private static async Task<DatabaseRowsResponse> RowsAsync(LambdaFixture fixture, string privateKey, string table, string query = "")
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/database/tables/{table}?{query}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<DatabaseRowsResponse>();
    }

    private static JsonObject Structured(JsonObject answer) => (JsonObject)answer["result"]!["structuredContent"]!;

    private static async Task<JsonObject> CallToolAsync(LambdaFixture fixture, string tool, JsonObject arguments)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");

        var message = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments }
        };

        request.Content = new StringContent(message.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        return (JsonObject)JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
    }

    private static async Task EnableAsync(LambdaFixture fixture, string privateKey)
    {
        using var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/data/database");

        Assert.AreEqual(HttpStatusCode.OK, on.StatusCode, await on.Content.ReadAsStringAsync());
    }

    private static async Task<DataStoreResponse> StoreAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/data/database");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<DataStoreResponse>();
    }

    private static string FileOf(LambdaFixture fixture, string privateKey)
    {
        var id = fixture.Meta.GetId(privateKey);

        return Path.Combine(fixture.Options.DatabaseDirectory, id.ToString()!, "database.db");
    }

    /// <summary>
    /// Saves a version of the given code and files, and puts it online.
    /// </summary>
    private static async Task SaveAsync(LambdaFixture fixture, string privateKey, string code, params (string Name, string Content)[] files)
    {
        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/versions?deploy=true", new VersionRequest(
        [
            new LambdaFile(LambdaSource.EntryName, code),
            .. files.Select(f => new LambdaFile(f.Name, f.Content))
        ]));

        var version = await saved.GetContentAsync<SavedVersionResponse>();

        Assert.IsTrue(version.Deployment!.Success, string.Join(" | ", version.Deployment.Diagnostics.Select(d => d.Message)));
    }

    private static async Task<string> ServedAsync(LambdaFixture fixture, string path)
    {
        using var served = await fixture.GetAsync(path);

        var content = await served.GetContentAsync();

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode, content);

        return content;
    }

    #endregion

}
