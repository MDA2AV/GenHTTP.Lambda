using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Lambdas;

/// <summary>
/// What is compiled as the server starts, so that the first visitor after a
/// restart does not wait for it.
/// </summary>
/// <remarks>
/// A restart is a lambda that is online with nothing compiled for it, which
/// is what evicting its handler leaves - the server does the same at startup,
/// in the background, once it answers.
/// </remarks>
[TestClass]
public sealed class WarmupTests
{

    /// <summary>
    /// Answers when it was built: the snippet runs as it is built, and the
    /// handler it returns answers every request after.
    /// </summary>
    private const string Stamped = """
        var built = DateTime.UtcNow.Ticks;

        return Inline.Create().Get(() => built.ToString());
        """;

    [TestMethod]
    public async Task LambdasInProductionAndTheSystemTierAreCompiledBeforeTheirFirstRequest()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var shop = await OnlineAsync(fixture, "shop", Stamped, LambdaTier.Production);
        var collector = await OnlineAsync(fixture, "collector", Stamped, LambdaTier.System);

        await RestartAsync(fixture, shop, collector);

        await fixture.WarmUpAsync();

        var visited = DateTime.UtcNow.Ticks;

        Assert.IsLessThan(visited, await BuiltAsync(fixture, "http://shop.localhost/"), "built before anybody asked for it");
        Assert.IsLessThan(visited, await BuiltAsync(fixture, "http://collector.localhost/"));

        Assert.IsTrue(Logged(fixture).Any(l => l.StartsWith("Warmed up 2 lambda(s)", StringComparison.Ordinal)), "and said so");
    }

    [TestMethod]
    public async Task ASandboxLambdaIsStillCompiledOnItsFirstRequest()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var trial = await OnlineAsync(fixture, "trial", Stamped, LambdaTier.Sandbox);

        await RestartAsync(fixture, trial);

        await fixture.WarmUpAsync();

        var visited = DateTime.UtcNow.Ticks;

        Assert.IsGreaterThanOrEqualTo(visited, await BuiltAsync(fixture, "http://trial.localhost/"), "something being tried waits for its first visitor");
    }

    [TestMethod]
    public async Task NothingIsCompiledThatIsNotServed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var stopped = await OnlineAsync(fixture, "stopped", Stamped, LambdaTier.Production);
        var blocked = await OnlineAsync(fixture, "blocked", Stamped, LambdaTier.Production);
        var trial = await OnlineAsync(fixture, "trial", Stamped, LambdaTier.Sandbox);

        await fixture.Meta.UndeployAsync(stopped.PrivateKey);
        await fixture.Meta.BlockAsync(blocked.PrivateKey, "Phishing");

        await RestartAsync(fixture, stopped, blocked, trial);

        var before = Deployed(fixture);

        await fixture.WarmUpAsync();

        Assert.AreEqual(before, Deployed(fixture), "offline, blocked or in the sandbox, none of them is built");
    }

    [TestMethod]
    public async Task ALambdaThatNoLongerBuildsDoesNotKeepTheOthersCold()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // builds the first time and refuses every time after: the counter is
        // the loaded assembly's, which a lambda built again with the same
        // settings reuses - so it is in production before it is built
        var flaky = await fixture.CreateLambdaAsync("flaky");

        await fixture.ChangeTierAsync(flaky.PrivateKey, LambdaTier.Production);

        using (var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{flaky.PrivateKey}/versions", new VersionRequest(
               [
                   new(LambdaSource.EntryName, $$"""
                       // a counter of its own, apart from any other test's: {{Guid.NewGuid():n}}
                       if (Builds.Count++ > 0)
                       {
                           throw new InvalidOperationException("Built once already.");
                       }

                       return Inline.Create().Get(() => "flaky");
                       """),
                   new("Builds.cs", $$"""
                       public static class Builds
                       {
                           public static int Count;

                           public const string Of = "{{Guid.NewGuid():n}}";
                       }
                       """)
               ])))
        {
            Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());
        }

        await fixture.DeployAsync(flaky.PrivateKey);

        var steady = await OnlineAsync(fixture, "steady", Stamped, LambdaTier.Production);

        await RestartAsync(fixture, flaky, steady);

        await fixture.WarmUpAsync();

        var visited = DateTime.UtcNow.Ticks;

        Assert.IsLessThan(visited, await BuiltAsync(fixture, "http://steady.localhost/"), "warmed whichever came first");

        Assert.IsTrue(fixture.Book.Read(0, null, LogLevel.Warning, 5000).Lines.Any(l => l.Text.StartsWith("Failed to warm up lambda flaky", StringComparison.Ordinal)),
                      "the one that failed is said in the log");
    }

    #region Helpers

    /// <summary>
    /// A lambda with the given code online, in the given tier.
    /// </summary>
    private static async Task<LambdaResponse> OnlineAsync(LambdaFixture fixture, string key, string code, LambdaTier tier)
    {
        var lambda = await fixture.CreateLambdaAsync(key);

        await fixture.DeployAsync(lambda.PrivateKey, code);

        if (tier != LambdaTier.Sandbox)
        {
            await fixture.ChangeTierAsync(lambda.PrivateKey, tier);
        }

        return lambda;
    }

    /// <summary>
    /// What a restart leaves of the lambdas: what they have online, and
    /// nothing compiled for it.
    /// </summary>
    private static async Task RestartAsync(LambdaFixture fixture, params LambdaResponse[] lambdas)
    {
        foreach (var lambda in lambdas)
        {
            fixture.Deployments.EvictAll((await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value);
        }
    }

    private static async Task<long> BuiltAsync(LambdaFixture fixture, string address)
    {
        using var served = await fixture.GetAsync(address);

        var content = await served.GetContentAsync();

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode, content);

        return long.Parse(content);
    }

    /// <summary>
    /// How many builds the deployment service has put online so far.
    /// </summary>
    private static int Deployed(LambdaFixture fixture)
        => fixture.Book.Read(0, null, LogLevel.Information, 50_000).Lines.Count(l => l.Text.StartsWith("Deployed lambda #", StringComparison.Ordinal));

    private static IEnumerable<string> Logged(LambdaFixture fixture)
        => fixture.Book.Read(0, null, LogLevel.Information, 50_000).Lines.Select(l => l.Text);

    #endregion

}
