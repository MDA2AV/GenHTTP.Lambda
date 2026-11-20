using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Services.Optimization;
using GenHTTP.Lambda.Services.QuietHours;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Maintenance;

/// <summary>
/// The platform keeps the databases in shape - its own and every lambda's -
/// each once a day in its quiet hours, unless the operator switched it off.
/// </summary>
/// <remarks>
/// On SQLite, where a rebuild gives a lambda back the room its deleted
/// records left; what it does on PostgreSQL is in <see cref="Integrations.PostgreSqlTests"/>.
/// </remarks>
[TestClass]
public sealed class OptimizationTests
{

    [TestMethod]
    public async Task ADatabaseMostlyDeletedIsGivenItsRoomBackInTheQuietHours()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var id = await NotesAsync(fixture, "tidy");

        var file = DatabaseOf(fixture, id);

        Fill(file, "INSERT INTO notes (id, text, created) VALUES (@id, @text, '2026-11-20T00:00:00Z')");
        Execute(file, "DELETE FROM notes WHERE id NOT IN ('kept', 'note-1999')");

        // the last one written: a table with a key keeps the ids of its rows
        var last = Execute(file, "SELECT rowid FROM notes WHERE id = 'note-1999'");

        var before = SizeOf(file);

        await Optimization(fixture).RunAsync(Quiet());

        Assert.IsLessThan(before / 4, SizeOf(file), "the room the deleted notes left, given back");

        Assert.AreEqual("2", await NeutralNotes.ServedAsync(fixture, "/lambda/tidy/count"), "and nothing of what it holds");
        Assert.AreEqual(last, Execute(file, "SELECT rowid FROM notes WHERE id = 'note-1999'"), "nor where it holds it");

        var detail = await DetailAsync(fixture, "tidy");

        Assert.IsNotNull(detail.Optimization);
        Assert.IsNull(detail.Optimization.Failure);
        Assert.Contains("VACUUM", detail.Optimization.Steps);
        Assert.Contains("PRAGMA optimize = 0x10002", detail.Optimization.Steps);
        Assert.IsLessThan(detail.Optimization.Before, detail.Optimization.After);

        var overview = await OverviewAsync(fixture);

        Assert.IsTrue(overview.Enabled);
        Assert.AreEqual(1, overview.Lambdas);
        Assert.AreEqual(0, overview.Failed);
        Assert.IsGreaterThan(0L, overview.Freed);

        Assert.IsNotNull(overview.Platform, "the platform's own records are optimized in the same hours");
        Assert.IsTrue(overview.Platform.Steps.Any(s => s.StartsWith("records: ")), string.Join(" | ", overview.Platform.Steps));
        Assert.IsTrue(overview.Platform.Steps.Any(s => s.StartsWith("analytics: ")), string.Join(" | ", overview.Platform.Steps));
    }

    [TestMethod]
    public async Task NothingIsOptimizedOutsideTheQuietHours()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        await NotesAsync(fixture, "busy");

        await Optimization(fixture).RunAsync(Busy());

        Assert.IsNull((await DetailAsync(fixture, "busy")).Optimization);

        var overview = await OverviewAsync(fixture);

        Assert.IsNull(overview.Platform);
        Assert.AreEqual(0, overview.Lambdas);
    }

    [TestMethod]
    public async Task NothingIsOptimizedAsTheServerStarts()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        await NotesAsync(fixture, "starting");

        // whatever the hour: the first minutes go to serving and warming up
        await Optimization(fixture).RunAsync(DateTime.UtcNow);

        Assert.IsNull((await OverviewAsync(fixture)).Platform);
    }

    [TestMethod]
    public async Task TheOperatorCanLeaveTheDatabasesToTheirOwnTools()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        await NotesAsync(fixture, "left");

        using (var off = await fixture.SendAsOperatorAsync(HttpMethod.Put, "/api/v1/admin/settings", new SettingsModel(OptimizeDatabases: false)))
        {
            Assert.AreEqual(HttpStatusCode.OK, off.StatusCode);
            Assert.IsFalse((await off.GetContentAsync<SettingsModel>()).OptimizeDatabases);
        }

        using (var settings = await fixture.GetAsOperatorAsync("/api/v1/admin/settings"))
        {
            Assert.IsFalse((await settings.GetContentAsync<SettingsModel>()).OptimizeDatabases);
        }

        await Optimization(fixture).RunAsync(Quiet());

        var overview = await OverviewAsync(fixture);

        Assert.IsFalse(overview.Enabled);
        Assert.IsNull(overview.Platform);
        Assert.IsNull((await DetailAsync(fixture, "left")).Optimization);
    }

    [TestMethod]
    public async Task ADatabaseIsOptimizedOnceADayAndOnlyWhereItWasVisitedSince()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        await NotesAsync(fixture, "daily");

        var quiet = Quiet();

        await Optimization(fixture).RunAsync(quiet);

        var first = (await DetailAsync(fixture, "daily")).Optimization;

        Assert.IsNotNull(first);

        // the next run of the same quiet hours finds it done
        await Optimization(fixture).RunAsync(quiet.AddMinutes(15));

        Assert.AreEqual(first.Optimized, (await DetailAsync(fixture, "daily")).Optimization!.Optimized);

        // a day later it was not visited since, so it is as it was left
        await Optimization(fixture).RunAsync(quiet.AddDays(1));

        Assert.AreEqual(first.Optimized, (await DetailAsync(fixture, "daily")).Optimization!.Optimized);

        Assert.IsGreaterThan(first.Optimized, (await OverviewAsync(fixture, quiet.AddDays(1))).Platform!.Optimized, "the platform's own are, every day");
    }

    /// <summary>
    /// A database that failed is tried again the next day, whether or not its
    /// lambda was visited since.
    /// </summary>
    [TestMethod]
    public async Task ADatabaseThatFailedIsTriedAgainTheNextDay()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var id = await NotesAsync(fixture, "failing");

        var quiet = Quiet();

        await Optimization(fixture).RunAsync(quiet);

        var databases = fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        // as a run that failed leaves it
        await using (var database = await databases.CreateDbContextAsync())
        {
            await database.DatabaseOptimizations.Where(o => o.LambdaId == id)
                          .ExecuteUpdateAsync(s => s.SetProperty(o => o.Failure, "disk I/O error").SetProperty(o => o.Steps, string.Empty));
        }

        await Optimization(fixture).RunAsync(quiet.AddDays(1));

        var again = (await DetailAsync(fixture, "failing")).Optimization;

        Assert.IsNotNull(again);
        Assert.IsNull(again.Failure);
        Assert.IsGreaterThan(quiet, again.Optimized);
    }

    [TestMethod]
    public async Task ARebuildThatWouldNumberRowsAnewIsLeftOut()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var id = await NotesAsync(fixture, "numbered");

        var file = DatabaseOf(fixture, id);

        // a table with neither a key of its own nor an index, whose row ids
        // an app may have kept
        Execute(file, "CREATE TABLE log (line TEXT)");
        Fill(file, "INSERT INTO log (line) VALUES (@text)");
        Execute(file, "DELETE FROM log WHERE rowid % 2 = 0");

        var last = Execute(file, "SELECT MAX(rowid) FROM log");

        await Optimization(fixture).RunAsync(Quiet());

        Assert.AreEqual(last, Execute(file, "SELECT MAX(rowid) FROM log"), "the rows keep their ids");

        var optimization = (await DetailAsync(fixture, "numbered")).Optimization;

        Assert.IsNotNull(optimization);
        Assert.DoesNotContain("VACUUM", optimization.Steps);
        Assert.IsTrue(optimization.Skipped.Any(s => s.StartsWith("VACUUM: ") && s.Contains("\"log\"")), string.Join(" | ", optimization.Skipped));
        Assert.Contains("PRAGMA optimize = 0x10002", optimization.Steps, "the statistics are taken all the same");
    }

    [TestMethod]
    public async Task ADatabaseThatIsNotThereIsNotMade()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("none");

        var id = (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value;

        var provider = fixture.Platform.Services.GetRequiredService<IDatabaseProvider>();

        var result = await provider.OptimizeAsync(new DataScope(id));

        Assert.AreEqual(DatabaseOptimization.None, result);
        Assert.IsFalse(Directory.Exists(Path.GetDirectoryName(DatabaseOf(fixture, id))), "nothing was made for it");
        Assert.IsFalse(await provider.ExistsAsync(new DataScope(id)), "it stays gone");
    }

    [TestMethod]
    public async Task ALambdaIsOptimizedInItsOwnQuietHours()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var id = await NotesAsync(fixture, "abroad");

        var start = DateTime.UtcNow.Date.AddDays(1);

        // the installation is quiet from 02:00 UTC, the lambda from 14:00
        var counts = fixture.Platform.Services.GetRequiredService<HourlyCounts>();

        var quietHours = fixture.Platform.Services.GetRequiredService<IQuietHoursService>();

        for (var hour = start; hour < start.AddDays(3); hour = hour.AddHours(1))
        {
            for (var i = 0; i < (hour.Hour is >= 2 and < 5 ? 1 : 60); i++)
            {
                counts.CountInstallation(hour.Hour);
            }

            for (var i = 0; i < (hour.Hour is >= 14 and < 17 ? 0 : 20); i++)
            {
                counts.CountLambda(id, hour.Hour);
            }

            await quietHours.FlushAsync(hour.AddMinutes(59));
        }

        var afternoon = start.AddDays(3).AddHours(15);

        await Optimization(fixture).RunAsync(afternoon);

        Assert.IsNotNull((await DetailAsync(fixture, "abroad")).Optimization, "in its quiet hours");
        Assert.IsNull((await OverviewAsync(fixture, afternoon)).Platform, "not in the installation's");

        var night = start.AddDays(4).AddHours(3);

        await Optimization(fixture).RunAsync(night);

        Assert.IsNotNull((await OverviewAsync(fixture, night)).Platform);
    }

    [TestMethod]
    public async Task ARemovedLambdaTakesWhatWasDoneToItsDatabaseWithIt()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var id = await NotesAsync(fixture, "removed");

        await Optimization(fixture).RunAsync(Quiet());

        var databases = fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        await using (var database = await databases.CreateDbContextAsync())
        {
            Assert.IsTrue(await database.DatabaseOptimizations.AnyAsync(o => o.LambdaId == id));
        }

        using (var removed = await fixture.SendAsOperatorAsync(HttpMethod.Delete, "/api/v1/admin/lambdas/removed"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, removed.StatusCode);
        }

        await using (var database = await databases.CreateDbContextAsync())
        {
            Assert.IsFalse(await database.DatabaseOptimizations.AnyAsync(o => o.LambdaId == id));
            Assert.IsTrue(await database.DatabaseOptimizations.AnyAsync(o => o.LambdaId == 0), "the platform's stays");
        }
    }

    #region Helpers

    /// <summary>
    /// A lambda with a database of notes, visited once - which is what makes
    /// its database worth looking at.
    /// </summary>
    private static async Task<long> NotesAsync(LambdaFixture fixture, string publicKey)
    {
        var lambda = await fixture.CreateLambdaAsync(publicKey);

        await NeutralNotes.SaveAsync(fixture, lambda.PrivateKey);

        await NeutralNotes.ServedAsync(fixture, $"/lambda/{publicKey}/add?text=kept");

        // when a lambda was last visited is written down by the maintenance
        await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow);

        var id = (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value;

        Execute(DatabaseOf(fixture, id), "UPDATE notes SET id = 'kept'");

        return id;
    }

    private static IOptimizationService Optimization(LambdaFixture fixture) => fixture.Platform.Services.GetRequiredService<IOptimizationService>();

    /// <summary>
    /// Tomorrow at four on the server's clock: in the hours used while the
    /// quiet hours are not known yet.
    /// </summary>
    private static DateTime Quiet() => TimeZoneInfo.ConvertTimeToUtc(DateTime.Now.Date.AddDays(1).AddHours(4), TimeZoneInfo.Local);

    /// <summary>
    /// Tomorrow at noon on the server's clock.
    /// </summary>
    private static DateTime Busy() => TimeZoneInfo.ConvertTimeToUtc(DateTime.Now.Date.AddDays(1).AddHours(12), TimeZoneInfo.Local);

    private static string DatabaseOf(LambdaFixture fixture, long id) => Path.Combine(fixture.Platform.Options.DataDirectory, "databases", id.ToString(), "database.db");

    private static long SizeOf(string file) => new FileInfo(file).Length + (File.Exists(file + "-wal") ? new FileInfo(file + "-wal").Length : 0);

    /// <summary>
    /// Writes a few megabytes of rows of four kilobytes each.
    /// </summary>
    private static void Fill(string file, string insert)
    {
        using var connection = Open(file);

        using var transaction = connection.BeginTransaction();

        using var command = connection.CreateCommand();

        command.CommandText = insert;
        command.Parameters.Add("@id", SqliteType.Text);
        command.Parameters.Add("@text", SqliteType.Text);

        for (var i = 0; i < 2000; i++)
        {
            command.Parameters["@id"].Value = $"note-{i}";
            command.Parameters["@text"].Value = new string((char)('a' + i % 26), 4000);
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private static object? Execute(string file, string sql)
    {
        using var connection = Open(file);

        using var command = connection.CreateCommand();

        command.CommandText = sql;

        return command.ExecuteScalar();
    }

    private static SqliteConnection Open(string file)
    {
        var connection = new SqliteConnection($"Data Source={file};Pooling=False");

        connection.Open();

        return connection;
    }

    private static async Task<AdminLambdaDetail> DetailAsync(LambdaFixture fixture, string publicKey)
    {
        using var response = await fixture.GetAsOperatorAsync($"/api/v1/admin/lambdas/{publicKey}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AdminLambdaDetail>();
    }

    private static async Task<OptimizationOverview> OverviewAsync(LambdaFixture fixture, DateTime? now = null)
    {
        if (now is { } at)
        {
            return await Optimization(fixture).OverviewAsync(at);
        }

        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/optimization");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<OptimizationOverview>();
    }

    #endregion

}
