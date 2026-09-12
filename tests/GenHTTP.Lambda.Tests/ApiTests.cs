using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// The endpoints the editor talks to.
/// </summary>
[TestClass]
public sealed class ApiTests
{

    [TestMethod]
    public async Task TermsHaveToBeAccepted()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.SendAsync(HttpMethod.Post, "/api/v1/lambdas", new CreateLambdaRequest("nope", false));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.GetContentAsync<ErrorResponse>();

        Assert.AreEqual("Invalid", error.Error);
    }

    [TestMethod]
    public async Task CreatingALambdaReturnsBothKeys()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("my-lambda");

        Assert.AreEqual("my-lambda", lambda.PublicKey);
        Assert.AreEqual("/lambda/my-lambda/", lambda.PublicPath);
        Assert.AreEqual($"/editor/{lambda.PrivateKey}", lambda.EditorPath);
        Assert.AreEqual(1, lambda.LatestVersion);
        Assert.IsNull(lambda.ActiveVersion);
    }

    [TestMethod]
    public async Task TakenKeysAreReported()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.CreateLambdaAsync("occupied");

        using var check = await fixture.GetAsync("/api/v1/lambdas/keys/occupied");

        var availability = await check.GetContentAsync<AvailabilityResponse>();

        Assert.IsFalse(availability.Available);

        using var conflict = await fixture.SendAsync(HttpMethod.Post, "/api/v1/lambdas", new CreateLambdaRequest("occupied", true));

        Assert.AreEqual(HttpStatusCode.Conflict, conflict.StatusCode);
    }

    [TestMethod]
    public async Task UnknownEditorKeysAreNotFound()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/lambdas/thereisnosuchkey");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.GetContentAsync<ErrorResponse>();

        Assert.AreEqual("NotFound", error.Error);
    }

    [TestMethod]
    public async Task VersionsAreListedAndCanBeRead()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                  new CodeRequest("return Content.From(Resource.FromString(\"v2\"));"));

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);

        using var listed = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions");

        var versions = await listed.GetContentAsync<List<VersionResponse>>();

        Assert.HasCount(2, versions);

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2");

        var content = await read.GetContentAsync<VersionContentResponse>();

        Assert.Contains("v2", content.Code);
    }

    [TestMethod]
    public async Task CodeCanBeCheckedBeforeItIsSaved()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/check",
                                                     new CodeRequest("return Content.From(Resource.FromString(File.ReadAllText(\"/etc/passwd\")));"));

        var outcome = await response.GetContentAsync<CompilationResponse>();

        Assert.IsFalse(outcome.Success);
        Assert.IsNotEmpty(outcome.Diagnostics);

        using var versions = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions");

        Assert.HasCount(1, await versions.GetContentAsync<List<VersionResponse>>(), "checking does not create a version");
    }

    [TestMethod]
    public async Task BrokenCodeIsRejectedWithDiagnostics()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new CodeRequest("return;"));

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);

        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment", new DeployRequest(2));

        Assert.AreEqual(422, (int)response.StatusCode);

        var deployment = await response.GetContentAsync<DeploymentResponse>();

        Assert.IsFalse(deployment.Success);
        Assert.IsNotEmpty(deployment.Diagnostics);
    }

    [TestMethod]
    public async Task LambdasCanBeMovedAndRemoved()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("first-key");

        using var moved = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/key", new ChangeKeyRequest("second-key"));

        Assert.AreEqual("second-key", (await moved.GetContentAsync<LambdaResponse>()).PublicKey);

        using var status = await fixture.GetAsync("/api/v1/lambdas/public/first-key");

        Assert.IsFalse((await status.GetContentAsync<StatusResponse>()).Exists);

        using var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}");

        Assert.IsTrue(deleted.IsSuccessStatusCode);

        using var gone = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}");

        Assert.AreEqual(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [TestMethod]
    public async Task ThePlatformDescribesItself()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/system");

        var platform = await response.GetContentAsync<PlatformResponse>();

        Assert.IsNotEmpty(platform.Terms);
        Assert.IsNotEmpty(platform.Template);
        Assert.IsNotEmpty(platform.Imports);
        Assert.IsNotEmpty(platform.Completions);
        Assert.AreEqual(fixture.Options.MaxCodeLength, platform.MaxCodeLength);
    }

    [TestMethod]
    public async Task TheApiIsDocumented()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/openapi.json");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        Assert.Contains("/lambdas", await response.GetContentAsync());
    }

}
