using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// A lambda split across several files.
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
    [DataRow("Greeter.cs", true)]
    [DataRow("my-types_2.cs", true)]
    public void OnlyUsableNamesAreAccepted(string name, bool usable)
        => Assert.AreEqual(usable, LambdaSource.IsValidName(name));

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
    [DataRow("www/app.css", true)]
    [DataRow("index.html", true)]
    [DataRow("img/logo-2.png", true)]
    [DataRow("a/b/c/d/e/f/g.css", false)]
    [DataRow("../escape.css", false)]
    [DataRow("/leading.css", false)]
    [DataRow("no-extension", false)]
    [DataRow(".hidden.css", false)]
    [DataRow("with space.css", false)]
    public void OnlyUsableAssetNamesAreAccepted(string name, bool usable)
        => Assert.AreEqual(usable, LambdaSource.IsValidAssetName(name));

    [TestMethod]
    public void AnAssetCostsNoneOfTheCodeBudget()
    {
        LambdaFile[] files =
        [
            new(LambdaSource.EntryName, "return null;"),
            new("www/app.css", new string('x', 5000))
        ];

        Assert.AreEqual("return null;".Length, LambdaSource.Length(files), "the code budget counts code");
        Assert.AreEqual(5000, LambdaSource.AssetBytes(files), "and the assets are counted on their own");
    }

    [TestMethod]
    public void BinaryAssetsArriveAsTheBytesTheyWere()
    {
        byte[] bytes = [0x89, 0x50, 0x4E, 0x47, 0x00, 0xFF];

        var file = new LambdaFile("logo.png", Convert.ToBase64String(bytes), "base64");

        CollectionAssert.AreEqual(bytes, file.Bytes);
        Assert.AreEqual(bytes.Length, LambdaSource.AssetBytes([file]));
    }

    [TestMethod]
    public void Base64ThatIsNotBase64IsRefused()
        => Assert.IsNotNull(LambdaSource.Validate([
               new LambdaFile(LambdaSource.EntryName, "return null;"),
               new LambdaFile("logo.png", "not base64 at all !!", "base64")
           ]));

    [TestMethod]
    public async Task AnAssetIsShippedAndServedWithoutBeingCompiled()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("shipper");

        var code = LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Assets.Files();"),
            // deliberately not valid C#: an asset must never reach the compiler
            new LambdaFile("app.css", "body { margin: 0 } /* if this compiled it would not */"),
            new LambdaFile("index.html", "<!doctype html><title>shipped</title>")
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
    public async Task AnAssetDroppedFromAVersionStopsBeingServed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("forgetful");

        await fixture.DeployAsync(lambda.PrivateKey, LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Assets.Files();"),
            new LambdaFile("gone.css", "body { margin: 0 }")
        ]));

        await fixture.DeployAsync(lambda.PrivateKey, LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Assets.Files();"),
            new LambdaFile("kept.css", "body { margin: 1px }")
        ]));

        using var gone = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/gone.css");
        using var kept = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/kept.css");

        Assert.AreEqual(HttpStatusCode.NotFound, gone.StatusCode, "a version ships what it ships, not what the last one did");
        Assert.AreEqual(HttpStatusCode.OK, kept.StatusCode);
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
