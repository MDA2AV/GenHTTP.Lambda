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

        Assert.AreEqual(ExampleCatalog.All.Count(), offered.Count);

        CollectionAssert.AreEquivalent(new[] { "basic", "advanced" }, listing.Groups.Select(g => g.Id).ToArray(),
                                       "examples are offered by how much there is to take in, not by what they are built from");
    }

    [TestMethod]
    public void AnExampleIsNotJustItsTemplate()
    {
        var starting = TemplateCatalog.Groups.SelectMany(g => g.Templates)
                                      .Where(t => !t.Hidden)
                                      .Select(t => t.Id)
                                      .ToHashSet(StringComparer.Ordinal);

        foreach (var example in ExampleCatalog.All.Where(e => e.Level == ExampleCatalog.Basic))
        {
            // a basic example showing the code the editor is about to hand
            // over teaches nobody anything
            Assert.IsFalse(starting.Contains(example.Id),
                           $"'{example.Id}' is one of the starting points rather than something finished");
        }
    }

    [TestMethod]
    public async Task AnExampleCarriesTheCodeItRuns()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/examples/shortener");

        var example = await response.GetContentAsync<ExampleResponse>();

        Assert.IsNotEmpty(example.Code);
        Assert.Contains("Inline.Create()", example.Code, "the page shows this instead of an editor");
        Assert.IsTrue(example.Files.Count >= 1);
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

        var first = await fixture.PrivateKeyOfAsync(ExampleCatalog.KeyFor("guestbook"));

        await fixture.SeedExamplesAsync();

        var second = await fixture.PrivateKeyOfAsync(ExampleCatalog.KeyFor("guestbook"));

        Assert.IsNotNull(first);
        Assert.AreEqual(first, second, "a restart must not replace the examples people have linked to");
    }

    [TestMethod]
    public async Task AnExampleThatIsNoLongerOneIsRetired()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // a lambda that looks exactly like an example this no longer keeps
        var stale = await fixture.Meta.CreateAsync("example-gone-away");

        await fixture.MarkAsExampleAsync(stale.PublicKey);

        await fixture.SeedExamplesAsync();

        var status = await fixture.Meta.GetStatusAsync("example-gone-away");

        Assert.IsFalse(status.Exists, "an example dropped from the catalogue would otherwise sit on its key for ever");

        // and the ones it does keep are untouched
        Assert.IsTrue((await fixture.Meta.GetStatusAsync(ExampleCatalog.KeyFor("guestbook"))).Exists);
    }

    [TestMethod]
    public async Task AnExampleSurvivesTheSweep()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedExamplesAsync();

        var key = ExampleCatalog.KeyFor("guestbook");

        // far enough ahead that everything else would be long gone
        await fixture.Meta.RunMaintenanceAsync(DateTime.UtcNow.AddYears(1));

        var status = await fixture.Meta.GetStatusAsync(key);

        Assert.IsTrue(status.Exists, "an example nobody has opened for a year is still wanted");
        Assert.IsTrue(status.Deployed, "and it should still be answering");
    }

    [TestMethod]
    public async Task EveryExampleAnswersWhereItSaysItDoes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedExamplesAsync();

        foreach (var example in ExampleCatalog.All.Where(e => !e.Socket))
        {
            /*
             * The route each example offers has to be one it actually answers.
             * Most of them mount nothing at their own root, so a route taken on
             * trust would send visitors to a 404 and teach them the examples
             * are broken - and the two that do serve their root are exactly
             * why this cannot be a rule about the string.
             */
            using var response = await fixture.GetAsync($"/lambda/{example.PublicKey}/{example.TryPath}");

            Assert.AreNotEqual(HttpStatusCode.NotFound, response.StatusCode,
                               $"'{example.Id}' offers a route it does not answer");
        }
    }

}
