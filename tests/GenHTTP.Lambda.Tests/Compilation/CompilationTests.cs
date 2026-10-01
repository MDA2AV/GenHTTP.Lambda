using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Compilation;

/// <summary>
/// What a snippet may and may not do, as decided by the compiler and the code guard.
/// </summary>
[TestClass]
public sealed class CompilationTests
{

    [TestMethod]
    public async Task ModulesAreAvailableWithoutUsings()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync("""
            var service = Inline.Create()
                                .Get(() => new { Answer = 42 });

            return Layout.Create()
                         .Add("answers", service);
            """);

        Assert.IsTrue(outcome.Success, string.Join("; ", outcome.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    [DataRow("var x = Task.FromResult(1).Result;\nreturn Inline.Create();", ".Result")]
    [DataRow("var x = new ValueTask<int>(1).Result;\nreturn Inline.Create();", ".Result of a value task")]
    [DataRow("Task<int>? t = null;\nvar x = t?.Result;\nreturn Inline.Create();", ".Result behind a null check")]
    [DataRow("Task.Delay(1).Wait();\nreturn Inline.Create();", ".Wait()")]
    [DataRow("var x = Task.FromResult(1).GetAwaiter().GetResult();\nreturn Inline.Create();", ".GetAwaiter().GetResult()")]
    [DataRow("var x = Task.FromResult(1).ConfigureAwait(false).GetAwaiter().GetResult();\nreturn Inline.Create();", "a configured awaiter")]
    [DataRow("Task.WaitAll(Task.Delay(1));\nreturn Inline.Create();", "Task.WaitAll")]
    [DataRow("var s = new SemaphoreSlim(1);\ns.Wait();\nreturn Inline.Create();", "a semaphore taken synchronously")]
    [DataRow("return Inline.Create().Get(() => Load().Result);\n\nstatic async Task<string> Load() { await Task.Yield(); return \"x\"; }", "in a route")]
    public async Task WaitingForATaskIsRefused(string code, string how)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync(code);

        Assert.IsFalse(outcome.Success, how);
        Assert.IsTrue(outcome.Diagnostics.Any(d => (d.Message.Contains("await", StringComparison.OrdinalIgnoreCase) && d.Message.Contains("one thread per core", StringComparison.Ordinal)) || d.Message.Contains("WaitAsync", StringComparison.Ordinal)), $"{how}: refused for waiting, saying what to do instead");
    }

    [TestMethod]
    public async Task AwaitingAndAResultOfItsOwnAreFine()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync("""
            var gate = new SemaphoreSlim(1);

            return Inline.Create().Get(async () =>
            {
                await gate.WaitAsync();

                try
                {
                    await Task.Delay(1);
                    return new Outcome(42).Result;
                }
                finally
                {
                    gate.Release();
                }
            });

            record Outcome(int Result);
            """);

        Assert.IsTrue(outcome.Success, string.Join("; ", outcome.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    public async Task SyntaxErrorsPointAtTheirLine()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync("""
            var greeting = "hello";
            return Content.From(Resource.FromString(greeting)
            """);

        Assert.IsFalse(outcome.Success);
        Assert.IsNotEmpty(outcome.Diagnostics);

        Assert.IsTrue(outcome.Diagnostics.All(d => d.Line > 0), "diagnostics are reported against the code of the user");
    }

    [TestMethod]
    public async Task SnippetsHaveToReturnAHandler()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = fixture.Meta.Create(null);

        // what the snippet returns is only known once it ran, so this is caught
        // when the lambda is deployed rather than when its code is checked
        fixture.Meta.Save(lambda.PrivateKey, "return 42;");

        var deployment = await fixture.Meta.DeployAsync(lambda.PrivateKey, null);

        Assert.IsFalse(deployment.Success);

        Assert.Contains("IHandler", string.Join(" ", deployment.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    [DataRow("return Content.From(Resource.FromString(File.ReadAllText(\"/etc/passwd\")));", "the file system of the host")]
    [DataRow("System.Diagnostics.Process.Start(\"sh\"); return Content.From(Resource.FromString(\"x\"));", "other processes")]
    [DataRow("var type = typeof(object).Assembly; return Content.From(Resource.FromString(\"x\"));", "reflection")]
    [DataRow("Environment.Exit(1); return Content.From(Resource.FromString(\"x\"));", "the lifetime of the host")]
    public async Task TheHostIsOffLimits(string code, string because)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync(code);

        Assert.IsFalse(outcome.Success, $"a lambda must not reach for {because}");
        Assert.IsNotEmpty(outcome.Diagnostics);
    }

    [TestMethod]
    [DataRow("System.IO.Path.Combine(\"a\", \"b\"); return Content.From(Resource.FromString(\"x\"));",
             "reached through its namespace")]
    [DataRow("var p = Path.GetTempPath(); return Content.From(Resource.FromString(p));",
             "written bare")]
    public async Task ABannedTypeIsStillRefusedHoweverItIsWritten(string code, string how)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync(code);

        Assert.IsFalse(outcome.Success, $"a banned type {how} is still that type");
    }

    [TestMethod]
    public async Task OutboundNetworkIsAllowed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // HttpClient and sockets used to be refused; a lambda may now reach the
        // network directly. This only has to compile.
        var outcome = await fixture.Deployments.ValidateAsync("""
            using System.Net.Http;
            using System.Net.Sockets;

            var http = new HttpClient();
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            return Content.From(Resource.FromString("networking is available"));
            """);

        Assert.IsTrue(outcome.Success, string.Join("; ", outcome.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    public async Task AMemberThatSharesItsNameWithABannedTypeIsAllowed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // the guard reads what was written rather than what it means, and a
        // handler has every reason to ask a request for its path
        var outcome = await fixture.Deployments.ValidateAsync(
            "return Inline.Create().Get((IRequest request) => new Seen(request.Header.Path.ToString()));\n\nrecord Seen(string Path);");

        Assert.IsTrue(outcome.Success, string.Join("; ", outcome.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    public async Task StorageGoesThroughTheWorkspace()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = fixture.Meta.Create(null);

        fixture.Meta.Save(lambda.PrivateKey, """
            Workspace.WriteText("note.txt", "kept");

            return Content.From(Resource.FromString(Workspace.ReadText("note.txt")));
            """);

        var deployment = await fixture.Meta.DeployAsync(lambda.PrivateKey, null);

        Assert.IsTrue(deployment.Success, string.Join("; ", deployment.Diagnostics.Select(d => d.Message)));

        using var response = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual("kept", await response.GetContentAsync());

        var written = Directory.GetFiles(fixture.Options.WorkspaceDirectory, "note.txt", SearchOption.AllDirectories);

        Assert.HasCount(1, written, "the file stays inside the workspace of the lambda");
    }

    [TestMethod]
    public async Task WorkspaceCanBeUsedFromEveryFile()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = fixture.Meta.Create(null);

        // a store in a file of its own is the first thing an agent writes, and
        // Workspace used to exist only in the top-level code of lambda.cs - so
        // this failed to compile, and took a whole build down with it
        await fixture.DeployAsync(lambda.PrivateKey, LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, """
                new Note().Keep("from a type in lambda.cs");

                return Content.From(Resource.FromString(Store.Read()));

                class Note
                {
                    public void Keep(string text) => Store.Write(text);
                }
                """),
            new LambdaFile("Store.cs", """
                public static class Store
                {
                    public static void Write(string text) => Workspace.WriteText("kept.txt", text);

                    public static string Read() => Workspace.ReadText("kept.txt");
                }
                """)
        ]));

        using var response = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual("from a type in lambda.cs", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task AssetsOutsideTheSnippetSaysWhereTheyAre()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // in another file Assets is the Files module's type of that name, which
        // has no ReadText - the message has to say what to write instead
        var outcome = await fixture.Deployments.ValidateAsync(LambdaSource.Serialize([
            new LambdaFile(LambdaSource.EntryName, "return Content.From(Resource.FromString(Shipped.Text()));"),
            new LambdaFile("Shipped.cs", """
                public static class Shipped
                {
                    public static string Text() => Assets.ReadText("notes.txt");
                }
                """)
        ]));

        Assert.IsFalse(outcome.Success);

        Assert.Contains("LambdaEnvironment.Assets", string.Join(" ", outcome.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    public async Task EveryLambdaCompilesIntoItsOwnNamespace()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // both lambdas declare a type of the same name, which would collide if
        // they were compiled into the same namespace
        const string Code = """
            return Inline.Create().Get(() => new Entry("shared name"));

            record Entry(string Name);
            """;

        var first = fixture.Meta.Create(null);
        var second = fixture.Meta.Create(null);

        foreach (var lambda in new[] { first, second })
        {
            fixture.Meta.Save(lambda.PrivateKey, Code);

            var deployment = await fixture.Meta.DeployAsync(lambda.PrivateKey, null);

            Assert.IsTrue(deployment.Success, string.Join("; ", deployment.Diagnostics.Select(d => d.Message)));
        }
    }

}
