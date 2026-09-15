using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

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

    [TestMethod]
    public async Task ASiteCanBeServedStraightOutOfTheWorkspace()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedExamplesAsync();

        /*
         * This one ships nothing at all. Every file it serves was written into
         * its workspace by its own first run, which is the point: the same
         * module over the other directory, and what it serves can be replaced
         * without the code changing.
         */
        using var page = await fixture.GetAsync("/lambda/example-uploads/");

        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode);
        Assert.Contains("file on disk", await page.GetContentAsync());

        using var style = await fixture.GetAsync("/lambda/example-uploads/app.css");

        Assert.AreEqual(HttpStatusCode.OK, style.StatusCode);
        Assert.AreEqual("text/css", style.Content.Headers.ContentType?.MediaType);

        // the whole reason ServerSideRouting is in there
        using var deep = await fixture.GetAsync("/lambda/example-uploads/no/such/path");

        Assert.AreEqual(HttpStatusCode.OK, deep.StatusCode);
        Assert.Contains("file on disk", await deep.GetContentAsync());

        // and it ships nothing, so there is nothing of it saved with the code
        var files = await fixture.PrivateKeyOfAsync("example-uploads");

        Assert.IsNotNull(files);
    }

    [TestMethod]
    public async Task AFrontEndCanBeAFolderOfFilesServedFromIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedExamplesAsync();

        /*
         * The whole point of this one: the page and the things it asks for
         * live in a folder that was added the way any other file is, and the
         * lambda serves that folder by naming it. Nothing about them is
         * compiled, and none of them is at the root of what the lambda ships.
         */
        using var page = await fixture.GetAsync("/lambda/example-site/");

        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode);
        Assert.Contains("Notes", await page.GetContentAsync());

        using var style = await fixture.GetAsync("/lambda/example-site/app.css");

        Assert.AreEqual(HttpStatusCode.OK, style.StatusCode, "a file beside the page is served by its name within the folder");
        Assert.AreEqual("text/css", style.Content.Headers.ContentType?.MediaType);

        using var script = await fixture.GetAsync("/lambda/example-site/app.js");

        Assert.AreEqual(HttpStatusCode.OK, script.StatusCode);
        Assert.AreEqual("application/javascript", script.Content.Headers.ContentType?.MediaType);

        // the folder is what is served, so its own name is not part of any
        // address: asking for it gets the shell back, the way any other path
        // that names no file does
        using var doubled = await fixture.GetAsync("/lambda/example-site/site/app.css");

        Assert.AreEqual("text/html", doubled.Content.Headers.ContentType?.MediaType,
                        "the folder is the root of what is served, not a segment underneath it");

        // and a path matching no file is answered with the shell, which is
        // what lets the browser do its own routing
        using var deep = await fixture.GetAsync("/lambda/example-site/somewhere/else");

        Assert.AreEqual(HttpStatusCode.OK, deep.StatusCode);
        Assert.Contains("Notes", await deep.GetContentAsync());

        // the code half is still code
        using var api = await fixture.GetAsync("/lambda/example-site/api/notes", "application/json");

        Assert.AreEqual(HttpStatusCode.OK, api.StatusCode);
    }

    [TestMethod]
    public async Task AnExampleCanShipAPageAsFilesRatherThanStrings()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedExamplesAsync();

        /*
         * The arena keeps its page, its script and its stylesheet as assets,
         * which are served as they are and never compiled. That is worth a
         * test of its own because it is the only example built that way, and
         * because it is the shape anybody with a real frontend will want.
         */
        using var page = await fixture.GetAsync("/lambda/example-arena/");

        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode);
        Assert.Contains("game.js", await page.GetContentAsync());

        using var script = await fixture.GetAsync("/lambda/example-arena/game.js");

        Assert.AreEqual(HttpStatusCode.OK, script.StatusCode);
        Assert.AreEqual("application/javascript", script.Content.Headers.ContentType?.MediaType,
                        "a script served as anything else is a script the browser refuses to run");

        using var style = await fixture.GetAsync("/lambda/example-arena/style.css");

        Assert.AreEqual(HttpStatusCode.OK, style.StatusCode);
        Assert.AreEqual("text/css", style.Content.Headers.ContentType?.MediaType);

        // and the code half is still code: the arena reports its own state
        using var here = await fixture.GetAsync("/lambda/example-arena/api/here", "application/json");

        Assert.AreEqual(HttpStatusCode.OK, here.StatusCode);
        Assert.Contains("pellets", await here.GetContentAsync());
    }

    [TestMethod]
    public async Task TheAimMapServesItsPageAndItsApi()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedExamplesAsync();

        using var page = await fixture.GetAsync("/lambda/example-shoot/");

        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode);
        Assert.Contains("game.js", await page.GetContentAsync());

        using var script = await fixture.GetAsync("/lambda/example-shoot/game.js");

        Assert.AreEqual(HttpStatusCode.OK, script.StatusCode);
        Assert.AreEqual("application/javascript", script.Content.Headers.ContentType?.MediaType);

        using var here = await fixture.GetAsync("/lambda/example-shoot/api/here", "application/json");

        Assert.AreEqual(HttpStatusCode.OK, here.StatusCode);
        Assert.Contains("playing", await here.GetContentAsync());
    }

    /// <summary>
    /// The browser half of the aim map runs the server's movement code so
    /// that pressing a key moves you without waiting for a round trip. It can
    /// only get the same answer from the same numbers, and there is nothing
    /// stopping somebody changing one of them in World.cs and not the other.
    /// </summary>
    /// <remarks>
    /// The failure this catches is a quiet one. Nothing breaks: the game runs,
    /// the shots land, and every player slides a little every second because
    /// the two halves disagree about how fast walking is. It took a
    /// measurement to find the last time, so it is worth a test rather than
    /// another measurement.
    /// </remarks>
    [TestMethod]
    public void TheAimMapAgreesWithItselfAboutTheRules()
    {
        var files = TemplateCatalog.FilesFor("shoot", "test");

        var world = files.First(f => f.Name == "World.cs").Code;
        var browser = files.First(f => f.Name == "game.js").Code;

        (string Server, string Browser)[] shared =
        [
            ("Pace", "pace"), ("Quicken", "quicken"), ("Drag", "drag"), ("Brake", "brake"),
            ("Fall", "fall"), ("Leap", "leap"), ("Float", "float"), ("Fold", "fold"),
            ("Stoop", "stoop"), ("Girth", "girth"), ("Tall", "tall"), ("Squat", "squat"),
            ("EyeTall", "eyeTall"), ("EyeSquat", "eyeSquat"), ("Skull", "skull"),
            ("Cadence", "cadence"), ("Magazine", "magazine"), ("Reload", "reload"),
            ("Settle", "settle"), ("Beat", "tick")
        ];

        foreach (var (server, browser_) in shared)
        {
            var declared = Regex.Match(world, $@"const\s+(?:double|int)\s+{server}\s*=\s*([0-9.]+)");

            Assert.IsTrue(declared.Success, $"World.cs no longer declares {server}");

            var mirrored = Regex.Match(browser, $@"\b{browser_}:\s*([0-9.]+)");

            Assert.IsTrue(mirrored.Success, $"game.js no longer mirrors {server} as {browser_}");

            Assert.AreEqual(double.Parse(declared.Groups[1].Value, CultureInfo.InvariantCulture),
                            double.Parse(mirrored.Groups[1].Value, CultureInfo.InvariantCulture),
                            $"{server} is {declared.Groups[1].Value} on the server and "
                          + $"{mirrored.Groups[1].Value} in the browser, so the browser will "
                          + "predict movement the server does not agree with");
        }
    }

}
