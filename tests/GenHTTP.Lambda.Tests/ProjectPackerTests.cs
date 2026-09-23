using System.IO.Compression;

using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// Taking a lambda away with you.
/// </summary>
[TestClass]
public sealed class ProjectPackerTests
{
    private static readonly IReadOnlyList<LambdaFile> Files =
    [
        new("lambda.cs", """
            var shelf = new Shelf();

            return Layout.Create()
                         .Add("books", Inline.Create().Get(() => shelf.All()))
                         .Add(Assets.App("site"));

            record Book(string Title);
            """),
        new("Shelf.cs", """
            public sealed class Shelf
            {
                public IEnumerable<string> All() => ["one", "two"];
            }
            """),
        new("site/index.html", "<!doctype html><title>x</title><h1>hello</h1>"),
        new("site/dot.gif", "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7", "base64")
    ];

    [TestMethod]
    public void EverythingNeededToOpenItIsInThere()
    {
        using var zip = new ZipArchive(new MemoryStream(ProjectPacker.Pack("my-lambda", Files)));

        var names = zip.Entries.Select(e => e.FullName).ToList();

        foreach (var wanted in (string[])
                 ["my-lambda/my-lambda.csproj", "my-lambda/Program.cs", "my-lambda/Lambda.cs",
                  "my-lambda/README.md", "my-lambda/Shelf.cs",
                  "my-lambda/assets/site/index.html", "my-lambda/assets/site/dot.gif"])
        {
            Assert.Contains(wanted, names);
        }

        Assert.DoesNotContain("my-lambda/lambda.cs", names,
                              "the snippet is the body of Program.cs, not a file beside it");
    }

    [TestMethod]
    public void TheSnippetBecomesAProgramThatHostsWhatItReturns()
    {
        using var zip = new ZipArchive(new MemoryStream(ProjectPacker.Pack("my-lambda", Files)));

        var program = Read(zip, "my-lambda/Program.cs");

        Assert.Contains("Host.Create()", program, "it has to host something");
        Assert.Contains("shelf.All()", program, "the snippet has to be in there");

        /*
         * A type declared at the end of a snippet cannot go inside a method,
         * so it has to come out and sit beside it - which is the one thing
         * about this that is not simply copying text across.
         */
        var body = program[program.IndexOf("BuildAsync", StringComparison.Ordinal)..];

        var inMethod = body.IndexOf("record Book", StringComparison.Ordinal);
        var closes = body.IndexOf("\n}", StringComparison.Ordinal);

        Assert.IsTrue(inMethod > closes, "the record has to be outside the method, not inside it");
    }

    [TestMethod]
    public void AnAssetKeepsItsBytes()
    {
        using var zip = new ZipArchive(new MemoryStream(ProjectPacker.Pack("my-lambda", Files)));

        var entry = zip.GetEntry("my-lambda/assets/site/dot.gif")!;

        using var stream = entry.Open();
        using var buffer = new MemoryStream();

        stream.CopyTo(buffer);

        var gif = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

        CollectionAssert.AreEqual(gif, buffer.ToArray(), "a base64 asset is written out as the bytes it stood for");
    }

    [TestMethod]
    public void AKeyThatIsNotAnIdentifierStillNamesAProject()
    {
        using var zip = new ZipArchive(new MemoryStream(ProjectPacker.Pack("9lives!", Files)));

        Assert.IsTrue(zip.Entries.Any(e => e.FullName.EndsWith(".csproj", StringComparison.Ordinal)));
        Assert.IsTrue(zip.Entries.All(e => !e.FullName.Contains('!')));
    }

    private static string Read(ZipArchive zip, string name)
    {
        using var stream = zip.GetEntry(name)!.Open();
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    [TestMethod]
    public async Task TheEditorCanFetchItAsAZip()
    {
        await using var fixture = await Infrastructure.LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("takeaway");

        using var answer = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export");

        Assert.AreEqual(System.Net.HttpStatusCode.OK, answer.StatusCode);
        Assert.AreEqual("application/zip", answer.Content.Headers.ContentType?.MediaType);

        var disposition = answer.Content.Headers.GetValues("Content-Disposition").Single();

        Assert.Contains("takeaway.zip", disposition, "a browser has to be told what to call it");

        using var zip = new ZipArchive(new MemoryStream(await answer.Content.ReadAsByteArrayAsync()));

        Assert.IsTrue(zip.Entries.Any(e => e.FullName.EndsWith("Program.cs", StringComparison.Ordinal)));
        Assert.IsTrue(zip.Entries.Any(e => e.FullName.EndsWith(".csproj", StringComparison.Ordinal)));
    }

}
