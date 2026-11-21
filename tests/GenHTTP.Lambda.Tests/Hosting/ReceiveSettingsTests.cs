using GenHTTP.Lambda.Server;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// How many reads the io_uring engine shares per reactor and lets a
/// connection have waiting, as the environment says - and that a connection
/// is slowed down rather than dropped, whatever it says.
/// </summary>
/// <remarks>
/// The environment belongs to the whole process, so these change it alone,
/// after the others, and put back what was there.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class ReceiveSettingsTests
{
    private static readonly string[] Variables = ["LAMBDA_RECEIVE_BUFFERS", "LAMBDA_RECEIVE_QUEUE_ENTRIES"];

    private readonly Dictionary<string, string?> _before = [];

    [TestInitialize]
    public void Remember()
    {
        foreach (var variable in Variables)
        {
            _before[variable] = Environment.GetEnvironmentVariable(variable);
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    [TestCleanup]
    public void Restore()
    {
        foreach (var (variable, value) in _before)
        {
            Environment.SetEnvironmentVariable(variable, value);
        }
    }

    [TestMethod]
    public void WithoutBeingToldAReactorSharesAThousandReads()
    {
        var (buffers, queue) = LambdaSettings.FromEnvironment().Server.Receiving();

        Assert.AreEqual(1024, buffers);
        Assert.AreEqual(2048, queue);
    }

    [TestMethod]
    public void AConnectionMayAlwaysHaveTwiceTheReadsOfItsReactorWaiting()
    {
        Environment.SetEnvironmentVariable("LAMBDA_RECEIVE_BUFFERS", "4096");
        Environment.SetEnvironmentVariable("LAMBDA_RECEIVE_QUEUE_ENTRIES", "1024");

        var (buffers, queue) = LambdaSettings.FromEnvironment().Server.Receiving();

        Assert.AreEqual(4096, buffers);
        Assert.AreEqual(8192, queue);
    }

    [TestMethod]
    public void ALongerQueueThanNeededIsKept()
    {
        Environment.SetEnvironmentVariable("LAMBDA_RECEIVE_QUEUE_ENTRIES", "8192");

        Assert.AreEqual(8192, LambdaSettings.FromEnvironment().Server.Receiving().QueueEntries);
    }

    [TestMethod]
    public void NumbersTheEngineCannotTakeAreRoundedUpToAPowerOfTwo()
    {
        Environment.SetEnvironmentVariable("LAMBDA_RECEIVE_BUFFERS", "1000");
        Environment.SetEnvironmentVariable("LAMBDA_RECEIVE_QUEUE_ENTRIES", "3000");

        var (buffers, queue) = LambdaSettings.FromEnvironment().Server.Receiving();

        Assert.AreEqual(1024, buffers);
        Assert.AreEqual(4096, queue);
    }

}
