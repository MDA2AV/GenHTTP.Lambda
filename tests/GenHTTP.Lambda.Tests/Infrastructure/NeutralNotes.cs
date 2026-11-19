using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// A list of notes written the way agents are taught: Entity Framework Core
/// and Evolve on <c>Database.GetConnection()</c>, a migration of plain SQL,
/// and nothing that names a database - so it runs on any the platform keeps.
/// </summary>
internal static class NeutralNotes
{

    public const string Code = """
        using (var connection = Database.GetConnection())
        {
            new Evolve(connection) { Locations = [Resources.Root + "migrations"], IsEraseDisabled = true }.Migrate();
        }

        return Inline.Create()
                     .Get("notes", () =>
                     {
                         using var db = new Notes(Database.GetConnection());

                         return string.Join(",", db.Entries.AsNoTracking().OrderBy(n => n.Created).Select(n => n.Text).ToList());
                     })
                     .Get("add", (string text) =>
                     {
                         using var db = new Notes(Database.GetConnection());

                         db.Entries.Add(new Note { Id = Guid.NewGuid().ToString("N"), Text = text, Created = DateTime.UtcNow.ToString("O") });

                         return db.SaveChanges().ToString();
                     })
                     .Get("done", () =>
                     {
                         using var db = new Notes(Database.GetConnection());

                         db.Entries.Where(n => n.Text == "milk").ExecuteUpdate(s => s.SetProperty(n => n.Done, true));

                         return string.Join(",", db.Entries.AsNoTracking().Where(n => !n.Done).Select(n => n.Text).ToList());
                     })
                     .Get("count", () =>
                     {
                         using var connection = Database.GetConnection();
                         using var command = connection.CreateCommand();

                         command.CommandText = "SELECT COUNT(*) FROM notes";

                         return Convert.ToInt64(command.ExecuteScalar()).ToString();
                     });

        public class Note
        {
            public string Id { get; set; }

            public string Text { get; set; }

            public string Created { get; set; }

            public bool Done { get; set; }
        }

        public class Notes(DbConnection connection) : DbContext
        {
            public DbSet<Note> Entries => Set<Note>();

            protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseDatabase(connection);

            protected override void OnModelCreating(ModelBuilder model) => model.Entity<Note>().ToTable("notes");
        }
        """;

    public const string Migration = "CREATE TABLE notes (id TEXT NOT NULL PRIMARY KEY, text TEXT NOT NULL, created TEXT NOT NULL, done BOOLEAN NOT NULL DEFAULT FALSE);";

    /// <summary>
    /// Switches the database of a lambda on, and puts the notes online.
    /// </summary>
    public static async Task SaveAsync(LambdaFixture fixture, string privateKey)
    {
        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode, await on.Content.ReadAsStringAsync());
        }

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/versions?deploy=true", new VersionRequest(
        [
            new LambdaFile(LambdaSource.EntryName, Code),
            new LambdaFile("resources/migrations/V1__Notes.sql", Migration)
        ]));

        var version = await saved.GetContentAsync<SavedVersionResponse>();

        Assert.IsTrue(version.Deployment!.Success, string.Join(" | ", version.Deployment.Diagnostics.Select(d => d.Message)));
    }

    public static async Task<string> ServedAsync(LambdaFixture fixture, string address)
    {
        using var served = await fixture.GetAsync(address);

        var content = await served.GetContentAsync();

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode, content);

        return content;
    }

}
