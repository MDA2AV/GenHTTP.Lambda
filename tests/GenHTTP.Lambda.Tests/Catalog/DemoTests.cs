using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Catalog;

/// <summary>
/// The lambdas the installation keeps online to be read, and the tier that
/// keeps anybody holding their announced keys from changing them.
/// </summary>
[TestClass]
public sealed class DemoTests
{

    [TestMethod]
    public async Task EveryDemoCompiles()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        foreach (var demo in DemoCatalog.All)
        {
            var outcome = await fixture.Deployments.ValidateAsync(TemplateCatalog.ForKey(demo.Id, demo.Key));

            Assert.IsTrue(outcome.Success, $"'{demo.Id}' did not compile:\n" + string.Join("\n",
                outcome.Diagnostics.Select(d => $"  {d.Severity} {d.File}:{d.Line} {d.Message}")));
        }
    }

    [TestMethod]
    public async Task EveryDemoIsOfferedAndOnline()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        using var response = await fixture.GetAsync("/api/v1/demos");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var demos = await response.GetContentAsync<List<DemoResponse>>();

        CollectionAssert.AreEqual(DemoCatalog.All.Select(d => d.Id).ToArray(), demos.Select(d => d.Id).ToArray());

        foreach (var demo in demos)
        {
            Assert.IsTrue(demo.Live, $"'{demo.Id}' should be answering once seeded");
            Assert.AreEqual(demo.PublicKey, demo.PrivateKey, "a demo's editor key is its public key, announced");
            Assert.AreEqual(LambdaSource.EntryName, demo.Files[0]);

            using var page = await fixture.GetAsync(demo.Path);

            Assert.AreEqual(HttpStatusCode.OK, page.StatusCode, $"'{demo.Id}' should serve its page");
            Assert.AreEqual("text/html", page.Content.Headers.ContentType?.MediaType);
        }
    }

    [TestMethod]
    public async Task ADemoIsReadLikeAnyOtherLambda()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        using var lambda = await fixture.GetAsync("/api/v1/lambdas/demo-crud");

        Assert.AreEqual(HttpStatusCode.OK, lambda.StatusCode);
        Assert.AreEqual(nameof(LambdaTier.Demo), (await lambda.GetContentAsync<LambdaResponse>()).Tier);

        using var version = await fixture.GetAsync("/api/v1/lambdas/demo-crud/versions/1");

        Assert.AreEqual(HttpStatusCode.OK, version.StatusCode, "its code and history are there to be read");

        using var logs = await fixture.GetAsync("/api/v1/lambdas/demo-crud/logs");

        Assert.AreEqual(HttpStatusCode.OK, logs.StatusCode, "and so is how it answers");

        using var files = await fixture.GetAsync("/api/v1/lambdas/demo-crud/files");

        Assert.AreEqual(HttpStatusCode.OK, files.StatusCode, "and what it keeps");
    }

    [TestMethod]
    public async Task NobodyCanChangeADemo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        const string key = "demo-crud";

        (HttpMethod Method, string Path, object? Body)[] attempts =
        [
            (HttpMethod.Post, $"/api/v1/lambdas/{key}/versions", LambdaFixture.Version("return Inline.Create();")),
            (HttpMethod.Post, $"/api/v1/lambdas/{key}/deployment/start", new DeploymentRequest(1)),
            (HttpMethod.Post, $"/api/v1/lambdas/{key}/deployment/stop", null),
            (HttpMethod.Patch, $"/api/v1/lambdas/{key}", new UpdateLambdaRequest("mine-now")),
            (HttpMethod.Delete, $"/api/v1/lambdas/{key}", null),
            (HttpMethod.Put, $"/api/v1/lambdas/{key}/files/tasks.json", new FileRequest(Convert.ToBase64String("[]"u8.ToArray()))),
            (HttpMethod.Delete, $"/api/v1/lambdas/{key}/files/tasks.json", null),
            (HttpMethod.Put, $"/api/v1/lambdas/{key}/folders/mine", null),
            (HttpMethod.Put, $"/api/v1/lambdas/{key}/showcase", new ShowcaseRequest("Mine", "Now mine", null)),
            (HttpMethod.Delete, $"/api/v1/lambdas/{key}/showcase", null)
        ];

        foreach (var (method, path, body) in attempts)
        {
            using var response = await fixture.SendAsync(method, path, body);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, $"{method} {path} should be refused");
        }

        var lambda = await fixture.Meta.GetAsync(key);

        Assert.AreEqual(1, lambda!.LatestVersion, "nothing was saved");
        Assert.IsNotNull(lambda.ActiveVersion, "and it is still online");
    }

    [TestMethod]
    public async Task CheckingCodeAgainstADemoIsNotChangingIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        using var response = await fixture.SendAsync(HttpMethod.Post, "/api/v1/lambdas/demo-crud/code/check",
                                                     LambdaFixture.Code("return Inline.Create();"));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task TheDemoTierIsNotHandedOutByHand()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        var lambda = await fixture.CreateLambdaAsync("somebodys");

        await Assert.ThrowsExactlyAsync<LambdaException>(async () => await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Demo),
                                                         "the seeder would delete it on the next start");

        await Assert.ThrowsExactlyAsync<LambdaException>(async () => await fixture.ChangeTierAsync("demo-crud", LambdaTier.Free),
                                                         "and it would be anybody's to change");
    }

    [TestMethod]
    public async Task KeysOfDemosCannotBeClaimed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var created = await fixture.SendAsync(HttpMethod.Post, "/api/v1/lambdas", new CreateLambdaRequest("demo-mine", true, null));

        Assert.AreEqual(HttpStatusCode.BadRequest, created.StatusCode);

        var lambda = await fixture.CreateLambdaAsync("mine");

        using var moved = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}", new UpdateLambdaRequest("demo-mine"));

        Assert.AreEqual(HttpStatusCode.BadRequest, moved.StatusCode);

        using var described = await fixture.GetAsync("/api/v1/keys/demo-mine");

        var key = await described.GetContentAsync<KeyResponse>();

        Assert.IsFalse(key.Valid);
        Assert.IsFalse(key.Available);
    }

    [TestMethod]
    public async Task ACopyOfADemoIsOnesOwn()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        var copy = await fixture.CreateLambdaAsync(template: "demo-crud");

        Assert.AreEqual(nameof(LambdaTier.Free), copy.Tier);

        await fixture.DeployAsync(copy.PrivateKey);

        using var tasks = await fixture.GetAsync($"/lambda/{copy.PublicKey}/tasks/", "application/json");

        Assert.AreEqual(HttpStatusCode.OK, tasks.StatusCode);
    }

    [TestMethod]
    public async Task DemosOutliveTheSweeps()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        // far enough ahead that everything else would be long gone
        await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow.AddYears(1));

        var status = await fixture.Meta.DescribeKeyAsync("demo-crud");

        Assert.IsTrue(status.Exists, "a demo nobody has opened for a year is still wanted");
        Assert.IsTrue(status.Deployed, "and it should still be answering");
    }

    [TestMethod]
    public async Task SeedingAgainChangesNothing()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        await fixture.SeedDemosAsync();

        var versions = await fixture.Meta.GetVersionsAsync("demo-crud");

        Assert.HasCount(1, versions, "a demo that is current is left alone");
        Assert.AreEqual(VersionOrigins.System, versions[0].Origin);
    }

    [TestMethod]
    public async Task ADemoThatIsNoLongerOneIsRetired()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var stale = await fixture.CreateLambdaAsync("example-gone-away");

        await fixture.MakeDemoAsync(stale.PublicKey);

        await fixture.SeedDemosAsync();

        Assert.IsFalse((await fixture.Meta.DescribeKeyAsync("example-gone-away")).Exists,
                       "a demo dropped from the catalogue should not sit on its key for ever");

        Assert.IsTrue((await fixture.Meta.DescribeKeyAsync("demo-crud")).Exists);
    }

}
