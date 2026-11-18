using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Integrations.Sqlite;

namespace GenHTTP.Lambda.Tests.Compilation;

/// <summary>
/// Following a name to where it was written.
/// </summary>
[TestClass]
public sealed class DefinitionTests
{
    private static readonly IReadOnlyList<LambdaFile> Files =
    [
        new("lambda.cs", """
            var greeter = new Greeter();

            return Inline.Create().Get(() => greeter.Greet("world"));
            """),
        new("Greeter.cs", """
            public sealed class Greeter
            {
                public string Greet(string who) => $"hello {who}";
            }
            """)
    ];

    [TestMethod]
    public void AClassUsedInTheSnippetIsFoundInTheFileItWasWrittenIn()
    {
        // "new Greeter()" on the first line of the snippet
        var at = DefinitionResolver.Resolve(Files, "lambda.cs", 0, "var greeter = new ".Length, SqliteDatabase.Binding);

        Assert.IsNotNull(at, "a class the lambda declares has somewhere to go");
        Assert.AreEqual("Greeter.cs", at.File);
        Assert.AreEqual(0, at.Line, "the class is declared on the first line of its file");
    }

    [TestMethod]
    public void AMethodIsFoundOnItsOwnLineRatherThanAtTheTopOfItsType()
    {
        // "greeter.Greet(" on the third line of the snippet
        var at = DefinitionResolver.Resolve(Files, "lambda.cs", 2, """
            return Inline.Create().Get(() => greeter.
            """.TrimEnd().Length, SqliteDatabase.Binding);

        Assert.IsNotNull(at);
        Assert.AreEqual("Greeter.cs", at.File);
        Assert.AreEqual(2, at.Line, "the method, not the class that holds it");
    }

    [TestMethod]
    public void SomethingFromTheFrameworkHasNowhereToGo()
    {
        // "Inline" is the platform's, not the lambda's
        var at = DefinitionResolver.Resolve(Files, "lambda.cs", 2, "return ".Length, SqliteDatabase.Binding);

        Assert.IsNull(at, "there is no file of this lambda to open, so it says so");
    }

    [TestMethod]
    public void ALocalIsFoundWhereItWasDeclared()
    {
        var at = DefinitionResolver.Resolve(Files, "lambda.cs", 2, """
            return Inline.Create().Get(() => gr
            """.TrimEnd().Length - 2, SqliteDatabase.Binding);

        Assert.IsNotNull(at);
        Assert.AreEqual("lambda.cs", at.File);
        Assert.AreEqual(0, at.Line, "declared on the first line of the snippet");
    }

    [TestMethod]
    public void NothingUnderTheCaretIsNotAnError()
    {
        Assert.IsNull(DefinitionResolver.Resolve(Files, "lambda.cs", 1, 0, SqliteDatabase.Binding));
        Assert.IsNull(DefinitionResolver.Resolve(Files, "nope.cs", 0, 0, SqliteDatabase.Binding));
        Assert.IsNull(DefinitionResolver.Resolve([], "lambda.cs", 0, 0, SqliteDatabase.Binding));
    }
}
