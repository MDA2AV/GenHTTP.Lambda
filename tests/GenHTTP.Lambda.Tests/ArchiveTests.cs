using System.IO.Compression;
using System.Net;
using System.Text;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// Versions downloaded and uploaded as a zip archive.
/// </summary>
[TestClass]
public sealed class ArchiveTests
{

    [TestMethod]
    public async Task AVersionCanBeDownloadedChangedAndUploadedAgain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var downloaded = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/1/zip");

        Assert.AreEqual(HttpStatusCode.OK, downloaded.StatusCode);
        Assert.AreEqual("application/zip", downloaded.Content.Headers.ContentType?.MediaType);

        var archive = await downloaded.Content.ReadAsByteArrayAsync();

        using (var zip = new ZipArchive(new MemoryStream(archive)))
        {
            Assert.IsNotNull(zip.GetEntry("lambda.cs"), "the files keep the names they have in the lambda");
        }

        var changed = Zip(("lambda.cs", Encoding.UTF8.GetBytes("return Content.From(Resource.FromString(Greeter.Text));")),
                          ("Greeter.cs", Encoding.UTF8.GetBytes("static class Greeter { public const string Text = \"from a zip\"; }")),
                          ("site/logo.gif", [0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x00, 0xFF]));

        using var uploaded = await UploadAsync(fixture, lambda.PrivateKey, changed);

        Assert.AreEqual(HttpStatusCode.Created, uploaded.StatusCode, await uploaded.Content.ReadAsStringAsync());

        var version = await uploaded.GetContentAsync<VersionResponse>();

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/{version.Version}");

        var content = await read.GetContentAsync<VersionContentResponse>();

        CollectionAssert.AreEqual(new[] { "lambda.cs", "Greeter.cs", "site/logo.gif" }, content.Files.Select(f => f.Name).ToArray());

        Assert.AreEqual("base64", content.Files[2].Encoding, "binary assets are kept as base64");

        var deployed = await fixture.DeployAsync(lambda.PrivateKey);

        Assert.IsTrue(deployed.Success);

        using var served = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual("from a zip", await served.GetContentAsync());
    }

    [TestMethod]
    public async Task AWrappingFolderAndHiddenFilesAreIgnored()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var archive = Zip(("project/lambda.cs", Encoding.UTF8.GetBytes("return Content.From(Resource.FromString(\"hi\"));")),
                          ("project/site/app.css", Encoding.UTF8.GetBytes("body { margin: 0 }")),
                          ("project/.git/HEAD", Encoding.UTF8.GetBytes("ref: refs/heads/main")),
                          ("project/.DS_Store", [0x00, 0x01]));

        using var uploaded = await UploadAsync(fixture, lambda.PrivateKey, archive);

        Assert.AreEqual(HttpStatusCode.Created, uploaded.StatusCode, await uploaded.Content.ReadAsStringAsync());

        var version = await uploaded.GetContentAsync<VersionResponse>();

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/{version.Version}");

        var content = await read.GetContentAsync<VersionContentResponse>();

        CollectionAssert.AreEqual(new[] { "lambda.cs", "site/app.css" }, content.Files.Select(f => f.Name).ToArray());

        Assert.IsNull(content.Files[1].Encoding, "text assets stay text");
    }

    [TestMethod]
    public async Task SomethingThatIsNotAZipIsRejected()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var uploaded = await UploadAsync(fixture, lambda.PrivateKey, Encoding.UTF8.GetBytes("not a zip"));

        Assert.AreEqual(HttpStatusCode.BadRequest, uploaded.StatusCode);
    }

    [TestMethod]
    public async Task AnArchiveWithoutTheEntryFileIsRejected()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var uploaded = await UploadAsync(fixture, lambda.PrivateKey, Zip(("Other.cs", Encoding.UTF8.GetBytes("class Other { }"))));

        Assert.AreEqual(HttpStatusCode.BadRequest, uploaded.StatusCode);
    }

    private static async Task<HttpResponseMessage> UploadAsync(LambdaFixture fixture, string privateKey, byte[] archive)
    {
        using var request = fixture.Host.GetRequest($"/api/v1/lambdas/{privateKey}/versions/zip", HttpMethod.Post);

        request.Content = new ByteArrayContent(archive);
        request.Content.Headers.ContentType = new("application/zip");

        return await fixture.Host.GetResponseAsync(request);
    }

    private static byte[] Zip(params (string Name, byte[] Content)[] files)
    {
        using var buffer = new MemoryStream();

        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            foreach (var (name, content) in files)
            {
                using var target = zip.CreateEntry(name).Open();

                target.Write(content);
            }
        }

        return buffer.ToArray();
    }

}
