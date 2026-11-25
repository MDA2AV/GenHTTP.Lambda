using System.Net;
using System.Runtime.CompilerServices;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// Several installations in one process and behind one server: each is an
/// installation of its own, and none of them notices the others.
/// </summary>
[TestClass]
public sealed class SeveralPlatformsTests
{

    #region Apart from one another

    [TestMethod]
    public async Task EveryInstallationHasItsOwnLambdas()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        await several.StartAsync("one");
        await several.StartAsync("two");

        await several.DeployAsync("one", "quiz", "var asked = 0; return Inline.Create().Get(() => $\"one {++asked}\");");
        await several.DeployAsync("two", "quiz", "var asked = 0; return Inline.Create().Get(() => $\"two {++asked}\");");
        await several.DeployAsync("one", "only-here", "return Inline.Create().Get(() => \"here\");");

        Assert.AreEqual("one 1", await several.AskAsync("one", "quiz"));
        Assert.AreEqual("one 2", await several.AskAsync("one", "quiz"));
        Assert.AreEqual("two 1", await several.AskAsync("two", "quiz"), "the same key is another lambda in another installation");

        Assert.AreEqual("here", await several.AskAsync("one", "only-here"));
        Assert.AreEqual("404", await several.AskAsync("two", "only-here"));
    }

    [TestMethod]
    public async Task TheSameCodeRunsOverTheDataOfItsOwnInstallation()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        await several.StartAsync("one");
        await several.StartAsync("two");

        const string code = """
            return Inline.Create()
                         .Get(() => Workspace.Exists("note.txt") ? Workspace.ReadText("note.txt") : "nothing")
                         .Put((string note) => Workspace.WriteText("note.txt", note));
            """;

        foreach (var name in new[] { "one", "two" })
        {
            var key = await several.DeployAsync(name, "notes", code);

            using var enabled = await several.SendAsync(HttpMethod.Put, name, $"/api/v1/lambdas/{key}/data/workspace");

            Assert.IsTrue(enabled.IsSuccessStatusCode, await enabled.Content.ReadAsStringAsync());
        }

        using var written = await several.SendAsync(HttpMethod.Put, "one", "/lambda/notes/?note=from-one");

        Assert.IsTrue(written.IsSuccessStatusCode, await written.Content.ReadAsStringAsync());

        Assert.AreEqual("from-one", await several.AskAsync("one", "notes"));
        Assert.AreEqual("nothing", await several.AskAsync("two", "notes"));
    }

    [TestMethod]
    public async Task TheOperatorOfOneIsNobodyInAnother()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        await several.StartAsync("one");
        await several.StartAsync("two");

        using var own = await several.GetAsync("one", "/api/v1/admin/overview", operatorOf: "one");
        using var other = await several.GetAsync("two", "/api/v1/admin/overview", operatorOf: "one");

        Assert.AreEqual(HttpStatusCode.OK, own.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, other.StatusCode);

        // the panel's limits are the installation's as well, though its
        // requests and those of the others arrive at the same handler tree
        using var brand = await several.GetAsync("two", "/api/v1/admin/limits", operatorOf: "two");

        Assert.AreEqual(HttpStatusCode.OK, brand.StatusCode);
    }

    [TestMethod]
    public async Task WhatIsPrintedIsInTheLogOfItsInstallationOnly()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        await several.StartAsync("one");
        await several.StartAsync("two");

        await several.DeployAsync("one", "printer", """
            Console.WriteLine("printed as it starts");
            return Inline.Create().Get(() => { Console.WriteLine("printed as it is asked"); return "done"; });
            """);

        Assert.AreEqual("done", await several.AskAsync("one", "printer"));

        Console.WriteLine("printed by the host");

        var one = several.LogOf("one");
        var two = several.LogOf("two");

        foreach (var text in new[] { "printed as it starts", "printed as it is asked" })
        {
            Assert.AreEqual("printer", one.Single(l => l.Text == text).Lambda);
            Assert.IsFalse(two.Any(l => l.Text == text), $"'{text}' is of no concern to another installation");
        }

        Assert.IsFalse(one.Concat(two).Any(l => l.Text == "printed by the host"), "nor is what the host says");
    }

    [TestMethod]
    public async Task WhatALambdaPrintsAsItIsWarmedUpIsItsOwn()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        await several.StartAsync("one");
        await several.StartAsync("two");

        var key = await several.DeployAsync("one", "warm", "Console.WriteLine(\"warming up\"); return Inline.Create().Get(() => \"warm\");");

        await several["one"].Services.GetRequiredService<IMetaService>().ChangeTierAsync(key, LambdaTier.Production);

        await several.StopAsync("one");
        await several.StartAsync("one");

        await several["one"].Services.GetRequiredService<LambdaWarmup>().WarmUpAsync();

        Assert.AreEqual("warm", several.LogOf("one").Single(l => l.Text == "warming up").Lambda);
        Assert.IsFalse(several.LogOf("two").Any(l => l.Text == "warming up"));
    }

    [TestMethod]
    public async Task TheProcessIsNotDescribedToAnOperator()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        await several.StartAsync("one");

        using var _ = await several.GetAsync("one", "/api/v1/system");

        using var response = await several.GetAsync("one", "/api/v1/telemetry", operatorOf: "one");

        var telemetry = await response.GetContentAsync<TelemetryResponse>();

        Assert.IsTrue(telemetry.Server.Shared);

        Assert.AreEqual(0, telemetry.Latest.ManagedBytes, "the memory is everybody's");
        Assert.AreEqual(0, telemetry.Latest.WorkingSetBytes);
        Assert.AreEqual(0, telemetry.Traffic.OpenSockets, "and so are the connections");

        Assert.IsGreaterThan(0, telemetry.Traffic.Requests, "what was asked of this installation is its own");

        Assert.IsFalse(File.Exists(Path.Combine(several.DataOf("one"), "run.json")), "how the process runs is not the installation's to write down");
    }

    #endregion

    #region What they share

    [TestMethod]
    public async Task WhereAddressesAreIsKnownToAllOfThem()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        var one = await several.StartAsync("one");
        var two = await several.StartAsync("two");

        // the test's own address, in both families
        one.Services.GetRequiredService<GeoTable>().Load(["test|PT|ipv4|127.0.0.0|16777216|20000101|allocated", "test|PT|ipv6|::1|128|20000101|allocated"]);

        using var _ = await several.GetAsync("two", "/api/v1/system");

        Assert.IsTrue(several.LogOf("two").Any(l => l.Country == "PT"), "read once, for every installation of the process");

        Assert.AreSame(one.Services.GetRequiredService<GeoTable>(), two.Services.GetRequiredService<GeoTable>());
    }

    [TestMethod]
    public async Task RequestsOfAllOfThemTakeTheSlotsOfTheProcess()
    {
        await using var several = await SeveralPlatforms.CreateAsync(o => o with { MaxConcurrency = 1 });

        await several.StartAsync("one");
        await several.StartAsync("two");

        await several.DeployAsync("one", "slow", "return Inline.Create().Get(async () => { await Task.Delay(7000); return \"at last\"; });");
        await several.DeployAsync("two", "quick", "return Inline.Create().Get(() => \"quick\");");

        var slow = several.AskAsync("one", "slow");

        await Task.Delay(500);

        Assert.AreEqual("503", await several.AskAsync("two", "quick"), "the one slot is taken for longer than anybody waits");

        Assert.AreEqual("at last", await slow);

        Assert.AreEqual("quick", await several.AskAsync("two", "quick"));
    }

    #endregion

    #region Coming and going

    [TestMethod]
    public async Task OneIsAddedAndTakenAwayWhileTheOthersGoOn()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        await several.StartAsync("one");

        await several.DeployAsync("one", "quiz", "return Inline.Create().Get(() => \"one\");");

        await several.StartAsync("two");

        await several.DeployAsync("two", "quiz", "return Inline.Create().Get(() => \"two\");");

        Assert.AreEqual("two", await several.AskAsync("two", "quiz"));

        await several.StopAsync("two");

        Assert.AreEqual("404", await several.AskAsync("two", "quiz"));

        // everything the API is made of, by a platform that is not the one
        // that was prepared last
        Assert.AreEqual("one", await several.AskAsync("one", "quiz"));

        await several.DeployAsync("one", "another", "return Inline.Create().Get(() => \"another\");");

        Assert.AreEqual("another", await several.AskAsync("one", "another"));
    }

    [TestMethod]
    public async Task OneThatIsStoppedAndStartedAgainIsWhatItWas()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        await several.StartAsync("one");

        await several.DeployAsync("one", "quiz", "return Inline.Create().Get(() => \"still here\");");

        Assert.AreEqual("still here", await several.AskAsync("one", "quiz"));

        for (var i = 0; i < 2; i++)
        {
            await several.StopAsync("one");
            await several.StartAsync("one");

            Assert.AreEqual("still here", await several.AskAsync("one", "quiz"));
        }
    }

    /// <summary>
    /// GenHTTP builds its invocation code from the files behind the assemblies
    /// that are loaded, so one of them removed would break every lambda that
    /// is deployed afterwards, in every installation.
    /// </summary>
    [TestMethod]
    public async Task OneThatIsRemovedWithItsDataLeavesTheOthersAbleToDeploy()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        await several.StartAsync("one");
        await several.StartAsync("two");

        // nothing in it that invocation code is built for, so nobody has read its file yet
        await several.DeployAsync("one", "page", "return Content.From(Resource.FromString(\"a page\"));");

        Assert.AreEqual("a page", await several.AskAsync("one", "page"));

        await several.StopAsync("one");

        Directory.Delete(several.DataOf("one"), true);

        await several.DeployAsync("two", "sums", "return Inline.Create().Get((int number) => $\"got {number}\");");

        using var response = await several.GetAsync("two", "/lambda/sums/?number=7");

        Assert.AreEqual("got 7", await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task OneThatIsStoppedLeavesTheOthersAbleToDeploy()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        await several.StartAsync("one");
        await several.StartAsync("two");

        await several.DeployAsync("one", "page", "return Content.From(Resource.FromString(\"a page\"));");

        // what a run before left behind is thrown away as it starts, which
        // this is not
        await several.StopAsync("one");
        await several.StartAsync("one");

        await several.DeployAsync("two", "sums", "return Inline.Create().Get((int number) => $\"got {number}\");");

        using var response = await several.GetAsync("two", "/lambda/sums/?number=7");

        Assert.AreEqual("got 7", await response.Content.ReadAsStringAsync());

        Assert.AreEqual("a page", await several.AskAsync("one", "page"));
    }

    [TestMethod]
    public async Task OneThatIsGoneIsLetGoOf()
    {
        await using var several = await SeveralPlatforms.CreateAsync();

        await several.StartAsync("other");

        var gone = await ComeAndGoAsync(several);

        // the server keeps what a request was served with until the next one
        for (var i = 0; i < 20 && gone.IsAlive; i++)
        {
            using var _ = await several.GetAsync("other", "/api/v1/system");

            await Task.Delay(100);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Assert.IsFalse(gone.IsAlive, "its lambdas stay loaded, and must not hold on to the installation they ran in");

        if (OperatingSystem.IsLinux())
        {
            var open = Directory.GetFiles("/proc/self/fd")
                                .Select(f => new FileInfo(f).LinkTarget)
                                .Where(f => f?.StartsWith(several.DataOf("one") + "/", StringComparison.Ordinal) ?? false)
                                .ToList();

            Assert.IsEmpty(open, "nor must any of its files stay open: " + string.Join(", ", open));
        }
    }

    /// <summary>
    /// In a method of its own, so nothing of the test's holds on to it.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task<WeakReference> ComeAndGoAsync(SeveralPlatforms several)
    {
        await several.StartAsync("one");

        var key = await several.DeployAsync("one", "notes", new LambdaFile(LambdaSource.EntryName, """
            return Inline.Create().Get(async () =>
            {
                await using var connection = await Database.GetConnectionAsync();

                Workspace.WriteText("note.txt", connection.State.ToString());

                return Workspace.ReadText("note.txt");
            });
            """));

        foreach (var kind in new[] { "workspace", "database", "secrets" })
        {
            using var enabled = await several.SendAsync(HttpMethod.Put, "one", $"/api/v1/lambdas/{key}/data/{kind}");

            Assert.IsTrue(enabled.IsSuccessStatusCode, await enabled.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("Open", await several.AskAsync("one", "notes"));

        var container = new WeakReference(several["one"].Services.GetRequiredService<LogBook>());

        await several.StopAsync("one");

        return container;
    }

    #endregion

}
