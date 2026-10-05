using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Tests.Git;

/// <summary>
/// The snippet of a lambda as the class Project.cs holds, and back: what a
/// push to its repository changed is what lambda.cs changes.
/// </summary>
[TestClass]
public sealed class ProjectSnippetTests
{

    [TestMethod]
    public void EveryDemoTakenOutOfTheClassIsPutBackAsItWas()
    {
        foreach (var demo in DemoCatalog.All)
        {
            var snippet = LambdaSource.Parse(TemplateCatalog.ForKey(demo.Id, demo.Key, demo: true)).Single(f => f.Name == LambdaSource.EntryName).Code;

            var project = ProjectSnippet.ForRepository(snippet);

            var unwrapped = ProjectSnippet.Unwrap(project);

            Assert.IsNull(unwrapped.Complaint, demo.Id);

            Assert.AreEqual(project, ProjectSnippet.ForRepository(unwrapped.Snippet!), $"{demo.Id}: a class taken out and put back is the class it was");

            Assert.AreEqual(Statements(snippet), Statements(unwrapped.Snippet!), $"{demo.Id}: the statements are what they were");
        }
    }

    [TestMethod]
    public void TheBodyIsMovedBackOutAndAStringKeepsItsOwnSpaces()
    {
        const string project = """"
            using System.Text;

            public static class Project
            {
                public static async Task<IHandler> CreateAsync() => Platform.Handlers.From(await BuildAsync());

                private static async Task<object> BuildAsync()
                {
                    var text = """
                  kept as it is
                  """;

                    if (text.Length > 0)
                    {
                        return Content.From(Resource.FromString(text));
                    }

                    return Content.From(Resource.FromString("empty"));
                }
            }

            public record Note(string Text);
            """";

        var unwrapped = ProjectSnippet.Unwrap(project);

        Assert.AreEqual(""""
            using System.Text;

            var text = """
                  kept as it is
                  """;

            if (text.Length > 0)
            {
                return Content.From(Resource.FromString(text));
            }

            return Content.From(Resource.FromString("empty"));

            public record Note(string Text);

            """", unwrapped.Snippet);

        Assert.AreEqual((9, 9), unwrapped.Locate(3, 1), "the third line of the snippet is the ninth of Project.cs, eight columns further in");
        Assert.AreEqual((22, 1), unwrapped.Locate(14, 1), "and the types are where they were");
    }

    [TestMethod]
    public void ABodyWrittenLessFarInKeepsItsShape()
    {
        var unwrapped = ProjectSnippet.Unwrap("""
            public static class Project
            {
              static async Task<object> BuildAsync()
              {
                if (true)
                {
                  return 1;
                }
              }
            }
            """);

        Assert.AreEqual("if (true)\n{\n  return 1;\n}\n", unwrapped.Snippet);
    }

    [TestMethod]
    public void WhatHasNoPlaceInASnippetIsRefused()
    {
        var member = ProjectSnippet.Unwrap("""
            public static class Project
            {
                private static readonly int Answer = 42;

                private static async Task<object> BuildAsync()
                {
                    return Answer;
                }
            }
            """);

        Assert.Contains("'Answer' cannot stay in the class Project", member.Complaint ?? string.Empty);

        var none = ProjectSnippet.Unwrap("public static class Something { }");

        Assert.Contains("keeps the class Project", none.Complaint ?? string.Empty);
    }

    [TestMethod]
    public void AnExportedProjectIsTakenOutAsWell()
    {
        var unwrapped = ProjectSnippet.Unwrap(ProjectSnippet.ForExport("return Content.From(Resource.FromString(\"x\"));\n"));

        Assert.AreEqual("return Content.From(Resource.FromString(\"x\"));\n", unwrapped.Snippet, "Build(), as the export has it, is the snippet as well");
    }

    /// <summary>
    /// The lines of a snippet that say something, a type's made public or not.
    /// </summary>
    private static string Statements(string snippet)
        => string.Join('\n', snippet.Split('\n')
                                    .Select(l => l.TrimEnd())
                                    .Where(l => l.Length > 0)
                                    .Select(l => l.StartsWith("public ", StringComparison.Ordinal) ? l["public ".Length..] : l));

}
