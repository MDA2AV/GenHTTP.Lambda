using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Web;

/// <summary>
/// The HTTP challenges of an ACME client, answered while the server runs -
/// for the platform and for a lambda's own domain alike.
/// </summary>
[TestClass]
public sealed class AcmeChallengeTests
{

    private const string Token = "Xk3-q_9ZtR2vB7mNcW4yLpAe";

    private const string Answer = "Xk3-q_9ZtR2vB7mNcW4yLpAe.thumbprint-of-the-account-key";

    [TestMethod]
    public async Task AChallengeIsAnsweredForThePlatform()
    {
        await using var fixture = await CreateAsync(out var root);

        Write(root, Token, Answer);

        using var response = await fixture.GetAsync($"/.well-known/acme-challenge/{Token}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(Answer, await response.GetContentAsync());
    }

    [TestMethod]
    public async Task AChallengeIsAnsweredForALambdasDomainInsteadOfTheLambda()
    {
        await using var fixture = await CreateAsync(out var root);

        var lambda = await fixture.CreateLambdaAsync("shop");

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(\":anything\", (string anything) => \"the lambda\");");

        await fixture.ChangeTierAsync(lambda.PrivateKey, LambdaTier.Premium);

        using (await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/domain", new DomainChangeRequest("shop.example.com"))) { }

        Write(root, Token, Answer);

        using var response = await fixture.GetAsync($"/.well-known/acme-challenge/{Token}", host: "shop.example.com");

        Assert.AreEqual(Answer, await response.GetContentAsync());
    }

    [TestMethod]
    public async Task AMissingChallengeIsNotFound()
    {
        await using var fixture = await CreateAsync(out _);

        using var response = await fixture.GetAsync($"/.well-known/acme-challenge/{Token}", accept: "text/html");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain(LambdaFixture.SpaMarkup, await response.GetContentAsync());
    }

    [TestMethod]
    [DataRow("/.well-known/acme-challenge/..%2F..%2Fsecret")]
    [DataRow("/.well-known/acme-challenge/../../secret")]
    [DataRow("/.well-known/acme-challenge/sub/token")]
    [DataRow("/.well-known/acme-challenge/")]
    public async Task NothingButATokenIsServed(string path)
    {
        await using var fixture = await CreateAsync(out var root);

        await File.WriteAllTextAsync(Path.Combine(root, "secret"), "do not serve me");

        using var response = await fixture.GetAsync(path);

        Assert.DoesNotContain("do not serve me", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task WithoutADirectoryThePathIsNotSpecial()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync($"/.well-known/acme-challenge/{Token}", accept: "text/html");

        Assert.Contains(LambdaFixture.SpaMarkup, await response.GetContentAsync(), "the single page application catches it, as before");
    }

    private static Task<LambdaFixture> CreateAsync(out string root)
    {
        var directory = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", "acme-" + Guid.NewGuid().ToString("n"));

        Directory.CreateDirectory(directory);

        root = directory;

        return LambdaFixture.CreateAsync(o => o with { AcmeDirectory = directory });
    }

    private static void Write(string root, string token, string answer)
    {
        var folder = Path.Combine(root, ".well-known", "acme-challenge");

        Directory.CreateDirectory(folder);

        File.WriteAllText(Path.Combine(folder, token), answer);
    }

}
