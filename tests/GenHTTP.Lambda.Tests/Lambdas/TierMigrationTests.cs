using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using EvolveDb.Migration;

using Microsoft.Data.Sqlite;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// An installation from before the tiers were named by purpose, started
/// with this release: what its lambdas and its panel's limits become.
/// </summary>
[TestClass]
public sealed class TierMigrationTests
{

    [TestMethod]
    public async Task FreeBecomesTheSandboxPremiumProductionAndNothingBecomesASystemLambda()
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        var file = new LambdaOptions { DataDirectory = directory }.DatabaseFile;

        Directory.CreateDirectory(directory);

        // the database as 0.2.0 left it: migrated as far as V24, with the
        // tiers and the limits under the names they had then
        await using (var connection = new SqliteConnection($"Data Source={file};Pooling=false"))
        {
            await connection.OpenAsync();

            new EvolveDb.Evolve(connection, _ => { })
            {
                Locations = [],
                EmbeddedResourceAssemblies = [typeof(LambdaDbContext).Assembly],
                EmbeddedResourceFilters = ["GenHTTP.Lambda.Data.Migrations"],
                IsEraseDisabled = true,
                MetadataTableName = "schema_versions",
                TargetVersion = new MigrationVersion("24")
            }.Migrate();

            await using var command = connection.CreateCommand();

            command.CommandText = """
                INSERT INTO lambdas (public_key, private_key, tier, created, modified, connectivity) VALUES
                    ('trying', 'key-trying', 'Free', '2026-10-01T00:00:00Z', '2026-10-01T00:00:00Z', NULL),
                    ('shop', 'key-shop', 'Premium', '2026-10-01T00:00:00Z', '2026-10-01T00:00:00Z', NULL),
                    ('declined', 'key-declined', 'Premium', '2026-10-01T00:00:00Z', '2026-10-01T00:00:00Z', 'Declined'),
                    ('asked', 'key-asked', 'Free', '2026-10-01T00:00:00Z', '2026-10-01T00:00:00Z', 'Requested'),
                    ('telemetry', 'key-telemetry', 'Premium', '2026-10-01T00:00:00Z', '2026-10-01T00:00:00Z', NULL);

                INSERT INTO settings (key, value) VALUES
                    ('limits.free.build-bytes', '2048'),
                    ('limits.premium.build-bytes', '8192'),
                    ('limits.free.analytics-days', '14'),
                    ('limits.free.versions', '20'),
                    ('limits.premium.versions', '80'),
                    ('limits.premium.features', '12'),
                    ('limits.free.workspace-bytes', '8192');
                """;

            await command.ExecuteNonQueryAsync();
        }

        SqliteConnection.ClearAllPools();

        await using var fixture = await LambdaFixture.CreateAsync(o => o with { DataDirectory = directory });

        Assert.AreEqual(nameof(LambdaTier.Sandbox), (await fixture.Meta.GetAsync("key-trying"))!.Tier);
        Assert.AreEqual(nameof(LambdaTier.Production), (await fixture.Meta.GetAsync("key-shop"))!.Tier);
        Assert.AreEqual(nameof(LambdaTier.Production), (await fixture.Meta.GetAsync("key-telemetry"))!.Tier, "nothing becomes a system lambda without the operator");

        Assert.AreEqual(0, (await fixture.Meta.ListAsync(new LambdaQuery(Tier: LambdaTier.System))).Matched);

        Assert.IsTrue((await fixture.Meta.GetAsync("key-declined"))!.Connectivity!.Allowed, "what a premium lambda could do, it still can");
        Assert.AreEqual(ConnectivityStatus.Requested, (await fixture.Meta.GetAsync("key-asked"))!.Connectivity!.Status, "and a sandbox's request waits as before");
        Assert.IsFalse((await fixture.Meta.GetAsync("key-trying"))!.Connectivity!.Allowed);

        var limits = fixture.Limits.Get();

        Assert.AreEqual(2048, limits.Sandbox.BuildBytes, "what the operator saved for the free tier");
        Assert.AreEqual(8192, limits.Production.BuildBytes, "and for the premium tier");
        Assert.AreEqual(14, limits.Sandbox.AnalyticsDays);
        Assert.AreEqual(8192 + LambdaOptions.RetiredDefaults["database-bytes"].Sandbox, limits.Sandbox.DataBytes, "the four allowances from before still read");
        Assert.AreEqual(80, limits.Versions, "the larger of the two, so no lambda keeps fewer");
        Assert.AreEqual(12, limits.Features);
        Assert.AreEqual(fixture.Options.System.BuildBytes, limits.System.BuildBytes, "the system tier starts with the defaults");
    }

}
