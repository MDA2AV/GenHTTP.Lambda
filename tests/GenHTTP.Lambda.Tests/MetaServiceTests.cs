using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// Creating, editing and retiring a lambda, without going through HTTP.
/// </summary>
[TestClass]
public sealed class MetaServiceTests
{

    [TestMethod]
    public async Task NewLambdasStartWithTheExample()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.Meta.CreateAsync(null);

        Assert.AreEqual("Free", lambda.Tier);
        Assert.AreEqual(1, lambda.LatestVersion);
        Assert.IsNull(lambda.ActiveVersion, "a new lambda is not online yet");

        var seeded = await fixture.Meta.GetVersionAsync(lambda.PrivateKey, 1);

        Assert.Contains(lambda.PublicKey, seeded.Code, "the example mentions the URL of the lambda");
    }

    [TestMethod]
    public async Task KeysAreClaimedExactlyOnce()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.Meta.CreateAsync("taken");

        var availability = await fixture.Meta.CheckKeyAsync("taken");

        Assert.IsFalse(availability.Available);
        Assert.IsNotNull(availability.Reason);

        var conflict = await Assert.ThrowsExactlyAsync<LambdaException>(async () => await fixture.Meta.CreateAsync("taken"));

        Assert.AreEqual(LambdaError.Conflict, conflict.Error);
    }

    [TestMethod]
    public async Task SavingCreatesANewVersion()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.Meta.CreateAsync(null);

        var saved = await fixture.Meta.SaveAsync(lambda.PrivateKey, "return Content.From(Resource.FromString(\"second\"));");

        Assert.AreEqual(2, saved.Version);

        var versions = await fixture.Meta.GetVersionsAsync(lambda.PrivateKey);

        Assert.HasCount(2, versions);
        Assert.AreEqual(2, versions[0].Version, "the newest version comes first");

        var content = await fixture.Meta.GetVersionAsync(lambda.PrivateKey, 2);

        Assert.Contains("second", content.Code);
    }

    [TestMethod]
    public async Task EmptyCodeIsRejected()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.Meta.CreateAsync(null);

        var error = await Assert.ThrowsExactlyAsync<LambdaException>(async () => await fixture.Meta.SaveAsync(lambda.PrivateKey, "   "));

        Assert.AreEqual(LambdaError.Invalid, error.Error);
    }

    [TestMethod]
    public async Task DeployingAndRetiringSwitchTheLambdaOnAndOff()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.Meta.CreateAsync(null);

        var deployment = await fixture.Meta.DeployAsync(lambda.PrivateKey, null);

        Assert.IsTrue(deployment.Success);
        Assert.AreEqual(1, deployment.Lambda?.ActiveVersion);

        Assert.IsNotNull(await fixture.Meta.ResolveAsync(lambda.PublicKey));

        var retired = await fixture.Meta.UndeployAsync(lambda.PrivateKey);

        Assert.IsNull(retired.ActiveVersion);
        Assert.IsNull(await fixture.Meta.ResolveAsync(lambda.PublicKey), "an undeployed lambda no longer resolves");

        var status = await fixture.Meta.GetStatusAsync(lambda.PublicKey);

        Assert.IsTrue(status.Exists);
        Assert.IsFalse(status.Deployed);
    }

    [TestMethod]
    public async Task CodeThatDoesNotBuildStaysOffline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.Meta.CreateAsync(null);

        await fixture.Meta.SaveAsync(lambda.PrivateKey, "return this is not csharp;");

        var deployment = await fixture.Meta.DeployAsync(lambda.PrivateKey, null);

        Assert.IsFalse(deployment.Success);
        Assert.IsNotEmpty(deployment.Diagnostics);
        Assert.IsNull(deployment.Lambda?.ActiveVersion, "the broken version did not go online");
    }

    [TestMethod]
    public async Task LambdasCanMoveToAnotherKey()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.Meta.CreateAsync("before");

        var moved = await fixture.Meta.ChangeKeyAsync(lambda.PrivateKey, "after");

        Assert.AreEqual("after", moved.PublicKey);
        Assert.IsFalse((await fixture.Meta.GetStatusAsync("before")).Exists);

        await fixture.Meta.CreateAsync("occupied");

        var conflict = await Assert.ThrowsExactlyAsync<LambdaException>(async () => await fixture.Meta.ChangeKeyAsync(lambda.PrivateKey, "occupied"));

        Assert.AreEqual(LambdaError.Conflict, conflict.Error);
    }

    [TestMethod]
    public async Task DeletingRemovesEverything()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.Meta.CreateAsync(null);

        await fixture.Meta.DeleteAsync(lambda.PrivateKey);

        Assert.IsNull(await fixture.Meta.GetAsync(lambda.PrivateKey));
        Assert.IsFalse((await fixture.Meta.GetStatusAsync(lambda.PublicKey)).Exists);
    }

    [TestMethod]
    public async Task MaintenanceRetiresDeploymentsBeforeItRemovesLambdas()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.Meta.CreateAsync(null);

        await fixture.Meta.DeployAsync(lambda.PrivateKey, null);

        var untouched = await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow);

        Assert.AreEqual(0, untouched.Undeployed);
        Assert.AreEqual(0, untouched.Deleted);

        var aged = DateTime.UtcNow + fixture.Options.DeploymentLifetime + TimeSpan.FromMinutes(1);

        var retired = await fixture.Meta.RunMaintenanceAsync(aged);

        Assert.AreEqual(1, retired.Undeployed);
        Assert.IsNull((await fixture.Meta.GetAsync(lambda.PrivateKey))?.ActiveVersion);

        var abandoned = DateTime.UtcNow + fixture.Options.Retention + TimeSpan.FromDays(1);

        var removed = await fixture.Meta.RunMaintenanceAsync(abandoned);

        Assert.AreEqual(1, removed.Deleted);
        Assert.IsNull(await fixture.Meta.GetAsync(lambda.PrivateKey));
    }

}
