using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// Creating a lambda from a link on another page, the way a "try this online"
/// button does it.
/// </summary>
[TestClass]
public sealed class InvitationTests
{

    [TestMethod]
    public async Task ALinkLandsInTheEditor()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/start", "text/html");

        Assert.AreEqual(HttpStatusCode.SeeOther, response.StatusCode);

        var location = response.Headers.Location?.ToString();

        Assert.IsNotNull(location);
        Assert.StartsWith("/editor/", location);
        Assert.EndsWith("?created=1", location, "the editor has to know it owes the visitor the terms");
    }

    [TestMethod]
    public async Task TheKeyIsGeneratedRatherThanAskedFor()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/start", "application/json");

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var lambda = await response.GetContentAsync<LambdaResponse>();

        Assert.IsNotEmpty(lambda.PublicKey);
        Assert.IsNotEmpty(lambda.PrivateKey);
        Assert.AreEqual($"/editor/{lambda.PrivateKey}", lambda.EditorPath);
    }

    [TestMethod]
    public async Task ATemplateCanBeLinkedTo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/start?template=websocket-reactive", "application/json");

        var lambda = await response.GetContentAsync<LambdaResponse>();

        using var seeded = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/1");

        var version = await seeded.GetContentAsync<VersionContentResponse>();

        Assert.Contains("Websocket.Reactive()", version.Code);
    }

    [TestMethod]
    public async Task AnUnknownTemplateIsStillRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/start?template=nope", "application/json");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task TheLambdaIsUsableStraightAway()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/start", "application/json");

        var lambda = await response.GetContentAsync<LambdaResponse>();

        // whoever follows the link should find something that already runs
        var deployment = await fixture.DeployAsync(lambda.PrivateKey);

        Assert.IsTrue(deployment.Success);

        using var served = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode);
    }

}
