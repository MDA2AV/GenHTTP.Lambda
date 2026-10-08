using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Diagnostics;

/// <summary>
/// What the server reports about itself while it runs.
/// </summary>
[TestClass]
public sealed class TelemetryTests
{

    [TestMethod]
    public async Task TheServerDescribesItself()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        using var response = await fixture.GetAsOperatorAsync("/api/v1/telemetry");

        var telemetry = await response.GetContentAsync<TelemetryResponse>();

        Assert.IsNotEmpty(telemetry.Server.Engine);
        Assert.IsNotEmpty(telemetry.Server.Runtime);
        Assert.IsGreaterThanOrEqualTo(1, telemetry.Server.Processors);
        Assert.IsGreaterThan(0, telemetry.Latest.ManagedBytes, "a running process holds a heap");
    }

    [TestMethod]
    public async Task TheFiguresAreOnlyForTheOperator()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        using var anonymous = await fixture.GetAsync("/api/v1/telemetry");

        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        using var request = fixture.Host.GetRequest("/api/v1/telemetry/lambdas", HttpMethod.Get);

        request.Headers.Add("X-Admin-Token", "not-the-token");

        using var wrong = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Forbidden, wrong.StatusCode);
    }

    [TestMethod]
    public async Task WithoutAPanelThereAreNoFigures()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/telemetry");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "an installation without a token serves them to nobody");
    }

    [TestMethod]
    public async Task RequestsAreCounted()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var telemetry = fixture.Application.Services.GetRequiredService<ITelemetryService>();

        var before = telemetry.TotalRequests;

        using var _ = await fixture.GetAsync("/api/v1/system");

        Assert.IsGreaterThan(before, telemetry.TotalRequests);
    }

    [TestMethod]
    public async Task ServerErrorsAreCountedApart()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var telemetry = fixture.Application.Services.GetRequiredService<ITelemetryService>();

        var before = telemetry.TotalFailed;

        // a lambda that does not exist is a 404, which is not the server failing
        using var _ = await fixture.GetAsync("/api/v1/lambdas/nosuchkey");

        Assert.AreEqual(before, telemetry.TotalFailed, "a client error is not a server error");
    }

    [TestMethod]
    public async Task SamplesAccumulateIntoASeries()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var telemetry = fixture.Application.Services.GetRequiredService<ITelemetryService>();

        telemetry.Sample();
        telemetry.Sample();

        var series = telemetry.Series(TimeSpan.FromMinutes(5));

        Assert.IsGreaterThanOrEqualTo(2, series.Count);
        Assert.IsTrue(series[0].Taken <= series[^1].Taken, "the series runs oldest first");
    }

    [TestMethod]
    public async Task TheWindowIsBounded()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        // more than a day is clamped rather than refused
        using var response = await fixture.GetAsOperatorAsync("/api/v1/telemetry?minutes=100000");

        var telemetry = await response.GetContentAsync<TelemetryResponse>();

        Assert.IsNotNull(telemetry.Latest);
    }

    [TestMethod]
    public async Task UpgradedConnectionsAreCountedApartFromAnswers()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var telemetry = fixture.Application.Services.GetRequiredService<ITelemetryService>();

        var lambda = await fixture.CreateLambdaAsync("sockets");

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Websocket.Functional()
                            .OnMessage((connection, message) => connection.WritePayloadAsync("hello"));
            """);

        var before = telemetry.TotalUpgrades;

        using var client = new System.Net.WebSockets.ClientWebSocket();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await fixture.ConnectAsync(client, "http://sockets.localhost/", timeout.Token);

        // open sockets are read from /proc and leave loopback out, so the
        // socket opened here is never among them - only the upgrade is ours
        Assert.IsGreaterThan(before, telemetry.TotalUpgrades);
    }

}
