using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
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

        using var check = await fixture.GetAsync("/api/v1/keys/occupied");

        var availability = await check.GetContentAsync<KeyResponse>();

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
                                                  LambdaFixture.Version("return Content.From(Resource.FromString(\"v2\"));"));

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);

        using var listed = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions");

        var versions = await listed.GetContentAsync<List<VersionResponse>>();

        Assert.HasCount(2, versions);

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/2");

        var content = await read.GetContentAsync<VersionContentResponse>();

        Assert.Contains("v2", content.Files[0].Code);
    }

    [TestMethod]
    public async Task ChangesApplyToTheNewestVersionAndCanDeployAtOnce()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                                  LambdaFixture.Version("return Content.From(Resource.FromString(\"before\"));"));

        using var changed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions/changes?deploy=true",
                                                    new VersionChangeRequest(null, null, [new FileEdit("lambda.cs", "before", "after")]));

        Assert.AreEqual(HttpStatusCode.Created, changed.StatusCode, await changed.Content.ReadAsStringAsync());

        var result = await changed.GetContentAsync<SavedVersionResponse>();

        Assert.AreEqual(3, result.Version);
        Assert.IsTrue(result.Deployment?.Success);

        using var served = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual("after", await served.GetContentAsync());
    }

    [TestMethod]
    public async Task CodeCanBeCheckedBeforeItIsSaved()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/code/check",
                                                     LambdaFixture.Code("return Content.From(Resource.FromString(File.ReadAllText(\"/etc/passwd\")));"));

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

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", LambdaFixture.Version("return;"));

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);

        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/start", new DeploymentRequest(2));

        Assert.AreEqual(422, (int)response.StatusCode);

        var deployment = await response.GetContentAsync<DeploymentOutcomeResponse>();

        Assert.IsFalse(deployment.Success);
        Assert.IsNotEmpty(deployment.Diagnostics);
    }

    [TestMethod]
    public async Task LambdasCanBeMovedAndRemoved()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("first-key");

        using var moved = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}", new UpdateLambdaRequest("second-key"));

        Assert.AreEqual("second-key", (await moved.GetContentAsync<LambdaResponse>()).PublicKey);

        using var status = await fixture.GetAsync("/api/v1/keys/first-key");

        Assert.IsFalse((await status.GetContentAsync<KeyResponse>()).Exists);

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
        Assert.IsNotEmpty(platform.Templates);
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

        var specification = await response.GetContentAsync();

        // one path of each resource, so a resource that is routed but not
        // discovered shows up here rather than as a gap in the browser
        foreach (var path in new[] { "/lambdas", "/versions", "/versions/zip", "/deployment/start", "/files", "/folders", "/code/check", "/keys", "/builds", "/system" })
        {
            Assert.Contains(path, specification, $"'{path}' is missing from the specification");
        }
    }

    [TestMethod]
    public async Task AKeyIsDescribedInOneAnswer()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("live-key");

        await fixture.DeployAsync(lambda.PrivateKey);

        var free = await DescribeAsync(fixture, "Free-Key");

        Assert.AreEqual("free-key", free.PublicKey, "the key comes back the way it would be stored");
        Assert.IsTrue(free.Valid);
        Assert.IsTrue(free.Available);
        Assert.IsFalse(free.Exists);

        var taken = await DescribeAsync(fixture, "live-key");

        Assert.IsFalse(taken.Available);
        Assert.IsTrue(taken.Exists);
        Assert.IsTrue(taken.Deployed);
        Assert.IsNotNull(taken.Reason);

        var invalid = await DescribeAsync(fixture, "no");

        Assert.IsFalse(invalid.Valid);
        Assert.IsFalse(invalid.Available);
        Assert.IsNotNull(invalid.Reason);
    }

    [TestMethod]
    public async Task TheDeploymentCanBeReadStartedAndStopped()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        Assert.IsFalse((await DeploymentOfAsync(fixture, lambda)).Deployed);

        await fixture.DeployAsync(lambda.PrivateKey);

        var online = await DeploymentOfAsync(fixture, lambda);

        Assert.IsTrue(online.Deployed);
        Assert.AreEqual(1, online.Version);
        Assert.IsNotNull(online.DeployedUntil);

        using var stopped = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/stop");

        Assert.AreEqual(HttpStatusCode.OK, stopped.StatusCode);

        Assert.IsFalse((await DeploymentOfAsync(fixture, lambda)).Deployed);
    }

    [TestMethod]
    public async Task AnUpdateChangesOnlyWhatItNames()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("stays-put");

        using var response = await fixture.SendAsync(HttpMethod.Patch, $"/api/v1/lambdas/{lambda.PrivateKey}", new UpdateLambdaRequest(null));

        Assert.AreEqual("stays-put", (await response.GetContentAsync<LambdaResponse>()).PublicKey);
    }

    [TestMethod]
    public async Task PathsBelowALambdaNobodyServesAreNotFound()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        // asked of every resource below /lambdas in turn, and none of them has it
        using var unknown = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/nothing");

        Assert.AreEqual(HttpStatusCode.NotFound, unknown.StatusCode);

        // a path that is served, with a method it is not served with
        using var wrong = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/versions");

        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, wrong.StatusCode);
    }

    [TestMethod]
    public async Task ThePlatformSaysWhetherItBuilds()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/system");

        var platform = await response.GetContentAsync<PlatformResponse>();

        Assert.IsFalse(platform.Build.Available, "there is no agent in the tests");
    }

    private static async Task<KeyResponse> DescribeAsync(LambdaFixture fixture, string key)
    {
        using var response = await fixture.GetAsync($"/api/v1/keys/{key}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<KeyResponse>();
    }

    private static async Task<DeploymentResponse> DeploymentOfAsync(LambdaFixture fixture, LambdaResponse lambda)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/deployment");

        return await response.GetContentAsync<DeploymentResponse>();
    }

}
