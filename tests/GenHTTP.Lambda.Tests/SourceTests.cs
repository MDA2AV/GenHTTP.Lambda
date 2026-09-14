using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

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
