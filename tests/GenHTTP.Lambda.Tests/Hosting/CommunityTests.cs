using System.Net;
using System.Text.Json;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// The community edition, as somebody runs it for themselves: one address,
/// the lambdas below /lambda/ on it, and nothing the cloud says about itself.
/// </summary>
[TestClass]
public sealed class CommunityTests
{

    private const string Code = """
        return Inline.Create()
                     .Get(() => "home");
        """;

    [TestMethod]
    public async Task ThereAreNoTermsToAccept()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync();

        using var created = await fixture.SendAsync(HttpMethod.Post, "/api/v1/lambdas", new CreateLambdaRequest("mine", false, null));

        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode, "an installation run for oneself has nobody to agree terms with");

        using var system = await fixture.GetAsync("/api/v1/system");

        var described = await system.GetContentAsync<PlatformResponse>();

        Assert.IsNull(described.Terms);
        Assert.IsNull(described.Operator.Contact);
        Assert.IsNull(described.Operator.Abuse);
        Assert.IsFalse(described.Operator.Legal);
    }

    [TestMethod]
    public async Task AnAgentCreatesALambdaWithoutAcceptingAnything()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync();

        using var response = await fixture.SendAsync(HttpMethod.Post, "/mcp", new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "tools/call",
            @params = new { name = "create_lambda", arguments = new { acceptTerms = false, publicKey = "agents" } }
        }, accept: "application/json, text/event-stream");

        var text = await response.Content.ReadAsStringAsync();

        Assert.Contains("http://localhost:8080/lambda/agents/", text, "and is given its address below /lambda/");
        Assert.DoesNotContain("acceptTerms must be true", text);
    }

    [TestMethod]
    public async Task ALambdaAnswersBelowLambda()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => o with { PublicUrl = "http://lambda.example.com" });

        var lambda = await fixture.CreateLambdaAsync("quiz");

        var deployment = await fixture.DeployAsync(lambda.PrivateKey, Code);

        Assert.AreEqual("http://lambda.example.com/lambda/quiz/", deployment.Lambda!.PublicUrl);

        using var home = await fixture.GetAsync("/lambda/quiz/");

        Assert.AreEqual("home", await home.GetContentAsync());
    }

    [TestMethod]
    public async Task TheFrontPageIsInTheLanguageOfTheVisitor()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(options =>
        {
            File.WriteAllText(Path.Combine(LambdaFixture.BundleOf(options, "site"), "pages.json"), JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["/"] = new { text = new Dictionary<string, object> { ["en"] = new { title = "Platform", description = "Yours." } } }
            }));

            return options;
        });

        using var request = fixture.Request("/");

        request.Headers.Add("Accept-Language", "en");

        using var response = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        Assert.AreEqual("/en", response.Headers.Location?.ToString());

        using var page = await fixture.GetAsync("/en", accept: "text/html");

        StringAssert.Contains(await page.GetContentAsync(), "<title>Platform - GenHTTP Lambda</title>");
    }

    [TestMethod]
    public async Task TheEditorNamesItsAddressToNobody()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync();

        var lambda = await fixture.CreateLambdaAsync("quiz");

        using var editor = await fixture.GetAsync($"/editor/{lambda.PrivateKey}", accept: "text/html");

        Assert.AreEqual(HttpStatusCode.OK, editor.StatusCode);

        // the editor key is in its address, and a lambda answers on the same
        // host here - so nothing it loads or links to is told the address
        Assert.AreEqual("no-referrer", string.Join(",", editor.Headers.GetValues("Referrer-Policy")));
        Assert.AreEqual("noindex, nofollow", string.Join(",", editor.Headers.GetValues("X-Robots-Tag")));
    }

    [TestMethod]
    public async Task AnAgentIsToldWhereTheApiOfThisInstallationIs()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => o with { PublicUrl = "https://lambda.example.com" });

        using var response = await fixture.SendAsync(HttpMethod.Post, "/mcp", new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "tools/call",
            @params = new { name = "platform_guide", arguments = new { } }
        }, accept: "application/json, text/event-stream");

        var text = await response.Content.ReadAsStringAsync();

        Assert.Contains("https://lambda.example.com/api/v1/openapi.json", text);
        Assert.DoesNotContain("genhttp.dev/api", text);
    }

}
