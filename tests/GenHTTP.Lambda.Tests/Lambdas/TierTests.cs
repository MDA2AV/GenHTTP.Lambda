using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Blocking;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// What a tier is for - a sandbox to try something, production for an app
/// people use, the system tier for what the installation depends on - and
/// what moving a lambda between them does and leaves alone.
/// </summary>
[TestClass]
public sealed class TierTests
{
    private const string Token = LambdaFixture.OperatorToken;

    #region Policy

    [TestMethod]
    public async Task EveryTierIsResolvedToOnePolicy()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var sandbox = fixture.Limits.PolicyOf(LambdaTier.Sandbox);

        Assert.AreEqual(32L * 1024 * 1024, sandbox.MaxVersionBytes);
        Assert.AreEqual(512L * 1024 * 1024, sandbox.MaxDataBytes);
        Assert.AreEqual(7, sandbox.AnalyticsDays);
        Assert.IsTrue(sandbox.ExpiresWhenUnused, "for trying something: it goes when nobody uses it");
        Assert.IsFalse(sandbox.OwnDomain);
        Assert.IsFalse(sandbox.Connectivity);
        Assert.IsTrue(sandbox.Blockable);
        Assert.IsFalse(sandbox.WarmedUp, "compiled on its first request");

        var production = fixture.Limits.PolicyOf(LambdaTier.Production);

        Assert.AreEqual(128L * 1024 * 1024, production.MaxVersionBytes);
        Assert.AreEqual(4096L * 1024 * 1024, production.MaxDataBytes);
        Assert.AreEqual(365, production.AnalyticsDays);
        Assert.IsFalse(production.ExpiresWhenUnused, "an app people use is kept");
        Assert.IsTrue(production.OwnDomain);
        Assert.IsTrue(production.Connectivity, "as the premium tier did");
        Assert.IsTrue(production.Showcased);
        Assert.IsTrue(production.WarmedUp, "compiled as the server starts");

        var system = fixture.Limits.PolicyOf(LambdaTier.System);

        Assert.IsFalse(system.ExpiresWhenUnused);
        Assert.IsTrue(system.OwnDomain);
        Assert.IsTrue(system.Installation, "the installation's own");
        Assert.IsFalse(system.Blockable);
        Assert.IsFalse(system.Showcased);
        Assert.IsFalse(system.DeletedByOwner);
        Assert.IsTrue(system.WarmedUp);
        Assert.IsTrue(system.Assignable, "which the operator moves a lambda to");
        Assert.AreEqual(production.MaxDataBytes, system.MaxDataBytes, "sane limits, not none");

        var demo = fixture.Limits.PolicyOf(LambdaTier.Demo);

        Assert.IsTrue(demo.ReadOnly);
        Assert.IsFalse(demo.WarmedUp, "the seeder compiles what changed, the rest waits for its first request");
        Assert.IsFalse(demo.Assignable);
        Assert.IsFalse(demo.Connectivity, "a demo never connects");
        Assert.AreEqual(sandbox.MaxVersionBytes, demo.MaxVersionBytes, "and is held to what anybody's lambda may have");

        Assert.AreEqual(LambdaTier.Production, fixture.Limits.Above(LambdaTier.Sandbox)!.Tier);
        Assert.IsNull(fixture.Limits.Above(LambdaTier.Production), "nothing beyond production for an app");
        Assert.IsNull(fixture.Limits.Above(LambdaTier.System), "nor is an owner pointed at the installation's own tiers");
        Assert.IsNull(fixture.Limits.Above(LambdaTier.Demo));
    }

    [TestMethod]
    public async Task WhatTheInstallationSetsForATierIsWhatEveryCheckReads()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with
        {
            Sandbox = o.Sandbox with { BuildBytes = 2048, Connectivity = true, Label = "Trying" },
            Production = o.Production with { BuildBytes = 1024 }
        });

        Assert.AreEqual(2048, fixture.Limits.PolicyOf(LambdaTier.Production).MaxVersionBytes, "production never has less than the sandbox");

        var lambda = await fixture.CreateLambdaAsync();

        Assert.AreEqual("Trying", lambda.TierPolicy.Label);
        Assert.IsTrue(lambda.Connectivity!.Allowed, "a sandbox that connects by default here");

        using var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new VersionRequest(Coding(3000)));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Contains("2 KB together in the tier 'Trying'", (await refused.GetContentAsync<ErrorResponse>()).Message, "named as the installation calls it");
    }

    #endregion

    #region Lifetime

    [TestMethod]
    public async Task OnlyTheSandboxIsTakenOfflineAndRemovedWhenUnused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Lifetimes);

        foreach (var (key, tier) in new[] { ("trying", LambdaTier.Sandbox), ("running", LambdaTier.Production), ("depended", LambdaTier.System) })
        {
            var lambda = await fixture.CreateLambdaAsync(key);

            await fixture.DeployAsync(lambda.PrivateKey);

            await fixture.ChangeTierAsync(lambda.PrivateKey, tier);

            var long_ago = DateTime.UtcNow.AddDays(-120);

            await TouchAsync(fixture, key, modified: long_ago, lastSeen: long_ago, tierChanged: long_ago);
        }

        var report = await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow);

        Assert.AreEqual(1, report.Deleted, "the sandbox lambda nobody used for four months");
        Assert.AreEqual(0, report.Undeployed);

        foreach (var key in new[] { "running", "depended" })
        {
            var kept = (await fixture.Meta.GetAsync((await fixture.Meta.GetPrivateKeyAsync(key))!))!;

            Assert.IsNotNull(kept.ActiveVersion, $"{key} is online");
            Assert.IsNull(kept.DeployedUntil, $"{key} has no deadline");
            Assert.IsNull(kept.KeptUntil);
        }

        Assert.IsNull(await fixture.Meta.GetPrivateKeyAsync("trying"));
    }

    [TestMethod]
    public async Task MovedToTheSandboxALambdaIsGivenTheWholeTimeFromTheMove()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Lifetimes);

        var lambda = await fixture.CreateLambdaAsync("demoted");

        await fixture.DeployAsync(lambda.PrivateKey);

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Production);

        var before = DateTime.UtcNow;

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Sandbox);

        // nobody visited or edited it for two months while it was in
        // production, where nothing measured that
        var quiet = DateTime.UtcNow.AddDays(-60);

        await TouchAsync(fixture, "demoted", modified: quiet, lastSeen: quiet, deployed: quiet);

        var report = await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow);

        Assert.AreEqual(0, report.Undeployed, "never counted back from the last visit");
        Assert.AreEqual(0, report.Deleted);

        var info = (await fixture.Meta.GetAsync(lambda.PrivateKey))!;

        Assert.IsTrue(info.DeployedUntil >= before + fixture.Options.DeploymentLifetime, "the clock starts at the move");

        Assert.AreEqual(1, (await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow + fixture.Options.DeploymentLifetime + TimeSpan.FromHours(1))).Undeployed,
                        "and runs from there like any sandbox lambda's");
    }

    [TestMethod]
    public async Task APreviewOfALambdaMovedToTheSandboxIsGivenTheWholeTimeAsWell()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Lifetimes);

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Production);

        var feature = await CreateFeatureAsync(fixture, lambda.PrivateKey, "Slow work");

        using (var started = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/preview/start"))
        {
            Assert.AreEqual(HttpStatusCode.OK, started.StatusCode, await started.Content.ReadAsStringAsync());
        }

        var features = fixture.Platform.Services.GetRequiredService<IFeatureService>();

        await using (var database = await Databases(fixture).CreateDbContextAsync())
        {
            var row = await database.Features.SingleAsync(f => f.Key == feature.Key);

            row.Modified = DateTime.UtcNow.AddDays(-60);

            await database.SaveChangesAsync();
        }

        Assert.AreEqual(0, await features.RunMaintenanceAsync(DateTime.UtcNow), "in production a preview is kept like the lambda");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Sandbox);

        Assert.AreEqual(0, await features.RunMaintenanceAsync(DateTime.UtcNow), "and moved to the sandbox, it is measured from the move");

        Assert.AreEqual(1, await features.RunMaintenanceAsync(DateTime.UtcNow + fixture.Options.DeploymentLifetime + TimeSpan.FromHours(1)));
    }

    #endregion

    #region Transitions

    [TestMethod]
    public async Task MovingALambdaToAnotherTierLeavesItRunning()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("steady");

        // counts in memory from when it started: started again, it would
        // count from one
        await fixture.DeployAsync(lambda.PrivateKey, """
            var count = 0;

            return Inline.Create().Get(() => (++count).ToString());
            """);

        Assert.AreEqual("1", await ServedAsync(fixture, "http://steady.localhost/"));

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Production);

        Assert.AreEqual("2", await ServedAsync(fixture, "http://steady.localhost/"), "promoted, it keeps running - though production connects and the sandbox did not");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.System);

        Assert.AreEqual("3", await ServedAsync(fixture, "http://steady.localhost/"));

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Sandbox);

        Assert.AreEqual("4", await ServedAsync(fixture, "http://steady.localhost/"), "and so it does moved back, built as it was without connecting");
    }

    [TestMethod]
    public async Task FilesBeyondTheRoomOfTheNewTierAreKeptAndReadButNothingIsAdded()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o.WithSizes(data: 4 * WorkspaceLimits.Block, productionData: 32 * WorkspaceLimits.Block));

        var lambda = await fixture.CreateLambdaAsync("crowded");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Production);

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create()
                         .Get("/write", (string name, int size) => { Workspace.WriteBytes(name, new byte[size]); return "written"; })
                         .Get("/read", (string name) => Workspace.ReadBytes(name).Length.ToString());
            """);

        var size = 8 * WorkspaceLimits.Block;

        Assert.AreEqual("written", await ServedAsync(fixture, $"http://crowded.localhost/write?name=big.bin&size={size}"));

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Sandbox);

        Assert.AreEqual(size.ToString(), await ServedAsync(fixture, "http://crowded.localhost/read?name=big.bin"), "what it has stays readable");

        using (var refused = await fixture.GetAsync("http://crowded.localhost/write?name=more.bin&size=1"))
        {
            Assert.AreEqual(HttpStatusCode.InternalServerError, refused.StatusCode, "writes fail, as they do when the data is full");
        }

        Assert.AreEqual(size.ToString(), await ServedAsync(fixture, "http://crowded.localhost/read?name=big.bin"), "and nothing was deleted");
    }

    [TestMethod]
    public async Task AFullWorkspaceSaysWhichTierHasMoreRoom()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o.WithSizes(data: 4 * WorkspaceLimits.Block, productionData: 32 * WorkspaceLimits.Block));

        var lambda = await fixture.CreateLambdaAsync();

        using var refused = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/files/big.bin",
                                                    new FileRequest(Convert.ToBase64String(new byte[8 * WorkspaceLimits.Block])));

        Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);

        var said = (await refused.GetContentAsync<ErrorResponse>()).Message;

        Assert.Contains($"in the tier 'Sandbox'", said, "the limit, and the tier it is the limit of, as the installation calls it");
        Assert.Contains($"In the tier 'Production' it may take {32 * WorkspaceLimits.Block} bytes", said, "and the tier that has more room");
    }

    [TestMethod]
    public async Task ADatabaseBeyondTheRoomOfTheNewTierIsReadButDoesNotGrow()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o.WithSizes(data: 256 * 1024, productionData: 4 * 1024 * 1024));

        var lambda = await fixture.CreateLambdaAsync("records");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Production);

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create()
                         .Get("/grow", () =>
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
                         })
                         .Get("/count", () =>
                         {
                             using var db = Database.GetConnection();
                             using var command = db.CreateCommand();

                             command.CommandText = "SELECT count(*) FROM blobs";

                             return command.ExecuteScalar()!.ToString()!;
                         });
            """);

        Assert.AreEqual("grew", await ServedAsync(fixture, "http://records.localhost/grow"), "four hundred kilobytes fit in production");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Sandbox);

        Assert.AreEqual("4", await ServedAsync(fixture, "http://records.localhost/count"), "its records are read as before");

        StringAssert.Contains(await ServedAsync(fixture, "http://records.localhost/grow"), "full", "and it grows no further");

        Assert.AreEqual("4", await ServedAsync(fixture, "http://records.localhost/count"), "nothing was deleted");
    }

    [TestMethod]
    public async Task AVersionLargerThanTheNewTierStaysAndGoesOnlineAgainButNoNewOneIsSaved()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o.WithSizes(build: 1024, productionBuild: 4096));

        var lambda = await fixture.CreateLambdaAsync("shrunk");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Production);

        int version;

        using (var large = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new VersionRequest(Coding(2000, "large"))))
        {
            Assert.AreEqual(HttpStatusCode.Created, large.StatusCode, await large.Content.ReadAsStringAsync());

            version = (await large.GetContentAsync<SavedVersionResponse>()).Version;
        }

        await fixture.DeployAsync(lambda.PrivateKey, "return Content.From(Resource.FromString(\"small\"));");

        // a feature as large as production allows, based on the newest version
        var feature = await CreateFeatureAsync(fixture, lambda.PrivateKey, "Large again");

        using (var filled = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/files", new FeatureFilesRequest(Coding(2000, "large again"))))
        {
            Assert.AreEqual(HttpStatusCode.OK, filled.StatusCode, await filled.Content.ReadAsStringAsync());
        }

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Sandbox);

        using (var back = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/start", new DeploymentRequest(version)))
        {
            Assert.AreEqual(HttpStatusCode.OK, back.StatusCode, await back.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("large", await ServedAsync(fixture, "http://shrunk.localhost/"), "the version it has goes online again");

        using (var refused = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new VersionRequest(Coding(2000, "again"))))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode);

            var said = (await refused.GetContentAsync<ErrorResponse>()).Message;

            Assert.Contains("1 KB together in the tier 'Sandbox'", said, "the limit, and its tier");
            Assert.Contains("In the tier 'Production' a version may come to 4 KB", said, "and the tier that allows more");
        }

        using var merge = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/merge", new MergeFeatureRequest());

        Assert.AreEqual(HttpStatusCode.BadRequest, merge.StatusCode, "nor is one merged: what a feature holds becomes a version");
    }

    #endregion

    #region System

    [TestMethod]
    public async Task ASystemLambdaIsNeverTakenOfflineNorRemoved()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Lifetimes);

        var lambda = await fixture.CreateLambdaAsync("collector");

        await fixture.DeployAsync(lambda.PrivateKey);

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.System);

        var long_ago = DateTime.UtcNow.AddYears(-1);

        await TouchAsync(fixture, "collector", modified: long_ago, lastSeen: long_ago, deployed: long_ago, tierChanged: long_ago);

        var report = await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow);

        Assert.AreEqual(0, report.Undeployed);
        Assert.AreEqual(0, report.Deleted);

        Assert.IsNotNull((await fixture.Meta.GetAsync(lambda.PrivateKey))!.ActiveVersion);
    }

    [TestMethod]
    public async Task ASystemLambdaCannotBeBlocked()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("collector");

        await fixture.DeployAsync(lambda.PrivateKey);

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.System);

        using (var refused = await AsOperatorAsync(fixture, HttpMethod.Post, "/api/v1/admin/lambdas/collector/block", new BlockRequest("Looks odd")))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode);
            Assert.Contains("cannot be blocked", (await refused.GetContentAsync<ErrorResponse>()).Message);
        }

        Assert.IsNull((await fixture.Meta.GetAsync(lambda.PrivateKey))!.Blocked, "and it is not");
        Assert.IsNotNull((await fixture.Meta.GetAsync(lambda.PrivateKey))!.ActiveVersion);

        // the explicit way: out of the tier first, as a decision of its own
        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Sandbox);

        using (var blocked = await AsOperatorAsync(fixture, HttpMethod.Post, "/api/v1/admin/lambdas/collector/block", new BlockRequest("Looks odd")))
        {
            Assert.AreEqual(HttpStatusCode.OK, blocked.StatusCode);
        }

        using var back = await AsOperatorAsync(fixture, HttpMethod.Put, "/api/v1/admin/lambdas/collector/tier", new TierRequest("System"));

        Assert.AreEqual(HttpStatusCode.Conflict, back.StatusCode, "a blocked lambda is not moved where it could not be unblocked");
    }

    [TestMethod]
    public async Task ABlockedAddressReachesNoSystemLambdaEither()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => LambdaFixture.WithPanel(o) with { UnblockableNetworks = [] });

        var lambda = await fixture.CreateLambdaAsync("collector");

        await fixture.DeployAsync(lambda.PrivateKey, "return Content.From(Resource.FromString(\"reported\"));");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.System);

        Assert.AreEqual("reported", await ServedAsync(fixture, "http://collector.localhost/"));

        using (var listing = await fixture.GetAsOperatorAsync("/api/v1/admin/blocks"))
        {
            var you = (await listing.GetContentAsync<AddressBlockListing>()).You!;

            await fixture.Platform.Services.GetRequiredService<IAddressBlocks>().BlockAsync([you], "Floods the collector", null);
        }

        using var refused = await fixture.GetAsync("http://collector.localhost/");

        Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode, "an address block refuses it everywhere");
    }

    [TestMethod]
    public async Task OnlyTheOperatorDeletesASystemLambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("collector");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.System);

        using (var refused = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}"))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode, "not with its editor key");
            Assert.Contains("Only the operator", (await refused.GetContentAsync<ErrorResponse>()).Message);
        }

        Assert.IsNotNull(await fixture.Meta.GetAsync(lambda.PrivateKey));

        using (var other = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/files/notes.txt", new FileRequest(Convert.ToBase64String("kept"u8.ToArray()))))
        {
            Assert.AreEqual(HttpStatusCode.OK, other.StatusCode, "the editor key works as before for everything else");
        }

        using var deleted = await AsOperatorAsync(fixture, HttpMethod.Delete, "/api/v1/admin/lambdas/collector");

        Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode, await deleted.Content.ReadAsStringAsync());
        Assert.IsNull(await fixture.Meta.GetAsync(lambda.PrivateKey));
    }

    [TestMethod]
    public async Task ASystemLambdaIsNeverInTheShowcase()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("collector");

        await fixture.DeployAsync(lambda.PrivateKey);

        using (var listed = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/showcase", new ShowcaseRequest("Collector", "Collects reports.", Convert.ToBase64String(Png))))
        {
            Assert.AreEqual(HttpStatusCode.OK, listed.StatusCode, await listed.Content.ReadAsStringAsync());
        }

        Assert.AreEqual(1, (await ShowcaseAsync(fixture)).Total);

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.System);

        Assert.AreEqual(0, (await ShowcaseAsync(fixture)).Total, "moved to the system tier, it is no longer listed");

        using (var refused = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/showcase", new ShowcaseRequest("Collector", "Collects reports.", Convert.ToBase64String(Png))))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode, "nor listed again");
        }

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Production);

        Assert.AreEqual(1, (await ShowcaseAsync(fixture)).Total, "its entry was kept, and is back once it leaves the tier");
    }

    #endregion

    #region Helpers

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];

    private static LambdaOptions Lifetimes(LambdaOptions options) => options with
    {
        DeploymentLifetime = TimeSpan.FromDays(30),
        Retention = TimeSpan.FromDays(90)
    };

    private static IDbContextFactory<LambdaDbContext> Databases(LambdaFixture fixture)
        => fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

    /// <summary>
    /// Moves the dates the sweeps read, as time passing would.
    /// </summary>
    private static async Task TouchAsync(LambdaFixture fixture, string key, DateTime modified, DateTime? lastSeen, DateTime? deployed = null, DateTime? tierChanged = null)
    {
        await using var database = await Databases(fixture).CreateDbContextAsync();

        var row = await database.Lambdas.SingleAsync(l => l.PublicKey == key);

        row.Modified = modified;
        row.LastSeen = lastSeen;

        if (deployed != null)
        {
            row.Deployed = deployed;
        }

        if (tierChanged != null)
        {
            row.TierChanged = tierChanged;
        }

        await database.SaveChangesAsync();
    }

    private static async Task<FeatureResponse> CreateFeatureAsync(LambdaFixture fixture, string privateKey, string name, int? @base = null)
    {
        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/features", new CreateFeatureRequest(name, null, @base));

        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode, await created.Content.ReadAsStringAsync());

        return await created.GetContentAsync<FeatureResponse>();
    }

    private static async Task<string> ServedAsync(LambdaFixture fixture, string address)
    {
        using var served = await fixture.GetAsync(address);

        var content = await served.GetContentAsync();

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode, content);

        return content;
    }

    private static async Task<Page<ShowcaseResponse>> ShowcaseAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsync("/api/v1/showcases/");

        return await response.GetContentAsync<Page<ShowcaseResponse>>();
    }

    private static async Task<HttpResponseMessage> AsOperatorAsync(LambdaFixture fixture, HttpMethod method, string path, object? body = null)
    {
        using var request = fixture.Host.GetRequest(path, method);

        request.Headers.Add("X-Admin-Token", Token);

        if (body != null)
        {
            request.Content = System.Net.Http.Json.JsonContent.Create(body, body.GetType(), options: new(System.Text.Json.JsonSerializerDefaults.Web));
        }

        return await fixture.Host.GetResponseAsync(request);
    }

    /// <summary>
    /// A lambda of about the given size that answers with what it is told.
    /// </summary>
    private static LambdaFile[] Coding(int bytes, string answer = "x")
    {
        var code = $"return Content.From(Resource.FromString(\"{answer}\"));\n// ";

        return [new(LambdaSource.EntryName, code + new string('a', bytes - code.Length))];
    }

    #endregion

}
