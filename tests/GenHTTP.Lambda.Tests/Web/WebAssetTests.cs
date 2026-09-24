using System.Net;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Web;

/// <summary>
/// How the frontend is served: the bundle's own files, and what a browser is
/// allowed to remember about them.
/// </summary>
[TestClass]
public sealed class WebAssetTests
{

    private const string Bundle = "index-BwtdxquV.js";

    /// <summary>
    /// A web root with a built bundle in it, the way a deployment looks.
    /// </summary>
    private static LambdaOptions WithBundle(LambdaOptions options)
    {
        var assets = Path.Combine(options.WebRoot, "assets");

        Directory.CreateDirectory(assets);

        File.WriteAllText(Path.Combine(assets, Bundle), "export const answer = 42;\n");

        return options;
    }

    /// <summary>
    /// The failure this guards against: a browser holding a stale index page
    /// asks for the bundle it was built with, that file is long gone, and server
    /// side routing hands it the index page with a 200 and a text/html type. The
    /// browser then parses markup as script, throws on the first angle bracket
    /// and never starts the application - with no failed request to show for it.
    /// </summary>
    [TestMethod]
    public async Task AMissingBundleIsNotFoundRatherThanTheIndexPage()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithBundle);

        using var response = await fixture.GetAsync("/assets/index-STALEHASH.js");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.IsFalse(body.Contains("<!doctype html", StringComparison.OrdinalIgnoreCase),
                       "a missing bundle was answered with the index page");
    }

    [TestMethod]
    public async Task TheBundleIsServedAndMayBeKeptForever()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithBundle);

        using var response = await fixture.GetAsync($"/assets/{Bundle}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("export const answer = 42;\n", await response.Content.ReadAsStringAsync());

        var cacheControl = response.Headers.CacheControl?.ToString() ?? "";

        StringAssert.Contains(cacheControl, "immutable");
        StringAssert.Contains(cacheControl, "max-age=31536000");
    }

    /// <summary>
    /// The index page may be reused, but only after asking - otherwise it
    /// outlives the bundle it names.
    /// </summary>
    [TestMethod]
    public async Task TheIndexPageIsRevalidated()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithBundle);

        using var response = await fixture.GetAsync("/");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(true, response.Headers.CacheControl?.NoCache);
    }

    /// <summary>
    /// Routes the client side router owns still resolve to the application, so
    /// narrowing the fallback did not break deep links into the editor.
    /// </summary>
    [TestMethod]
    public async Task AClientRouteStillReachesTheApplication()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithBundle);

        var lambda = await fixture.CreateLambdaAsync("deep-link");

        using var response = await fixture.GetAsync(lambda.EditorPath, accept: "text/html");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        StringAssert.Contains(body, "<div id=\"app\">");
    }

    /// <summary>
    /// The front page plays a video, and Safari will not play one from a server
    /// that answers a range with the whole file.
    /// </summary>
    [TestMethod]
    public async Task AVideoCanBeReadInRanges()
    {
        await using var fixture = await LambdaFixture.CreateAsync(options =>
        {
            var media = Path.Combine(options.WebRoot, "media");

            Directory.CreateDirectory(media);

            File.WriteAllBytes(Path.Combine(media, "clip.mp4"), Enumerable.Range(0, 1000).Select(i => (byte)i).ToArray());

            return options;
        });

        using var request = fixture.Host.GetRequest("/media/clip.mp4");

        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 99);

        using var response = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.AreEqual(100, (await response.Content.ReadAsByteArrayAsync()).Length);
    }

    /// <summary>
    /// A web root without a built frontend still starts, and the asset route is
    /// simply not there.
    /// </summary>
    [TestMethod]
    public async Task AWebRootWithoutABundleStillServes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }


    [TestMethod]
    public async Task ABinaryAssetSurvivesBeingSavedAgain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("pictures");

        // a one pixel gif, which is bytes no text encoding leaves intact
        var gif = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

        var files = new object[]
        {
            new { name = "lambda.cs", code = "return Layout.Create().Add(Assets.App(\"site\"));" },
            new { name = "site/index.html", code = "<!doctype html><title>x</title><img src=\"dot.gif\">" },
            new { name = "site/dot.gif", code = Convert.ToBase64String(gif), encoding = "base64" }
        };

        using var saved = await fixture.SendAsync(HttpMethod.Post,
            $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new { files });

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.GetContentAsync());

        var first = System.Text.Json.Nodes.JsonNode.Parse(await saved.GetContentAsync())!["version"]!.GetValue<int>();

        /*
         * Read it back the way the editor does, and send exactly that back.
         * The editor used not to model the encoding at all, so it came back
         * without one - and the base64 was then stored as the literal text of
         * itself, which is an image turned into a text file of its own
         * spelling with nothing to say it had happened.
         */
        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/{first}");

        var content = await read.GetContentAsync();

        Assert.Contains("base64", content, "the encoding has to come back out, or it cannot be sent back in");

        Assert.DoesNotContain("\"isCode\"", content, "the shape of a file is what it is, not what can be worked out from it");
        Assert.DoesNotContain("\"bytes\"", content, "and certainly not a second copy of every file");

        var body = System.Text.Json.Nodes.JsonNode.Parse(content)!;

        using var again = await fixture.SendAsync(HttpMethod.Post,
            $"/api/v1/lambdas/{lambda.PrivateKey}/versions",
            new { files = body["files"]!.DeepClone() });

        Assert.AreEqual(HttpStatusCode.Created, again.StatusCode);

        var version = System.Text.Json.Nodes.JsonNode.Parse(await again.GetContentAsync())!["version"]!.GetValue<int>();

        using var deployed = await fixture.SendAsync(HttpMethod.Post,
            $"/api/v1/lambdas/{lambda.PrivateKey}/deployment/start", new { version });

        Assert.AreEqual(HttpStatusCode.OK, deployed.StatusCode, await deployed.GetContentAsync());

        using var served = await fixture.GetAsync("/lambda/pictures/dot.gif");

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode);

        var bytes = await served.Content.ReadAsByteArrayAsync();

        Assert.AreEqual(gif.Length, bytes.Length, "the image is the image, not the letters of its base64");
        CollectionAssert.AreEqual(gif, bytes);
    }


    [TestMethod]
    public async Task AFrontEndCanLiveInTheWorkspaceInstead()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("uploaded");

        /*
         * The other answer to where a page goes. Nothing is shipped with the
         * code at all: the lambda serves whatever is in its workspace, which
         * somebody uploads and can change again without redeploying.
         */
        await fixture.DeployAsync(lambda.PrivateKey,
            "return Layout.Create().Add(Workspace.App());");

        using var before = await fixture.GetAsync("/lambda/uploaded/");

        Assert.AreNotEqual(HttpStatusCode.InternalServerError, before.StatusCode,
                           "an empty workspace is not an error, just an empty site");

        using var written = await fixture.SendAsync(HttpMethod.Put,
            $"/api/v1/lambdas/{lambda.PrivateKey}/files/index.html",
            new { content = Convert.ToBase64String("<!doctype html><title>up</title><h1>uploaded</h1>"u8.ToArray()) });

        Assert.AreEqual(HttpStatusCode.OK, written.StatusCode);

        // no redeploy: the files are read where they lie
        using var served = await fixture.GetAsync("/lambda/uploaded/");

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode);
        Assert.Contains("uploaded", await served.GetContentAsync());
    }

}
