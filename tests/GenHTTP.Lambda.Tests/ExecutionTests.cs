using System.Net;

using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// What visitors of a lambda get to see.
/// </summary>
[TestClass]
public sealed class ExecutionTests
{

    [TestMethod]
    public async Task TheExampleWorksOutOfTheBox()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("example");

        await fixture.DeployAsync(lambda.PrivateKey);

        using var index = await fixture.GetAsync("/lambda/example/");

        Assert.AreEqual(HttpStatusCode.OK, index.StatusCode);
        Assert.Contains("It works", await index.GetContentAsync());

        using var books = await fixture.GetAsync("/lambda/example/books/", "application/json");

        Assert.AreEqual(HttpStatusCode.OK, books.StatusCode);
        Assert.Contains("Dune", await books.GetContentAsync());

        using var specification = await fixture.GetAsync("/lambda/example/openapi.json");

        Assert.AreEqual(HttpStatusCode.OK, specification.StatusCode);
    }

    [TestMethod]
    public async Task OnlyTheDeployedVersionIsServed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, "return Content.From(Resource.FromString(\"live\"));");

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                  LambdaFixture.Version("return Content.From(Resource.FromString(\"draft\"));"));

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);

        using var served = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual("live", await served.GetContentAsync(), "saving does not deploy");

        await fixture.DeployAsync(lambda.PrivateKey);

        using var updated = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual("draft", await updated.GetContentAsync());
    }

    [TestMethod]
    public async Task UndeployedLambdasStopServing()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        using var undeployed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/stop");

        Assert.AreEqual(HttpStatusCode.OK, undeployed.StatusCode);

        using var response = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/", "text/html");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task MissingLambdasSendBrowsersToTheApplication()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var browser = await fixture.GetAsync("/lambda/nothing-here/", "text/html");

        Assert.AreEqual(HttpStatusCode.NotFound, browser.StatusCode);
        Assert.AreEqual(LambdaFixture.SpaMarkup, await browser.GetContentAsync(), "the application explains the situation");

        using var client = await fixture.GetAsync("/lambda/nothing-here/", "application/json");

        Assert.AreEqual(HttpStatusCode.NotFound, client.StatusCode);
        Assert.Contains("nothing-here", await client.GetContentAsync());
    }

    [TestMethod]
    public async Task PathsALambdaDoesNotServeAre404()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey);

        using var response = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/nowhere");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("does not serve", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task FailingLambdasExplainThemselves()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, """
            string Boom() => throw new InvalidOperationException("the lambda is broken");

            return Inline.Create()
                         .Get(() => Boom());
            """);

        using var response = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/", "text/html");

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

        var content = await response.GetContentAsync();

        Assert.Contains("the lambda is broken", content);
        Assert.DoesNotContain("at GenHTTP", content, "stack traces stay on the server");
    }

    [TestMethod]
    public async Task TheApplicationAnswersEveryOtherPath()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        foreach (var path in new[] { "/", "/editor/create", "/editor/some-private-key" })
        {
            using var response = await fixture.GetAsync(path, "text/html");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, path);
            Assert.AreEqual(LambdaFixture.SpaMarkup, await response.GetContentAsync(), path);
        }
    }

}
