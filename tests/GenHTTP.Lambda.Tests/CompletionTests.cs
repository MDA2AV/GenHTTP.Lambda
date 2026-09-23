using GenHTTP.Lambda.Services.Deployment.Compilation;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// What the editor offers where the caret is.
/// </summary>
[TestClass]
public sealed class CompletionTests
{

    [TestMethod]
    public void MembersOfAGenHttpTypeAreOffered()
    {
        // the case the old flat list could not answer: what follows a dot
        var found = At("return Inline.Create().", 0, 23);

        var labels = found.Select(c => c.Label).ToList();

        Assert.Contains("Get", labels, "the routes a functional handler offers");
        Assert.Contains("Post", labels);
        Assert.Contains("Delete", labels);
        Assert.IsTrue(found.Where(c => c.Label == "Get").All(c => c.Kind == "method"));
    }

    [TestMethod]
    public void StaticMembersAreOfferedOnTheTypeItself()
    {
        var found = At("return Layout.", 0, 14);

        Assert.Contains("Create", found.Select(c => c.Label).ToList(), "Layout.Create is how a layout starts");
    }

    [TestMethod]
    public void MembersOfSomethingTheUserDeclaredAreOffered()
    {
        var found = At("""
            var note = new Note("hi", 1);
            note.

            record Note(string Text, int Number);
            """, 1, 5);

        var labels = found.Select(c => c.Label).ToList();

        Assert.Contains("Text", labels);
        Assert.Contains("Number", labels);
    }

    [TestMethod]
    public void WhatIsInScopeIsOfferedWhereThereIsNoDot()
    {
        var found = At("""
            var greeting = "hello";

            """, 2, 0);

        var labels = found.Select(c => c.Label).ToList();

        Assert.Contains("greeting", labels, "a local the user declared");
        Assert.Contains("Inline", labels, "and the module surface");
        Assert.Contains("Workspace", labels, "and the directory the lambda is given");
    }

    [TestMethod]
    public void NothingTheGuardWouldRejectIsOffered()
    {
        var labels = At("""
            var x = 1;

            """, 2, 0).Select(c => c.Label).ToList();

        Assert.DoesNotContain("File", labels, "suggesting what will not compile is worse than suggesting nothing");
        Assert.DoesNotContain("Process", labels);
        Assert.DoesNotContain("Thread", labels);
    }

    [TestMethod]
    public void TheScaffoldingIsNotOffered()
    {
        var labels = At("var x = 1;\n", 1, 0).Select(c => c.Label).ToList();

        Assert.IsEmpty(labels.Where(l => l.StartsWith("__", StringComparison.Ordinal)),
                       "the generated wrapper is not the user's business");
    }

    [TestMethod]
    public void SignaturesComeWithTheSuggestion()
    {
        var get = At("return Inline.Create().", 0, 23).First(c => c.Label == "Get");

        Assert.IsNotEmpty(get.Detail, "the editor shows this beside the name");
        StringAssert.Contains(get.Detail, "Get");
    }

    private static IReadOnlyList<ResolvedCompletion> At(string code, int line, int column)
        => CompletionResolver.Resolve(code, line, column);

}
