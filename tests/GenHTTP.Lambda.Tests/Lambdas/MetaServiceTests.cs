using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Lambdas;

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

        var lambda = fixture.Meta.Create(null);

        Assert.AreEqual("Free", lambda.Tier);
        Assert.AreEqual(1, lambda.LatestVersion);
        Assert.IsNull(lambda.ActiveVersion, "a new lambda is not online yet");

        var seeded = fixture.Meta.GetVersion(lambda.PrivateKey, 1);

        Assert.Contains(lambda.PublicKey, seeded.Code, "the example mentions the URL of the lambda");
    }

    [TestMethod]
    public async Task KeysAreClaimedExactlyOnce()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        fixture.Meta.Create("taken");

        var availability = fixture.Meta.DescribeKey("taken");

        Assert.IsFalse(availability.Available);
        Assert.IsNotNull(availability.Reason);

        var conflict = await Assert.ThrowsExactlyAsync<LambdaException>(async () => fixture.Meta.Create("taken"));

        Assert.AreEqual(LambdaError.Conflict, conflict.Error);
    }

    [TestMethod]
    public async Task SavingCreatesANewVersion()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = fixture.Meta.Create(null);

        var saved = fixture.Meta.Save(lambda.PrivateKey, "return Content.From(Resource.FromString(\"second\"));");

        Assert.AreEqual(2, saved.Version);

        var versions = fixture.Meta.GetVersions(lambda.PrivateKey);

        Assert.HasCount(2, versions);
        Assert.AreEqual(2, versions[0].Version, "the newest version comes first");

        var content = fixture.Meta.GetVersion(lambda.PrivateKey, 2);

        Assert.Contains("second", content.Code);
    }

    [TestMethod]
    public async Task EmptyCodeIsRejected()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = fixture.Meta.Create(null);

        var error = await Assert.ThrowsExactlyAsync<LambdaException>(async () => fixture.Meta.Save(lambda.PrivateKey, "   "));

        Assert.AreEqual(LambdaError.Invalid, error.Error);
    }

    [TestMethod]
    public async Task DeployingAndRetiringSwitchTheLambdaOnAndOff()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = fixture.Meta.Create(null);

        var deployment = await fixture.Meta.DeployAsync(lambda.PrivateKey, null);

        Assert.IsTrue(deployment.Success);
        Assert.AreEqual(1, deployment.Lambda?.ActiveVersion);

        Assert.IsNotNull(fixture.Meta.Resolve(lambda.PublicKey));

        var retired = fixture.Meta.Undeploy(lambda.PrivateKey);

        Assert.IsNull(retired.ActiveVersion);
        Assert.IsNull(fixture.Meta.Resolve(lambda.PublicKey), "an undeployed lambda no longer resolves");

        var status = fixture.Meta.DescribeKey(lambda.PublicKey);

        Assert.IsTrue(status.Exists);
        Assert.IsFalse(status.Deployed);
    }

    [TestMethod]
    public async Task CodeThatDoesNotBuildStaysOffline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = fixture.Meta.Create(null);

        fixture.Meta.Save(lambda.PrivateKey, "return this is not csharp;");

        var deployment = await fixture.Meta.DeployAsync(lambda.PrivateKey, null);

        Assert.IsFalse(deployment.Success);
        Assert.IsNotEmpty(deployment.Diagnostics);
        Assert.IsNull(deployment.Lambda?.ActiveVersion, "the broken version did not go online");
    }

    [TestMethod]
    public async Task LambdasCanMoveToAnotherKey()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = fixture.Meta.Create("before");

        var moved = fixture.Meta.ChangeKey(lambda.PrivateKey, "after");

        Assert.AreEqual("after", moved.PublicKey);
        Assert.IsFalse((fixture.Meta.DescribeKey("before")).Exists);

        fixture.Meta.Create("occupied");

        var conflict = await Assert.ThrowsExactlyAsync<LambdaException>(async () => fixture.Meta.ChangeKey(lambda.PrivateKey, "occupied"));

        Assert.AreEqual(LambdaError.Conflict, conflict.Error);
    }

    [TestMethod]
    public async Task DeletingRemovesEverything()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = fixture.Meta.Create(null);

        fixture.Meta.Delete(lambda.PrivateKey);

        Assert.IsNull(fixture.Meta.Get(lambda.PrivateKey));
        Assert.IsFalse((fixture.Meta.DescribeKey(lambda.PublicKey)).Exists);
    }

    [TestMethod]
    public async Task MaintenanceRetiresDeploymentsBeforeItRemovesLambdas()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = fixture.Meta.Create(null);

        await fixture.Meta.DeployAsync(lambda.PrivateKey, null);

        var untouched = fixture.Meta.RunMaintenance(DateTime.UtcNow);

        Assert.AreEqual(0, untouched.Undeployed);
        Assert.AreEqual(0, untouched.Deleted);

        var aged = DateTime.UtcNow + fixture.Options.DeploymentLifetime + TimeSpan.FromMinutes(1);

        var retired = fixture.Meta.RunMaintenance(aged);

        Assert.AreEqual(1, retired.Undeployed);
        Assert.IsNull((fixture.Meta.Get(lambda.PrivateKey))?.ActiveVersion);

        var abandoned = DateTime.UtcNow + fixture.Options.Retention + TimeSpan.FromDays(1);

        var removed = fixture.Meta.RunMaintenance(abandoned);

        Assert.AreEqual(1, removed.Deleted);
        Assert.IsNull(fixture.Meta.Get(lambda.PrivateKey));
    }

}
