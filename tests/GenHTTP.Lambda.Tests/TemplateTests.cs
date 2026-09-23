using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// The examples a lambda can be started from, and the choice the creation
/// assistant makes on behalf of whoever does not care.
/// </summary>
[TestClass]
public sealed class TemplateTests
{

    [TestMethod]
    public void EveryTemplateCanBeRead()
    {
        foreach (var group in TemplateCatalog.Groups)
        {
            Assert.IsNotEmpty(group.Templates, $"the '{group.Id}' group is empty");

            foreach (var template in group.Templates)
            {
                var code = TemplateCatalog.ForKey(template.Id, "my-key");

                Assert.IsNotEmpty(code, $"'{template.Id}' has no code");
                Assert.DoesNotContain("{{KEY}}", code, $"'{template.Id}' still carries its placeholder");
            }
        }
    }

    [TestMethod]
    public void TheApplicationsAreSplitIntoFiles()
    {
        foreach (var id in (string[])["chat", "game"])
        {
            var files = TemplateCatalog.FilesFor(id, "my-key");

            Assert.IsTrue(files.Count > 1, $"'{id}' is long enough that it should not be one file");
            Assert.AreEqual(LambdaSource.EntryName, files[0].Name, "the snippet comes first");

            foreach (var file in files)
            {
                Assert.IsTrue(LambdaSource.IsValidName(file.Name), $"'{file.Name}' is not a usable name");
                Assert.DoesNotContain("{{KEY}}", file.Code, $"'{file.Name}' still carries its placeholder");
            }
        }
    }

    [TestMethod]
    public void EveryGroupIsOffered()
    {
        CollectionAssert.AreEquivalent(new[] { "rest", "websocket", "app" },
                                       TemplateCatalog.Groups.Select(g => g.Id).ToArray());

        Assert.HasCount(3, TemplateCatalog.Groups.Single(g => g.Id == "websocket").Templates, "one per flavour");
    }

    [TestMethod]
    [DataRow("rest-service")]
    [DataRow("rest-minimal")]
    [DataRow("rest-webservice")]
    [DataRow("websocket-functional")]
    [DataRow("websocket-reactive")]
    [DataRow("websocket-imperative")]
    public async Task EveryTemplateCompiles(string id)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync(publicKey: null, template: id);

        var deployment = await fixture.DeployAsync(lambda.PrivateKey);

        Assert.IsTrue(deployment.Success, string.Join("; ", deployment.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    public async Task TheSeededCodeIsTheTemplateThatWasAskedFor()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync(publicKey: null, template: "websocket-imperative");

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/1");

        var version = await response.GetContentAsync<VersionContentResponse>();

        Assert.Contains("Websocket.Imperative()", version.Files[0].Code);
        Assert.Contains(lambda.PublicKey, version.Files[0].Code, "the key belongs in the comments");
    }

    [TestMethod]
    public async Task NoTemplateMeansTheDefaultOne()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/1");

        var version = await response.GetContentAsync<VersionContentResponse>();

        Assert.Contains("Inline.Create()", version.Files[0].Code);
    }

    [TestMethod]
    public async Task AnUnknownTemplateIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.SendAsync(HttpMethod.Post, "/api/v1/lambdas",
            new { publicKey = (string?)null, acceptedTerms = true, template = "does-not-exist" });

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

}
