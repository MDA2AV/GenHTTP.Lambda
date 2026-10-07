using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// A lambda split across several files: its code, and its resources.
/// </summary>
[TestClass]
public sealed class SourceTests
{

    [TestMethod]
    public void CodeWrittenBeforeThereWereFilesIsStillOneFile()
    {
        var files = LambdaSource.Parse("return Inline.Create().Get(() => \"hello\");");

        Assert.HasCount(1, files);
        Assert.AreEqual(LambdaSource.EntryName, files[0].Name);
        Assert.Contains("Inline.Create()", files[0].Code);
    }

    [TestMethod]
    public void OneFileIsStoredAsItself()
    {
        var stored = LambdaSource.Serialize([new LambdaFile(LambdaSource.EntryName, "return null;")]);

        Assert.AreEqual("return null;", stored, "nothing should be able to tell it apart from what was stored before");
    }

    [TestMethod]
    public void SeveralFilesSurviveTheRoundTrip()
    {
        LambdaFile[] written = [new(LambdaSource.EntryName, "return new Greeter().Handler();"), new("Greeter.cs", "class Greeter { }")];

        var read = LambdaSource.Parse(LambdaSource.Serialize(written));

        Assert.HasCount(2, read);
        Assert.AreEqual("Greeter.cs", read[1].Name);
        Assert.AreEqual("class Greeter { }", read[1].Code);
    }

    [TestMethod]
    [DataRow("", false)]
    [DataRow("Greeter", false)]
    [DataRow("../escape.cs", false)]
    [DataRow("with space.cs", false)]
    [DataRow("has\"quote.cs", false)]
    [DataRow("9lives.cs", false)]
    [DataRow("Greeter..cs", false)]
    [DataRow("Greeter.cs", true)]
    [DataRow("my-types_2.cs", true)]
    [DataRow("Store.Queries.cs", true)]
    public void OnlyUsableNamesAreAccepted(string name, bool usable)
        => Assert.AreEqual(usable, LambdaSource.IsValidName(name));

    [TestMethod]
    [DataRow("models/Item.cs", null)]
    [DataRow("a/b/c/Deep.cs", null)]
    [DataRow("models/9lives.cs", "not a usable name for a C# file")]
    [DataRow("Platform/Mine.cs", "has for its own")]
    [DataRow("resources/samples/odd name.cs", "not a usable name for a resource")]
    public void CSharpIsNamedInAnyFolder(string name, string? complaint)
    {
        var said = LambdaSource.Complaint(name);

        if (complaint == null)
        {
            Assert.IsNull(said, name);
            Assert.IsTrue(LambdaSource.IsCompiled(name), "and it is compiled, as in a C# project");
        }
        else
        {
            Assert.IsNotNull(said, name);
            Assert.Contains(complaint, said);
        }
    }

    [TestMethod]
    public void AVersionOfTheFirstLayoutIsReadInTodays()
    {
        // as a version was stored before the files it serves were resources:
        // those at the root, and what was written about it in .lambda/
        const string stored = """
            {"version":1,"files":[
              {"name":"lambda.cs","code":"return Assets.App(\"web\");"},
              {"name":"store.cs","code":"class Store { }"},
              {"name":"index.html","code":"<p>home</p>"},
              {"name":"web/app.js","code":"go();"},
              {"name":"migrations/V1__Create.sql","code":"CREATE TABLE x (id INTEGER);"},
              {"name":"logo.png","code":"iVBORw==","encoding":"base64"},
              {"name":".lambda/docs/product.md","code":"# What it is"},
              {"name":".lambda/tests/smoke.mjs","code":"check();"},
              {"name":".lambda/tests/Check.cs","code":"not compiled"},
              {"name":"samples/Sample.cs","code":"served, not compiled"},
              {"name":".lambda/build/web/package.json","code":"{}"},
              {"name":".lambda/build/web/.gitignore","code":"node_modules/"}
            ]}
            """;

        var files = LambdaSource.Parse(stored.ReplaceLineEndings(string.Empty));

        CollectionAssert.AreEqual(new[]
        {
            "lambda.cs", "store.cs", "resources/index.html", "resources/web/app.js", "resources/migrations/V1__Create.sql", "resources/logo.png",
            "docs/product.md", "tests/smoke.mjs", "tests/Check.cs.txt", "resources/samples/Sample.cs", "build/web/package.json", "build/web/.gitignore"
        }, files.Select(f => f.Name).ToArray(), "the C# where it was, what it served below resources/, and .lambda/ at the top of the code - "
                                               + "its C#, never compiled, under a name that still is not");

        Assert.AreEqual("iVBORw==", files.Single(f => f.Name == "resources/logo.png").Code, "nothing but the names changes");
        Assert.AreEqual("base64", files.Single(f => f.Name == "resources/logo.png").Encoding);

        Assert.IsNull(LambdaSource.Validate(files), "whatever the first layout held is a valid lambda in today's");

        CollectionAssert.AreEqual(new[] { "lambda.cs", "store.cs" }, files.Where(f => f.IsCompiled).Select(f => f.Name).ToArray(),
                                  "and what was compiled is what is compiled");

        var written = LambdaSource.Serialize(files);

        StringAssert.StartsWith(written, "{\"version\":2,", "written again, it is written in today's layout");
        Assert.IsTrue(LambdaSource.Same(stored.ReplaceLineEndings(string.Empty), written), "and is the same lambda");
    }

    [TestMethod]
    public void TheFirstFileHasToBeTheEntry()
        => Assert.IsNotNull(LambdaSource.Validate([new LambdaFile("Greeter.cs", "class Greeter { }")]));

    [TestMethod]
    public void TwoFilesCannotShareAName()
        => Assert.IsNotNull(LambdaSource.Validate([
               new LambdaFile(LambdaSource.EntryName, ""),
               new LambdaFile("Greeter.cs", ""),
               new LambdaFile("greeter.cs", "")
           ]));

    [TestMethod]
    public async Task ATypeInAnotherFileIsReachableFromTheSnippet()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var code = LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName,
                "return Inline.Create().Get(() => Greeter.Greet(\"world\"));"),
            new LambdaFile("Greeter.cs",
                "static class Greeter\n{\n    public static Welcome Greet(string who) => new Welcome($\"hello {who}\");\n}\n\npublic record Welcome(string Text);")
        ]);

        var outcome = await fixture.Deployments.ValidateAsync(code);

        Assert.IsTrue(outcome.Success, string.Join("; ", outcome.Diagnostics.Select(d => $"{d.File}:{d.Line} {d.Message}")));
    }

    [TestMethod]
    public async Task AMistakeInAnotherFileIsReportedAgainstThatFile()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var code = LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Inline.Create().Get(() => Greeter.Greet());"),
            new LambdaFile("Greeter.cs", "static class Greeter\n{\n    public static string Greet() => nope;\n}")
        ]);

        var outcome = await fixture.Deployments.ValidateAsync(code);

        Assert.IsFalse(outcome.Success);

        var complaint = outcome.Diagnostics.FirstOrDefault(d => d.File == "Greeter.cs");

        Assert.IsNotNull(complaint, "a mistake has to point at the file it is in, not at the snippet");
        Assert.AreEqual(3, complaint.Line, "and at the line it is on in that file");
    }

    [TestMethod]
    public async Task FilesSurviveBeingSavedAndReadBack()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("split");

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new
        {
            files = new[]
            {
                new { name = "lambda.cs", code = "return Inline.Create().Get(() => new Greeting(\"hi\"));" },
                new { name = "Greeting.cs", code = "public record Greeting(string Text);" }
            }
        });

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);

        // creating a lambda already seeds version one from its template, so
        // this is version two and the version has to be read from the answer
        var stored = await saved.GetContentAsync<VersionResponse>();

        using var read = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/{stored.Version}");

        var content = await read.GetContentAsync<VersionContentResponse>();

        Assert.HasCount(2, content.Files);
        Assert.AreEqual("Greeting.cs", content.Files[1].Name);
        Assert.AreEqual("lambda.cs", content.Files[0].Name);
        Assert.Contains("Inline.Create()", content.Files[0].Code, "the snippet comes first");
    }

    [TestMethod]
    public async Task AFileNameThatWouldEscapeIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("nasty");

        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new
        {
            files = new[]
            {
                new { name = "lambda.cs", code = "return null;" },
                new { name = "../../escape.cs", code = "class X { }" }
            }
        });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    [DataRow("resources/www/app.css", true)]
    [DataRow("resources/index.html", true)]
    [DataRow("resources/img/logo-2.png", true)]
    [DataRow("resources/a/b/c/d/e/f/g.css", false)]
    [DataRow("resources/../escape.css", false)]
    [DataRow("resources//leading.css", false)]
    [DataRow("resources/no-extension", false)]
    [DataRow("resources/.hidden.css", false)]
    [DataRow("resources/with space.css", false)]
    [DataRow("www/app.css", false)]
    public void OnlyUsableResourceNamesAreAccepted(string name, bool usable)
        => Assert.AreEqual(usable, LambdaSource.IsValidResourceName(name));

    [TestMethod]
    public void AVersionIsCountedWholeInBytes()
    {
        LambdaFile[] files =
        [
            new(LambdaSource.EntryName, "return null;"),
            new("resources/www/app.css", new string('x', 5000)),
            new("docs/product.md", "é")
        ];

        Assert.AreEqual("return null;".Length + 5000 + 2, LambdaSource.Size(files), "the code and the resources together, as bytes");
    }

    [TestMethod]
    public void BinaryFilesArriveAsTheBytesTheyWere()
    {
        byte[] bytes = [0x89, 0x50, 0x4E, 0x47, 0x00, 0xFF];

        var file = new LambdaFile("resources/logo.png", Convert.ToBase64String(bytes), "base64");

        CollectionAssert.AreEqual(bytes, file.Bytes);
        Assert.AreEqual(bytes.Length, LambdaSource.Size([file]));
    }

    [TestMethod]
    public void Base64ThatIsNotBase64IsRefused()
        => Assert.IsNotNull(LambdaSource.Validate([
               new LambdaFile(LambdaSource.EntryName, "return null;"),
               new LambdaFile("resources/logo.png", "not base64 at all !!", "base64")
           ]));

    [TestMethod]
    public async Task AResourceIsShippedAndServedWithoutBeingCompiled()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("shipper");

        var code = LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Resources.Files();"),
            // deliberately not valid C#: a resource must never reach the compiler
            new LambdaFile("resources/app.css", "body { margin: 0 } /* if this compiled it would not */"),
            new LambdaFile("resources/index.html", "<!doctype html><title>shipped</title>")
        ]);

        var deployment = await fixture.DeployAsync(lambda.PrivateKey, code);

        Assert.IsTrue(deployment.Success, string.Join("; ", deployment.Diagnostics.Select(d => d.Message)));

        using var css = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/app.css");

        Assert.AreEqual(HttpStatusCode.OK, css.StatusCode);
        Assert.Contains("margin: 0", await css.Content.ReadAsStringAsync());
        Assert.Contains("text/css", css.Content.Headers.ContentType?.MediaType ?? "",
                        "the content type comes from the extension");
    }

    [TestMethod]
    public async Task AResourceDroppedFromAVersionStopsBeingServed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("forgetful");

        await fixture.DeployAsync(lambda.PrivateKey, LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Resources.Files();"),
            new LambdaFile("resources/gone.css", "body { margin: 0 }")
        ]));

        await fixture.DeployAsync(lambda.PrivateKey, LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Resources.Files();"),
            new LambdaFile("resources/kept.css", "body { margin: 1px }")
        ]));

        using var gone = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/gone.css");
        using var kept = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/kept.css");

        Assert.AreEqual(HttpStatusCode.NotFound, gone.StatusCode, "a version ships what it ships, not what the last one did");
        Assert.AreEqual(HttpStatusCode.OK, kept.StatusCode);
    }

    [TestMethod]
    public async Task AVersionThatOnlyChangesWhatIsNotCompiledIsServedWithoutBeingCompiledAgain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("restyled");

        await fixture.DeployAsync(lambda.PrivateKey, LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Resources.Files();"),
            new LambdaFile("resources/app.css", "body { color: red }"),
            new LambdaFile("docs/product.md", "# Red")
        ]));

        await fixture.DeployAsync(lambda.PrivateKey, LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Resources.Files();"),
            new LambdaFile("resources/app.css", "body { color: blue }"),
            new LambdaFile("docs/product.md", "# Blue"),
            new LambdaFile("tools/generate.mjs", "a tool's script, which is no part of the program")
        ]));

        using var css = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/app.css");

        Assert.Contains("blue", await css.Content.ReadAsStringAsync(), "the new version serves its own resources");

        // every assembly compiled for a lambda is written beside the others, and
        // stays loaded for as long as the process runs
        var compiled = Directory.GetFiles(fixture.Options.AssemblyDirectory, "*.dll", SearchOption.AllDirectories);

        Assert.HasCount(1, compiled, "the code did not change, so neither does what it compiles to");
    }

    [TestMethod]
    public async Task AHelperNamedAfterABannedTypeIsTheAuthorsOwn()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // the name is banned as a type; as somebody's own helper it is theirs
        var outcome = await fixture.Deployments.ValidateAsync(
            "static string File(string name) => name.Trim();\n\n"
          + "return Inline.Create().Get(() => new Thing(File(\" x \")));\n\nrecord Thing(string Name);");

        Assert.IsTrue(outcome.Success, string.Join("; ", outcome.Diagnostics.Select(d => d.Message)));

        // and the type of that name is still out of reach
        var reaching = await fixture.Deployments.ValidateAsync(
            "return Content.From(Resource.FromString(System.IO.File.ReadAllText(\"/etc/passwd\")));");

        Assert.IsFalse(reaching.Success, "declaring a helper must not open the door to the type it is named after");
    }

    [TestMethod]
    public async Task TheGuardReadsEveryFileRatherThanOnlyTheSnippet()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var code = LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Inline.Create().Get(() => Sneaky.Read());"),
            // reflection is refused in the snippet; moving it must not help
            new LambdaFile("Sneaky.cs", "static class Sneaky\n{\n    public static string Read() => typeof(string).GetType().Name;\n}")
        ]);

        var outcome = await fixture.Deployments.ValidateAsync(code);

        Assert.IsFalse(outcome.Success, "the guard has to look at the files the snippet is split into");
    }

}
