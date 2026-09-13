using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// The two timers a free lambda runs on, and the dates the editor shows for them.
/// </summary>
[TestClass]
public sealed class LifetimeTests
{

    [TestMethod]
    public async Task ALambdaKnowsWhenItWouldBeRemoved()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}");

        var described = await response.GetContentAsync<LambdaResponse>();

        Assert.IsNull(described.DeployedUntil, "nothing is online yet");
        Assert.AreEqual(described.Modified + fixture.Options.Retention, described.KeptUntil);
    }

    [TestMethod]
    public async Task ADeploymentKnowsWhenItWouldGoOffline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}");

        var described = await response.GetContentAsync<LambdaResponse>();

        Assert.IsNotNull(described.DeployedAt);
        Assert.AreEqual(described.DeployedAt + fixture.Options.DeploymentLifetime, described.DeployedUntil);
    }

    [TestMethod]
    public async Task UndeployingForgetsTheDate()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        using var undeployed = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment");

        var described = await undeployed.GetContentAsync<LambdaResponse>();

        Assert.IsNull(described.DeployedAt);
        Assert.IsNull(described.DeployedUntil);
    }

    [TestMethod]
    public async Task TheLifetimeRunsFromTheDeploymentAndNotFromTheCode()
    {
        // a day of lifetime, and a version written well before it was deployed
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { DeploymentLifetime = TimeSpan.FromDays(1) });

        var meta = fixture.Application.Services.GetRequiredService<IMetaService>();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        // maintenance run as if two hours had passed: the code is older than
        // that, so a lifetime measured from the code would take this offline
        var report = await meta.RunMaintenanceAsync(DateTime.UtcNow.AddHours(2));

        Assert.AreEqual(0, report.Undeployed, "a deployment two hours old is not stale");

        var still = await meta.GetAsync(lambda.PrivateKey);

        Assert.IsNotNull(still?.ActiveVersion);
    }

    [TestMethod]
    public async Task ADeploymentIsTakenOfflineOnceItsDayIsUp()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { DeploymentLifetime = TimeSpan.FromDays(1) });

        var meta = fixture.Application.Services.GetRequiredService<IMetaService>();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        var report = await meta.RunMaintenanceAsync(DateTime.UtcNow.AddDays(2));

        Assert.AreEqual(1, report.Undeployed);

        var after = await meta.GetAsync(lambda.PrivateKey);

        Assert.IsNull(after?.ActiveVersion);
        Assert.IsNull(after?.DeployedUntil, "and it no longer claims to be online until anything");
    }

    [TestMethod]
    public async Task DeployingAgainStartsTheDayOver()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        using var first = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}");

        var before = (await first.GetContentAsync<LambdaResponse>()).DeployedUntil;

        await Task.Delay(1100);

        await fixture.DeployAsync(lambda.PrivateKey);

        using var second = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}");

        var after = (await second.GetContentAsync<LambdaResponse>()).DeployedUntil;

        Assert.IsNotNull(before);
        Assert.IsNotNull(after);
        Assert.IsGreaterThan(before.Value, after.Value, "the editor promises that deploying again extends it");
    }

}
