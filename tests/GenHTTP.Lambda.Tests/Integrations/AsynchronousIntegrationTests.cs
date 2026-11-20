using System.Collections.Concurrent;
using System.Data.Common;
using System.Net;
using System.Text;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Integrations;

/// <summary>
/// An installation whose data is kept on infrastructure that does not answer
/// at once - a database server, object storage, a vault - as the integrations'
/// contracts allow: what a lambda and the platform ask of it is awaited, and
/// the plain calls of code written before wait for it without deadlocking.
/// </summary>
/// <remarks>
/// The platform's own integrations complete every task before they return it,
/// so these stand in for others with ones that yield first (see
/// <see cref="YieldingIntegrations"/>).
/// </remarks>
[TestClass]
public sealed class AsynchronousIntegrationTests
{

    private const string Migration = "CREATE TABLE notes (id TEXT NOT NULL PRIMARY KEY, text TEXT NOT NULL);";

    /// <summary>
    /// A lambda written as agents are taught: its data awaited.
    /// </summary>
    private const string Awaited = """
        await using (var connection = await Database.GetConnectionAsync())
        {
            new Evolve(connection) { Locations = [Resources.Root + "migrations"], IsEraseDisabled = true }.Migrate();
        }

        return Inline.Create()
                     .Get("records/add", async (string text) =>
                     {
                         await using var db = new Notes(await Database.GetConnectionAsync());

                         db.Entries.Add(new Note { Id = Guid.NewGuid().ToString("N"), Text = text });

                         return (await db.SaveChangesAsync()).ToString();
                     })
                     .Get("records", async () =>
                     {
                         await using var db = new Notes(await Database.GetConnectionAsync());

                         return string.Join(",", (await db.Entries.AsNoTracking().Select(n => n.Text).ToListAsync()).Order());
                     })
                     .Get("files/write", async (string name, string text) =>
                     {
                         await Workspace.CreateFolderAsync("notes");
                         await Workspace.WriteTextAsync("notes/" + name, text);

                         return string.Join(",", await Workspace.ListAsync()) + ";" + string.Join(",", await Workspace.FoldersAsync());
                     })
                     .Get("files/read", async (string name) => await Workspace.ExistsAsync("notes/" + name) ? await Workspace.ReadTextAsync("notes/" + name) : "none")
                     .Get("files/delete", async (string name) =>
                     {
                         await Workspace.DeleteAsync("notes/" + name);

                         return (await Workspace.ExistsAsync("notes/" + name)).ToString();
                     })
                     .Get("secret", async () => await Secret.ExistsAsync("TOKEN") ? await Secret.ReadAsync("TOKEN") : "none");
        """ + Model;

    /// <summary>
    /// The same lambda as code written before its data was awaited: plain
    /// calls, which have to keep working.
    /// </summary>
    private const string Plain = """
        using (var connection = Database.GetConnection())
        {
            new Evolve(connection) { Locations = [Resources.Root + "migrations"], IsEraseDisabled = true }.Migrate();
        }

        return Inline.Create()
                     .Get("records/add", (string text) =>
                     {
                         using var db = new Notes(Database.GetConnection());

                         db.Entries.Add(new Note { Id = Guid.NewGuid().ToString("N"), Text = text });

                         return db.SaveChanges().ToString();
                     })
                     .Get("records", () =>
                     {
                         using var db = new Notes(Database.GetConnection());

                         return string.Join(",", db.Entries.AsNoTracking().Select(n => n.Text).ToList().Order());
                     })
                     .Get("files/write", (string name, string text) =>
                     {
                         Workspace.CreateFolder("notes");
                         Workspace.WriteText("notes/" + name, text);

                         return string.Join(",", Workspace.List()) + ";" + string.Join(",", Workspace.Folders());
                     })
                     .Get("files/read", (string name) => Workspace.Exists("notes/" + name) ? Workspace.ReadText("notes/" + name) : "none")
                     .Get("files/delete", (string name) =>
                     {
                         Workspace.Delete("notes/" + name);

                         return Workspace.Exists("notes/" + name).ToString();
                     })
                     .Get("secret", () => Secret.Exists("TOKEN") ? Secret.Read("TOKEN") : "none");
        """ + Model;

    private const string Model = """


        public class Note
        {
            public string Id { get; set; }

            public string Text { get; set; }
        }

        public class Notes(DbConnection connection) : DbContext
        {
            public DbSet<Note> Entries => Set<Note>();

            protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseDatabase(connection);

            protected override void OnModelCreating(ModelBuilder model) => model.Entity<Note>().ToTable("notes");
        }
        """;

    #region Lambdas

    [TestMethod]
    public async Task ALambdaAwaitsItsDataWhereItIsKeptElsewhere()
    {
        var counter = new YieldingIntegrations.Counter();

        await using var fixture = await CreateAsync(counter);

        var lambda = await PrepareAsync(fixture, "awaited", Awaited);

        await ServesItsDataAsync(fixture, "/lambda/awaited", lambda.PrivateKey);

        Assert.IsTrue(counter.Database > 0, "the database was asked, and waited for");
        Assert.IsTrue(counter.Files > 0, "and the files");
        Assert.IsTrue(counter.Secrets > 0, "and the secrets");

        await DraftsCopyItAsync(fixture, lambda.PrivateKey);

        await MergesExportsAndSwitchesOffAsync(fixture, lambda.PrivateKey, Awaited);

        // a draft merged in its turn, the version saved in the lambda's turn
        // inside it, each statement of the records waited for
        Assert.IsTrue(counter.Records > 0, "and the platform's own records");
    }

    [TestMethod]
    public async Task CodeWrittenBeforeWaitsForDataKeptElsewhere()
    {
        var counter = new YieldingIntegrations.Counter();

        await using var fixture = await CreateAsync(counter);

        var lambda = await PrepareAsync(fixture, "plain", Plain);

        await ServesItsDataAsync(fixture, "/lambda/plain", lambda.PrivateKey);

        await DraftsCopyItAsync(fixture, lambda.PrivateKey);
    }

    [TestMethod]
    public async Task ALambdaWhoseDataIsKeptElsewhereGoesWithItsData()
    {
        var counter = new YieldingIntegrations.Counter();

        await using var fixture = await CreateAsync(counter);

        var lambda = await PrepareAsync(fixture, "gone", Awaited);

        Assert.AreEqual("1", await ServedAsync(fixture, "/lambda/gone/records/add?text=milk"));
        Assert.AreEqual("notes/a.txt;notes", await ServedAsync(fixture, "/lambda/gone/files/write?name=a.txt&text=first"));

        var id = (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value;

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode, await deleted.Content.ReadAsStringAsync());
        }

        var services = fixture.Platform.Services;

        Assert.IsFalse(await services.GetRequiredService<IDatabaseVault>().ExistsAsync(id), "its database is gone");
        Assert.AreEqual(0L, await services.GetRequiredService<IWorkspaceVault>().SizeOfAsync(id, null), "its files");
        Assert.IsEmpty(await services.GetRequiredService<ISecretVault>().ListAsync(id, null), "and its secrets");
    }

    /// <summary>
    /// What an installation on the Ioxide engine relies on: the platform's own
    /// integrations - SQLite, local directories, secrets in its own records -
    /// answer on the thread that asks, so a request awaiting them never
    /// leaves its reactor, and the plain calls of older code wait for nothing.
    /// The same for what a lambda sends over the connection it is handed,
    /// through ADO.NET and Entity Framework.
    /// </summary>
    [TestMethod]
    public async Task ThePlatformsOwnIntegrationsAnswerOnTheThreadThatAsks()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        foreach (var kind in new[] { "database", "secrets" })
        {
            using var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/{kind}");

            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode, await on.Content.ReadAsStringAsync());
        }

        using (var secret = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/TOKEN", new SecretRequest("s3cret")))
        {
            Assert.AreEqual(HttpStatusCode.OK, secret.StatusCode, await secret.Content.ReadAsStringAsync());
        }

        var id = (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value;

        var services = fixture.Platform.Services;

        var databases = services.GetRequiredService<IDatabaseVault>();

        var files = services.GetRequiredService<IWorkspaceVault>().Bind(id, null, databases.DataRoomFor(id, null), c => databases.SizeOfAsync(id, null, c));

        AtOnce(files.CreateFolderAsync("notes", CancellationToken.None), "a folder is made");
        AtOnce(files.WriteAsync("notes/a.txt", Encoding.UTF8.GetBytes("first"), CancellationToken.None), "a file is written");
        Assert.AreEqual("first", Encoding.UTF8.GetString(AtOnce(files.ReadAsync("notes/a.txt", CancellationToken.None), "and read")));
        Assert.IsTrue(AtOnce(files.ExistsAsync("notes/a.txt", CancellationToken.None), "and found"));
        CollectionAssert.AreEqual(new[] { "notes/a.txt" }, AtOnce(files.ListAsync(CancellationToken.None), "and listed"));
        AtOnce(files.DeleteAsync("notes/a.txt", CancellationToken.None), "and deleted");

        var secrets = services.GetRequiredService<ISecretVault>().ReaderFor(id, null);

        Assert.AreEqual("s3cret", AtOnce(secrets.ReadAsync("TOKEN", true, CancellationToken.None), "a secret is loaded"));
        Assert.AreEqual("s3cret", AtOnce(secrets.ReadAsync("TOKEN", true, CancellationToken.None), "and read from memory"));

        await using var connection = AtOnce(databases.ConnectorFor(id, null).ConnectAsync(CancellationToken.None), "a connection is opened");

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "CREATE TABLE notes (id TEXT NOT NULL PRIMARY KEY, text TEXT NOT NULL)";

            AtOnce(command.ExecuteNonQueryAsync(), "a statement is executed");
        }

        await using var db = new Probe(connection, services.GetRequiredService<IDatabaseProvider>());

        db.Notes.Add(new ProbeNote { Id = "1", Text = "milk" });

        Assert.AreEqual(1, AtOnce(db.SaveChangesAsync(), "Entity Framework saves"));
        Assert.AreEqual("milk", AtOnce(db.Notes.AsNoTracking().ToListAsync(), "and queries").Single().Text);
        Assert.AreEqual(1, AtOnce(db.Notes.CountAsync(), "and counts"));
    }

    /// <summary>
    /// What the platform keeps in its own records is awaited as well, and
    /// over SQLite every step of it - a context taken from the pool, a query,
    /// a save, a turn taken, the context given back - completes before the
    /// task is handed out, so a request asking for it never leaves its
    /// reactor either: reading a lambda, loading what answers to its key,
    /// saving a version in its turn, what the operator writes.
    /// </summary>
    [TestMethod]
    public async Task ThePlatformsOwnRecordsAnswerOnTheThreadThatAsks()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("records");

        await fixture.DeployAsync(lambda.PrivateKey);

        var services = fixture.Platform.Services;

        var meta = fixture.Meta;

        var features = services.GetRequiredService<IFeatureService>();
        var settings = services.GetRequiredService<ISettingsService>();
        var limits = services.GetRequiredService<ILimitsService>();
        var builds = services.GetRequiredService<IBuildService>();

        Assert.AreEqual("records", AtOnce(meta.GetAsync(lambda.PrivateKey), "a lambda is read")!.PublicKey);

        Assert.IsNotNull(AtOnce(meta.ResolveAsync("records"), "what answers to its key is loaded"));
        Assert.IsNotNull(AtOnce(meta.ResolveAsync("records"), "and answered from memory"));

        var version = AtOnce(meta.SaveAsync(lambda.PrivateKey, "return Content.From(Resource.FromString(\"two\"));"), "a version is saved, in the lambda's turn");

        Assert.AreEqual(2, version.Version);
        Assert.AreEqual(2, AtOnce(meta.GetVersionsAsync(lambda.PrivateKey), "the versions are listed").Count);

        AtOnce(meta.ChangeViewAsync(lambda.PrivateKey, EditorView.Simple), "a setting of the lambda is changed");

        Assert.AreEqual(0, AtOnce(features.ListAsync(lambda.PrivateKey), "its drafts are listed").Count);

        Assert.IsTrue(AtOnce(meta.ListAsync(new LambdaQuery()), "the panel lists the lambdas").Matched > 0);

        Assert.IsFalse(AtOnce(settings.SaveAsync(settings.Get() with { OptimizeDatabases = false }), "a switch of the operator's is written").OptimizeDatabases);
        Assert.IsFalse(settings.Get().OptimizeDatabases, "and read from memory");

        Assert.IsFalse(AtOnce(builds.SaveSettingsAsync(builds.GetSettings() with { Builds = false }), "what the build agent offers is written").Builds);
        Assert.IsFalse(builds.GetSettings().Builds, "and read from memory");

        AtOnce(limits.SaveAsync(limits.Get()), "the limits are written");

        AtOnce(meta.UndeployAsync(lambda.PrivateKey), "the lambda is taken offline, in its turn");

        Assert.IsNull(AtOnce(meta.ResolveAsync("records"), "after which nothing answers to its key"));
    }

    /// <summary>
    /// What answers to a key is kept in memory until something is written to
    /// the platform's records, and an awaited write says so as the synchronous
    /// ones did - an update in place and a save of what was read alike - so a
    /// lambda never goes on serving what it no longer is.
    /// </summary>
    [TestMethod]
    public async Task AnAwaitedWriteMakesWhatServingHoldsStale()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("cached");

        await fixture.DeployAsync(lambda.PrivateKey);

        Assert.IsNotNull(await fixture.Meta.ResolveAsync("cached"), "it answers, and is remembered");

        var databases = fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        // past the services, so nothing but the write itself tells the caches
        await using (var database = await databases.CreateDbContextAsync())
        {
            await database.Lambdas.Where(l => l.PublicKey == "cached")
                          .ExecuteUpdateAsync(u => u.SetProperty(l => l.ActiveVersion, (int?)null));
        }

        Assert.IsNull(await fixture.Meta.ResolveAsync("cached"), "an update in place makes it stale");

        await using (var database = await databases.CreateDbContextAsync())
        {
            var row = await database.Lambdas.SingleAsync(l => l.PublicKey == "cached");

            row.ActiveVersion = 1;

            await database.SaveChangesAsync();
        }

        Assert.IsNotNull(await fixture.Meta.ResolveAsync("cached"), "and so does a save");
    }

    /// <summary>
    /// A query of the platform's records that is enumerated rather than
    /// awaited compiles - it binds to LINQ's own methods - and runs
    /// synchronously, on PostgreSQL holding a thread while the server
    /// answers. It still answers, is said in the log once per statement, and
    /// fails the test it ran in.
    /// </summary>
    [TestMethod]
    public async Task ARecordReadWithoutAwaitingIsSaidInTheLog()
    {
        var fixture = await LambdaFixture.CreateAsync();

        var services = fixture.Platform.Services;

        await using (var database = await services.GetRequiredService<IDbContextFactory<LambdaDbContext>>().CreateDbContextAsync())
        {
            foreach (var _ in Enumerable.Range(0, 2))
            {
                Assert.IsNotNull(database.Settings.AsNoTracking().ToList(), "it answers");
            }
        }

        var synchronous = services.GetRequiredService<SynchronousRecords>();

        Assert.AreEqual(2, synchronous.Count, "each time it runs");

        var (lines, _, _) = services.GetRequiredService<ILogBook>().Read(0, null, LogLevel.Warning, 1000);

        Assert.AreEqual(1, lines.Count(l => l.Text.StartsWith("Queried the platform's records synchronously", StringComparison.Ordinal)), "said once for the statement");

        await Assert.ThrowsExactlyAsync<AssertFailedException>(async () => await fixture.DisposeAsync());
    }

    /// <summary>
    /// On the Ioxide engine a request runs on the reactor of its core, which
    /// runs what is posted to it only once the request lets go of it. A plain
    /// call that waited for an integration there with the reactor's context
    /// still on the thread would wait for itself: what the integration awaits
    /// would be posted back to the very thread that waits for it.
    /// </summary>
    [TestMethod]
    public async Task APlainCallWaitsWithoutDeadlockingTheThreadItRunsOn()
    {
        var counter = new YieldingIntegrations.Counter();

        await using var fixture = await CreateAsync(counter);

        var lambda = await PrepareAsync(fixture, "reactor", Awaited);

        var id = (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value;

        var services = fixture.Platform.Services;

        var databases = services.GetRequiredService<IDatabaseVault>();

        var connector = databases.ConnectorFor(id, null);
        var secrets = services.GetRequiredService<ISecretVault>().ReaderFor(id, null);
        var files = services.GetRequiredService<IWorkspaceVault>().Bind(id, null, databases.DataRoomFor(id, null), c => databases.SizeOfAsync(id, null, c));

        var answers = Reactor.Run(() =>
        {
            using var connection = connector.Connect();

            files.Write("reactor.txt", Encoding.UTF8.GetBytes("written"));

            return $"{connection.State}|{Encoding.UTF8.GetString(files.Read("reactor.txt"))}|{secrets.Read("TOKEN", true)}";
        });

        Assert.AreEqual("Open|written|s3cret", answers);
    }

    #endregion

    #region Helpers

    private static Task<LambdaFixture> CreateAsync(YieldingIntegrations.Counter counter)
        => LambdaFixture.CreateCommunityAsync(database: YieldingIntegrations.Database(counter), files: YieldingIntegrations.Files(counter),
                                              secrets: YieldingIntegrations.Secrets(counter));

    /// <summary>
    /// A lambda with every kind of data on, a secret stored, and the code online.
    /// </summary>
    private static async Task<LambdaResponse> PrepareAsync(LambdaFixture fixture, string key, string code)
    {
        var lambda = await fixture.CreateLambdaAsync(key);

        foreach (var kind in new[] { "database", "workspace", "secrets" })
        {
            using var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/{kind}");

            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode, await on.Content.ReadAsStringAsync());
        }

        using (var secret = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/TOKEN", new SecretRequest("s3cret")))
        {
            Assert.AreEqual(HttpStatusCode.OK, secret.StatusCode, await secret.Content.ReadAsStringAsync());
        }

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions?deploy=true", new VersionRequest(
        [
            new LambdaFile(LambdaSource.EntryName, code),
            new LambdaFile("resources/migrations/V1__Notes.sql", Migration)
        ]));

        var version = await saved.GetContentAsync<SavedVersionResponse>();

        Assert.IsTrue(version.Deployment!.Success, string.Join(" | ", version.Deployment.Diagnostics.Select(d => d.Message)));

        return lambda;
    }

    private static async Task ServesItsDataAsync(LambdaFixture fixture, string address, string privateKey)
    {
        Assert.AreEqual("1", await ServedAsync(fixture, $"{address}/records/add?text=milk"));
        Assert.AreEqual("1", await ServedAsync(fixture, $"{address}/records/add?text=bread"));
        Assert.AreEqual("bread,milk", await ServedAsync(fixture, $"{address}/records"));

        Assert.AreEqual("notes/a.txt;notes", await ServedAsync(fixture, $"{address}/files/write?name=a.txt&text=first"));
        Assert.AreEqual("first", await ServedAsync(fixture, $"{address}/files/read?name=a.txt"));
        Assert.AreEqual("False", await ServedAsync(fixture, $"{address}/files/delete?name=a.txt"));
        Assert.AreEqual("none", await ServedAsync(fixture, $"{address}/files/read?name=a.txt"));

        Assert.AreEqual("s3cret", await ServedAsync(fixture, $"{address}/secret"));

        // what the owner reads goes through the same integrations
        using (var rows = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/database/tables/notes"))
        {
            Assert.AreEqual(HttpStatusCode.OK, rows.StatusCode, await rows.Content.ReadAsStringAsync());
        }

        using (var database = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/database"))
        {
            var overview = await database.GetContentAsync<DatabaseResponse>();

            Assert.IsTrue(overview.Used, "the code connects to it, whichever form it calls");
            Assert.IsTrue(overview.Tables.Any(t => t.Name == "notes"));
        }

        using var summary = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/summary");

        var said = await summary.GetContentAsync<LambdaSummaryResponse>();

        Assert.IsTrue(said.Storage.UsesDatabase);
        Assert.IsTrue(said.Storage.UsesWorkspace);
        Assert.IsEmpty(said.Storage.MissingSecrets ?? [], "the one it reads is there");
    }

    /// <summary>
    /// A draft starts with a copy of the data, copied through the integrations
    /// away from the request, and leaves the lambda's own alone.
    /// </summary>
    private static async Task DraftsCopyItAsync(LambdaFixture fixture, string privateKey)
    {
        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/features", new CreateFeatureRequest("Try"));

        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode, await created.Content.ReadAsStringAsync());

        var feature = await created.GetContentAsync<FeatureResponse>();

        using (var started = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/features/{feature.Key}/preview/start"))
        {
            Assert.AreEqual(HttpStatusCode.OK, started.StatusCode, await started.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("bread,milk", await ServedAsync(fixture, $"/features/{feature.Key}/records"), "the records were copied");
        Assert.AreEqual("s3cret", await ServedAsync(fixture, $"/features/{feature.Key}/secret"), "and the secrets");

        Assert.AreEqual("1", await ServedAsync(fixture, $"/features/{feature.Key}/records/add?text=tried"));

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{privateKey}/features/{feature.Key}"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode, await deleted.Content.ReadAsStringAsync());
        }
    }

    private static async Task<string> ServedAsync(LambdaFixture fixture, string address)
    {
        using var served = await fixture.GetAsync(address);

        var content = await served.GetContentAsync();

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode, content);

        return content;
    }

    /// <summary>
    /// Merging a draft, making its test data again, exporting the app and
    /// switching a kind of data off - each through integrations that wait.
    /// </summary>
    private static async Task MergesExportsAndSwitchesOffAsync(LambdaFixture fixture, string privateKey, string code)
    {
        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/features", new CreateFeatureRequest("Merged"));

        var feature = await created.GetContentAsync<FeatureResponse>();

        using (var refreshed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/features/{feature.Key}/data/refresh"))
        {
            Assert.AreEqual(HttpStatusCode.OK, refreshed.StatusCode, await refreshed.Content.ReadAsStringAsync());
        }

        using (var put = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/features/{feature.Key}/files", new FeatureFilesRequest(
               [
                   new LambdaFile(LambdaSource.EntryName, code),
                   new LambdaFile("resources/migrations/V1__Notes.sql", Migration),
                   new LambdaFile("docs/product.md", "Notes, kept elsewhere.")
               ], Change: "Says what it is")))
        {
            Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, await put.Content.ReadAsStringAsync());
        }

        using (var merged = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/features/{feature.Key}/merge", new MergeFeatureRequest(Deploy: true)))
        {
            Assert.AreEqual(HttpStatusCode.Created, merged.StatusCode, await merged.Content.ReadAsStringAsync());

            Assert.IsTrue((await merged.GetContentAsync<FeatureMergeResponse>()).Deployment!.Success);
        }

        using (var export = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/export"))
        {
            Assert.AreEqual(HttpStatusCode.OK, export.StatusCode, await export.Content.ReadAsStringAsync());

            using var archive = new System.IO.Compression.ZipArchive(await export.Content.ReadAsStreamAsync());

            Assert.IsTrue(archive.Entries.Any(e => e.FullName.EndsWith("/database/database.db", StringComparison.Ordinal)), "the records go with the code");
        }

        foreach (var kind in new[] { "workspace", "database" })
        {
            using var off = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{privateKey}/data/{kind}");

            Assert.AreEqual(HttpStatusCode.OK, off.StatusCode, await off.Content.ReadAsStringAsync());
        }

        using var database = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/database");

        Assert.IsFalse((await database.GetContentAsync<DatabaseResponse>()).Enabled);
    }

    /// <summary>
    /// The result of what was asked, which has to be there before anything is awaited.
    /// </summary>
    private static T AtOnce<T>(ValueTask<T> pending, string what)
    {
        Assert.IsTrue(pending.IsCompletedSuccessfully, $"{what} on the thread that asked");

        return pending.Result;
    }

    private static T AtOnce<T>(Task<T> pending, string what)
    {
        Assert.IsTrue(pending.IsCompletedSuccessfully, $"{what} on the thread that asked");

        return pending.Result;
    }

    private static void AtOnce(ValueTask pending, string what)
    {
        Assert.IsTrue(pending.IsCompletedSuccessfully, $"{what} on the thread that asked");

        pending.GetAwaiter().GetResult();
    }

    private sealed class ProbeNote
    {
        public string Id { get; set; } = "";

        public string Text { get; set; } = "";
    }

    private sealed class Probe(DbConnection connection, IDatabaseProvider provider) : DbContext
    {
        public DbSet<ProbeNote> Notes => Set<ProbeNote>();

        protected override void OnConfiguring(DbContextOptionsBuilder options) => provider.ConfigureContext(options, connection);

        protected override void OnModelCreating(ModelBuilder model) => model.Entity<ProbeNote>().ToTable("notes");
    }

    /// <summary>
    /// A thread like a reactor of the Ioxide engine: what is posted to its
    /// context runs on it only once the work it is doing is done.
    /// </summary>
    private sealed class Reactor : SynchronizationContext
    {
        private readonly ConcurrentQueue<(SendOrPostCallback Callback, object? State)> _posted = new();

        public override void Post(SendOrPostCallback d, object? state) => _posted.Enqueue((d, state));

        public override SynchronizationContext CreateCopy() => this;

        public static T Run<T>(Func<T> work)
        {
            var reactor = new Reactor();

            T? result = default;

            Exception? failed = null;

            var thread = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(reactor);

                try
                {
                    result = work();

                    Assert.AreSame(reactor, SynchronizationContext.Current, "the context the thread had is given back");
                }
                catch (Exception e)
                {
                    failed = e;
                }

                // only now: a continuation posted while it worked was waited for in vain
                while (reactor._posted.TryDequeue(out var posted))
                {
                    posted.Callback(posted.State);
                }
            })
            {
                // a thread that deadlocked must not keep the test run from ending
                IsBackground = true
            };

            thread.Start();

            Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(30)), "the plain call waited for something posted to the thread it blocked");

            if (failed != null)
            {
                throw new AssertFailedException(failed.ToString());
            }

            return result!;
        }
    }

    #endregion

}
