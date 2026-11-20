using GenHTTP.Lambda.Server;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// Which engine an installation is served on, as its environment says: the
/// one that runs anywhere unless the operator names another.
/// </summary>
/// <remarks>
/// The environment belongs to the whole process, so these change it alone,
/// after the others, and put back what was there.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class EngineSettingsTests
{
    private string? _before;

    [TestInitialize]
    public void Remember()
    {
        _before = Environment.GetEnvironmentVariable("LAMBDA_ENGINE");
        Environment.SetEnvironmentVariable("LAMBDA_ENGINE", null);
    }

    [TestCleanup]
    public void Restore() => Environment.SetEnvironmentVariable("LAMBDA_ENGINE", _before);

    [TestMethod]
    public void WithoutBeingToldTheServerRunsOnTheInternalEngine()
    {
        Assert.AreEqual(LambdaEngine.Internal, LambdaSettings.FromEnvironment().Server.Engine);
    }

    [TestMethod]
    public void AnEngineNobodyKnowsFallsBackToTheInternalEngine()
    {
        Environment.SetEnvironmentVariable("LAMBDA_ENGINE", "io-uring");

        Assert.AreEqual(LambdaEngine.Internal, LambdaSettings.FromEnvironment().Server.Engine);
    }

    [TestMethod]
    public void IoxideIsUsedWhereItIsNamed()
    {
        Environment.SetEnvironmentVariable("LAMBDA_ENGINE", "ioxide");

        Assert.AreEqual(LambdaEngine.Ioxide, LambdaSettings.FromEnvironment().Server.Engine);
    }

}
