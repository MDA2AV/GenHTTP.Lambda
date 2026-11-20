using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Secrets;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Tests.Infrastructure;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Integrations;

/// <summary>
/// The life of an integration, as GenHTTP's handlers live theirs: built by a
/// builder that only configures it, prepared as the platform starts, let go
/// of with the platform - and let go of as well where another could not be
/// prepared.
/// </summary>
[TestClass]
public sealed class IntegrationLifecycleTests
{

    [TestMethod]
    public async Task EveryIntegrationIsPreparedAsThePlatformStartsAndLetGoOfWithIt()
    {
        var counter = new YieldingIntegrations.Counter();

        var root = NewRoot();

        try
        {
            await using (await Build(root, YieldingIntegrations.Secrets(counter), counter))
            {
                CollectionAssert.AreEqual(new[] { "prepared database", "prepared files", "prepared secrets" }, counter.Lifecycle.ToArray(),
                                          "in the order they were built, before the platform is handed out");
            }

            CollectionAssert.AreEqual(new[] { "prepared database", "prepared files", "prepared secrets", "disposed secrets", "disposed files", "disposed database" },
                                      counter.Lifecycle.ToArray(), "the other way round as the platform goes");
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [TestMethod]
    public async Task AnIntegrationThatCannotBeReachedStopsThePlatformAndEveryOneBuiltIsLetGoOf()
    {
        var counter = new YieldingIntegrations.Counter();

        var root = NewRoot();

        try
        {
            await Assert.ThrowsExactlyAsync<HttpRequestException>(async () => await Build(root, YieldingIntegrations.UnreachableSecrets(counter), counter));

            CollectionAssert.AreEqual(new[] { "prepared database", "prepared files", "disposed secrets", "disposed files", "disposed database" },
                                      counter.Lifecycle.ToArray(), "the one that failed as well");
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static string NewRoot() => Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

    private static async Task<LambdaPlatform> Build(string root, IIntegrationBuilder<ISecretProvider> secrets, YieldingIntegrations.Counter counter)
    {
        var book = new LogBook(100);

        return await LambdaPlatform.Create()
                                   .Options(new LambdaOptions { DataDirectory = root })
                                   .Logging(Loggers, book)
                                   .Database(YieldingIntegrations.Database(counter))
                                   .Files(YieldingIntegrations.Files(counter))
                                   .Secrets(secrets)
                                   .BuildAsync();
    }

    /// <summary>
    /// Loggers that write nowhere: a platform lets go of none it was handed,
    /// so one set serves every test here.
    /// </summary>
    private static readonly ILoggerFactory Loggers = LoggerFactory.Create(_ => { });

}
