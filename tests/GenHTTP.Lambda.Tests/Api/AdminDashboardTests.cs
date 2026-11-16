using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Census;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// What the operator's dashboards are drawn from: the lambdas counted, what
/// waits for a decision, the room left, and a lambda summed up without
/// opening its editor.
/// </summary>
[TestClass]
public sealed class AdminDashboardTests
{

    #region Overview

    [TestMethod]
    public async Task TheOverviewCountsTheLambdasAndWhatTheyUse()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var online = await fixture.CreateLambdaAsync("online");
        var keeping = await fixture.CreateLambdaAsync("keeping");
        await fixture.CreateLambdaAsync("quiet");

        await fixture.DeployAsync(online.PrivateKey);

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{keeping.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        await fixture.SeedDemosAsync();

        var overview = await OverviewAsync(fixture);

        var lambdas = overview.Lambdas;

        Assert.AreEqual(3, lambdas.Total, "the demos are counted apart");
        Assert.IsGreaterThan(0, lambdas.Demos);
        Assert.AreEqual(1, lambdas.Online);
        Assert.AreEqual(2, lambdas.Offline);
        Assert.AreEqual(3, lambdas.CreatedDay);
        Assert.AreEqual(3, lambdas.Tiers["Sandbox"]);
        Assert.AreEqual(lambdas.Demos, lambdas.Tiers["Demo"]);
        Assert.AreEqual(1, lambdas.Uses.Database, "a demo's database is not counted");
        Assert.AreEqual(3, lambdas.Uses.Workspace, "the workspace comes switched on");
        Assert.AreEqual(3, lambdas.Full);

        // the newest first, without the demos' being any different
        Assert.IsTrue(overview.Newest.Any(l => l.PublicKey == "quiet"));
    }

    [TestMethod]
    public async Task TheOverviewSaysWhatWaitsForTheOperator()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var asking = await fixture.CreateLambdaAsync("asking");

        var before = await OverviewAsync(fixture);

        Assert.AreEqual(0, before.Attention.ConnectivityWaiting);
        Assert.IsNull(before.Attention.ConnectivityWaitingSince);

        using (var asked = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{asking.PrivateKey}/connectivity", new ConnectivityRequest("The weather from api.example.org")))
        {
            Assert.AreEqual(HttpStatusCode.OK, asked.StatusCode);
        }

        var after = await OverviewAsync(fixture);

        Assert.AreEqual(1, after.Attention.ConnectivityWaiting);
        Assert.IsNotNull(after.Attention.ConnectivityWaitingSince);
        Assert.AreEqual(1, after.Lambdas.Uses.Requested);
    }

    [TestMethod]
    public async Task TheOverviewNamesTheLambdasThatFailedInTheLastHour()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var broken = await fixture.CreateLambdaAsync("broken");

        await fixture.DeployAsync(broken.PrivateKey, "return Inline.Create().Get(() => { throw new InvalidOperationException(\"no\"); });");

        using (var failed = await fixture.GetAsync("http://broken.localhost/"))
        {
            Assert.AreEqual(HttpStatusCode.InternalServerError, failed.StatusCode);
        }

        var overview = await OverviewAsync(fixture);

        var failing = overview.Failing.Single();

        Assert.AreEqual("broken", failing.PublicKey);
        Assert.AreEqual(1, failing.HourFailed);
        Assert.AreEqual(1, failing.DayRequests);
    }

    #endregion

    #region Storage

    [TestMethod]
    public async Task TheStorageSaysWhatIsLeftAndWhatTakesTheRoom()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var keeping = await fixture.CreateLambdaAsync("keeping");

        await fixture.DeployAsync(keeping.PrivateKey);

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{keeping.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        // measured now rather than at the next hour, so the lambda is in it
        await fixture.Platform.Services.GetRequiredService<ICensusService>().MeasureAsync();

        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/storage");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var storage = await response.GetContentAsync<AdminStorageResponse>();

        Assert.IsNotNull(storage.Measured);
        Assert.IsNotNull(storage.Disk, "the volume of a temporary folder describes itself");
        Assert.IsGreaterThan(0, storage.Disk.Free);

        Assert.IsTrue(storage.Parts.Any(p => p.Kind == "versions" && p.Bytes > 0));
        Assert.IsTrue(storage.Parts.Any(p => p.Kind == "platform" && p.Bytes > 0));

        var largest = storage.Largest.Single(l => l.PublicKey == "keeping");

        Assert.IsGreaterThan(0, largest.VersionBytes);
        Assert.IsGreaterThan(0, largest.DataBytes, "an empty database is a file");
        Assert.AreEqual(largest.DataBytes + largest.VersionBytes + largest.DraftBytes + largest.OtherBytes, largest.TotalBytes);

        Assert.IsEmpty(storage.Over);
    }

    [TestMethod]
    public async Task TheStorageIsMeasuredTheFirstTimeItIsAskedFor()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        await fixture.CreateLambdaAsync("anything");

        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/storage");

        var storage = await response.GetContentAsync<AdminStorageResponse>();

        Assert.IsNotNull(storage.Measured, "measured where it never was, rather than answered empty");
    }

    [TestMethod]
    public async Task TheStorageIsBehindTheToken()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        using var storage = await fixture.SendAsync(HttpMethod.Get, "/api/v1/admin/storage");

        Assert.AreEqual(HttpStatusCode.Unauthorized, storage.StatusCode);

        using var overview = await fixture.SendAsync(HttpMethod.Get, "/api/v1/admin/overview");

        Assert.AreEqual(HttpStatusCode.Unauthorized, overview.StatusCode);
    }

    #endregion

    #region One lambda

    [TestMethod]
    public async Task TheOperatorSeesWhatALambdaUsesWithoutItsEditor()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("summed");

        await fixture.DeployAsync(lambda.PrivateKey);

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        using (var drafted = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Dark mode")))
        {
            Assert.AreEqual(HttpStatusCode.Created, drafted.StatusCode);
        }

        using (var published = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/source", new SourceRequest("MIT", null)))
        {
            Assert.AreEqual(HttpStatusCode.OK, published.StatusCode);
        }

        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/lambdas/summed");

        var detail = await response.GetContentAsync<AdminLambdaDetail>();

        Assert.IsNotNull(detail.Uses);
        Assert.IsTrue(detail.Uses.Database);
        Assert.IsTrue(detail.Uses.Workspace, "as it comes");
        Assert.IsFalse(detail.Uses.Secrets);
        Assert.AreEqual(1, detail.Uses.Drafts);
        Assert.IsTrue(detail.Uses.Source);

        Assert.AreEqual("Dark mode", detail.Features!.Single().Name);
        Assert.IsTrue(detail.Source!.Published);
        Assert.AreEqual("MIT", detail.Source.License.Id);
        Assert.IsNull(detail.Showcase);

        // the owner's overview, as the owner reads it
        Assert.IsNotNull(detail.Summary);
        Assert.IsTrue(detail.Summary.Storage.DatabaseEnabled);
        Assert.AreEqual(detail.Lambda.ActiveVersion, detail.Summary.Live?.Version);
    }

    #endregion

    #region Helpers

    private static async Task<AdminOverviewResponse> OverviewAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/overview");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AdminOverviewResponse>();
    }

    #endregion

}
