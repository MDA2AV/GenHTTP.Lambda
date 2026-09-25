using System.Net;

using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Execution;

/// <summary>
/// What visitors of a lambda get to see.
/// </summary>
[TestClass]
public sealed class ExecutionTests
{

    [TestMethod]
    public async Task ANewLambdaWorksOutOfTheBox()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("example");

        await fixture.DeployAsync(lambda.PrivateKey);

        using var index = await fixture.GetAsync("/lambda/example/");

        Assert.AreEqual(HttpStatusCode.OK, index.StatusCode);
        Assert.Contains("Hello", await index.GetContentAsync());
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
    public async Task ARouteThatReadTheBodyStillAnswersWithItsOwnStatus()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create()
                         .Post((Note note) => Refuse(note));

            Note Refuse(Note note) => throw new ProviderException(ResponseStatus.BadRequest, "a note needs a text");

            record Note(string Text);
            """);

        /*
         * Once a route has read the body, the headers of the request are gone,
         * and the error page used to look for Accept there - so every refusal
         * of a route with a body became a 500 about headers.
         */
        using var json = await fixture.SendAsync(HttpMethod.Post, $"/lambda/{lambda.PublicKey}/", new { text = "" }, "application/json");

        Assert.AreEqual(HttpStatusCode.BadRequest, json.StatusCode);
        Assert.Contains("a note needs a text", await json.GetContentAsync());

        using var html = await fixture.SendAsync(HttpMethod.Post, $"/lambda/{lambda.PublicKey}/", new { text = "" }, "text/html");

        Assert.AreEqual(HttpStatusCode.BadRequest, html.StatusCode);
        Assert.AreEqual("text/html", html.Content.Headers.ContentType?.MediaType, "a browser still gets a page");
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
