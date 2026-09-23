using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Testing;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// The arena is the example everything else is measured against, and it is
/// large enough that a change to it can fail to compile without any other test
/// saying which line.
/// </summary>
[TestClass]
public sealed class ArenaCompileTests
{

    [TestMethod]
    public async Task TheArenaTemplateCompiles()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var code = TemplateCatalog.ForKey("arena", "arena-check");

        var outcome = await fixture.Deployments.ValidateAsync(code);

        Assert.IsTrue(outcome.Success,
            "the arena did not compile:\n" + string.Join("\n",
                outcome.Diagnostics.Select(d => $"  {d.Severity} {d.File}:{d.Line} {d.Message}")));

        /*
         * Compiling is not coming up. The handler is built by running the
         * snippet, and inspected afterwards - so a snippet that compiles can
         * still throw on the way to becoming a handler, or be rejected by the
         * inspector, and the only thing that catches either is deploying it.
         */
        var lambda = await fixture.CreateLambdaAsync("arena-live");

        var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
                                            LambdaFixture.Version(TemplateCatalog.ForKey("arena", "arena-live")));

        saved.Dispose();

        var deployment = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/start",
                                                 new DeploymentRequest(null));

        var result = await deployment.GetContentAsync<DeploymentOutcomeResponse>();

        deployment.Dispose();

        Assert.IsTrue(result.Success,
            "the arena compiled but would not come up:\n" + string.Join("\n",
                result.Diagnostics.Select(d => $"  {d.Severity} {d.File}:{d.Line} {d.Message}")));

        // and it has to answer where it says it does, which for the arena is
        // its own root - the page, not the socket
        using var page = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual(System.Net.HttpStatusCode.OK, page.StatusCode, "the arena does not serve its own page");
    }

}
