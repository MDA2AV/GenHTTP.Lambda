using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Catalog;

/// <summary>
/// What a new lambda starts with: nothing much, or a copy of a demo.
/// </summary>
[TestClass]
public sealed class TemplateTests
{

    [TestMethod]
    public async Task NothingChosenMeansAnEmptyLambdaThatWorks()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("blank");

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/1");

        var version = await response.GetContentAsync<VersionContentResponse>();

        Assert.HasCount(1, version.Files, "empty means one file");
        Assert.Contains("blank", version.Files[0].Code, "the key belongs in the comments");

        await fixture.DeployAsync(lambda.PrivateKey);

        using var answer = await fixture.GetAsync("/lambda/blank/");

        Assert.AreEqual(HttpStatusCode.OK, answer.StatusCode);
    }

    [TestMethod]
    public void ACopyOfADemoDoesNotClaimToBeOne()
    {
        foreach (var demo in DemoCatalog.All)
        {
            var copy = TemplateCatalog.FilesFor(demo.Id, "mine");

            var original = DemoCatalog.FilesFor(demo);

            Assert.AreEqual(LambdaSource.EntryName, copy[0].Name, "the snippet comes first");
            Assert.IsNull(LambdaSource.Validate(copy), $"a copy of {demo.Id} would be refused");
            CollectionAssert.AreEqual(original.Select(f => f.Name).ToArray(), copy.Select(f => f.Name).ToArray(), "the same files");

            foreach (var file in copy)
            {
                Assert.DoesNotContain("read-only demo", file.Code, $"{demo.Id}/{file.Name} is a copy, and its owner may change it");
                Assert.DoesNotContain("[demo]", file.Code, $"{demo.Id}/{file.Name} still carries a marker");
                Assert.DoesNotContain("{{KEY}}", file.Code);
            }

            Assert.IsTrue(original.Any(f => f.Code.Contains("read-only demo", StringComparison.Ordinal)),
                          $"the demo {demo.Id} itself says what it is");
            Assert.IsFalse(original.Any(f => f.Code.Contains("[demo]", StringComparison.Ordinal)), "without its markers");
        }
    }

    [TestMethod]
    public async Task EveryDemoIsOfferedAsSomethingToBuild()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/system");

        var platform = await response.GetContentAsync<PlatformResponse>();

        CollectionAssert.AreEqual(DemoCatalog.All.Select(d => d.Id).Append(TemplateCatalog.EmptyId).ToArray(),
                                  platform.Starters.Select(s => s.Id).ToArray());

        Assert.IsNull(platform.Starters[^1].Demo, "starting empty copies nothing");
    }

    [TestMethod]
    public async Task AnUnknownStartingPointIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.SendAsync(HttpMethod.Post, "/api/v1/lambdas",
            new { publicKey = (string?)null, acceptedTerms = true, template = "rest-service" });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

}
