using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// A service worker registered for the whole of a host sees every page that
/// browser opens there afterwards - so a lambda answering on the platform's
/// own host may not widen a worker's scope beyond its own path.
/// </summary>
[TestClass]
public sealed class ServiceWorkerTests
{

    private const string Worker = """
        return Inline.Create()
                     .Get("sw.js", (IRequest request) => request.Respond()
                                                                .Header("Service-Worker-Allowed", "/")
                                                                .Content("self.addEventListener('fetch', () => {});")
                                                                .Build());
        """;

    [TestMethod]
    public async Task ALambdaAtAHostOfItsOwnDecidesItsWorkersScope()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("worker");

        await fixture.DeployAsync(lambda.PrivateKey, Worker);

        using var script = await fixture.GetAsync("http://worker.localhost/sw.js");

        Assert.AreEqual(HttpStatusCode.OK, script.StatusCode);
        Assert.AreEqual("/", string.Join(",", script.Headers.GetValues("Service-Worker-Allowed")));
    }

    [TestMethod]
    public async Task APreviewMayNotTakeOverThePlatform()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("worker");

        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Offline"));

        var feature = await created.GetContentAsync<FeatureResponse>();

        using var put = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/files",
                                                new FeatureFilesRequest([new LambdaFile(LambdaSource.EntryName, Worker)]));

        Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, await put.Content.ReadAsStringAsync());

        using var started = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/preview/start");

        Assert.AreEqual(HttpStatusCode.OK, started.StatusCode, await started.Content.ReadAsStringAsync());

        using var script = await fixture.GetAsync(feature.PreviewPath + "sw.js");

        Assert.AreEqual(HttpStatusCode.InternalServerError, script.StatusCode);

        StringAssert.Contains(await script.GetContentAsync(), "Service-Worker-Allowed");
        Assert.IsFalse(script.Headers.Contains("Service-Worker-Allowed"));
    }

    [TestMethod]
    public async Task ALambdaBelowAPathMayNotTakeOverThePlatform()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync();

        var lambda = await fixture.CreateLambdaAsync("worker");

        await fixture.DeployAsync(lambda.PrivateKey, Worker);

        using var script = await fixture.GetAsync("/lambda/worker/sw.js");

        Assert.AreEqual(HttpStatusCode.InternalServerError, script.StatusCode);

        Assert.IsFalse(script.Headers.Contains("Service-Worker-Allowed"));

        Assert.IsTrue(fixture.Book.Read(0, null, Microsoft.Extensions.Logging.LogLevel.Trace, 10_000).Lines
                             .Any(l => l.Text.Contains("Refused Service-Worker-Allowed from lambda worker", StringComparison.Ordinal)),
                      "and the log says why");
    }

}
