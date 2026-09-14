using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// The lambdas the installation keeps online for people to look at.
/// </summary>
[TestClass]
public sealed class ExampleTests
{

    [TestMethod]
    public async Task EveryExampleIsOffered()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/examples");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var listing = await response.GetContentAsync<ExampleListingResponse>();

        Assert.IsNotEmpty(listing.Groups);

        var offered = listing.Groups.SelectMany(g => g.Examples).ToList();

        Assert.AreEqual(ExampleCatalog.All.Count(e => !e.Hidden), offered.Count);
    }

    [TestMethod]
    public async Task AnExampleCarriesTheCodeItRuns()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/examples/rest-minimal");

        var example = await response.GetContentAsync<ExampleResponse>();

        Assert.IsNotEmpty(example.Code);
        Assert.Contains("Inline.Create()", example.Code, "the page shows this instead of an editor");
        Assert.Contains(example.PublicKey, example.Code, "and the key in its comments is the one it is hosted at");
    }

    [TestMethod]
    public async Task AnUnknownExampleIsNotFound()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/examples/nope");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task NoExampleHandsOutItsEditorKey()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // seeding creates them for real, so there are editor keys to leak
        await fixture.SeedExamplesAsync();

        using var listing = await fixture.GetAsync("/api/v1/examples");

        var body = await listing.Content.ReadAsStringAsync();

        foreach (var example in ExampleCatalog.All)
        {
            var privateKey = await fixture.PrivateKeyOfAsync(example.PublicKey);

            Assert.IsNotNull(privateKey, "the example should exist after seeding");

            // this is the whole of what makes a lambda editable; an example is
            // read only precisely because nobody outside has one
            Assert.DoesNotContain(privateKey, body);
        }
    }

    [TestMethod]
    public async Task SeedingTwiceChangesNothing()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedExamplesAsync();

        var first = await fixture.PrivateKeyOfAsync(ExampleCatalog.KeyFor("rest-minimal"));

        await fixture.SeedExamplesAsync();

        var second = await fixture.PrivateKeyOfAsync(ExampleCatalog.KeyFor("rest-minimal"));

        Assert.IsNotNull(first);
        Assert.AreEqual(first, second, "a restart must not replace the examples people have linked to");
    }

    [TestMethod]
    public async Task AnExampleSurvivesTheSweep()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedExamplesAsync();

        var key = ExampleCatalog.KeyFor("rest-minimal");

        // far enough ahead that everything else would be long gone
        await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow.AddYears(1));

        var status = await fixture.Meta.GetStatusAsync(key);

        Assert.IsTrue(status.Exists, "an example nobody has opened for a year is still wanted");
        Assert.IsTrue(status.Deployed, "and it should still be answering");
    }

    [TestMethod]
    public void EveryExampleKnowsWhereItIsWorthCalling()
    {
        foreach (var example in ExampleCatalog.All)
        {
            // most of them mount nothing at their own root, so offering the
            // root would answer a visitor with a 404 and teach them the
            // examples are broken
            Assert.IsNotNull(example.TryPath);

            if (!example.Socket)
            {
                Assert.IsNotEmpty(example.TryPath, $"'{example.Id}' has no route worth calling");
            }
        }
    }

}
