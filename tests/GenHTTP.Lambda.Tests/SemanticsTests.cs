using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// What the compiler tells the editor about the names in a snippet.
/// </summary>
[TestClass]
public sealed class SemanticsTests
{

    [TestMethod]
    public void NamesAreClassifiedByWhatTheyAre()
    {
        const string Code = """
            var greeting = "hello";

            return Inline.Create().Get((int id) => greeting);
            """;

        var tokens = SemanticClassifier.Classify(Code);

        Assert.AreEqual("local", Find(Code, tokens, 0, "greeting"), "a declared variable");
        Assert.AreEqual("type", Find(Code, tokens, 2, "Inline"), "a type from the module catalogue");
        Assert.AreEqual("method", Find(Code, tokens, 2, "Create"), "a method on it");
        Assert.AreEqual("parameter", Find(Code, tokens, 2, "id"), "a lambda parameter");
    }

    [TestMethod]
    public void PositionsAreInTheCodeTheUserWrote()
    {
        // the compiled file has scaffolding above this line; the editor knows
        // nothing about it and would colour the wrong row if we reported it
        var tokens = SemanticClassifier.Classify("""
            // one
            // two
            var here = 1;
            """);

        var local = tokens.Single(t => t.Kind == "local");

        Assert.AreEqual(2, local.Line, "the third line of the snippet, not of the generated file");
        Assert.AreEqual(4, local.Column);
        Assert.AreEqual(4, local.Length);
    }

    [TestMethod]
    public void ATypeTheUserDeclaresIsKnown()
    {
        const string Code = """
            return Inline.Create().Get(() => new Note("hi"));

            record Note(string Text);
            """;

        var tokens = SemanticClassifier.Classify(Code);

        Assert.AreEqual("type", Find(Code, tokens, 0, "Note"), "used before it is declared");
        Assert.AreEqual("type", Find(Code, tokens, 2, "Note"), "and where it is declared");
    }

    [TestMethod]
    public void CodeThatDoesNotCompileStillGetsWhatCanBeResolved()
    {
        // the editor colours while you type, which is mostly while it is broken
        const string Code = "var x = Inline.Create(";

        var tokens = SemanticClassifier.Classify(Code);

        Assert.IsNotEmpty(tokens);
        Assert.AreEqual("type", Find(Code, tokens, 0, "Inline"));
    }

    [TestMethod]
    public async Task TheEndpointNeedsTheEditorKey()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.SendAsync(HttpMethod.Post, "/api/v1/lambdas/nosuchkey/semantics",
            new CodeRequest("var x = 1;"));

        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task TheEndpointAnswersTheEditor()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/semantics",
            new CodeRequest("return Inline.Create().Get(() => 1);"));

        var semantics = await response.GetContentAsync<SemanticsResponse>();

        Assert.IsNotEmpty(semantics.Tokens);
        Assert.ContainsSingle(semantics.Tokens.Where(t => t.Kind == "type"));
    }

    [TestMethod]
    public void VarIsLeftToTheGrammar()
    {
        const string Code = "var greeting = \"hello\";";

        var tokens = SemanticClassifier.Classify(Code);

        // it resolves to string, and saying so would paint a keyword as a type
        Assert.IsNull(Find(Code, tokens, 0, "var"));
        Assert.AreEqual("local", Find(Code, tokens, 0, "greeting"));
    }

    /// <summary>
    /// The kind of a named token, located by where the name actually sits -
    /// two names on one line are often the same length, and matching on that
    /// finds whichever came first.
    /// </summary>
    private static string? Find(string code, IReadOnlyList<ClassifiedToken> tokens, int line, string name)
    {
        var text = code.ReplaceLineEndings("\n").Split('\n')[line];

        var column = text.IndexOf(name, StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, column, $"'{name}' is not on line {line}");

        return tokens.FirstOrDefault(t => t.Line == line && t.Column == column)?.Kind;
    }

}
