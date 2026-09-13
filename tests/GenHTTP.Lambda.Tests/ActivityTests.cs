using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// What each lambda is recorded as having done.
/// </summary>
[TestClass]
public sealed class ActivityTests
{

    [TestMethod]
    public async Task RequestsAreCountedAgainstTheLambdaThatServedThem()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var busy = await fixture.CreateLambdaAsync("busy");
        var quiet = await fixture.CreateLambdaAsync("quiet");

        await fixture.DeployAsync(busy.PrivateKey, "return Inline.Create().Get(() => \"busy\");");
        await fixture.DeployAsync(quiet.PrivateKey, "return Inline.Create().Get(() => \"quiet\");");

        for (var i = 0; i < 3; i++)
        {
            using var _ = await fixture.GetAsync("/lambda/busy/");
        }

        var activity = await DescribeAsync(fixture);

        Assert.AreEqual(3, activity.Lambdas.Single(l => l.PublicKey == "busy").Requests);
        Assert.IsEmpty(activity.Lambdas.Where(l => l.PublicKey == "quiet"), "a lambda nobody visited has nothing to show");
    }

    [TestMethod]
    public async Task TheBusiestComesFirst()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        foreach (var key in new[] { "one", "two" })
        {
            var lambda = await fixture.CreateLambdaAsync(key);
            await fixture.DeployAsync(lambda.PrivateKey, $"return Inline.Create().Get(() => \"{key}\");");
        }

        using var _ = await fixture.GetAsync("/lambda/one/");

        for (var i = 0; i < 4; i++)
        {
            using var __ = await fixture.GetAsync("/lambda/two/");
        }

        var activity = await DescribeAsync(fixture);

        Assert.AreEqual("two", activity.Lambdas[0].PublicKey);
    }

    [TestMethod]
    public async Task AFailingLambdaIsCountedAsFailing()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("broken");

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create().Get(() => { throw new InvalidOperationException("boom"); return "never"; });
            """);

        using var _ = await fixture.GetAsync("/lambda/broken/");

        var entry = (await DescribeAsync(fixture)).Lambdas.Single(l => l.PublicKey == "broken");

        Assert.AreEqual(1, entry.Requests);
        Assert.AreEqual(1, entry.Failed);
    }

    [TestMethod]
    public async Task TimingAndVolumeAreRecorded()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("measured");

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(() => \"0123456789\");");

        using var _ = await fixture.GetAsync("/lambda/measured/");

        var entry = (await DescribeAsync(fixture)).Lambdas.Single(l => l.PublicKey == "measured");

        Assert.IsGreaterThan(0, entry.AverageMillis);
        Assert.IsGreaterThanOrEqualTo(entry.AverageMillis, entry.SlowestMillis);
        Assert.IsGreaterThan(0, entry.BytesOut);
        Assert.IsNotNull(entry.LastSeen);
    }

    [TestMethod]
    public async Task DeletingALambdaForgetsIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("temporary");

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(() => \"hi\");");

        using var _ = await fixture.GetAsync("/lambda/temporary/");

        Assert.ContainsSingle((await DescribeAsync(fixture)).Lambdas.Where(l => l.PublicKey == "temporary"));

        await fixture.Meta.DeleteAsync(lambda.PrivateKey);

        Assert.IsEmpty((await DescribeAsync(fixture)).Lambdas.Where(l => l.PublicKey == "temporary"));
    }

    [TestMethod]
    public async Task TheOverviewCanBeKeptPrivate()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { PublicActivity = false });

        using var response = await fixture.GetAsync("/api/v1/telemetry/lambdas");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        // and it is only the serving that stops, not the counting
        var lambda = await fixture.CreateLambdaAsync("counted");

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(() => \"hi\");");

        using var _ = await fixture.GetAsync("/lambda/counted/");

        var telemetry = fixture.Application.Services.GetRequiredService<LambdaTelemetry>();

        Assert.ContainsSingle(telemetry.Describe().Where(l => l.PublicKey == "counted"));
    }

    private static async Task<ActivityResponse> DescribeAsync(LambdaFixture fixture)
    {
        using var response = await fixture.GetAsync("/api/v1/telemetry/lambdas");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<ActivityResponse>();
    }

}
