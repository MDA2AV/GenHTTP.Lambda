using System.Diagnostics;
using System.IO.Compression;
using System.Net;

using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// Taking a lambda away with you.
/// </summary>
[TestClass]
public sealed class ProjectPackerTests
{
    private static readonly ExportedLambda Lambda = new("my-lambda", 7, new DateTime(2026, 9, 12), "Adds a search box\nto the list", "https://genhttp.dev/lambda/my-lambda/", new DateTime(2026, 9, 30));

    private static readonly IReadOnlyList<LambdaFile> Files =
    [
        new("lambda.cs", """
            // the shelf everything is served from
            var shelf = new Shelf();

            return Layout.Create()
                         .Add("books", Inline.Create().Get(() => shelf.All()))
                         .Add(Assets.App("site"));

            record Book(string Title);
            """),
        new("shelf.cs", """
            public sealed class Shelf
            {
                private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

                public IEnumerable<string> All() => ["one", "two"];
            }
            """),
        new("site/index.html", "<!doctype html><title>x</title><h1>hello</h1>"),
        new("site/dot.gif", "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7", "base64")
    ];

    #region Layout

    [TestMethod]
    public void TheProjectIsLaidOutTheDotNetWay()
    {
        var names = Names(ProjectPacker.Pack(Lambda, Files));

        foreach (var wanted in (string[])
                 ["my-lambda/my-lambda.csproj", "my-lambda/Program.cs", "my-lambda/Project.cs", "my-lambda/Shelf.cs",
                  "my-lambda/Platform/Usings.cs", "my-lambda/Platform/LambdaEnvironment.cs", "my-lambda/Platform/Folder.cs",
                  "my-lambda/Platform/Handlers.cs", "my-lambda/Dockerfile",
                  "my-lambda/assets/site/index.html", "my-lambda/assets/site/dot.gif"])
        {
            Assert.Contains(wanted, names);
        }

        Assert.DoesNotContain("my-lambda/lambda.cs", names, "the snippet is Project.cs, not a file beside it");
        Assert.DoesNotContain("my-lambda/shelf.cs", names, "code files are named the .NET way");
    }

    [TestMethod]
    public void ItReferencesTheGenHttpTheServerRunsWithTheInternalEngine()
    {
        var project = Read(ProjectPacker.Pack(Lambda, Files), "my-lambda/my-lambda.csproj");

        var version = typeof(IHandler).Assembly.GetName().Version!;

        Assert.Contains($"""<PackageReference Include="GenHTTP.Full" Version="{version.Major}.{version.Minor}.{version.Build}" />""", project);
        Assert.Contains("<TargetFramework>net10.0</TargetFramework>", project);
    }

    [TestMethod]
    public void ProgramHostsTheProjectAndSaysWhereItCameFrom()
    {
        var program = Read(ProjectPacker.Pack(Lambda, Files), "my-lambda/Program.cs");

        Assert.Contains(".Handler(Project.Create())", program);
        Assert.Contains(".Defaults()", program);
        Assert.Contains(".RunAsync()", program);
        Assert.DoesNotContain(".Port(", program, "the default port is what the documentation says");
        Assert.DoesNotContain("Console.", program, "the server logs for itself");

        Assert.Contains("https://genhttp.dev", program);
        Assert.Contains("https://genhttp.org/documentation/", program);
        Assert.Contains("my-lambda (https://genhttp.dev/lambda/my-lambda/)", program);
        Assert.Contains("7, saved 2026-09-12", program);
        Assert.Contains("Adds a search box to the list", program, "a change is one line, whatever it was written as");
        Assert.Contains("2026-09-30", program);
    }

    #endregion

    #region Snippet

    [TestMethod]
    public void TheSnippetIsTheBodyOfProjectAndItsTypesSitBesideIt()
    {
        var project = Read(ProjectPacker.Pack(Lambda, Files), "my-lambda/Project.cs");

        Assert.Contains("public static IHandler Create() => Platform.Handlers.From(Build());", project);
        Assert.Contains("        // the shelf everything is served from\n        var shelf = new Shelf();", project,
                        "the statements keep their comments and move into the method");

        // GenHTTP generates the code invoking a handler into an assembly of its
        // own, so a type in a handler's signature has to be public
        var type = project.IndexOf("public record Book(string Title);", StringComparison.Ordinal);

        Assert.IsGreaterThan(project.IndexOf("\n}", StringComparison.Ordinal), type, "the record is declared outside the class");
    }

    [TestMethod]
    public void ASnippetThatWaitsIsCreatedAsynchronously()
    {
        IReadOnlyList<LambdaFile> files =
        [
            new("lambda.cs", """
                await Task.Delay(1);

                return Content.From(Resource.FromString("x"));
                """)
        ];

        var zip = ProjectPacker.Pack(Lambda, files);

        Assert.Contains(".Handler(await Project.CreateAsync())", Read(zip, "my-lambda/Program.cs"));
        Assert.Contains("private static async Task<object> BuildAsync()", Read(zip, "my-lambda/Project.cs"));
    }

    [TestMethod]
    public void AStringSpanningLinesKeepsItsWhitespace()
    {
        IReadOnlyList<LambdaFile> files =
        [
            new("lambda.cs", "var text = @\"one\n  two\";\n\nreturn Content.From(Resource.FromString(text));")
        ];

        var project = Read(ProjectPacker.Pack(Lambda, files), "my-lambda/Project.cs");

        Assert.Contains("        var text = @\"one\n  two\";", project);
    }

    #endregion

    #region Files

    [TestMethod]
    public void AnAssetKeepsItsBytes()
    {
        using var zip = new ZipArchive(new MemoryStream(ProjectPacker.Pack(Lambda, Files)));

        using var stream = zip.GetEntry("my-lambda/assets/site/dot.gif")!.Open();
        using var buffer = new MemoryStream();

        stream.CopyTo(buffer);

        var gif = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

        CollectionAssert.AreEqual(gif, buffer.ToArray(), "a base64 asset is written out as the bytes it stood for");
    }

    [TestMethod]
    public void AKeyThatIsNotAnIdentifierStillNamesAProject()
    {
        var names = Names(ProjectPacker.Pack(Lambda with { PublicKey = "9lives!" }, Files));

        Assert.IsTrue(names.Any(n => n.EndsWith(".csproj", StringComparison.Ordinal)));
        Assert.IsTrue(names.All(n => !n.Contains('!')));
    }

    [TestMethod]
    public void OtherFilesSeeWhatThePlatformImportedForThem()
    {
        var usings = Read(ProjectPacker.Pack(Lambda, Files), "my-lambda/Platform/Usings.cs");

        // Shelf.cs names JsonSerializerOptions without a using, as it could on the platform
        Assert.Contains("global using System.Text.Json;", usings);
        Assert.Contains("global using GenHTTP.Modules.Layouting;", usings);
        Assert.Contains("global using static Platform.LambdaScope;", usings);
    }

    #endregion

    #region Acceptance

    [TestMethod]
    public async Task TheEditorCanFetchItAsAZip()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("takeaway");

        using var answer = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export");

        Assert.AreEqual(HttpStatusCode.OK, answer.StatusCode);
        Assert.AreEqual("application/zip", answer.Content.Headers.ContentType?.MediaType);

        var disposition = answer.Content.Headers.GetValues("Content-Disposition").Single();

        Assert.Contains("takeaway.zip", disposition, "a browser has to be told what to call it");

        var names = Names(await answer.Content.ReadAsByteArrayAsync());

        Assert.Contains("takeaway/Program.cs", names);
        Assert.Contains("takeaway/takeaway.csproj", names);
    }

    /// <summary>
    /// The one test that matters: what comes out builds.
    /// </summary>
    /// <remarks>
    /// Runs the .NET SDK on every demo, so it takes a while and needs the
    /// package feed - but an export that only looks right is how this broke
    /// before, and the demos use most of what a lambda can.
    /// </remarks>
    [TestMethod]
    public async Task EveryDemoExportsToAProjectThatBuilds()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var builds = new List<Task<(string Demo, int Exit, string Output)>>();

        foreach (var demo in DemoCatalog.All)
        {
            var lambda = await fixture.CreateLambdaAsync($"export-{demo.Id}", demo.Id);

            using var answer = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export");

            Assert.AreEqual(HttpStatusCode.OK, answer.StatusCode);

            var directory = Path.Combine(fixture.Options.DataDirectory, "exports", demo.Id);

            await ZipFile.ExtractToDirectoryAsync(new MemoryStream(await answer.Content.ReadAsByteArrayAsync()), directory);

            builds.Add(BuildAsync(demo.Id, Path.Combine(directory, lambda.PublicKey)));
        }

        foreach (var (demo, exit, output) in await Task.WhenAll(builds))
        {
            Assert.AreEqual(0, exit, $"The export of {demo} does not build:\n{output}");
        }
    }

    private static async Task<(string, int, string)> BuildAsync(string demo, string directory)
    {
        var start = new ProcessStartInfo("dotnet", "build -c Release -nologo -clp:ErrorsOnly")
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(start)!;

        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return (demo, process.ExitCode, await output + await error);
    }

    #endregion

    #region Helpers

    private static List<string> Names(byte[] zip)
    {
        using var archive = new ZipArchive(new MemoryStream(zip));

        return archive.Entries.Select(e => e.FullName).ToList();
    }

    private static string Read(byte[] zip, string name)
    {
        using var archive = new ZipArchive(new MemoryStream(zip));

        using var stream = archive.GetEntry(name)!.Open();
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    #endregion

}
