using System.Net;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Tests.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// What keeps a lambda online, and what eventually takes it away.
/// </summary>
[TestClass]
public sealed class InactivityTests
{

    [TestMethod]
    public async Task ADeploymentIsNotTakenDownJustForBeingOld()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with
        {
            DeploymentLifetime = TimeSpan.FromDays(30),
            Retention = TimeSpan.FromDays(30)
        });

        var lambda = await fixture.CreateLambdaAsync("popular");

        await fixture.DeployAsync(lambda.PrivateKey);

        // deployed a fortnight ago, and visited five minutes ago
        await Touch(fixture, "popular", deployed: DateTime.UtcNow.AddDays(-14),
                    modified: DateTime.UtcNow.AddDays(-14), lastSeen: DateTime.UtcNow.AddMinutes(-5));

        var report = await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow);

        Assert.AreEqual(0, report.Undeployed, "being used is what keeps it up, not being recent");
        Assert.AreEqual(0, report.Deleted);
    }

    [TestMethod]
    public async Task OneNobodyHasVisitedForAMonthGoesOffline()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with
        {
            DeploymentLifetime = TimeSpan.FromDays(30),
            Retention = TimeSpan.FromDays(90)
        });

        var lambda = await fixture.CreateLambdaAsync("forgotten");

        await fixture.DeployAsync(lambda.PrivateKey);

        var old = DateTime.UtcNow.AddDays(-45);

        await Touch(fixture, "forgotten", deployed: old, modified: old, lastSeen: old);

        var report = await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow);

        Assert.AreEqual(1, report.Undeployed);
        Assert.AreEqual(0, report.Deleted, "offline, but still there to be deployed again");
    }

    [TestMethod]
    public async Task BeingEditedCountsAsMuchAsBeingVisited()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with
        {
            DeploymentLifetime = TimeSpan.FromDays(30),
            Retention = TimeSpan.FromDays(90)
        });

        var lambda = await fixture.CreateLambdaAsync("tended");

        await fixture.DeployAsync(lambda.PrivateKey);

        // nobody has ever called it, but its author was here yesterday
        await Touch(fixture, "tended", deployed: DateTime.UtcNow.AddDays(-60),
                    modified: DateTime.UtcNow.AddDays(-1), lastSeen: null);

        var report = await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow);

        Assert.AreEqual(0, report.Undeployed);
    }

    [TestMethod]
    public async Task ExamplesAreTheInstallationsOwnAndAreLeftAlone()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with
        {
            DeploymentLifetime = TimeSpan.FromDays(30),
            Retention = TimeSpan.FromDays(30)
        });

        var lambda = await fixture.CreateLambdaAsync("an-example");

        await fixture.DeployAsync(lambda.PrivateKey);

        var old = DateTime.UtcNow.AddDays(-400);

        await Touch(fixture, "an-example", deployed: old, modified: old, lastSeen: old, example: true);

        var report = await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow);

        Assert.AreEqual(0, report.Undeployed);
        Assert.AreEqual(0, report.Deleted);
    }

    [TestMethod]
    public async Task TrafficIsWrittenDownSoItSurvivesARestart()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("visited");

        await fixture.DeployAsync(lambda.PrivateKey);

        using var response = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow);

        await using var database = await Databases(fixture).CreateDbContextAsync();

        var row = await database.Lambdas.SingleAsync(l => l.PublicKey == "visited");

        Assert.IsNotNull(row.LastSeen, "the counters are in memory; this is where they land");
    }

    private static IDbContextFactory<LambdaDbContext> Databases(LambdaFixture fixture)
        => fixture.Application.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

    private static async Task Touch(LambdaFixture fixture, string key, DateTime deployed, DateTime modified,
                                    DateTime? lastSeen, bool example = false)
    {
        await using var database = await Databases(fixture).CreateDbContextAsync();

        var row = await database.Lambdas.SingleAsync(l => l.PublicKey == key);

        row.Deployed = deployed;
        row.Modified = modified;
        row.LastSeen = lastSeen;
        row.IsExample = example;

        await database.SaveChangesAsync();
    }

}
